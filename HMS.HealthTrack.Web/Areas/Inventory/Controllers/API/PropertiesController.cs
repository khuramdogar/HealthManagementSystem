using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class PropertiesController : ApiController
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;

      public PropertiesController(IDbContextInventoryContext context, ICustomLogger logger)
      {
         _context = context;
         _logger = logger;
      }

      // GET api/Products
      public DataSourceResult GetProperties([System.Web.Http.ModelBinding.ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request)
      {
         try
         {
            return _context.Properties.ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Stock Locations with the search term '{0}'."));
            return new DataSourceResult();
         }
      }

      // GET api/Products/5
      public Property GetProperty(int id)
      {
         try
         {
            var property = _context.Properties.Find(id);
            if (property == null)
               throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            return property;
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Property with ID '{0}'.", id));
            return null;
         }
      }

      // PUT api/Products/5
      public HttpResponseMessage PutProperty(int id, Property property)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != property.PropertyId)
            {
               return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            _context.Entry(property).State = EntityState.Modified;

            try
            {
               _context.ObjectContext.SaveChanges();
               SystemSettings.Refresh();
            }
            catch (DbUpdateConcurrencyException ex)
            {
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem updating the Property with ID '{0}'.", id));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      // POST api/Products
      public HttpResponseMessage PostProperty(Property property)
      {
         try
         {
            if (ModelState.IsValid)
            {
               _context.Properties.Add(property);
               _context.ObjectContext.SaveChanges();

               var result = new DataSourceResult
               {
                  Data = new[] { property },
                  Total = 1
               };
               HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, result);
               response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = property.PropertyId }));
               return response;
            }
            else
            {
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem creating a property with the name '{0}'.", property.PropertyName));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      // DELETE api/Products/5
      public HttpResponseMessage DeleteProperty(int id)
      {
         try
         {
            var product = _context.Properties.Find(id);
            if (product == null)
            {
               return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _context.Properties.Remove(product);

            try
            {
               _context.ObjectContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
               return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting the Property with ID '{0}'.", id));
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }

      protected override void Dispose(bool disposing)
      {
         _context.ObjectContext.SaveChanges();
         base.Dispose(disposing);
      }

   }
}