using DevExpress.XtraReports.UI;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Orders
{
	public class PrintOrdersDisplayModel
	{
		public int ProductId { get; set; }
		public XtraReport Report { get; set; }
	}
}