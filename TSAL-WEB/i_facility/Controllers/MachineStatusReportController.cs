using i_facility.Models;
using i_facilitylibrary;
using i_facilitylibrary.DAO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace i_facility.Controllers
{
    public class MachineStatusReportController : Controller
    {
        i_facility_tsalEntities1 Serverdb = new i_facility_tsalEntities1();

        IConnectionFactory _conn;
        ReportsDao obj = new ReportsDao();
        Dao obj1 = new Dao();
        Dao1 obj2 = new Dao1();

        // GET: MachineStatusReport

        public ActionResult DashboardStatus()
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //var result = db.tblmachinedetails.Select(m=>m.ShopNo).Distinct();
            ViewData["PlantID"] = new SelectList(obj2.GetPlantList(), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(obj2.GetShopList1(), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(obj2.GetCellList(), "CellID", "CellName");
            ViewData["WorkCenterID"] = new SelectList(obj2.GetmachineList1(), "MachineID", "MachineInvNo");
            //ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            //ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            //ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            //ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineInvNo");


            return View();
        }
        [HttpPost]
        public ActionResult DashboardStatus(String StartDate)
        {
            string from = Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd");
            string TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime Stdate = Convert.ToDateTime(from);
            DateTime temp = Stdate.AddDays(-1);
            //DateTime endate = Convert.ToDateTime(ToDate);
            string PrvCorrectedDate = temp.ToString("yyyy-MM-dd");            
            return RedirectToAction("Index", new { CorrectedDate = from, PrvCorrectedDate = PrvCorrectedDate });     
        }

        public ActionResult Index(string CorrectedDate,string PrvCorrectedDate)
        {
            //if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            //{
            //    return RedirectToAction("Login", "Login", null);
            //}

            Session["colordata"] = null;
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            ViewBag.StartDate = CorrectedDate;

            //calculating Corrected Date
            TimeSpan currentHourMint = new TimeSpan(05, 59, 59);
            TimeSpan RealCurrntHour = System.DateTime.Now.TimeOfDay;
            //string CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            //if (RealCurrntHour < currentHourMint)
            //{
            //    CorrectedDate = DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd");
            //}

            // getting all machine details and their count.
            var macData = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0);
            int mc = macData.Count();
            ViewBag.macCount = mc;

            int[] macid = new int[mc];
            int macidlooper = 0;
            foreach (var v in macData)
            {
                macid[macidlooper++] = v.MachineID;
            }
            Session["macid"] = macid;
            ViewBag.macCount = mc;

            int[,] maindata = new int[mc, 5];
            //int[,] maindata = new int[mc, 6];
            // write a raw query to get sum of powerOff, Operating, Idle, BreakDown, PlannedMaintenance. 

            using (MsqlConnection mc1 = new MsqlConnection())
            {
                mc1.open();
                //SqlCommand cmd1 = new SqlCommand("SELECT MachineID,sum(MachineOffTime) as op,sum(OperatingTime)as o,sum(IdleTime) as it,sum(BreakdownTime)as bt FROM "+ MsqlConnection.DbName + ".tblmimics where CorrectedDate='" + CorrectedDate + "'and MachineID IN (select distinct(MachineID) from tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);
                //SqlCommand cmd1 = " SELECT MachineID, sum(convert(int, MachineOffTime) )as op,sum(convert(int, OperatingTime)) as o,sum(convert(int, IdleTime)) as it,sum(convert(int, BreakdownTime)) as bt FROM "+ MsqlConnection.DbName + ".tblmimics where CorrectedDate = '2019-06-21'and MachineID IN(select distinct(MachineID) from tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID",
                //SqlCommand cmd1 = new SqlCommand("SELECT MachineID,sum(convert (int,MachineOffTime) )as op,sum(convert (int,OperatingTime))as o,sum(convert (int,IdleTime)) as it,sum(convert (int,BreakdownTime))as bt FROM "+ MsqlConnection.DbName + ".tblmimics where CorrectedDate='" + CorrectedDate + "' and MachineID IN (select distinct(MachineID) from tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);

                SqlCommand cmd1 = new SqlCommand("SELECT MachineID, sum(convert(int, MachineOffTime) )as op,sum(convert(int, OperatingTime)) as o,sum(convert(int, IdleTime)) as it,sum(convert(int, BreakdownTime)) as bt FROM "+ MsqlConnection.DbName + ".tblmimics where CorrectedDate = '" + CorrectedDate + "' and MachineID IN(select distinct(MachineID) from " + MsqlConnection.DbName + ".tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);

                SqlDataReader datareader = cmd1.ExecuteReader();
                int maindatalooper1 = 0;

                while (datareader.Read())
                {
                    int maindatalooper2 = 0;
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(0);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(1);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(2);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(3);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(4);
                    maindatalooper1++;
                }
                mc1.close();
            }
            Session["colordata"] = maindata;
            //var tblMainDT = db.tbllivedailyprodstatus.Include(t => t.tblmachinedetail).Where(m => m.CorrectedDate == CorrectedDate).OrderBy(m => m.StartTime);
            //return View(tblMainDT.ToList());

            //Get Modes for All Machines for Today
            //List<tbllivemodedb> tblModeDT = db.tblmodes.Where(m => m.CorrectedDate == CorrectedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0).OrderBy(m => m.MachineID).ThenBy(m => m.StartTime).ToList();

            List<tblmode> tblModeDT = Serverdb.tblmodes.Where(m => m.CorrectedDate == CorrectedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 1).OrderBy(m => m.MachineID).ThenBy(m => m.StartTime).ToList();
            List<tblmode> tblModeDTCurr = Serverdb.tblmodes.Where(m => m.CorrectedDate == CorrectedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 0).OrderBy(m => m.MachineID).ThenByDescending(m => m.ModeID).ToList();

            //Get Latest Mode for each machine and Update the DurationInSec Column
            List<tblmode> CurrentModesOfAllMachines = (from row in tblModeDT
                                                             where row.IsCompleted == 0
                                                             select row).ToList();
            int PrvMachineID = 0;
            foreach (var row in tblModeDTCurr)
            {
                // DateTime startDateTime = Convert.ToDateTime( row.StartTime);
                // int DurInSec = Convert.ToInt32( DateTime.Now.Subtract(startDateTime).TotalSeconds );
                // //row.DurationInSec = Convert.ToInt32( DateTime.Now.Subtract(startDateTime).TotalSeconds );
                // int ModeID = row.ModeID;
                //foreach ( var tom in tblModeDT.Where(w => w.ModeID == ModeID)) {
                //             tom.DurationInSec = DurInSec;
                //         }

                if (PrvMachineID != row.MachineID)
                {
                    DateTime startDateTime = Convert.ToDateTime(row.StartTime);
                    int DurInSec = Convert.ToInt32(DateTime.Now.Subtract(startDateTime).TotalSeconds);
                    //row.DurationInSec = Convert.ToInt32( DateTime.Now.Subtract(startDateTime).TotalSeconds );
                    int ModeID = row.ModeID;
                    row.DurationInSec = DurInSec;
                    tblModeDT.Add(row);
                    //foreach (var tom in tblModeDT.Where(w => w.ModeID == ModeID))
                    //{

                    //}
                    PrvMachineID = row.MachineID;
                }
            }
            List<DBMode> ShowMode = new List<DBMode>();
            //Update DurationInSec to Minutes
            foreach (var MainRow in tblModeDT.Where(m => m.DurationInSec > 0))
            {
                DBMode ShowModeItem = new DBMode();
                ShowModeItem.ColorCode = MainRow.ColorCode;
                ShowModeItem.CorrectedDate = MainRow.CorrectedDate;
                ShowModeItem.DurationInSec = MainRow.DurationInSec / 60.00;
                ShowModeItem.EndTime = MainRow.EndTime;
                ShowModeItem.InsertedBy = MainRow.InsertedBy;
                ShowModeItem.InsertedOn = MainRow.InsertedOn;
                ShowModeItem.IsCompleted = MainRow.IsCompleted;
                ShowModeItem.IsDeleted = MainRow.IsDeleted;
                ShowModeItem.MachineID = MainRow.MachineID;
                ShowModeItem.Mode = MainRow.Mode;
                ShowModeItem.ModeID = MainRow.ModeID;
                ShowModeItem.ModifiedBy = MainRow.ModifiedBy;
                ShowModeItem.ModifiedOn = MainRow.ModifiedOn;
                ShowModeItem.StartTime = MainRow.StartTime;
                ShowModeItem.tblmachinedetail = MainRow.tblmachinedetail;
                ShowMode.Add(ShowModeItem);
                MainRow.DurationInSec = Convert.ToInt32(MainRow.DurationInSec / 60);
            };

            List<string> ShopNames = Serverdb.tblmodes.Where(m => m.CorrectedDate == CorrectedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0).Select(m => m.tblmachinedetail.ShopNo).Distinct().ToList();
            ViewBag.DistinctShops = ShopNames;

            //return View(tblModeDT);
            return View(ShowMode);
        }

    }
}