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
    
    public partial class tblpreactorschedule
    {
        public int ScheduleId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime InsertedOn { get; set; }
        public int InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string Path { get; set; }
        public Nullable<int> IsNetwork { get; set; }
        public string DomainName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Nullable<System.TimeSpan> OutputGenerationTime { get; set; }
        public string FileFormat { get; set; }
        public int IsStart { get; set; }
    }
}