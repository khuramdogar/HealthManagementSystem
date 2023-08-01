using System;
using System.Web.Http;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Security;
using HMS.HealthTrack.Web.Data.Repositories.Security;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class PermissionsController : ApiController
   {
      private readonly ISecurityUnitOfWork _securityUnitOfWork;
      private readonly ICustomLogger _logger;

      public PermissionsController(ISecurityUnitOfWork securityUnitOfWork, ICustomLogger logger)
      {
         _securityUnitOfWork = securityUnitOfWork;
         _logger = logger;
      }
      
      // GET api/ProductMappings
      [HttpPost]
      public IHttpActionResult UserPermissions(HealthTrackAuthorisation authorisation)
      {
         try
         {
            _securityUnitOfWork.GrantUserPermission(authorisation);
            _securityUnitOfWork.Commit();
            return Ok();
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("Failed to grant permissions for user '{0}' and keyword '{1}'", authorisation.User_ID, authorisation.Keyword));
            return BadRequest("Failed to grant user a permissions " + exception);
         }
      }

      // GET api/ProductMappings
      [HttpDelete]
      public IHttpActionResult Delete(HealthTrackAuthorisation authorisation)
      {
         try
         {
            _securityUnitOfWork.RemovePermission(authorisation);
            _securityUnitOfWork.Commit();
            return Ok();
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("Failed to deny a user a permission for the user '{0}' and keyword '{1}'", authorisation.User_ID, authorisation.Keyword));
            return BadRequest("Failed to deny a user a permission " + exception);
         }
      }
   }
}
