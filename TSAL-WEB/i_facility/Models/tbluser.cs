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
    
    public partial class tbluser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbluser()
        {
            this.tblmachineallocations = new HashSet<tblmachineallocation>();
        }
    
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int PrimaryRole { get; set; }
        public int SecondaryRole { get; set; }
        public string DisplayName { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> MachineID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmachineallocation> tblmachineallocations { get; set; }
        public virtual tblmachinedetail tblmachinedetail { get; set; }
        public virtual tblrole tblrole { get; set; }
        public virtual tblrole tblrole1 { get; set; }
    }
}
