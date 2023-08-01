using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using DevExpress.XtraPrinting.Native;
using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
    public abstract class MediatedController : ApiController
    {
       private IMediator _mediator;

       protected IMediator Mediator => _mediator ?? (_mediator = DependencyResolver.Current.GetService<IMediator>());
   }
}
