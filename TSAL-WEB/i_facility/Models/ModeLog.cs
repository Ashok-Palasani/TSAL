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
    
    public partial class ModeLog
    {
        public int mid { get; set; }
        public string MailSentDateTime { get; set; }
        public int MachineId { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
    }
}