//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CalOEEDaily
{
    using System;
    using System.Collections.Generic;
    
    public partial class opcuttimereport
    {
        public int idOpCutTimeReport { get; set; }
        public double Operatingtimes { get; set; }
        public double Cuttingtimes { get; set; }
        public double PowerOnTime { get; set; }
        public System.TimeSpan Timeinserted { get; set; }
        public int Shift { get; set; }
        public string ReportDate { get; set; }
    }
}