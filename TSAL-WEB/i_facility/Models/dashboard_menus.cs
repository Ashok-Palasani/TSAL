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
    
    public partial class dashboard_menus
    {
        public int id { get; set; }
        public string MenuName { get; set; }
        public string MenuUrl { get; set; }
        public string ImageUrl { get; set; }
        public Nullable<int> MenuId { get; set; }
        public string ColourDiv { get; set; }
        public string Style { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}