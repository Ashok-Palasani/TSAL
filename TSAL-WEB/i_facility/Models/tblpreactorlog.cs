//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace i_facility.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblpreactorlog
    {
        public int LogId { get; set; }
        public string OutputFileName { get; set; }
        public string LogInfo { get; set; }
        public Nullable<System.DateTime> reportGenaratedTime { get; set; }
        public int Isgenerated { get; set; }
        public Nullable<int> ScheduleID { get; set; }
        public string CorrectedDate { get; set; }
    }
}