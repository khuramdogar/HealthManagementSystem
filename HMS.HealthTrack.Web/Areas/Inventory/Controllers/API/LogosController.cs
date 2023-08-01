using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class LogosController : ApiController
   {
      private readonly IStockLocationRepository _stockLocationRepository;
      private readonly ICustomLogger _logger;

      public LogosController(IStockLocationRepository stockLocationRepository, ICustomLogger logger)
      {
         _stockLocationRepository = stockLocationRepository;
         _logger = logger;
      }

      [HttpPost]
      public async Task<IHttpActionResult> Post(string id)
      {
         try
         {
            int locationId;
            if (!int.TryParse(id, out locationId) || !Request.Content.IsMimeMultipartContent())
            {
               _logger.Warning("Invalid request received for uploading Logo for {LogoId}", id);
               throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Request."));
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            foreach (var file in provider.Contents)
            {
               var readTask = file.ReadAsByteArrayAsync();
               var location = _stockLocationRepository.Find(locationId);
               if (location != null)
               {
                  location.LogoImage = readTask.Result;
               }
               _stockLocationRepository.Commit();
               return Ok();
            }
            throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Request."));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem uploading a logo for the Location with ID {LocationId}", id);
            return InternalServerError(exception);
         }
      }

      [HttpGet]
      public HttpResponseMessage Get(string id)
      {
         try
         {
            int locationId;
            if (!int.TryParse(id, out locationId))
            {
               _logger.Warning("Attepting to retrieve logo which does not exist {LogoId}", id);
               return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            }

            var location = _stockLocationRepository.Find(locationId);
            if (location == null)
            {
               _logger.Warning("Attepting to retrieve logo which does not exist {LogoId}", id);
               return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (location.LogoImage == null)
            {
               _logger.Warning("Attepting to retrieve logo which does not exist {LogoId}", id);
               return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
               Content = new StreamContent(new MemoryStream(location.LogoImage.ToArray()))
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            return response;
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem retrieving the logo for the Location {LocationId}", id);
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
         }
      }
   }
}
