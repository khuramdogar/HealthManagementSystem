using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ProductUpdates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   public class ProductUpdatesController : ApiController
   {
      private readonly IProductRepository _productRepository;

      public ProductUpdatesController(IProductRepository productRepository)
      {
         _productRepository = productRepository;
      }
      
      [HttpGet]
      public JsonResult<IEnumerable<ProductUpdate>> GetProducts(DateTime? since)
      {
         var products = _productRepository.FindAll();
         if (since.HasValue)
            products = products.Where(p => !p.LastModifiedOn.HasValue || p.LastModifiedOn.Value >= since);
         
         return Json(products.Select(p => Mapper.Map<Product, ProductUpdate>(p)).AsEnumerable());
      }
   }
}
