using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
   public class tblolecaldetails
    {

        public int Oleid { get; set; }
        public int? Opid { get; set; }
        public int? OpWorkingDuration { get; set; }
        public int? LossDuration { get; set; }
        public int? MachineId { get; set; }
        public string CorrectedDate { get; set; }
        public string Ottime { get; set; }
        public int? Shift { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public int Isdeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public Nullable<double> Blue { get; set; }
        public Nullable<double> Green { get; set; }
        public Nullable<double> SettingTime { get; set; }
        public Nullable<double> ROALossess { get; set; }
        public Nullable<double> DownTimeBreakdown { get; set; }
        public Nullable<double> SummationOfSCTvsPP { get; set; }
        public Nullable<double> ScrapQtyTime { get; set; }
        public Nullable<double> ReWOTime { get; set; }
    }
}
