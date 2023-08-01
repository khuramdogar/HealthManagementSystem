using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Results;
using System.Web.Mvc;
using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.OrderChannels;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class OrderChannelsController : Controller
   {
      private readonly IOrderChannelRepository _orderChannelRepository;

      public OrderChannelsController(IOrderChannelRepository orderChannelRepository)
      {
         _orderChannelRepository = orderChannelRepository;
      }

      [HttpPost]
      public ActionResult AddProductToChannel(OrderChannelProduct orderChannelProduct)
      {
         var result =_orderChannelRepository.AddProductToChannel(orderChannelProduct.ProductId,orderChannelProduct.OrderChannelId,orderChannelProduct.Reference,orderChannelProduct.AutomaticOrder);
         _orderChannelRepository.Commit();
         
         return Json(Mapper.Map<OrderChannelProduct, ProductChannelsModel>(result));
      }

      [HttpPost]
      public ActionResult RemoveProductChannel(OrderChannelProduct orderChannelProduct)
      {
         _orderChannelRepository.UnassignProductOrderChannel(orderChannelProduct.ProductId, orderChannelProduct.OrderChannelId);
         _orderChannelRepository.Commit();
         return new HttpStatusCodeResult(HttpStatusCode.OK);
      }

      [HttpPost]
      public JsonResult GetChannelsForProduct([DataSourceRequest] DataSourceRequest request, int productId)
      {
         var channels = _orderChannelRepository.GetChannelsForProduct(productId);
         var data = channels.Select(Mapper.Map<OrderChannelProduct, ProductChannelsModel>);
         
         return Json(data.ToDataSourceResult(request));
      }

      private IEnumerable<OrderChannelModel> GetOrderChannels()
      {
         var channels = _orderChannelRepository.GetAvailableChannels();
         return channels.Select(Mapper.Map<OrderChannel, OrderChannelModel>);
      }

      [HttpGet]
      public JsonResult GetAvailableChannels()
      {
         var data = GetOrderChannels();
         return Json(data, JsonRequestBehavior.AllowGet);
      }
   }
}