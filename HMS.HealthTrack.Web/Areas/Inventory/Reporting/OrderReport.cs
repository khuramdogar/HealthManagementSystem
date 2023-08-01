using DevExpress.XtraReports.UI;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Orders;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using ImageSizeMode = DevExpress.XtraPrinting.ImageSizeMode;

namespace HMS.HealthTrack.Web.Areas.Inventory.Reporting
{
   public partial class OrderReport : DevExpress.XtraReports.UI.XtraReport
   {
      public OrderReport()
      {
         InitializeComponent();
      }

      public OrderReport(Image logo, StockLocation deliveryLocation, StockLocation senderLocation, OrderStatus orderStatus, List<PrintOrdersReportModel> reportModel)
      {
         InitializeComponent();

         DataSource = reportModel;
         BackOrderedItems.DataSource = reportModel;
         CancelledItems.DataSource = reportModel;
         Products.DataSource = reportModel;

         if (senderLocation != null && senderLocation.Address != null)
         {
            var senderAddress = senderLocation.Address;
            var formattedAddress = Formatter.FormattedAddress(deliveryLocation.Name, senderAddress.Address1, senderAddress.Address2,
               senderAddress.Suburb, senderAddress.State, senderAddress.PostCode, null, null, true);
            SenderAddress.Text = formattedAddress;
         }

         if (deliveryLocation != null && deliveryLocation.Address != null)
         {
            var deliveryAddress = deliveryLocation.Address;
            var formattedAddress = Formatter.FormattedAddress(deliveryLocation.Name, deliveryAddress.Address1, deliveryAddress.Address2,
               deliveryAddress.Suburb, deliveryAddress.State, deliveryAddress.PostCode, null, null, true);
            DeliveryAddress.Text = formattedAddress;
         }

         if (logo != null)
         {
            pbLogo.Image = logo;
            pbLogo.Sizing = ImageSizeMode.StretchImage;
         }

         if (logo == null && senderLocation == null)
         {
            TopMargin.Visible = false;
         }

         var model = reportModel.FirstOrDefault();
         if (model == null) return;

         if (model.OrderDetails.DateCreated.HasValue)
            lblDateCreated.Text = model.OrderDetails.DateCreated.Value.ToShortDateString();
         lblOrderId.Text = string.Format("#{0}", model.OrderDetails.OrderId);

         if (orderStatus != OrderStatus.Invoiced)
         {
            lblSubHeading.Visible = false;
            var padding = lblOrderHeading.Padding;
            padding.Right = padding.Right + 10;

            lblReorderHeading.Visible = false;
            lblReorderDetail.Visible = false;
            lblDescriptionHeading.Width += lblReorderHeading.Width;
            lblDescriptionDetail.Width += lblReorderDetail.Width;
         }

         if (orderStatus == OrderStatus.PartiallyReceived)
         {
            ProductsSubTotalFooter.Visible = true;
         }

         var showAllSections = orderStatus == OrderStatus.PartiallyReceived || orderStatus == OrderStatus.Complete;
         lblReceivedItems.Visible = showAllSections && model.Products.Count > 0;
         BackOrderedItems.Visible = showAllSections && model.BackOrderProducts.Count > 0;
         CancelledItems.Visible = showAllSections && model.CancelledProducts.Count > 0;
      }

      private void lblOrderName_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
      {
         var label = (XRLabel)sender;

         var currentValue = label.Text;
         label.Text = string.Format("Order - {0}", currentValue).ToUpper();
      }

      private void ShrinkNullRow_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
      {
         var row = (XRTableRow)sender;
         if (row == null) return;
         if (string.IsNullOrWhiteSpace(row.Cells[1].Text))
         {
            e.Cancel = true;
         }

         if (row.Cells[0].Text == "Rebate code:")
         {
            e.Cancel = true;
         }
      }

   }
}
