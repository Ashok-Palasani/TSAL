using i_facility.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace i_facility.Controllers
{
    public class CriticalMachineController : Controller
    {
        i_facility_tsalEntities1 db = new i_facility_tsalEntities1();

        // GET: CriticalMachine
        public ActionResult Index()
        {
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            Session["Error"] = null;
            var Escalationdata = db.tblemailescalations.Where(m => m.IsDeleted == 0 && m.IsCritical == 1).Take(50).ToList();
            return View(Escalationdata);
        }


        public JsonResult DeleteAllExc(MyValues data)
        {
            string result = "false";
            try
            {
                if (data.values.Count() > 0)
                {
                    foreach (var id in data.values)
                    {
                        int exclationId = Convert.ToInt32(id);
                        var deleteRow = db.tblemailescalations.Where(x => x.IsDeleted == 0 && x.EMailEscalationID == exclationId && x.IsCritical == 1).FirstOrDefault();
                        if (deleteRow != null)
                        {
                            deleteRow.IsDeleted = 1;
                            db.SaveChanges();
                            result = "True";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            //ViewBag.RL1 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1), "LossCodeID", "LossCode");
            //ViewBag.RL1 += new SelectList(db.tblbreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakDownCodesLevel == 1), "BreakDownCodeID", "BreakDownCode");
            //List<string, string> BList = db.tblbreakdowncodes.Where(m => m.IsDeleted == 0 && m.BreakDownCodesLevel == 1).Select(new { m=>m.BreakDownCodeID,m.BreakDownCode}).ToList();

            ViewBag.RL1 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "9999" && m.MessageType != "PM"), "LossCodeID", "LossCode");
            ViewBag.RL2 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 4), "LossCodeID", "LossCode");
            ViewBag.RL3 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 4), "LossCodeID", "LossCode");

            ViewBag.RL4 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1 && m.HoldCode != "9999" && m.MessageType != "PM"), "HoldCodeID", "HoldCode");
            ViewBag.RL5 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 4), "HoldCodeID", "HoldCode");
            ViewBag.RL6 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 4), "HoldCodeID", "HoldCode");


            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineInvNo");
            return View();
        }
        [HttpPost]
        public ActionResult Create(tblemailescalation tee)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            #region//ActiveLog Code
            int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            // ActiveLogStorage Obj = new ActiveLogStorage();
            // Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            #endregion

            //Email validation
            string RL1 = "";
            string RL2 = "";
            string RL3 = "";
            int flage = 0;
            int EmailEscID = Convert.ToInt32(tee.EMailEscalationID);
            if (tee.ReasonLevel1 != 0 && tee.ReasonLevel1 != null)
            {
                RL1 = Convert.ToString(tee.ReasonLevel1);
                RL2 = Convert.ToString(tee.ReasonLevel2);
                RL3 = Convert.ToString(tee.ReasonLevel3);
                flage = 0;
            }
            else if (tee.HoldReason1 != 0 && tee.HoldReason1 != null)
            {
                RL1 = Convert.ToString(tee.HoldReason1);
                RL2 = Convert.ToString(tee.HoldReason2);
                RL3 = Convert.ToString(tee.HoldReason3);
                flage = 1;
            }
            string Plantidstring = Convert.ToString(tee.PlantID);
            string Shopidstring = Convert.ToString(tee.ShopID);
            string Cellidstring = Convert.ToString(tee.CellID);
            string WorkCenteridstring = Convert.ToString(tee.WorkCenterID);
            int hours = Convert.ToInt32(tee.Hours);
            int minutes = Convert.ToInt32(tee.Minutes);
            string ValidEscalation = null;

            ValidEscalation = IsItValidEscalation(flage, RL1, RL2, RL3, Plantidstring, Shopidstring, Cellidstring, WorkCenteridstring, hours, minutes, EmailEscID);

            if (ValidEscalation == null)
            {
                int reasonId = 0;
                if (tee.ReasonLevel1 != 0 && tee.ReasonLevel1 != null)
                {
                    reasonId = Convert.ToInt32(tee.ReasonLevel1);
                }
                else if (tee.HoldReason1 != 0 && tee.HoldReason1 != null)
                {
                    reasonId = Convert.ToInt32(tee.HoldReason1);
                }
                string msgType = GetReasonType(reasonId, flage);

                tee.MessageType = msgType;
                tee.CreatedBy = UserID;
                tee.CreatedOn = DateTime.Now;
                tee.IsCritical = 1;
                tee.IsDeleted = 0;

                db.tblemailescalations.Add(tee);
                db.SaveChanges();
            }
            else
            {
                Session["Error"] = ValidEscalation;
                ViewBag.RL1 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1), "LossCodeID", "LossCode", tee.ReasonLevel1);
                ViewBag.RL2 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == tee.ReasonLevel1), "LossCodeID", "LossCode", tee.ReasonLevel2);
                ViewBag.RL3 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 3 && m.LossCodesLevel2ID == tee.ReasonLevel2), "LossCodeID", "LossCode", tee.ReasonLevel3);

                ViewBag.RL4 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1), "HoldCodeID", "HoldCode", tee.HoldReason1);
                ViewBag.RL5 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.HoldCodesLevel1ID == tee.HoldReason1), "HoldCodeID", "HoldCode");
                ViewBag.RL6 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 3 && m.HoldCodesLevel2ID == tee.HoldReason2), "HoldCodeID", "HoldCode");


                ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
                ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
                ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "MachineID", "MachineDispName", tee.WorkCenterID);

                return View(tee);
            }

            return RedirectToAction("Index");
        }


        public JsonResult CreateHoldEsclation(string esName, int hl1Value, int hl2Value, int hl3Value, int plantValue, int shopValue, int cellValue, int machineValue, int hourValue, int minuteValue, string toMailValue, string ccMailValue)
        {
            string result = "false";

            if (esName != "" && hl1Value != 0 && plantValue != 0 && toMailValue != "")
            {
                tblemailescalation addRow = new tblemailescalation();
                addRow.EMailEscalationName = esName;
                if (ccMailValue != "")
                {
                    addRow.CcList = ccMailValue;
                }
                if (cellValue != 0)
                {
                    addRow.CellID = cellValue;
                }
                if (shopValue != 0)
                {
                    addRow.ShopID = shopValue;
                }
                if (plantValue != 0)
                {
                    addRow.PlantID = plantValue;
                }
                if (machineValue != 0)
                {
                    addRow.WorkCenterID = machineValue;
                }

                addRow.CreatedBy = 1;
                addRow.CreatedOn = DateTime.Now;
                addRow.HoldReason1 = hl1Value;
                if (hl2Value != 0)
                {
                    addRow.HoldReason2 = hl2Value;
                }
                if (hl3Value != 0)
                {
                    addRow.HoldReason3 = hl3Value;
                }

                addRow.Hours = hourValue;
                addRow.Minutes = minuteValue;
                addRow.ToList = toMailValue;
                addRow.IsCritical = 1;
                db.tblemailescalations.Add(addRow);
                db.SaveChanges();
                result = "true";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            tblemailescalation tee = db.tblemailescalations.Find(id);

            ViewBag.RL1 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.LossCode != "9999" && m.MessageType != "PM"), "LossCodeID", "LossCode", tee.ReasonLevel1);
            ViewBag.RL2 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == tee.ReasonLevel1), "LossCodeID", "LossCode", tee.ReasonLevel2);
            ViewBag.RL3 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 3 && m.LossCodesLevel2ID == tee.ReasonLevel2), "LossCodeID", "LossCode", tee.ReasonLevel3);

            ViewBag.RL4 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1), "HoldCodeID", "HoldCode", tee.HoldReason1);
            ViewBag.RL5 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.HoldCodesLevel1ID == tee.HoldReason1), "HoldCodeID", "HoldCode");
            ViewBag.RL6 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 3 && m.HoldCodesLevel2ID == tee.HoldReason2), "HoldCodeID", "HoldCode");


            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
            ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
            ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
            //bool tick = doesThisMachineHasCELL(Convert.ToInt32(tee.WorkCenterID));
            if (tee.CellID != null || tee.CellID != 0)
            {
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == tee.CellID), "MachineID", "MachineInvNo", tee.WorkCenterID);
            }
            else
            {
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "MachineID", "MachineInvNo", tee.WorkCenterID);
            }

            return View(tee);
        }
        [HttpPost]
        public ActionResult Edit(tblemailescalation tee)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            #region//ActiveLog Code
            int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            // ActiveLogStorage Obj = new ActiveLogStorage();
            // Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            #endregion
            string RL1 = "";
            string RL2 = "";
            string RL3 = "";
            int flage = 0;
            //Email validation
            int EmailEscID = Convert.ToInt32(tee.EMailEscalationID);
            if (tee.ReasonLevel1 != 0 && tee.ReasonLevel1 != null)
            {
                RL1 = Convert.ToString(tee.ReasonLevel1);
                RL2 = Convert.ToString(tee.ReasonLevel2);
                RL3 = Convert.ToString(tee.ReasonLevel3);
                flage = 0;
            }
            else if (tee.HoldReason1 != 0 && tee.HoldReason1 != null)
            {
                RL1 = Convert.ToString(tee.HoldReason1);
                RL2 = Convert.ToString(tee.HoldReason2);
                RL3 = Convert.ToString(tee.HoldReason3);
                flage = 1;
            }
            string Plantidstring = Convert.ToString(tee.PlantID);
            string Shopidstring = Convert.ToString(tee.ShopID);
            string Cellidstring = Convert.ToString(tee.CellID);
            string WorkCenteridstring = Convert.ToString(tee.WorkCenterID);
            int hours = Convert.ToInt32(tee.Hours);
            int minutes = Convert.ToInt32(tee.Minutes);
            string ValidEscalation = null;

            ValidEscalation = IsItValidEscalation(flage, RL1, RL2, RL3, Plantidstring, Shopidstring, Cellidstring, WorkCenteridstring, hours, minutes, EmailEscID);

            if (ValidEscalation == null)
            {
                int reasonId = 0;
                if (tee.ReasonLevel1 != 0 && tee.ReasonLevel1 != null)
                {
                    reasonId = Convert.ToInt32(tee.ReasonLevel1);
                }
                else if (tee.HoldReason1 != 0 && tee.HoldReason1 != null)
                {
                    reasonId = Convert.ToInt32(tee.HoldReason1);
                }
                string msgType = GetReasonType(reasonId, flage);

                tee.MessageType = msgType;
                tee.ModifiedBy = UserID;
                tee.ModifiedOn = DateTime.Now;
                tee.IsDeleted = 0;

                db.Entry(tee).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                Session["Error"] = ValidEscalation;
                ViewBag.RL1 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1), "LossCodeID", "LossCode", tee.ReasonLevel1);
                ViewBag.RL2 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == tee.ReasonLevel1), "LossCodeID", "LossCode", tee.ReasonLevel2);
                ViewBag.RL3 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 3 && m.LossCodesLevel2ID == tee.ReasonLevel2), "LossCodeID", "LossCode", tee.ReasonLevel3);

                ViewBag.RL4 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1), "HoldCodeID", "HoldCode", tee.HoldReason1);
                ViewBag.RL5 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.HoldCodesLevel1ID == tee.HoldReason1), "HoldCodeID", "HoldCode");
                ViewBag.RL6 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 3 && m.HoldCodesLevel2ID == tee.HoldReason2), "HoldCodeID", "HoldCode");


                ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
                ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
                ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "MachineID", "MachineDispName", tee.WorkCenterID);

                return View(tee);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Copy(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            tblemailescalation tee = db.tblemailescalations.Find(id);

            ViewBag.RL1 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1), "LossCodeID", "LossCode", tee.ReasonLevel1);
            ViewBag.RL2 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == tee.ReasonLevel1), "LossCodeID", "LossCode", tee.ReasonLevel2);
            ViewBag.RL3 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 3 && m.LossCodesLevel2ID == tee.ReasonLevel2), "LossCodeID", "LossCode", tee.ReasonLevel3);

            ViewBag.RL4 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1), "HoldCodeID", "HoldCode", tee.HoldReason1);
            ViewBag.RL5 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.HoldCodesLevel1ID == tee.HoldReason1), "HoldCodeID", "HoldCode");
            ViewBag.RL6 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 3 && m.HoldCodesLevel2ID == tee.HoldReason2), "HoldCodeID", "HoldCode");


            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
            ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
            ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
            //bool tick = doesThisMachineHasCELL(Convert.ToInt32(tee.ShopID));
            if (tee.CellID != null || tee.CellID != 0)
            {
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == tee.CellID), "MachineID", "MachineDispName", tee.WorkCenterID);
            }
            else
            {
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "MachineID", "MachineDispName", tee.WorkCenterID);
            }

            return View(tee);
        }
        [HttpPost]
        public ActionResult Copy(tblemailescalation tee)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            #region//ActiveLog Code
            int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            // ActiveLogStorage Obj = new ActiveLogStorage();
            // Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            #endregion

            //Email validation
            string RL1 = "";
            string RL2 = "";
            string RL3 = "";
            int flage = 0;
            int EmailEscID = 0;
            if (tee.ReasonLevel1 != 0 && tee.ReasonLevel1 != null)
            {
                RL1 = Convert.ToString(tee.ReasonLevel1);
                RL2 = Convert.ToString(tee.ReasonLevel2);
                RL3 = Convert.ToString(tee.ReasonLevel3);
                flage = 0;
            }
            else if (tee.HoldReason1 != 0 && tee.HoldReason1 != null)
            {
                RL1 = Convert.ToString(tee.HoldReason1);
                RL2 = Convert.ToString(tee.HoldReason2);
                RL3 = Convert.ToString(tee.HoldReason3);
                flage = 1;
            }
            string Plantidstring = Convert.ToString(tee.PlantID);
            string Shopidstring = Convert.ToString(tee.ShopID);
            string Cellidstring = Convert.ToString(tee.CellID);
            string WorkCenteridstring = Convert.ToString(tee.WorkCenterID);
            int hours = Convert.ToInt32(tee.Hours);
            int minutes = Convert.ToInt32(tee.Minutes);
            string ValidEscalation = null;

            ValidEscalation = IsItValidEscalation(flage, RL1, RL2, RL3, Plantidstring, Shopidstring, Cellidstring, WorkCenteridstring, hours, minutes, EmailEscID);

            if (ValidEscalation == null)
            {
                int reasonId = 0;
                if (tee.ReasonLevel1 != 0 && tee.ReasonLevel1 != null)
                {
                    reasonId = Convert.ToInt32(tee.ReasonLevel1);
                }
                else if (tee.HoldReason1 != 0 && tee.HoldReason1 != null)
                {
                    reasonId = Convert.ToInt32(tee.HoldReason1);
                }
                string msgType = GetReasonType(reasonId, flage);

                tee.MessageType = msgType;
                tee.CreatedBy = UserID;
                tee.CreatedOn = DateTime.Now;
                tee.IsCritical = 1;
                tee.IsDeleted = 0;

                db.tblemailescalations.Add(tee);
                db.SaveChanges();
            }
            else
            {
                Session["Error"] = ValidEscalation;
                ViewBag.RL1 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1), "LossCodeID", "LossCode", tee.ReasonLevel1);
                ViewBag.RL2 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == tee.ReasonLevel1), "LossCodeID", "LossCode", tee.ReasonLevel2);
                ViewBag.RL3 = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 3 && m.LossCodesLevel2ID == tee.ReasonLevel2), "LossCodeID", "LossCode", tee.ReasonLevel3);

                ViewBag.RL4 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1), "HoldCodeID", "HoldCode", tee.HoldReason1);
                ViewBag.RL5 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.HoldCodesLevel1ID == tee.HoldReason1), "HoldCodeID", "HoldCode");
                ViewBag.RL6 = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 3 && m.HoldCodesLevel2ID == tee.HoldReason2), "HoldCodeID", "HoldCode");


                ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
                ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
                ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "MachineID", "MachineDispName", tee.WorkCenterID);

                return View(tee);
            }

            return RedirectToAction("Index");
        }

        bool doesThisMachineHasCELL(int macid)
        {
            bool result = false;
            var machdetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == macid).SingleOrDefault();

            if (Convert.ToInt32(machdetails.CellID) != 0)
            {
                result = true;
            }
            return result;
        }

        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            int UserID1 = id;
            //ViewBag.IsConfigMenu = 0;
            tblemailescalation tee = db.tblemailescalations.Find(id);
            tee.IsDeleted = 1;
            tee.ModifiedBy = UserID1;
            tee.ModifiedOn = System.DateTime.Now;
            //start Logging
            int UserID = Convert.ToInt32(Session["UserId"]);
            String Username = Session["Username"].ToString();
            //string CompleteModificationdetail = "Deleted Parts/Item";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            db.Entry(tee).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public string IsItValidEscalation(int flage, string RL1, string RL2, string RL3, string Plantid, string Shopid, string Cellid, string WorkCenterid, int Hours, int Minutes, int emailescID = 0, string ForUpload = null)
        {
            List<int> DoesThisPlanOverlapUpwards = new List<int>(), DoesThisPlanOverlapDownwards = new List<int>(), DoesThisPlanOverlapAll = new List<int>();
            string msg = null;

            if (!String.IsNullOrEmpty(Shopid))
            {
                if (!String.IsNullOrEmpty(Cellid))
                {
                    if (!String.IsNullOrEmpty(WorkCenterid))
                    {
                        int wcid = Convert.ToInt32(WorkCenterid);
                        DoesThisPlanOverlapUpwards = Escalation_OverlapCheckerForMachine(flage, wcid, RL1, RL2, RL3, Hours, Minutes, emailescID);
                        DoesThisPlanOverlapDownwards = Escalation_OverlapCheckerForMachineDownwards(flage, wcid, RL1, RL2, RL3, Hours, Minutes, emailescID);
                    }
                    else
                    {
                        int cellid = Convert.ToInt32(Cellid);
                        DoesThisPlanOverlapUpwards = Escalation_OverlapCheckerForCell(flage, cellid, RL1, RL2, RL3, Hours, Minutes, emailescID);
                        DoesThisPlanOverlapDownwards = Escalation_OverlapCheckerForCellDownwards(flage, cellid, RL1, RL2, RL3, Hours, Minutes, emailescID);
                    }
                }
                else
                {
                    int shopid = Convert.ToInt32(Shopid);
                    DoesThisPlanOverlapUpwards = Escalation_OverlapCheckerForShop(flage, shopid, RL1, RL2, RL3, Hours, Minutes, emailescID);
                    DoesThisPlanOverlapDownwards = Escalation_OverlapCheckerForShopDownwards(flage, shopid, RL1, RL2, RL3, Hours, Minutes, emailescID);
                }
            }
            else
            {
                int plantid = Convert.ToInt32(Plantid);
                DoesThisPlanOverlapUpwards = Escalation_OverlapCheckerForPlant(flage, plantid, RL1, RL2, RL3, Hours, Minutes, emailescID);
                DoesThisPlanOverlapDownwards = Escalation_OverlapCheckerForPlantDownwards(flage, plantid, RL1, RL2, RL3, Hours, Minutes, emailescID);
            }

            ////move all id's into one list and only takes distinct.
            DoesThisPlanOverlapAll.AddRange(DoesThisPlanOverlapUpwards);
            DoesThisPlanOverlapAll.AddRange(DoesThisPlanOverlapDownwards);

            DoesThisPlanOverlapAll = (from n in DoesThisPlanOverlapAll
                                      select n).Distinct().ToList();

            if (DoesThisPlanOverlapAll.Count == 0) //plan doesn't ovelap. So commit.
            {
                msg = null;
            }
            else
            {
                var results = db.tblemailescalations.Where(m => m.IsDeleted == 0).Where(x => DoesThisPlanOverlapAll.Contains(x.EMailEscalationID));
                string OLPD = null;
                if (ForUpload == null)
                {
                    OLPD = "<div  style='font-size:.75vw'>";
                    OLPD += "<p><span>This E-mail Escalations conflicts with  </span></p>";
                    foreach (var row in results)
                    {
                        OLPD += "<p><span>E-Mail Escalation Name : " + row.EMailEscalationName + " </span></p>";
                    }
                    OLPD += "</div>";
                }
                else
                {
                    int i = 0;
                    foreach (var row in results)
                    {
                        if (i == 0)
                        {
                            OLPD += row.EMailEscalationID;
                        }
                        else
                        {
                            OLPD += "," + row.EMailEscalationID;
                        }
                        i++;
                    }
                }
                msg = OLPD;
            }
            return msg;

        }

        public List<int> Escalation_OverlapCheckerForPlantDownwards(int flage, int plantid, string RL1, string RL2, string RL3, int Hours = 0, int Minutes = 0, int emailescID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int PlantID = plantid;
            DataTable dataHolder = new DataTable();

            //1st check if its shop has a Plan.
            //so get its shopid.
            var shopdetails = db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID).ToList();
            foreach (var shoprow in shopdetails)
            {
                int shopId = shoprow.ShopID;
                overlappingPlanId = Escalation_OverlapCheckerForShopDownwards(flage, shopId, RL1, RL2, RL3, Hours, Minutes, emailescID);
                if (overlappingPlanId.Count > 0)
                {
                    break;
                }
            }

            if (overlappingPlanId.Count == 0)
            {
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                String sql = null;
                if (emailescID != 0)
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }

                    }
                    else
                    {
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }

                    }
                }
                else
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                    else
                    {
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID is null and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null  and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();

                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }
            }
            return overlappingPlanId;
        }
        public List<int> Escalation_OverlapCheckerForShopDownwards(int flage, int shopid, string RL1, string RL2, string RL3, int Hours = 0, int Minutes = 0, int emailescID = 0)
        {
            List<int> overlappingPlanId = new List<int>(), overlappingPlanId1 = new List<int>(), overlappingPlanId2 = new List<int>();
            int ShopID = shopid;

            //1st check if its Cells has a Plan.
            //so get its cellid.
            var celldetails = db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            foreach (var cellrow in celldetails)
            {
                int cellId = cellrow.CellID;
                overlappingPlanId = Escalation_OverlapCheckerForCellDownwards(flage, cellId, RL1, RL2, RL3, Hours, Minutes, emailescID);
                if (overlappingPlanId.Count > 0)
                {
                    break;
                }
            }

            var machinedetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            foreach (var machinerow in machinedetails)
            {
                int machineId = machinerow.MachineID;
                overlappingPlanId1 = Escalation_OverlapCheckerForMachineDownwards(flage, machineId, RL1, RL2, RL3, Hours, Minutes, emailescID);
                if (overlappingPlanId1.Count > 0)
                {
                    break;
                }
            }

            //move all id's into one list.

            overlappingPlanId2.AddRange(overlappingPlanId);
            overlappingPlanId2.AddRange(overlappingPlanId1);

            if (overlappingPlanId2.Count == 0)
            {
                DataTable dataHolder = new DataTable();
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string sql = null;
                if (Hours == 0 && Minutes == 0)
                {
                    if (emailescID != 0)
                    {
                        if (RL2 != "" && RL3 == "") //IT's RL2
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and  ((ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null ) or  ((ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = null )  ) and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            else
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and ((HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null ) or  ((HoldReason1 = '" + RL1 + "'  and HoldReason2 = null )  ) and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and  ((ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null ) or  ((ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = null )  ) and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and  ((HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null ) or  ((HoldReason1 = '" + RL1 + "'    and HoldReason2 = null )  ) and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "") //IT's RL3
                        {

                            //1st and 2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and (( ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null) or ( ReasonLevel1 = '" + RL1 + "'   and ReasonLevel2 = null ) or (ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 =  null )  ) and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and (( HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null) or ( HoldReason1 = '" + RL1 + "'   and HoldReason2 = null ) or (HoldReason2 = '" + RL2 + "'  and HoldReason3 =  null )  ) and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and (( ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "') or ( ReasonLevel1 = '" + RL1 + "'   and ReasonLevel2 = null ) or (ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 =  null )  ) and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and (( HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "') or ( HoldReason1 = '" + RL1 + "'   and HoldReason2 = null ) or (HoldReason2 = '" + RL2 + "'  and HoldReason3 =  null )  ) and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                        }
                        else //IT's RL1
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and  ( ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null)   and EMailEscalationID != " + emailescID + " and IsCritical=1 ;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and (( HoldReason1 = '" + RL1 + "'  and HoldReason2 is null  and HoldReason3 is null)  and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }

                        }
                    }
                    else
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and IsCritical=1 ;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and IsCritical=1 ;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null  and HoldReason3 is null and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and IsCritical=1;";
                            }
                        }
                    }
                }
                else
                {
                    if (emailescID != 0)
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                    }
                    else
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation  where IsDeleted = 0 and  ShopID = '" + ShopID + "' and CellID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();
                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId2.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }
            }
            return overlappingPlanId2;
        }
        public List<int> Escalation_OverlapCheckerForCellDownwards(int flage, int cellid, string RL1, string RL2, string RL3, int Hours = 0, int Minutes = 0, int emailescID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int CellID = cellid;
            DataTable dataHolder = new DataTable();
            //1st check if its machines has a Plan.
            //so get its machineids.
            var machinedetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            foreach (var machinerow in machinedetails)
            {
                int machineId = machinerow.MachineID;
                overlappingPlanId = Escalation_OverlapCheckerForMachineDownwards(flage, machineId, RL1, RL2, RL3, Hours, Minutes, emailescID);
                if (overlappingPlanId.Count > 0)
                {
                    break;
                }
            }

            if (overlappingPlanId.Count == 0)
            {
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string sql = null;
                if (Hours == 0 && Minutes == 0)
                {
                    if (emailescID != 0)
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and EMailEscalationID != " + emailescID + "  and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and EMailEscalationID != " + emailescID + "  and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and HoldReason3 is null  and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                        }
                    }
                    else
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null  and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null  and HoldReason3 is null  and IsCritical=1;";
                            }

                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "'  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "'  and IsCritical=1 ;";
                            }
                        }
                        else
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                        }
                    }
                }
                else
                {
                    if (emailescID != 0)
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + "  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + "  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }

                        }
                    }
                    else
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }

                        }
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();
                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }

            }
            return overlappingPlanId;
        }
        public List<int> Escalation_OverlapCheckerForMachineDownwards(int flage, int wc, string RL1, string RL2, string RL3, int Hours = 0, int Minutes = 0, int emailescID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int WorkCenterID = wc;
            DataTable dataHolder = new DataTable();

            MsqlConnection mc = new MsqlConnection();
            mc.open();

            string sql = null;
            if (Hours == 0 && Minutes == 0)
            {
                if (emailescID != 0)
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                        }
                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and holdReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                        }
                    }
                    else
                    {
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                        }
                    }
                }
                else
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null  and IsCritical=1;";
                        }

                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                        }

                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null  and IsCritical=1;";
                        }
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "'  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "'  and IsCritical=1;";
                        }
                    }
                    else
                    {
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                        }
                    }
                }
            }
            else
            {
                if (emailescID != 0)
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and   WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }

                    }
                    else
                    {
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                }
                else
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }

                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                    else
                    {
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + WorkCenterID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                }
            }
            SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
            da.Fill(dataHolder);
            mc.close();

            for (int i = 0; i < dataHolder.Rows.Count; i++)
            {
                overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
            }
            return overlappingPlanId;
        }

        public List<int> Escalation_OverlapCheckerForPlant(int flage, int plantid, string RL1, string RL2, string RL3, int Hours = 0, int Minutes = 0, int emailescID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int PlantID = plantid;

            DataTable dataHolder = new DataTable();
            MsqlConnection mc = new MsqlConnection();
            mc.open();
            String sql = null;
            string wcString = null;
            string cString = null;
            string sString = null;

            if (Hours == 0 && Minutes == 0)
            {
                if (emailescID != 0)
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID  is null and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and EMailEscalationID != " + emailescID + "  and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID  is null and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and EMailEscalationID != " + emailescID + "  and HoldReason3 is null  and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID  is null and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID  is null and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and HoldReason3 is null  and IsCritical=1;";
                        }
                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                    }
                    else
                    {
                        sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ReasonLevel1 = '" + RL1 + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                    }
                }
                else
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null  and IsCritical=1;";
                        }
                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";

                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 = '" + RL2 + "' and HoldReason3 is null  and IsCritical=1;";
                        }
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "'  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "'  and IsCritical=1;";
                        }
                    }
                    else
                    {
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                        }
                    }
                }
            }
            else
            {
                if (emailescID != 0)
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID  is null and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + "  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID  is null and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + "  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }

                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID  is null and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ShopID  is null and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }

                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }

                    }
                    else
                    {
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and ReasonLevel1 = '" + RL1 + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "' and HoldReason1 = '" + RL1 + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                }
                else
                {
                    if (RL2 != "" && RL3 == "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";

                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }

                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                    else if (RL3 != "")
                    {
                        //1st Level Reason Duplicate
                        DataTable dataHolderInner = new DataTable();
                        MsqlConnection mcInner = new MsqlConnection();
                        mcInner.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                        daInner.Fill(dataHolderInner);
                        mcInner.close();
                        for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                            return overlappingPlanId;
                        }

                        //2nd Level Reason duplicate
                        DataTable dataHolderInner2 = new DataTable();
                        MsqlConnection mcInner2 = new MsqlConnection();
                        mcInner2.open();
                        //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                        daInner2.Fill(dataHolderInner2);
                        mcInner2.close();
                        for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                        {
                            overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                            return overlappingPlanId;
                        }
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID  is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                    else
                    {
                        if (flage == 0)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID is null  and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                        else if (flage == 1)
                        {
                            sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  PlantID = '" + PlantID + "'  and ShopID  is null and CellID is null  and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                        }
                    }
                }
            }
            SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
            da.Fill(dataHolder);
            mc.close();
            for (int i = 0; i < dataHolder.Rows.Count; i++)
            {
                overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
            }
            return overlappingPlanId;
        }
        public List<int> Escalation_OverlapCheckerForShop(int flage, int shopid, string RL1, string RL2, string RL3, int Hours = 0, int Minutes = 0, int emailescID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int ShopID = shopid;

            //1st check if its Plant has a Plan.
            //so get its plantid.
            var plantdetails = db.tblshops.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).FirstOrDefault();
            int plantId = plantdetails.PlantID;
            overlappingPlanId = Escalation_OverlapCheckerForPlant(flage, plantId, RL1, RL2, RL3, Hours, Minutes, emailescID);

            if (overlappingPlanId.Count == 0)
            {
                DataTable dataHolder = new DataTable();
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string sql = null;
                if (Hours == 0 && Minutes == 0)
                {
                    if (emailescID != 0)
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null  and HoldReason3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                            }
                        }
                    }
                    else
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null  and WorkCenterID  is null and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null  and IsCritical=1;";
                            }

                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "'  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "'  and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                            }
                        }
                    }
                }
                else
                {
                    if (emailescID != 0)
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";

                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and ReasonLevel1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID   is null   and WorkCenterID   is null  and HoldReason1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                    }
                    else
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID  is null   and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID is null   and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  ShopID = '" + ShopID + "'  and CellID is null   and WorkCenterID is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();

                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i]["EMailEscalationID"]));
                }
            }
            return overlappingPlanId;
        }
        public List<int> Escalation_OverlapCheckerForCell(int flage, int cellid, string RL1, string RL2, string RL3, int Hours = 0, int Minutes = 0, int emailescID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int CellID = cellid;
            DataTable dataHolder = new DataTable();

            //1st check if its Shop has a Plan.
            //so get its shopid.
            var Celldetails = db.tblcells.Where(m => m.IsDeleted == 0 && m.CellID == CellID).FirstOrDefault();
            int shopId = Celldetails.ShopID;
            overlappingPlanId = Escalation_OverlapCheckerForShop(flage, shopId, RL1, RL2, RL3, Hours, Minutes, emailescID);

            if (overlappingPlanId.Count == 0)
            {
                MsqlConnection mc = new MsqlConnection();
                string sql = null;
                if (Hours == 0 && Minutes == 0)
                {
                    if (emailescID != 0)
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();

                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1 ;";
                            }

                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and HoldReason3 is null and IsCritical=1 ;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                            }
                        }
                    }
                    else
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null  and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {

                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "'  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "'  and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                            }
                        }
                    }
                }
                else
                {
                    if (emailescID != 0)
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID   is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID   is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and HoldReason3 is null and IsCritical=1 ;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID   is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID   is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null  and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null  and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                    }
                    else
                    {
                        if (RL2 != "" && RL3 == "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else if (RL3 != "")
                        {
                            //1st Level Reason Duplicate
                            DataTable dataHolderInner = new DataTable();
                            MsqlConnection mcInner = new MsqlConnection();
                            mcInner.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                            daInner.Fill(dataHolderInner);
                            mcInner.close();
                            for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                return overlappingPlanId;
                            }

                            //2nd Level Reason duplicate
                            DataTable dataHolderInner2 = new DataTable();
                            MsqlConnection mcInner2 = new MsqlConnection();
                            mcInner2.open();
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and HoldReason1 = '" + RL1 + "' and HoldReason2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                            }
                            SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                            daInner2.Fill(dataHolderInner2);
                            mcInner2.close();
                            for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                            {
                                overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                return overlappingPlanId;
                            }
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                        else
                        {
                            if (flage == 0)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                            else if (flage == 1)
                            {
                                sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID  is null  and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                            }
                        }
                    }
                }
                mc.open();
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();

                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }

            }
            return overlappingPlanId;
        }
        public List<int> Escalation_OverlapCheckerForMachine(int flage, int wc, string RL1, string RL2, string RL3, int Hours = 0, int Minutes = 0, int emailescID = 0)
        {
            List<int> overlappingPlanId = new List<int>(), overlappingPlanId1 = new List<int>(), overlappingPlanId2 = new List<int>();
            int MachineID = wc;
            DataTable dataHolder = new DataTable();

            //1st check if it has a Cell else go for Shop

            var machinedetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).FirstOrDefault();
            if (machinedetails.CellID.HasValue)
            {
                int cellId = Convert.ToInt32(machinedetails.CellID);
                overlappingPlanId = Escalation_OverlapCheckerForCell(flage, cellId, RL1, RL2, RL3, Hours, Minutes, emailescID);
            }
            else
            {
                int shopId = Convert.ToInt32(machinedetails.ShopID);
                overlappingPlanId1 = Escalation_OverlapCheckerForShop(flage, shopId, RL1, RL2, RL3, Hours, Minutes, emailescID);
            }

            overlappingPlanId2.AddRange(overlappingPlanId);
            overlappingPlanId2.AddRange(overlappingPlanId1);

            if (overlappingPlanId2.Count == 0)
            {
                using (MsqlConnection mc = new MsqlConnection())
                {
                    string sql = null;
                    if (Hours == 0 && Minutes == 0)
                    {
                        if (emailescID != 0)
                        {
                            if (RL2 != "" && RL3 == "")
                            {
                                //1st Level Reason Duplicate
                                DataTable dataHolderInner = new DataTable();
                                MsqlConnection mcInner = new MsqlConnection();
                                mcInner.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and EMailEscalationID != " + emailescID + " and HoldReason3 is null  and IsCritical=1;";
                                }
                                SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                                daInner.Fill(dataHolderInner);
                                mcInner.close();
                                for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                    return overlappingPlanId;
                                }
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and  ReasonLevel3 is null  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and  HoldReason3 is null  and IsCritical=1;";
                                }
                            }
                            else if (RL3 != "")
                            {
                                //1st Level Reason Duplicate
                                DataTable dataHolderInner = new DataTable();
                                MsqlConnection mcInner = new MsqlConnection();
                                mcInner.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";

                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = is null  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = is null  and HoldReason3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                                }
                                SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                                daInner.Fill(dataHolderInner);
                                mcInner.close();
                                for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                    return overlappingPlanId;
                                }

                                //2nd Level Reason duplicate
                                DataTable dataHolderInner2 = new DataTable();
                                MsqlConnection mcInner2 = new MsqlConnection();
                                mcInner2.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and EMailEscalationID != " + emailescID + "  and IsCritical=1 ;";
                                }
                                SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                                daInner2.Fill(dataHolderInner2);
                                mcInner2.close();
                                for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                    return overlappingPlanId;
                                }

                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + "  and IsCritical=1;";
                                }
                            }
                            else
                            {
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                                }
                            }
                        }
                        else
                        {
                            if (RL2 != "" && RL3 == "")
                            {
                                //1st Level Reason Duplicate
                                DataTable dataHolderInner = new DataTable();
                                MsqlConnection mcInner = new MsqlConnection();
                                mcInner.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and  ReasonLevel3 is null  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "' and HoldReason2 is null and  HoldReason3 is null  and IsCritical=1;";
                                }
                                SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                                daInner.Fill(dataHolderInner);
                                mcInner.close();
                                for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                    return overlappingPlanId;
                                }
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and  ReasonLevel3 is null  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and  HoldReason3 is null  and IsCritical=1;";
                                }
                            }
                            else if (RL3 != "")
                            {
                                //1st Level Reason Duplicate
                                DataTable dataHolderInner = new DataTable();
                                MsqlConnection mcInner = new MsqlConnection();
                                mcInner.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null  and HoldReason3 is null  and IsCritical=1 ;";
                                }
                                SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                                daInner.Fill(dataHolderInner);
                                mcInner.close();
                                for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                    return overlappingPlanId;
                                }

                                //2nd Level Reason duplicate
                                DataTable dataHolderInner2 = new DataTable();
                                MsqlConnection mcInner2 = new MsqlConnection();
                                mcInner2.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null  and IsCritical=1;";
                                }
                                SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                                daInner2.Fill(dataHolderInner2);
                                mcInner2.close();
                                for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                    return overlappingPlanId;
                                }
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and IsCritical=1 ;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "'  and IsCritical=1;";
                                }
                            }
                            else
                            {
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null  and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null  and IsCritical=1;";
                                }
                            }
                        }
                    }
                    else
                    {
                        if (emailescID != 0)
                        {
                            if (RL2 != "" && RL3 == "")
                            {
                                //1st Level Reason Duplicate
                                DataTable dataHolderInner = new DataTable();
                                MsqlConnection mcInner = new MsqlConnection();
                                mcInner.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and EMailEscalationID != " + emailescID + "  and  ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and EMailEscalationID != " + emailescID + "  and  HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                                daInner.Fill(dataHolderInner);
                                mcInner.close();
                                for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                    return overlappingPlanId;
                                }
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and  ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and EMailEscalationID != " + emailescID + "  and  HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                            }
                            else if (RL3 != "")
                            {
                                //1st Level Reason Duplicate
                                DataTable dataHolderInner = new DataTable();
                                MsqlConnection mcInner = new MsqlConnection();
                                mcInner.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                                daInner.Fill(dataHolderInner);
                                mcInner.close();
                                for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                    return overlappingPlanId;
                                }

                                //2nd Level Reason duplicate
                                DataTable dataHolderInner2 = new DataTable();
                                MsqlConnection mcInner2 = new MsqlConnection();
                                mcInner2.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                                daInner2.Fill(dataHolderInner2);
                                mcInner2.close();
                                for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                    return overlappingPlanId;
                                }
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and EMailEscalationID != " + emailescID + " and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                            }
                            else
                            {
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "' and EMailEscalationID != " + emailescID + "  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                            }
                        }
                        else
                        {
                            if (RL2 != "" && RL3 == "")
                            {
                                //1st Level Reason Duplicate
                                DataTable dataHolderInner = new DataTable();
                                MsqlConnection mcInner = new MsqlConnection();
                                mcInner.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 is null and  ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 is null and  HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                                daInner.Fill(dataHolderInner);
                                mcInner.close();
                                for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                    return overlappingPlanId;
                                }
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'    and ReasonLevel2 = '" + RL2 + "' and  ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'    and HoldReason2 = '" + RL2 + "' and  HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                            }
                            else if (RL3 != "")
                            {
                                //1st Level Reason Duplicate
                                DataTable dataHolderInner = new DataTable();
                                MsqlConnection mcInner = new MsqlConnection();
                                mcInner.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2 is null and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                SqlDataAdapter daInner = new SqlDataAdapter(sql, mcInner.msqlConnection);
                                daInner.Fill(dataHolderInner);
                                mcInner.close();
                                for (int i = 0; i < dataHolderInner.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner.Rows[i][0]));
                                    return overlappingPlanId;
                                }

                                //2nd Level Reason duplicate
                                DataTable dataHolderInner2 = new DataTable();
                                MsqlConnection mcInner2 = new MsqlConnection();
                                mcInner2.open();
                                //sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  CellID = '" + CellID + "' and WorkCenterID is null  and ReasonLevel1 = '" + RL1 + "' and ReasonLevel2  = '" + RL2 + "' and EMailEscalationID != " + emailescID + " and ReasonLevel3 is null ;";
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                SqlDataAdapter daInner2 = new SqlDataAdapter(sql, mcInner2.msqlConnection);
                                daInner2.Fill(dataHolderInner2);
                                mcInner2.close();
                                for (int i = 0; i < dataHolderInner2.Rows.Count; i++)
                                {
                                    overlappingPlanId.Add(Convert.ToInt32(dataHolderInner2.Rows[i][0]));
                                    return overlappingPlanId;
                                }
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 = '" + RL2 + "'  and ReasonLevel3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";

                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 = '" + RL2 + "'  and HoldReason3 = '" + RL3 + "' and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";

                                }
                            }
                            else
                            {
                                if (flage == 0)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and ReasonLevel1 = '" + RL1 + "'  and ReasonLevel2 is null and ReasonLevel3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                                else if (flage == 1)
                                {
                                    sql = "SELECT EMailEscalationID FROM "+MsqlConnection.DbName+".tblemailescalation where IsDeleted = 0 and  WorkCenterID = '" + MachineID + "' and HoldReason1 = '" + RL1 + "'  and HoldReason2 is null and HoldReason3 is null and Hours = " + Hours + " and Minutes = " + Minutes + " and IsCritical=1;";
                                }
                            }
                        }
                    }
                    mc.open();
                    SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                    da.Fill(dataHolder);
                    mc.close();
                }
                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId2.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }
            }
            return overlappingPlanId2;
        }

        public JsonResult GetRL2(int RL1ID)
        {
            var RL2Data = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == RL1ID), "LossCodeID", "LossCode");
            return Json(RL2Data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRL3(int RL2ID)
        {
            var RL2Data = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 3 && m.LossCodesLevel2ID == RL2ID), "LossCodeID", "LossCode");
            return Json(RL2Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRL5(int RL4ID)
        {
            var RL2Data = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.HoldCodesLevel1ID == RL4ID), "HoldCodeID", "HoldCode");
            return Json(RL2Data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRL6(int RL5ID)
        {
            var RL2Data = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 3 && m.HoldCodesLevel2ID == RL5ID), "HoldCodeID", "HoldCode");
            return Json(RL2Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetShop(int PlantID)
        {
            List<ShopDetails> listShopDetails = new List<ShopDetails>();
            bool check = false;
            var shopDetails = db.tblshops.Where(x => x.IsDeleted == 0).Select(x => new { x.ShopID, x.ShopName }).ToList();
            foreach (var row in shopDetails)
            {
                var machineid = db.tblmachinedetails.Where(x => x.IsDeleted == 0 && x.ShopID == row.ShopID).ToList();
                foreach (var machDet in machineid)
                {
                    var criticalMach = db.tblCriticalMachines.Where(x => x.isDeleted == 0 && x.machineId == machDet.MachineID && x.isCritical == 1).FirstOrDefault();
                    if (criticalMach != null)
                    {
                        check = true;
                    }
                }
                if (check)
                {
                    check = false;
                    ShopDetails objShopDetails = new ShopDetails();
                    objShopDetails.Value = row.ShopID;
                    objShopDetails.Text = row.ShopName;
                    listShopDetails.Add(objShopDetails);
                }
            }

            //var ShopData = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName");
            return Json(listShopDetails, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCell(int ShopID)
        {
            List<CellDetails> listCellDetails = new List<CellDetails>();
            bool check = false;
            var cellDetails = db.tblcells.Where(x => x.IsDeleted == 0).Select(x => new { x.CellID, x.CellName }).ToList();
            foreach (var row in cellDetails)
            {
                var machineid = db.tblmachinedetails.Where(x => x.IsDeleted == 0 && x.CellID == row.CellID).ToList();
                foreach (var machDet in machineid)
                {
                    var criticalMach = db.tblCriticalMachines.Where(x => x.isDeleted == 0 && x.machineId == machDet.MachineID && x.isCritical == 1).FirstOrDefault();
                    if (criticalMach != null)
                    {
                        check = true;
                    }
                }
                if (check)
                {
                    check = false;
                    CellDetails objCellDetails = new CellDetails();
                    objCellDetails.Value = row.CellID;
                    objCellDetails.Text = row.CellName;
                    listCellDetails.Add(objCellDetails);
                }
            }
            //var CellData = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID), "CellID", "CellName");
            return Json(listCellDetails, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Cell(int CellID)
        {
            List<MachineDetails> listMachDetails = new List<MachineDetails>();

            var machineid = db.tblmachinedetails.Where(x => x.IsDeleted == 0 && x.CellID == CellID).ToList();
            foreach (var machDet in machineid)
            {
                var criticalMach = db.tblCriticalMachines.Where(x => x.isDeleted == 0 && x.machineId == machDet.MachineID && x.isCritical == 1).FirstOrDefault();
                if (criticalMach != null)
                {
                    MachineDetails objCellDetails = new MachineDetails();
                    objCellDetails.Value = machDet.MachineID;
                    objCellDetails.Text = machDet.MachineInvNo;
                    listMachDetails.Add(objCellDetails);
                }
            }
            //var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineInvNo");
            return Json(listMachDetails, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Shop(int ShopID)
        {
            List<MachineDetails> listMachDetails = new List<MachineDetails>();

            var machineid = db.tblmachinedetails.Where(x => x.IsDeleted == 0 && x.ShopID == ShopID).ToList();
            foreach (var machDet in machineid)
            {
                var criticalMach = db.tblCriticalMachines.Where(x => x.isDeleted == 0 && x.machineId == machDet.MachineID && x.isCritical == 1).FirstOrDefault();
                if (criticalMach != null)
                {
                    MachineDetails objCellDetails = new MachineDetails();
                    objCellDetails.Value = machDet.MachineID;
                    objCellDetails.Text = machDet.MachineInvNo;
                    listMachDetails.Add(objCellDetails);
                }
            }
            //var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.CellID.Equals(null) && m.IsNormalWC == 0), "MachineID", "MachineInvNo");
            return Json(listMachDetails, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMailIDs(string Prefix, string AllVal)
        {
            //This format was working.
            //List<string> existingMailIds = new List<string> {"jack@gmail.com","manasvi@gmail.com","jill"};

            List<string> existingMailIds = new List<string>();
            if (AllVal.Contains(','))
            {
                string[] mails = AllVal.Split(',');
                existingMailIds.AddRange(mails);
            }

            //This will slow us down
            //tblmailid[] MailData = (from c in db.tblmailids
            //                        where !existingMailIds.Contains(c.EmailID) && c.IsDeleted == 0
            //                        select c).ToArray();

            var MailDataToView = (from N in db.tblmailids
                                  where N.EmailID.StartsWith(Prefix) && !existingMailIds.Contains(N.EmailID) && N.IsDeleted == 0
                                  select new { N.EmailID });

            return Json(MailDataToView, JsonRequestBehavior.AllowGet);
        }

        public string GetReasonType(int reasonId, int flage)
        {
            string reason = null;
            if (flage == 0)
            {
                var lossCodeData = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == reasonId).FirstOrDefault();
                reason = lossCodeData.MessageType;
                if (reason == "PM")
                {
                    reason = "BREAKDOWN";
                }
                if (reason == "Setup")
                {
                    reason = "IDLE";
                }
                if (reason == "NoCode")
                {
                    reason = "IDLE";
                }
            }
            else if (flage == 1)
            {
                var lossCodeData = db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodeID == reasonId).FirstOrDefault();
                if (lossCodeData != null)
                {
                    //reason = lossCodeData.HoldCode;
                    reason = "HOLD";
                }
                //if (reason == "PM")
                //{
                //    reason = "BREAKDOWN";
                //}
                //if (reason == "Setup")
                //{
                //    reason = "IDLE";
                //}
                //if (reason == "NoCode")
                //{
                //    reason = "IDLE";
                //}
            }
            return reason;
        }

        public ActionResult ImportEmailEscalationDetails()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            return View();
        }
        [HttpPost]
        public ActionResult ImportEmailEscalationDetails(HttpPostedFileBase file, string UploadType, string LHType)
        {

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            int UserID = Convert.ToInt32(Session["UserId"]);
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //Deleting Excel file
            #region
            string fileLocation1 = Server.MapPath("~/Content/");
            DirectoryInfo di = new DirectoryInfo(fileLocation1);
            FileInfo[] files = di.GetFiles("*.xlsx").Where(p => p.Extension == ".xlsx").ToArray();
            foreach (FileInfo file1 in files)
                try
                {
                    file1.Attributes = FileAttributes.Normal;
                    System.IO.File.Delete(file1.FullName);
                }
                catch { }
            #endregion

            DataSet ds = new DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();
                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }
                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }
                    excelConnection1.Close();
                    excelConnection.Close();
                }
                if (fileExtension.ToString().ToLower().Equals(".xml"))
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["FileUpload"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["FileUpload"].SaveAs(fileLocation);
                    XmlTextReader xmlreader = new XmlTextReader(fileLocation);
                    // DataSet ds = new DataSet();
                    ds.ReadXml(xmlreader);
                    xmlreader.Close();
                }

                string Errors = null;
                if (UploadType == "OverWrite") // Accept only New Codes
                {
                    #region
                    for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                    {

                        bool IsValid = false;
                        //Validations
                        string EscName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        var escNameDup = db.tblemailescalations.Where(m => m.EMailEscalationName == EscName && m.IsDeleted == 0).FirstOrDefault();
                        if (escNameDup != null)
                        {
                            Errors += "Duplicate Email EscalationName  " + EscName + " \n";
                            continue;
                        }
                        int hours = 0, minutes = 0;

                        string Duration = Convert.ToString(ds.Tables[0].Rows[i][9]);
                        //if ((Duration.Length == 8) && Regex.IsMatch(Duration, @"^[0-9]{2}:[0-9]{2}:[0-9]{2}$"))
                        if (Duration.Length >= 11)
                        {
                            try
                            {
                                Duration = Duration.Substring(Duration.Length - 8);
                                string[] ArrayTime = Duration.Split(':');
                                hours = Convert.ToInt32(ArrayTime[0]);
                                minutes = Convert.ToInt32(ArrayTime[1]);
                            }
                            catch (Exception e)
                            {
                                Errors += "Duration should be of Format (HH:mm) For " + EscName + " \n";
                                continue;
                            }
                        }
                        else
                        {
                            try
                            {
                                string[] ArrayTime = Duration.Split(':');
                                hours = Convert.ToInt32(ArrayTime[0]);
                                minutes = Convert.ToInt32(ArrayTime[1]);
                            }
                            catch (Exception e)
                            {
                                Errors += "Duration should be of Format (HH:mm) For " + EscName + " \n";
                                continue;
                            }
                        }
                        string ToList = Convert.ToString(ds.Tables[0].Rows[i][10]);
                        if (ToList.Length > 0 && string.IsNullOrEmpty(ToList.Trim()))
                        {
                            Errors += "Duplicate Email EscalationName  " + EscName + " \n";
                            continue;
                        }

                        //Other Crusial Variables ( Plant , Reason1 )
                        if (Convert.ToString(ds.Tables[0].Rows[i][1]) == null || Convert.ToString(ds.Tables[0].Rows[i][6]) == null)
                        {
                            Errors += "Reason Leve1 and PlantName Cannot be null for EscalationName  " + EscName + " \n";
                            continue;
                        }

                        string RL1 = "", RL1Name = "", RL2 = "", RL2Name = "", RL3 = "", RL3Name = "";
                        int flage = 0;
                        //Email validation
                        int EmailEscID = 0;
                        if (LHType == "loss")
                        {
                            RL1Name = Convert.ToString(ds.Tables[0].Rows[i][6]);
                            RL1 = Convert.ToString(db.tbllossescodes.Where(m => m.LossCode == RL1Name && m.LossCodesLevel == 1).Select(m => m.LossCodeID).FirstOrDefault());
                            RL2Name = Convert.ToString(ds.Tables[0].Rows[i][7]);
                            RL2 = Convert.ToString(db.tbllossescodes.Where(m => m.LossCode == RL2Name && m.LossCodesLevel == 2).Select(m => m.LossCodeID).FirstOrDefault());
                            RL3Name = Convert.ToString(ds.Tables[0].Rows[i][8]);
                            RL3 = Convert.ToString(db.tbllossescodes.Where(m => m.LossCode == RL3Name && m.LossCodesLevel == 3).Select(m => m.LossCodeID).FirstOrDefault());
                            flage = 0;
                        }
                        else if (LHType == "hold")
                        {
                            RL1Name = Convert.ToString(ds.Tables[0].Rows[i][6]);
                            RL1 = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCode == RL1Name && m.HoldCodesLevel == 1).Select(m => m.HoldCodeID).FirstOrDefault());
                            RL2Name = Convert.ToString(ds.Tables[0].Rows[i][7]);
                            RL2 = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCode == RL2Name && m.HoldCodesLevel == 2).Select(m => m.HoldCodeID).FirstOrDefault());
                            RL3Name = Convert.ToString(ds.Tables[0].Rows[i][8]);
                            RL3 = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCode == RL3Name && m.HoldCodesLevel == 3).Select(m => m.HoldCodeID).FirstOrDefault());
                            flage = 1;
                        }
                        string PlantNameString = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        string Plantidstring = Convert.ToString(db.tblplants.Where(m => m.PlantName == PlantNameString).Select(m => m.PlantID).FirstOrDefault());
                        string ShopNameString = Convert.ToString(ds.Tables[0].Rows[i][2]);
                        string Shopidstring = Convert.ToString(db.tblshops.Where(m => m.ShopName == ShopNameString).Select(m => m.ShopID).FirstOrDefault());
                        string CellNameString = Convert.ToString(ds.Tables[0].Rows[i][3]);
                        string Cellidstring = Convert.ToString(db.tblcells.Where(m => m.CellName == CellNameString).Select(m => m.CellID).FirstOrDefault());
                        string WCNameString = Convert.ToString(ds.Tables[0].Rows[i][4]);
                        string WorkCenteridstring = Convert.ToString(db.tblmachinedetails.Where(m => m.MachineDispName == WCNameString).Select(m => m.MachineID).FirstOrDefault());
                        string ValidEscalation = null;

                        if (Plantidstring == "0")
                        {
                            Plantidstring = null;
                            Errors += "PlantName is InValid for EscalationName  " + EscName + " \n";
                            continue;
                        }
                        if (Shopidstring == "0") { Shopidstring = null; }
                        if (Cellidstring == "0") { Cellidstring = null; }
                        if (WorkCenteridstring == "0") { WorkCenteridstring = null; }

                        ValidEscalation = IsItValidEscalation(flage, RL1, RL2, RL3, Plantidstring, Shopidstring, Cellidstring, WorkCenteridstring, hours, minutes, EmailEscID);
                        if (ValidEscalation == null)
                        {
                            tblemailescalation tee = new tblemailescalation();
                            int reasonId = Convert.ToInt32(RL1);
                            string msgType = GetReasonType(reasonId, flage);
                            tee.CcList = Convert.ToString(ds.Tables[0].Rows[i][11]);
                            tee.ToList = Convert.ToString(ds.Tables[0].Rows[i][10]);

                            if (flage == 0)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.ReasonLevel1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.ReasonLevel2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.ReasonLevel3 = RL3idINT;
                                }
                            }
                            else if (flage == 1)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.HoldReason1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.HoldReason2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.HoldReason3 = RL3idINT;
                                }
                            }

                            int wcidINT = 0;
                            int.TryParse(WorkCenteridstring, out wcidINT);
                            if (wcidINT != 0)
                            {
                                tee.WorkCenterID = wcidINT;
                            }
                            int cellidINT = 0;
                            int.TryParse(Cellidstring, out cellidINT);
                            if (cellidINT != 0)
                            {
                                tee.CellID = cellidINT;
                            }
                            int shopidINT = 0;
                            int.TryParse(Shopidstring, out shopidINT);
                            if (shopidINT != 0)
                            {
                                tee.ShopID = shopidINT;
                            }
                            int plantidINT = 0;
                            int.TryParse(Plantidstring, out plantidINT);
                            if (plantidINT != 0)
                            {
                                tee.PlantID = plantidINT;
                            }

                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.EMailEscalationName = EscName;
                            tee.Hours = hours;
                            tee.Minutes = minutes;
                            tee.MessageType = msgType;
                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.IsDeleted = 0;

                            db.tblemailescalations.Add(tee);
                            db.SaveChanges();
                        }
                        else
                        {
                            Errors += ValidEscalation + " \n";
                            continue;
                        }
                    }
                    #endregion
                }
                else if (UploadType == "New") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                    {

                        bool IsValid = false;
                        //Validations
                        string EscName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        var escNameDup = db.tblemailescalations.Where(m => m.EMailEscalationName == EscName && m.IsDeleted == 0).FirstOrDefault();
                        if (escNameDup != null)
                        {
                            Errors += "Duplicate Email EscalationName  " + EscName + " \n";
                            continue;
                        }
                        int hours = 0, minutes = 0;

                        string Duration = Convert.ToString(ds.Tables[0].Rows[i][9]);
                        //if ((Duration.Length == 8) && Regex.IsMatch(Duration, @"^[0-9]{2}:[0-9]{2}:[0-9]{2}$"))
                        if (Duration.Length >= 11)
                        {
                            try
                            {
                                Duration = Duration.Substring(Duration.Length - 8);
                                string[] ArrayTime = Duration.Split(':');
                                hours = Convert.ToInt32(ArrayTime[0]);
                                minutes = Convert.ToInt32(ArrayTime[1]);
                            }
                            catch (Exception e)
                            {
                                Errors += "Duration should be of Format (HH:mm) For " + EscName + " \n";
                                continue;
                            }
                        }
                        else
                        {
                            try
                            {
                                string[] ArrayTime = Duration.Split(':');
                                hours = Convert.ToInt32(ArrayTime[0]);
                                minutes = Convert.ToInt32(ArrayTime[1]);
                            }
                            catch (Exception e)
                            {
                                Errors += "Duration should be of Format (HH:mm) For " + EscName + " \n";
                                continue;
                            }
                        }
                        string ToList = Convert.ToString(ds.Tables[0].Rows[i][10]);
                        if (ToList.Length > 0 && string.IsNullOrEmpty(ToList.Trim()))
                        {
                            Errors += "Duplicate Email EscalationName  " + EscName + " \n";
                            continue;
                        }

                        //Other Crusial Variables ( Plant , Reason1 )
                        if (Convert.ToString(ds.Tables[0].Rows[i][1]) == null || Convert.ToString(ds.Tables[0].Rows[i][6]) == null)
                        {
                            Errors += "Reason Leve1 and PlantName Cannot be null for EscalationName  " + EscName + " \n";
                            continue;
                        }

                        string RL1 = "", RL1Name = "", RL2 = "", RL2Name = "", RL3 = "", RL3Name = "";
                        int flage = 0;
                        //Email validation
                        int EmailEscID = 0;
                        if (LHType == "loss")
                        {
                            RL1Name = Convert.ToString(ds.Tables[0].Rows[i][6]);
                            RL1 = Convert.ToString(db.tbllossescodes.Where(m => m.LossCode == RL1Name && m.LossCodesLevel == 1).Select(m => m.LossCodeID).FirstOrDefault());
                            RL2Name = Convert.ToString(ds.Tables[0].Rows[i][7]);
                            RL2 = Convert.ToString(db.tbllossescodes.Where(m => m.LossCode == RL2Name && m.LossCodesLevel == 2).Select(m => m.LossCodeID).FirstOrDefault());
                            RL3Name = Convert.ToString(ds.Tables[0].Rows[i][8]);
                            RL3 = Convert.ToString(db.tbllossescodes.Where(m => m.LossCode == RL3Name && m.LossCodesLevel == 3).Select(m => m.LossCodeID).FirstOrDefault());
                            flage = 0;
                        }
                        else if (LHType == "hold")
                        {
                            RL1Name = Convert.ToString(ds.Tables[0].Rows[i][6]);
                            RL1 = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCode == RL1Name && m.HoldCodesLevel == 1).Select(m => m.HoldCodeID).FirstOrDefault());
                            RL2Name = Convert.ToString(ds.Tables[0].Rows[i][7]);
                            RL2 = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCode == RL2Name && m.HoldCodesLevel == 2).Select(m => m.HoldCodeID).FirstOrDefault());
                            RL3Name = Convert.ToString(ds.Tables[0].Rows[i][8]);
                            RL3 = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCode == RL3Name && m.HoldCodesLevel == 3).Select(m => m.HoldCodeID).FirstOrDefault());
                            flage = 1;
                        }
                        string PlantNameString = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        string Plantidstring = Convert.ToString(db.tblplants.Where(m => m.PlantName == PlantNameString).Select(m => m.PlantID).FirstOrDefault());
                        string ShopNameString = Convert.ToString(ds.Tables[0].Rows[i][2]);
                        string Shopidstring = Convert.ToString(db.tblshops.Where(m => m.ShopName == ShopNameString).Select(m => m.ShopID).FirstOrDefault());
                        string CellNameString = Convert.ToString(ds.Tables[0].Rows[i][3]);
                        string Cellidstring = Convert.ToString(db.tblcells.Where(m => m.CellName == CellNameString).Select(m => m.CellID).FirstOrDefault());
                        string WCNameString = Convert.ToString(ds.Tables[0].Rows[i][4]);
                        string WorkCenteridstring = Convert.ToString(db.tblmachinedetails.Where(m => m.MachineDispName == WCNameString).Select(m => m.MachineID).FirstOrDefault());
                        string ValidEscalation = null;

                        if (Plantidstring == "0")
                        {
                            Plantidstring = null;
                            Errors += "PlantName is InValid for EscalationName  " + EscName + " \n";
                            continue;
                        }
                        if (Shopidstring == "0") { Shopidstring = null; }
                        if (Cellidstring == "0") { Cellidstring = null; }
                        if (WorkCenteridstring == "0") { WorkCenteridstring = null; }

                        ValidEscalation = IsItValidEscalation(flage, RL1, RL2, RL3, Plantidstring, Shopidstring, Cellidstring, WorkCenteridstring, hours, minutes, EmailEscID, "upload");
                        if (ValidEscalation == null)
                        {
                            tblemailescalation tee = new tblemailescalation();
                            int reasonId = Convert.ToInt32(RL1);
                            string msgType = GetReasonType(reasonId, 0);
                            tee.CcList = Convert.ToString(ds.Tables[0].Rows[i][11]);
                            tee.ToList = Convert.ToString(ds.Tables[0].Rows[i][10]);

                            if (flage == 0)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.ReasonLevel1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.ReasonLevel2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.ReasonLevel3 = RL3idINT;
                                }
                            }
                            else if (flage == 1)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.HoldReason1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.HoldReason2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.HoldReason3 = RL3idINT;
                                }
                            }

                            int wcidINT = 0;
                            int.TryParse(WorkCenteridstring, out wcidINT);
                            if (wcidINT != 0)
                            {
                                tee.WorkCenterID = wcidINT;
                            }
                            int cellidINT = 0;
                            int.TryParse(Cellidstring, out cellidINT);
                            if (cellidINT != 0)
                            {
                                tee.CellID = cellidINT;
                            }
                            int shopidINT = 0;
                            int.TryParse(Shopidstring, out shopidINT);
                            if (shopidINT != 0)
                            {
                                tee.ShopID = shopidINT;
                            }
                            int plantidINT = 0;
                            int.TryParse(Plantidstring, out plantidINT);
                            if (plantidINT != 0)
                            {
                                tee.PlantID = plantidINT;
                            }

                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.EMailEscalationName = EscName;
                            tee.Hours = hours;
                            tee.Minutes = minutes;
                            tee.MessageType = msgType;
                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.IsDeleted = 0;

                            db.tblemailescalations.Add(tee);
                            db.SaveChanges();
                        }
                        else
                        {
                            // 1) Delete all Comma seperated escalationID's in ValidEscalation 
                            if (ValidEscalation.Length > 0)
                            {
                                string[] EscIDs = ValidEscalation.Split(',');
                                foreach (var id in EscIDs)
                                {
                                    try
                                    {
                                        db.tblemailescalations.Remove(db.tblemailescalations.Find(id));
                                    }
                                    catch (SqlException mse)
                                    {
                                    }
                                    catch (Exception e)
                                    {
                                    }
                                }

                            }


                            // 2) Insert the New Row
                            tblemailescalation tee = new tblemailescalation();
                            int reasonId = Convert.ToInt32(RL1);
                            string msgType = GetReasonType(reasonId, flage);
                            tee.CcList = Convert.ToString(ds.Tables[0].Rows[i][11]);
                            tee.ToList = Convert.ToString(ds.Tables[0].Rows[i][10]);

                            if (flage == 0)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.ReasonLevel1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.ReasonLevel2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.ReasonLevel3 = RL3idINT;
                                }
                            }
                            else if (flage == 1)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.HoldReason1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.HoldReason2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.HoldReason3 = RL3idINT;
                                }
                            }

                            int wcidINT = 0;
                            int.TryParse(WorkCenteridstring, out wcidINT);
                            if (wcidINT != 0)
                            {
                                tee.WorkCenterID = wcidINT;
                            }
                            int cellidINT = 0;
                            int.TryParse(Cellidstring, out cellidINT);
                            if (cellidINT != 0)
                            {
                                tee.CellID = cellidINT;
                            }
                            int shopidINT = 0;
                            int.TryParse(Shopidstring, out shopidINT);
                            if (shopidINT != 0)
                            {
                                tee.ShopID = shopidINT;
                            }
                            int plantidINT = 0;
                            int.TryParse(Plantidstring, out plantidINT);
                            if (plantidINT != 0)
                            {
                                tee.PlantID = plantidINT;
                            }

                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.EMailEscalationName = EscName;
                            tee.Hours = hours;
                            tee.Minutes = minutes;
                            tee.MessageType = msgType;
                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.IsDeleted = 0;

                            db.tblemailescalations.Add(tee);
                            db.SaveChanges();
                        }
                    }
                    #endregion
                }
                else if (UploadType == "Update")
                {
                    #region
                    for (int i = 1; i < ds.Tables[0].Rows.Count; i++)
                    {

                        bool IsValid = false;
                        //Validations
                        string EscName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        //var escNameDup = db.tblemailescalations.Where(m => m.EMailEscalationName == EscName && m.IsDeleted == 0).FirstOrDefault();
                        //if (escNameDup != null)
                        //{
                        //    Errors += "Duplicate Email EscalationName  " + EscName + " \n";
                        //    continue;
                        //}
                        int hours = 0, minutes = 0;

                        string Duration = Convert.ToString(ds.Tables[0].Rows[i][9]);
                        //if ((Duration.Length == 8) && Regex.IsMatch(Duration, @"^[0-9]{2}:[0-9]{2}:[0-9]{2}$"))
                        if (Duration.Length >= 11)
                        {
                            try
                            {
                                Duration = Duration.Substring(Duration.Length - 8);
                                string[] ArrayTime = Duration.Split(':');
                                hours = Convert.ToInt32(ArrayTime[0]);
                                minutes = Convert.ToInt32(ArrayTime[1]);
                            }
                            catch (Exception e)
                            {
                                Errors += "Duration should be of Format (HH:mm) For " + EscName + " \n";
                                continue;
                            }
                        }
                        else
                        {
                            try
                            {
                                string[] ArrayTime = Duration.Split(':');
                                hours = Convert.ToInt32(ArrayTime[0]);
                                minutes = Convert.ToInt32(ArrayTime[1]);
                            }
                            catch (Exception e)
                            {
                                Errors += "Duration should be of Format (HH:mm) For " + EscName + " \n";
                                continue;
                            }
                        }
                        string ToList = Convert.ToString(ds.Tables[0].Rows[i][10]);
                        if (ToList.Length > 0 && string.IsNullOrEmpty(ToList.Trim()))
                        {
                            Errors += "Duplicate Email EscalationName  " + EscName + " \n";
                            continue;
                        }

                        //Other Crusial Variables ( Plant , Reason1 )
                        if (Convert.ToString(ds.Tables[0].Rows[i][1]) == null || Convert.ToString(ds.Tables[0].Rows[i][6]) == null)
                        {
                            Errors += "Reason Leve1 and PlantName Cannot be null for EscalationName  " + EscName + " \n";
                            continue;
                        }

                        string RL1 = "", RL1Name = "", RL2 = "", RL2Name = "", RL3 = "", RL3Name = "";
                        int flage = 0;
                        //Email validation
                        int EmailEscID = 0;
                        if (LHType == "loss")
                        {
                            RL1Name = Convert.ToString(ds.Tables[0].Rows[i][6]);
                            RL1 = Convert.ToString(db.tbllossescodes.Where(m => m.LossCode == RL1Name && m.LossCodesLevel == 1).Select(m => m.LossCodeID).FirstOrDefault());
                            RL2Name = Convert.ToString(ds.Tables[0].Rows[i][7]);
                            RL2 = Convert.ToString(db.tbllossescodes.Where(m => m.LossCode == RL2Name && m.LossCodesLevel == 2).Select(m => m.LossCodeID).FirstOrDefault());
                            RL3Name = Convert.ToString(ds.Tables[0].Rows[i][8]);
                            RL3 = Convert.ToString(db.tbllossescodes.Where(m => m.LossCode == RL3Name && m.LossCodesLevel == 3).Select(m => m.LossCodeID).FirstOrDefault());
                            flage = 0;
                        }
                        else if (LHType == "hold")
                        {
                            RL1Name = Convert.ToString(ds.Tables[0].Rows[i][6]);
                            RL1 = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCode == RL1Name && m.HoldCodesLevel == 1).Select(m => m.HoldCodeID).FirstOrDefault());
                            RL2Name = Convert.ToString(ds.Tables[0].Rows[i][7]);
                            RL2 = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCode == RL2Name && m.HoldCodesLevel == 2).Select(m => m.HoldCodeID).FirstOrDefault());
                            RL3Name = Convert.ToString(ds.Tables[0].Rows[i][8]);
                            RL3 = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCode == RL3Name && m.HoldCodesLevel == 3).Select(m => m.HoldCodeID).FirstOrDefault());
                            flage = 1;
                        }
                        string PlantNameString = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        string Plantidstring = Convert.ToString(db.tblplants.Where(m => m.PlantName == PlantNameString).Select(m => m.PlantID).FirstOrDefault());
                        string ShopNameString = Convert.ToString(ds.Tables[0].Rows[i][2]);
                        string Shopidstring = Convert.ToString(db.tblshops.Where(m => m.ShopName == ShopNameString).Select(m => m.ShopID).FirstOrDefault());
                        string CellNameString = Convert.ToString(ds.Tables[0].Rows[i][3]);
                        string Cellidstring = Convert.ToString(db.tblcells.Where(m => m.CellName == CellNameString).Select(m => m.CellID).FirstOrDefault());
                        string WCNameString = Convert.ToString(ds.Tables[0].Rows[i][4]);
                        string WorkCenteridstring = Convert.ToString(db.tblmachinedetails.Where(m => m.MachineDispName == WCNameString).Select(m => m.MachineID).FirstOrDefault());
                        string ValidEscalation = null;

                        if (Plantidstring == "0")
                        {
                            Plantidstring = null;
                            Errors += "PlantName is InValid for EscalationName  " + EscName + " \n";
                            continue;
                        }
                        if (Shopidstring == "0") { Shopidstring = null; }
                        if (Cellidstring == "0") { Cellidstring = null; }
                        if (WorkCenteridstring == "0") { WorkCenteridstring = null; }

                        ValidEscalation = IsItValidEscalation(flage, RL1, RL2, RL3, Plantidstring, Shopidstring, Cellidstring, WorkCenteridstring, hours, minutes, EmailEscID, "upload");
                        if (ValidEscalation == null)
                        {
                            tblemailescalation tee = new tblemailescalation();
                            int reasonId = Convert.ToInt32(RL1);
                            string msgType = GetReasonType(reasonId, flage);
                            tee.CcList = Convert.ToString(ds.Tables[0].Rows[i][11]);
                            tee.ToList = Convert.ToString(ds.Tables[0].Rows[i][10]);

                            if (flage == 0)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.ReasonLevel1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.ReasonLevel2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.ReasonLevel3 = RL3idINT;
                                }
                            }
                            else if (flage == 1)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.HoldReason1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.HoldReason2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.HoldReason3 = RL3idINT;
                                }
                            }

                            int wcidINT = 0;
                            int.TryParse(WorkCenteridstring, out wcidINT);
                            if (wcidINT != 0)
                            {
                                tee.WorkCenterID = wcidINT;
                            }
                            int cellidINT = 0;
                            int.TryParse(Cellidstring, out cellidINT);
                            if (cellidINT != 0)
                            {
                                tee.CellID = cellidINT;
                            }
                            int shopidINT = 0;
                            int.TryParse(Shopidstring, out shopidINT);
                            if (shopidINT != 0)
                            {
                                tee.ShopID = shopidINT;
                            }
                            int plantidINT = 0;
                            int.TryParse(Plantidstring, out plantidINT);
                            if (plantidINT != 0)
                            {
                                tee.PlantID = plantidINT;
                            }

                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.EMailEscalationName = EscName;
                            tee.Hours = hours;
                            tee.Minutes = minutes;
                            tee.MessageType = msgType;
                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.IsDeleted = 0;

                            db.tblemailescalations.Add(tee);
                            db.SaveChanges();
                        }
                        else
                        {
                            // 1) Delete all Comma seperated escalationID's in ValidEscalation 
                            if (ValidEscalation.Length > 0)
                            {
                                string[] EscIDs = ValidEscalation.Split(',');
                                foreach (var id in EscIDs)
                                {
                                    try
                                    {
                                        db.tblemailescalations.Remove(db.tblemailescalations.Find(id));
                                    }
                                    catch (SqlException mse)
                                    {
                                    }
                                    catch (Exception e)
                                    {
                                    }
                                }

                            }


                            // 2) Insert the New Row
                            tblemailescalation tee = new tblemailescalation();
                            int reasonId = Convert.ToInt32(RL1);
                            string msgType = GetReasonType(reasonId, flage);
                            tee.CcList = Convert.ToString(ds.Tables[0].Rows[i][11]);
                            tee.ToList = Convert.ToString(ds.Tables[0].Rows[i][10]);

                            if (flage == 0)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.ReasonLevel1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.ReasonLevel2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.ReasonLevel3 = RL3idINT;
                                }
                            }
                            else if (flage == 1)
                            {
                                int RL1idINT = 0;
                                int.TryParse(RL1, out RL1idINT);
                                if (RL1idINT != 0)
                                {
                                    tee.HoldReason1 = RL1idINT;
                                }
                                int RL2idINT = 0;
                                int.TryParse(RL2, out RL2idINT);
                                if (RL2idINT != 0)
                                {
                                    tee.HoldReason2 = RL2idINT;
                                }
                                int RL3idINT = 0;
                                int.TryParse(RL3, out RL3idINT);
                                if (RL3idINT != 0)
                                {
                                    tee.HoldReason3 = RL3idINT;
                                }
                            }

                            int wcidINT = 0;
                            int.TryParse(WorkCenteridstring, out wcidINT);
                            if (wcidINT != 0)
                            {
                                tee.WorkCenterID = wcidINT;
                            }
                            int cellidINT = 0;
                            int.TryParse(Cellidstring, out cellidINT);
                            if (cellidINT != 0)
                            {
                                tee.CellID = cellidINT;
                            }
                            int shopidINT = 0;
                            int.TryParse(Shopidstring, out shopidINT);
                            if (shopidINT != 0)
                            {
                                tee.ShopID = shopidINT;
                            }
                            int plantidINT = 0;
                            int.TryParse(Plantidstring, out plantidINT);
                            if (plantidINT != 0)
                            {
                                tee.PlantID = plantidINT;
                            }

                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.EMailEscalationName = EscName;
                            tee.Hours = hours;
                            tee.Minutes = minutes;
                            tee.MessageType = msgType;
                            tee.CreatedBy = UserID;
                            tee.CreatedOn = DateTime.Now;
                            tee.IsDeleted = 0;

                            db.tblemailescalations.Add(tee);
                            db.SaveChanges();
                        }
                    }
                    #endregion
                }

                Session["Errors"] = Errors;
            }
            return RedirectToAction("Index");
        }

        public ActionResult ExportEmailEscalationDetails(string lossreason)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserId"]);

            #region Excel and Stuff
            DateTime frda = DateTime.Now;
            String FileDir = @"C:\TataReport\ReportsList\Upload\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "EMailEscalation" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "EMailEscalation" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;
            ExcelWorksheet worksheetGraph = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"));
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"));
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            //Header
            worksheet.Cells["A" + 2].Value = "Escalation Name";
            worksheet.Cells["B" + 2].Value = "Plant";
            worksheet.Cells["C" + 2].Value = "Shop";
            worksheet.Cells["D" + 2].Value = "Cell";
            worksheet.Cells["E" + 2].Value = "WorkCenter";
            worksheet.Cells["F" + 2].Value = "LossType";
            worksheet.Cells["G" + 2].Value = "Level1Code";
            worksheet.Cells["H" + 2].Value = "Level2Code";
            worksheet.Cells["I" + 2].Value = "Level3Code";
            worksheet.Cells["J" + 2].Value = "Duration(HH:mm)";
            worksheet.Cells["K" + 2].Value = "To List";
            worksheet.Cells["L" + 2].Value = "CC List";


            worksheet.Cells["A1:I1"].Style.Font.Bold = true;

            #endregion
            //var emailescData = db.tblemailescalations.Where(m => m.IsDeleted == 0).OrderBy(m => m.PlantID).ThenBy(m => m.ShopID).ThenBy(m => m.CellID).ToList();
            var emailescData = new List<tblemailescalation>();
            if (lossreason == "loss")
            {
                emailescData = db.tblemailescalations.Where(m => m.IsDeleted == 0 && m.ReasonLevel1 != null).OrderBy(m => m.CellID).Take(10).ToList();
            }
            else if (lossreason == "hold")
            {
                emailescData = db.tblemailescalations.Where(m => m.IsDeleted == 0 && m.HoldReason1 != null).OrderBy(m => m.CellID).Take(10).ToList();
            }
            int i = 3;
            foreach (var row in emailescData)
            {
                int PlantID = Convert.ToInt32(row.PlantID);
                string PlantName = Convert.ToString(db.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault());
                int ShopID = Convert.ToInt32(row.ShopID);
                string ShopName = Convert.ToString(db.tblshops.Where(m => m.ShopID == ShopID).Select(m => m.ShopName).FirstOrDefault());
                int CellID = Convert.ToInt32(row.CellID);
                string CellName = Convert.ToString(db.tblcells.Where(m => m.CellID == CellID).Select(m => m.CellName).FirstOrDefault());
                int WCID = Convert.ToInt32(row.WorkCenterID);
                string WCName = Convert.ToString(db.tblmachinedetails.Where(m => m.MachineID == WCID).Select(m => m.MachineDispName).FirstOrDefault());

                int Level1ID = 0, Level2ID = 0, Level3ID = 0;
                string Level1Name = "", Level2Name = "", Level3Name = "";
                if (lossreason == "loss")
                {
                    Level1ID = Convert.ToInt32(row.ReasonLevel1);
                    Level1Name = Convert.ToString(db.tbllossescodes.Where(m => m.LossCodeID == Level1ID).Select(m => m.LossCode).FirstOrDefault());
                    Level2ID = Convert.ToInt32(row.ReasonLevel2);
                    Level2Name = Convert.ToString(db.tbllossescodes.Where(m => m.LossCodeID == Level2ID).Select(m => m.LossCode).FirstOrDefault());
                    Level3ID = Convert.ToInt32(row.ReasonLevel3);
                    Level3Name = Convert.ToString(db.tbllossescodes.Where(m => m.LossCodeID == Level3ID).Select(m => m.LossCode).FirstOrDefault());
                }
                else if (lossreason == "hold")
                {
                    Level1ID = Convert.ToInt32(row.HoldReason1);
                    Level1Name = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCodeID == Level1ID).Select(m => m.HoldCode).FirstOrDefault());
                    Level2ID = Convert.ToInt32(row.HoldReason2);
                    Level2Name = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCodeID == Level2ID).Select(m => m.HoldCode).FirstOrDefault());
                    Level3ID = Convert.ToInt32(row.HoldReason3);
                    Level3Name = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCodeID == Level3ID).Select(m => m.HoldCode).FirstOrDefault());
                }

                worksheet.Cells["A" + i].Value = row.EMailEscalationName;
                worksheet.Cells["B" + i].Value = PlantName;
                worksheet.Cells["C" + i].Value = ShopName;
                worksheet.Cells["D" + i].Value = CellName;
                worksheet.Cells["E" + i].Value = WCName;
                worksheet.Cells["F" + i].Value = row.MessageType;
                worksheet.Cells["G" + i].Value = Level1Name;
                worksheet.Cells["H" + i].Value = Level2Name;
                worksheet.Cells["I" + i].Value = Level3Name;
                //worksheet.Cells["J" + i].Style.Numberformat.Format = "hh:mm:ss";
                worksheet.Cells["J" + i].Value = Convert.ToString(row.Hours) + ":" + Convert.ToString(row.Minutes);
                worksheet.Cells["K" + i].Value = row.ToList;
                worksheet.Cells["L" + i].Value = row.CcList;

                i++;
            }

            #region Save and Download
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "EMailEscalation" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "EMailEscalation" + frda.ToString("yyyy-MM-dd") + ".xlsx";
            if (file1.Exists)
            {
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
                Response.AddHeader("Content-Length", file1.Length.ToString());
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.WriteFile(file1.FullName);
                Response.Flush();
                Response.Close();
            }
            #endregion

            return RedirectToAction("Index");
        }

    }

    public class ShopDetails
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class CellDetails
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class MachineDetails
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    //public class MyValues1
    //{
    //    public List<string> values { get; set; }
    //}
}