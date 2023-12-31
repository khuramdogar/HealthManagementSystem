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
    
    public partial class StockTake
    {
        public StockTake()
        {
            this.StockTakeItems = new HashSet<StockTakeItem>();
        }
    
        public int StockTakeId { get; set; }
        public StockTakeStatus Status { get; set; }
        public string Message { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public string SubmittedBy { get; set; }
        public Nullable<System.DateTime> SubmittedOn { get; set; }
        public int LocationId { get; set; }
        public System.DateTime StockTakeDate { get; set; }
        public StockTakeSource Source { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<StockTakeItem> StockTakeItems { get; set; }
        public virtual StockLocation Location { get; set; }
    }
}
