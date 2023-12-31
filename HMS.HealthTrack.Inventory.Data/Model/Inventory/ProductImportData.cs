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
    
    public partial class ProductImportData
    {
        public ProductImportData()
        {
            this.ProductImports = new HashSet<ProductImport>();
        }
    
        public int ProductImportDataId { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> ImportedOn { get; set; }
        public ProductImportStatus Status { get; set; }
        public byte[] ProductsData { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public string DeletedBy { get; set; }
        public Nullable<System.DateTime> DeletedOn { get; set; }
        public string Message { get; set; }
    
        public virtual ICollection<ProductImport> ProductImports { get; set; }
    }
}
