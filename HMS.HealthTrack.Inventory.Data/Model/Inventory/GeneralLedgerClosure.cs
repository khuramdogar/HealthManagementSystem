//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
    using System;
    using System.Collections.Generic;
    
    public partial class GeneralLedgerClosure
    {
        public int ParentId { get; set; }
        public int ChildId { get; set; }
        public int Depth { get; set; }
    
        public virtual GeneralLedger GeneralLedgerParent { get; set; }
        public virtual GeneralLedger GeneralLedgerChild { get; set; }
    }
}