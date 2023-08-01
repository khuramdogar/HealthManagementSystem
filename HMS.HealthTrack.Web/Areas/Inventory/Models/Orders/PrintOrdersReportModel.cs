using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using System;
using System.Collections.Generic;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Orders
{
   public class PrintOrdersReportModel
   {
      public OrderDetails OrderDetails { get; set; }
      public List<OrderLineItem> BackOrderProducts { get; set; }
      public List<OrderLineItem> CancelledProducts { get; set; }
      public List<OrderLineItem> Products { get; set; }
   }

   public class OrderDetails
   {
      public int OrderId { get; set; }
      public string Name { get; set; }
      public string Notes { get; set; }
      public DateTime? DateCreated { get; set; }
      public string CreatedBy { get; set; }
      public string Status { get; set; }
      public decimal Total { get; set; }
      public string ChargeAccount { get; set; }
      public string LedgerCode { get; set; }
      public OrderStatus OrderStatus { get; set; }
   }

   public class OrderLineItem : ProductDetails
   {
      public int Quantity { get; set; }
      public decimal LineTotal
      {
         get
         {
            return Quantity * UnitPrice;
         }
      }

      public DateTime? ReceivedOn { get; set; }

      // patient details
      public string PatientName { get; set; }
      public string PatientId { get; set; }
      public string PatientDOB { get; set; }
      public string MedicareNumber { get; set; }
      public string PatientDetails { get; set; }
   }
}