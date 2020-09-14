using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using i_facility;
using i_facility.Models;

namespace i_facility.Controllers
{
    public class DDLController : Controller
    {
        i_facility_tsalEntities db = new i_facility_tsalEntities();
        
        // GET: DDL
        public ActionResult DDLList(int DDLID = 0, string MacInvNo = null, int ToHMI = 0)
        {
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            var tbllogin = db.tblusers.Include(t => t.tblmachinedetail).Where(m => m.IsDeleted == 0);
            return View(tbllogin);
        }
    }
}