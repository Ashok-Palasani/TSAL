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
    
    public partial class scrapQty
    {
        public int scrapId { get; set; }
        public string workOder { get; set; }
        public string workCenter { get; set; }
        public string decription { get; set; }
        public string operationNo { get; set; }
        public string postgDate { get; set; }
        public Nullable<int> yield { get; set; }
        public Nullable<decimal> setup { get; set; }
        public Nullable<decimal> mach { get; set; }
        public Nullable<int> labour { get; set; }
        public string rev { get; set; }
        public Nullable<int> ccldConf { get; set; }
        public Nullable<int> ScrapQty1 { get; set; }
        public Nullable<int> isDeleted { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> modifiedOn { get; set; }
        public Nullable<int> modifiedBy { get; set; }
    }
}
