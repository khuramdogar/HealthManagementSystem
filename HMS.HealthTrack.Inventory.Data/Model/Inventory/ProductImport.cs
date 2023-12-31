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
    
    public partial class ProductImport
    {
        public ProductImport()
        {
            this.ProductImportGeneralLedgerCodes = new HashSet<ProductImportGeneralLedgerCode>();
        }
    
        public int ProductImportId { get; set; }
        public int ProductImportDataId { get; set; }
        public bool Processed { get; set; }
        public Nullable<System.DateTime> ProcessedOn { get; set; }
        public string InternalProductId { get; set; }
        public string SPC { get; set; }
        public string LPC { get; set; }
        public string UPN { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string GLC { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public string Category { get; set; }
        public string MinimumOrder { get; set; }
        public string OrderMultiple { get; set; }
        public string ReorderThreshold { get; set; }
        public string TargetStockLevel { get; set; }
        public string PublicUnitPrice { get; set; }
        public string PrivateUnitPrice { get; set; }
        public string Consignment { get; set; }
        public string Sterile { get; set; }
        public bool Invalid { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<int> LedgerId { get; set; }
        public Nullable<int> SupplierId { get; set; }
        public string ReorderSetting { get; set; }
        public string ProductSettings { get; set; }
        public string UseCategorySettings { get; set; }
        public string Message { get; set; }
        public string RebateCode { get; set; }
    
        public virtual GeneralLedger GeneralLedger { get; set; }
        public virtual Product Product { get; set; }
        public virtual Supplier Supplier1 { get; set; }
        public virtual ICollection<ProductImportGeneralLedgerCode> ProductImportGeneralLedgerCodes { get; set; }
        public virtual ProductImportData ProductImportData { get; set; }
    }
}
