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
    
    public partial class tblSPgenericworkentry
    {
        public int SPGWEntryID { get; set; }
        public int GWCodeID { get; set; }
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public string CorrectedDate { get; set; }
        public int MachineID { get; set; }
        public string Shift { get; set; }
        public string GWCodeDesc { get; set; }
        public string GWCode { get; set; }
        public int DoneWithRow { get; set; }
        public string BatchNo { get; set; }
    }
}