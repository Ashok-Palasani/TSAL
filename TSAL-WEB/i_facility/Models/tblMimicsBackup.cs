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
    
    public partial class tblMimicsBackup
    {
        public int mbid { get; set; }
        public Nullable<int> mid { get; set; }
        public string machineOnTime { get; set; }
        public string operatingTime { get; set; }
        public string setupTime { get; set; }
        public string idleTime { get; set; }
        public string machineOffTime { get; set; }
        public string breakdownTime { get; set; }
        public Nullable<int> machineId { get; set; }
        public string shift { get; set; }
        public string correctedDate { get; set; }
    }
}