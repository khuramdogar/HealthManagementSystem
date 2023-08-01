using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   #region Stock Adjustment

   [MetadataType(typeof(StockAdjustmentMeta))]
   public partial class StockAdjustment
   {
   }

   public class StockAdjustmentMeta
   {
      [Key]
      [Display(Name = "ID", Description = "Stock adjustment identifier")]
      public int StockAdjustmentId { get; set; }

      [Display(Name = "Stock ID", Description = "The received stock batch the adjusted item(s) came from")]
      public int StockId { get; set; }

      [Display(Name = "Adjusted on", Description = "When the item was adjusted")]
      public DateTime? AdjustedOn { get; set; }

      [Display(Name = "Adjusted by", Description = "Who adjusted the item")]
      public string AdjustedBy { get; set; }

      [Display(Name = "Quantity", Description = "How many items were adjusted")]
      public int Quantity { get; set; }

      [Display(Name = "Created by")]
      public string CreatedBy { get; set; }

      [Display(Name = "Created on")]
      public DateTime? CreatedOn { get; set; }

      [Display(Name = "Deleted on")]
      public DateTime? DeletedOn { get; set; }

      [Display(Name = "Deleted by")]
      public string DeletedBy { get; set; }

      [Display(Name = "Deletion reason")]
      public string DeletionReason { get; set; }

      [Display(Name = "Patient")]
      public int? PatientId { get; set; }

      [Display(Name = "Clinical record")]
      public long? ClinicalRecordId { get; set; }

      [Display(Name = "Adjustment source")]
      public AdjustmentSource Source { get; set; }
   }

   #endregion

   #region HealthTrack Consumption Meta

   [MetadataType(typeof(HealthTrackConsumptionMeta))]
   public partial class HealthTrackConsumption
   {
   }


   public class HealthTrackConsumptionMeta
   {
      [Key]
      [Display(Name = "ID", Description = "HealthTrack consumption identifier")]
      public int UsedId { get; set; }

      public int ProductId { get; set; }
      public string SPC { get; set; }
      public string LPC { get; set; }
      public string ScanCode { get; set; }
      public string Name { get; set; }
      public string SerialNumber { get; set; }
      public short? Quantity { get; set; }
      public int? Location { get; set; }
      public string PatientMRN { get; set; }
      public long? ContainerId { get; set; }
      public int? PatientId { get; set; }
      public string ClinicalGroup { get; set; }
      public string ClinicalSubGroup { get; set; }
      public string Manufacturer { get; set; }
      public string GL { get; set; }

      [Display(Name = "Rebate code")]
      public string RebateCode { get; set; }

      public string Description { get; set; }
      public decimal? Price { get; set; }
      public string BuyCurrency { get; set; }
      public double? BuyCurrencyRate { get; set; }
      public bool? deleted { get; set; }
      public DateTime? deletionDate { get; set; }
      public string deletionUser { get; set; }

      [Display(Name = "Date modified")]
      public DateTime? LastModifiedOn { get; set; }

      [Display(Name = "Modified by")]
      public string LastModifiedBy { get; set; }

      [Display(Name = "Date created")]
      public DateTime? dateCreated { get; set; }

      [Display(Name = "Created by")]
      public string userCreated { get; set; }

      [Display(Name = "Processing status")]
      public ConsumptionProcessingStatus ProcessingStatus { get; set; }

      [Display(Name = "Processing message")]
      public string ProcessingStatusMessage { get; set; }

      public string LotNumber { get; set; }
      public string LocationName { get; set; }
      public bool? Reported { get; set; }
      public string ReportedBy { get; set; }
      public DateTime? ReportedOn { get; set; }

      [Display(Name = "Consumed on")]
      public DateTime? ConsumedOn { get; set; }
   }

   #endregion

   #region Inventory Product

   [MetadataType(typeof(ProductMeta))]
   public partial class Product
   {
      public string SelectedCategories { get; set; }
   }

   public class ProductMeta
   {
      [Required]
      [Display(Name = "Product ID")]
      [Key]
      public int ProductId { get; set; }

      [Display(Name = "SPC", Description = "Supplier Product Code")]
      public string SPC { get; set; }

      [Display(Name = "LPC", Description = "Local Product Code")]
      public string LPC { get; set; }

      [Display(Name = "UPN", Description = "Universal Product Number")]
      public string UPN { get; set; }

      [Display(Name = "Description")]
      [Required]
      public string Description { get; set; }

      [Display(Name = "GLC", Description = "General Ledger Code")]
      public string GLC { get; set; }

      [Display(Name = "Manufacturer")]
      public string Manufacturer { get; set; }

      [Display(Name = "Expired")]
      public bool UseExpired { get; set; }

      [Display(Name = "Sterile")]
      public bool UseSterile { get; set; }

      [Display(Name = "Max uses", Description = "The maximum number of time this product can be used/reused")]
      public int MaxUses { get; set; }

      [Display(Name = "Additional description")]
      public string Notes { get; set; }

      [Display(Name = "Special requirements")]
      public string SpecialRequirements { get; set; }

      [Display(Name = "Primary supplier")]
      public int? PrimarySupplier { get; set; }

      [Display(Name = "Secondary supplier")]
      public int? SecondarySupplier { get; set; }

      [Display(Name = "Order in multiples of", Description = "Item should only be ordered in specified multiples")]
      public int OrderMultiple { get; set; }

      [Display(Name = "Low stock threshold", Description = "The stock level that triggers reordering")]
      public int ReorderThreshold { get; set; }

      [Display(Name = "Target stock level", Description = "The desired number of items to have in stock")]
      public int? TargetStockLevel { get; set; }

      [Display(Name = "Minimum order", Description = "The minimum number of items to order")]
      public int MinimumOrder { get; set; }

      [Display(Name = "Consignment")]
      public bool IsConsignment { get; set; }

      [Display(Name = "Price model")]
      public int? PriceModelId { get; set; }

      [Display(Name = "Rebate code")]
      public string RebateCode { get; set; }

      [Display(Name = "Buy price")]
      public decimal? BuyPrice { get; set; }

      [Display(Name = "Currency")]
      public string BuyCurrency { get; set; }

      [Display(Name = "Exchange rate")]
      public string BuyCurrencyRate { get; set; }

      [Display(Name = "Sell price")]
      public decimal? SellPrice { get; set; }

      [Display(Name = "Search categories")]
      public string SelectedCategories { get; set; }

      public decimal? MarkUp { get; set; }

      [Display(Name = "Deleted on")]
      public DateTime? DeletedOn { get; set; }

      [Display(Name = "Deleted by")]
      public string DeletedBy { get; set; }

      [Display(Name = "Last modified on")]
      public DateTime? LastModifiedOn { get; set; }

      [Display(Name = "Last modified by")]
      public string LastModifiedBy { get; set; }

      [Display(Name = "Created on")]
      public DateTime CreatedOn { get; set; }

      [Display(Name = "Created by")]
      public string CreatedBy { get; set; }

      [Display(Name = "Automatic re-order")]
      public ReorderSettings AutoReorderSetting { get; set; }

      [Display(Name = "Report consumption")]
      public bool ReportConsumption { get; set; }

      [Display(Name = "Use payment class price")]
      public bool UsePaymentClassPrice { get; set; }

      [Display(Name = "Status")]
      public ProductStatus ProductStatus { get; set; }

      [Display(Name = "In error")]
      public bool InError { get; set; }

      [Display(Name = "Manage stock")]
      public bool ManageStock { get; set; }
   }

   #endregion

   #region Order

   public class OrderMeta
   {
      [Display(Name = "ID")]
      [Key]
      public int InventoryOrderId { get; set; }

      [Display(Name = "Created on")]
      public DateTime? DateCreated { get; set; }

      [Display(Name = "Created by")]
      public string CreatedBy { get; set; }

      [Display(Name = "Delivery location")]
      public int DeliveryLocationId { get; set; }

      [Display(Name = "Required by")]
      [DataType(DataType.Date)]
      public DateTime? NeedBy { get; set; }

      [Display(Name = "Urgent")]
      public bool IsUrgent { get; set; }

      [Display(Name = "Last modified on")]
      public DateTime? LastModifiedOn { get; set; }

      [Display(Name = "Last modified by")]
      public string LastModifiedBy { get; set; }

      [Display(Name = "Deleted on")]
      public DateTime? DeletedOn { get; set; }

      [Display(Name = "Deleted by")]
      public string DeletedBy { get; set; }
   }

   [MetadataType(typeof(OrderMeta))]
   public partial class Order
   {
   }

   public class OrderItemMeta
   {
      [Display(Name = "Item Id")]
      [Key]
      public int OrderItemId { get; set; }

      [Display(Name = "Order Id")]
      public int InventoryOrderId { get; set; }

      public int ProductId { get; set; }
      public int Quantity { get; set; }

      [Display(Name = "Unit Price")]
      public decimal? UnitPrice { get; set; }

      public bool IncludeRebateCode { get; set; }
   }

   [MetadataType(typeof(OrderItemMeta))]
   public partial class OrderItem
   {
   }

   #endregion

   #region Inventory_Stock

   [MetadataType(typeof(InventoryStockMeta))]
   public partial class Stock
   {
      public StringBuilder AuditDescription
      {
         get
         {
            var description = new StringBuilder();
            description.AppendFormat("An inventory item of type '{0}' ", ProductId);
            if (!string.IsNullOrWhiteSpace(SerialNumber))
               description.AppendFormat("with a serial number of '{0}' ", SerialNumber);
            return description;
         }
      }

      public bool HasSerial => !string.IsNullOrWhiteSpace(SerialNumber);
   }

   internal class InventoryStockMeta
   {
      [Display(Name = "ID")]
      [Key]
      public int StockId { get; set; }

      [Display(Name = "Product ID")]
      public int ProductId { get; set; }

      [Display(Name = "Received on")]
      public DateTime ReceivedOn { get; set; }

      [Display(Name = "Item status")]
      public StockStatus StockStatus { get; set; }

      [Display(Name = "Serial number")]
      public string SerialNumber { get; set; }

      [Display(Name = "Item owner", Description = "Who possesses the item")]
      public int? Owner { get; set; }

      [Display(Name = "Stored at", Description = "Where the item is stored")]
      public int? StoredAt { get; set; }

      [Display(Name = "Manufacturer batch", Description = "The unique manufacturing batch of the item")]
      public int? BatchNumber { get; set; }

      [Display(Name = "Price model", Description = "The price model set when the item was received")]
      public int? PriceModelOnReceipt { get; set; }

      [Display(Name = "Buy price", Description = "The price the item was bought for")]
      public decimal? BoughtPrice { get; set; }

      [Display(Name = "GST rate", Description = "The rate of purchase tax paid when the item was bought")]
      public int? TaxRateOnReceipt { get; set; }

      [Display(Name = "Sale price", Description = "Sale price, if fixed, at the time of purchase")]
      public decimal? SellPrice { get; set; }

      [Display(Name = "Expiry date", Description = "The date the item should not be used past")]
      public DateTime? ExpiresOn { get; set; }

      [Display(Name = "Deletion date", Description = "When the item was deleted")]
      public DateTime? DeletedOn { get; set; }

      [Display(Name = "Deleted by", Description = "Who deleted the item")]
      public string DeletedBy { get; set; }

      [Display(Name = "Deletion reason", Description = "Why this item was deleted")]
      public string DeletionReason { get; set; }

      [Display(Name = "Last modified on", Description = "When the item was last changed")]
      public DateTime? LastModifiedOn { get; set; }

      [Display(Name = "Last modified by", Description = "Who changed the item last")]
      public string LastModifiedBy { get; set; }

      [Display(Name = "Created on", Description = "When the item was created")]
      public DateTime? CreatedOn { get; set; }

      [Display(Name = "Created by", Description = "Who created the item")]
      public string CreatedBy { get; set; }

      [Display(Name = "Parent Order", Description = "The order the item came from")]
      public int OrderItemId { get; set; }

      public int Quantity { get; set; }
   }

   #endregion

   #region Categories

   public class CategoryMeta
   {
      [Display(Name = "ID")]
      [Key]
      public int CategoryId { get; set; }

      [Display(Name = "Parent Category")]
      public int? ParentId { get; set; }

      [Display(Name = "Category Name")]
      public string CategoryName { get; set; }

      public bool Deleted { get; set; }

      [Display(Name = "Deleted By")]
      public string DeletedBy { get; set; }

      [Display(Name = "Date Last Modified")]
      public DateTime? LastModifiedDate { get; set; }

      [Display(Name = "User Last Modified")]
      public string LastModifiedUser { get; set; }

      [Display(Name = "Created On")]
      public DateTime CreationDate { get; set; }

      [Display(Name = "Created By")]
      public string UserCreated { get; set; }

      [Display(Name = "Deleted On")]
      public DateTime? DeletedOn { get; set; }
   }

   [MetadataType(typeof(CategoryMeta))]
   public partial class Category
   {
   }

   [MetadataType(typeof(ProductCategoryMeta))]
   public partial class ProductCategory
   {
   }

   public class ProductCategoryMeta
   {
      [Display(Name = "Id")]
      [Key]
      public int InventoryCategoryId { get; set; }

      public int Inv_ID { get; set; }
      public int CategoryId { get; set; }
   }

   #endregion

   #region Stock Sets

   public class StockSetMeta
   {
      [Key]
      [Display(Name = "ID")]
      public int StockSetId { get; set; }

      public string Name { get; set; }
   }

   [MetadataType(typeof(StockSetMeta))]
   public partial class StockSet
   {
   }

   #endregion

   #region Stock Requests

   public class StockRequestMeta
   {
      [Key]
      [Display(Name = "ID")]
      public int StockRequestId { get; set; }

      [Required(ErrorMessage = "Please select a valid product")]
      [Display(Name = "Product ID")]
      public int ProductId { get; set; }

      [Display(Name = "Requested quantity")]
      public int RequestedQuantity { get; set; }

      [Display(Name = "Created on")]
      public DateTime CreatedOn { get; set; }

      [Display(Name = "Created by")]
      public string CreatedBy { get; set; }

      [Display(Name = "Status")]
      public int RequestStatus { get; set; }

      [Display(Name = "Location")]
      public int RequestLocationId { get; set; }

      [Display(Name = "Urgent")]
      public bool IsUrgent { get; set; }

      [Display(Name = "Last modified by")]
      public string LastModifiedBy { get; set; }

      [Display(Name = "Last modified on")]
      public DateTime? LastModifiedOn { get; set; }

      [Display(Name = "Approved quantity")]
      public int ApprovedQuantity { get; set; }
   }

   [MetadataType(typeof(StockRequestMeta))]
   public partial class ProductStockRequest
   {
   }

   #endregion

   #region ExternalProductMapping

   public class ExternalProductMappingMeta
   {
      [Key]
      [Display(Name = "Mapping ID")]
      public int ProductMappingId { get; set; }

      [Display(Name = "Source")]
      public ProductMappingSource ProductSource { get; set; }

      [Display(Name = "External ID")]
      public int ExternalProductId { get; set; }

      [Display(Name = "Product ID")]
      public int InventoryProductId { get; set; }

      [Display(Name = "Deleted On")]
      public DateTime? DeletedOn { get; set; }

      [Display(Name = "Deleted By")]
      public string DeletedBy { get; set; }

      [Display(Name = "Date Last Modified")]
      public DateTime? LastModifiedOn { get; set; }

      [Display(Name = "User Last Modified")]
      public string LastModifiedBy { get; set; }

      [Display(Name = "Created On")]
      public DateTime CreatedOn { get; set; }

      [Display(Name = "Created By")]
      public string CreatedBy { get; set; }
   }

   [MetadataType(typeof(ExternalProductMappingMeta))]
   public partial class ExternalProductMapping
   {
   }

   #endregion

   #region Location

   public class StockLocationMeta
   {
      [Key]
      public int LocationId { get; set; }

      public string Name { get; set; }

      [Display(Name = "Created on")]
      public DateTime CreatedOn { get; set; }

      [Display(Name = "Created by")]
      public string CreatedBy { get; set; }

      [Display(Name = "Last modified on")]
      public DateTime? LastModifiedOn { get; set; }

      [Display(Name = "Last modified by")]
      public string LastModifiedUser { get; set; }

      [Display(Name = "Deleted on")]
      public DateTime? DeletedOn { get; set; }

      [Display(Name = "Deleted by")]
      public string DeletedBy { get; set; }

      [Display(Name = "Logo")]
      public byte[] LogoImage { get; set; }
   }

   [MetadataType(typeof(StockLocationMeta))]
   public partial class StockLocation
   {
   }

   #endregion

   #region Address

   public class AddressMeta
   {
      [Key]
      [Display(Name = "Address Id")]
      public int AddressId { get; set; }

      [Display(Name = "Address 1")]
      public string Address1 { get; set; }

      [Display(Name = "Address 2")]
      public string Address2 { get; set; }

      public string Suburb { get; set; }
      public string State { get; set; }

      [Display(Name = "Post code")]
      public string PostCode { get; set; }

      public string Country { get; set; }
      public string Department { get; set; }
   }

   [MetadataType(typeof(AddressMeta))]
   public partial class Address
   {
   }

   #endregion

   #region Company

   public class CompanyMeta
   {
      [Key]
      public int company_ID { get; set; }
   }

   [MetadataType(typeof(CompanyMeta))]
   public partial class Company
   {
   }

   #endregion

   #region StockSetItems

   public class StockSetItemMeta
   {
      [Key]
      [Column(Order = 1)]
      public int ProductId { get; set; }

      [Key]
      [Column(Order = 2)]
      public int StockSetId { get; set; }
   }

   [MetadataType(typeof(StockSetItemMeta))]
   public partial class StockSetItem
   {
   }

   #endregion

   #region ProductPrice

   public class ProductPriceMeta
   {
      [Key]
      public int PriceId { get; set; }

      public int ProductId { get; set; }

      [Display(Name = "Buy Price")]
      public decimal? BuyPrice { get; set; }

      [Display(Name = "Buy Currency")]
      public string BuyCurrency { get; set; }

      [Display(Name = "Buy Currency Rate")]
      public string BuyCurrencyRate { get; set; }

      [Display(Name = "Sell Price")]
      public decimal? SellPrice { get; set; }

      [Display(Name = "Type")]
      public int PriceTypeId { get; set; }
   }

   [MetadataType(typeof(ProductPriceMeta))]
   public partial class ProductPrice
   {
   }

   #endregion

   #region Properties

   public class PropertyMeta
   {
      [Key]
      public int PropertyId { get; set; }

      public string PropertyName { get; set; }
      public string PropertyValue { get; set; }
      public string Description { get; set; }
   }

   [MetadataType(typeof(PropertyMeta))]
   public partial class Property
   {
   }

   #endregion

   #region Supplier

   public class SupplierMeta
   {
      [Key]
      public int company_ID { get; set; }
   }

   [MetadataType(typeof(SupplierMeta))]
   public partial class Supplier
   {
   }

   #endregion

   #region AddressCorporate

   public class AddressCorporateMeta
   {
      [Key]
      public int addressCorporate_ID { get; set; }

      public int? owner_ID { get; set; }
      public int addressType { get; set; }
      public bool primaryAddress { get; set; }
      public bool invoiceAddress { get; set; }
      public bool mailingAddress { get; set; }

      [Display(Name = "Address 1")]
      public string address1 { get; set; }

      [Display(Name = "Address 2")]
      public string address2 { get; set; }

      [Display(Name = "Suburb")]
      public string suburb { get; set; }

      [Display(Name = "State")]
      public string state { get; set; }

      [Display(Name = "Post code")]
      public string postcode { get; set; }

      [Display(Name = "Country")]
      public string country { get; set; }

      [Display(Name = "Department")]
      public string department { get; set; }

      [Display(Name = "Phone")]
      public string phoneNumber { get; set; }

      [Display(Name = "Fax")]
      public string faxNumber { get; set; }

      [Display(Name = "Email")]
      public string email { get; set; }

      [Display(Name = "Website")]
      public string webSite { get; set; }

      public string ABN { get; set; }
      public string ACN { get; set; }

      [Display(Name = "Title")]
      public string contactTitle { get; set; }

      [Display(Name = "First name")]
      public string contactFirstname { get; set; }

      [Display(Name = "Surname")]
      public string contactSurname { get; set; }

      public string subCompanyName { get; set; }
   }

   [MetadataType(typeof(AddressCorporateMeta))]
   public partial class AddressCorporate
   {
   }

   #endregion

   #region SupplierModel

   public class SupplierModelMeta
   {
      [Key]
      public int? company_ID { get; set; }

      [Display(Name = "Created by")]
      public string CreatedBy { get; set; }

      [Display(Name = "Created on")]
      public DateTime CreatedOn { get; set; }

      [Display(Name = "Last modified on")]
      public DateTime? LastModifiedOn { get; set; }

      [Display(Name = "Last modified by")]
      public string LastModifiedBy { get; set; }

      [Required]
      public string Name { get; set; }

      [Display(Name = "Address 1")]
      public string Address1 { get; set; }

      [Display(Name = "Address 2")]
      public string Address2 { get; set; }

      [Display(Name = "Suburb")]
      public string Suburb { get; set; }

      [Display(Name = "State")]
      public string State { get; set; }

      [Display(Name = "Post code")]
      public string PostCode { get; set; }

      [Display(Name = "Country")]
      public string Country { get; set; }

      [Display(Name = "Department")]
      public string Department { get; set; }

      [Display(Name = "Phone")]
      public string PhoneNumber { get; set; }

      [Display(Name = "Fax")]
      public string FaxNumber { get; set; }

      [Display(Name = "Email")]
      public string Email { get; set; }

      [Display(Name = "Website")]
      public string WebSite { get; set; }

      [Display(Name = "Title")]
      public string ContactTitle { get; set; }

      [Display(Name = "First name")]
      public string ContactFirstname { get; set; }

      [Display(Name = "Surname")]
      public string ContactSurname { get; set; }
   }

   [MetadataType(typeof(SupplierModelMeta))]
   public partial class SupplierModel
   {
   }

   #endregion

   #region ChargeAccount

   public class ChargeAccountMeta
   {
      public int AccountId { get; set; }
      public string AccountName { get; set; }
      public string GLC { get; set; }
      public int? ParentId { get; set; }

      [Display(Name = "Created by")]
      public string CreatedBy { get; set; }

      [Display(Name = "Created on")]
      public DateTime CreatedOn { get; set; }

      [Display(Name = "Last modified by")]
      public string LastModifiedBy { get; set; }

      [Display(Name = "Last modified on")]
      public DateTime? LastModifiedOn { get; set; }
   }

   [MetadataType(typeof(ChargeAccountMeta))]
   public class ChargeAccount
   {
   }

   #endregion

   public class UserPreferenceMeta
   {
      [Display(Name = "User Id")]
      public string User_ID { get; set; }

      [Display(Name = "Location")]
      public int? LocationId { get; set; }
   }

   [MetadataType(typeof(UserPreferenceMeta))]
   public partial class UserPreference
   {
   }

   #region DashboardNotification

   public class DashboardNotificationMeta
   {
      public string DashboardNotificationId { get; set; }
      public string Title { get; set; }
      public string Description { get; set; }
      public string Icon { get; set; }
      public int Priority { get; set; }
      public bool ShowWhenZero { get; set; }
      public bool Disabled { get; set; }
      public string Area { get; set; }
      public int ItemCount { get; set; }
   }

   [MetadataType(typeof(DashboardNotificationMeta))]
   public partial class DashboardNotification
   {
   }

   #endregion
}