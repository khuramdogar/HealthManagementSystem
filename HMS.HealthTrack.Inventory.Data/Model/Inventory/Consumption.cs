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
    
    public partial class Consumption
    {
        public long ConsumptionId { get; set; }
        public int ConsumptionReference { get; set; }
        public Nullable<int> ProductId { get; set; }
        public string SPC { get; set; }
        public string UPC { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public System.DateTime ConsumedOn { get; set; }
        public string SerialNumber { get; set; }
        public string BatchNumber { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public string Consumer { get; set; }
        public string ApplicationId { get; set; }
        public string Description { get; set; }
        public string RebateCode { get; set; }
    
        public virtual ConsumptionManagement ConsumptionManagement { get; set; }
    }
}