using System;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class StockRequestRepository : IStockRequestRepository
   {
      private readonly IDbContextInventoryContext _context;

      public StockRequestRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public ProductStockRequest Find(int id)
      {
         return (from s in _context.ProductStockRequests where s.StockRequestId == id select s).Include(s => s.Product).Include(s => s.OrderItemSources.Select(ois => ois.OrderItem.Order))
            .Include(s => s.Location).FirstOrDefault();
      }

      public IQueryable<ProductStockRequest> Find(int[] ids)
      {
         return
            _context.ProductStockRequests.Where(request => ids.Contains(request.StockRequestId))
               .Include(request => request.Product);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public void Update(ProductStockRequest stockRequest)
      {
         stockRequest.LastModifiedOn = DateTime.Now;
         _context.Entry(stockRequest).State = EntityState.Modified;
      }

      public IQueryable<ProductStockRequest> FindCurrentRequests()
      {
         return _context.ProductStockRequests.Where(psr => psr.RequestStatus == RequestStatus.Open || psr.RequestStatus == RequestStatus.Ordered || psr.RequestStatus == RequestStatus.Approved)
            .Include(psr => psr.Product.PrimarySupplierCompany);
      }

      public IQueryable<ProductStockRequest> FindOpenRequests()
      {
         return _context.ProductStockRequests.Where(psr => psr.RequestStatus == RequestStatus.Open);
      }

      public IQueryable<ProductStockRequest> FindApprovedRequests()
      {
         return _context.ProductStockRequests.Where(psr => psr.RequestStatus == RequestStatus.Approved);
      }

      public ProductStockRequest CreateRequest(int productId, int requestedQuantity, int? locationId, bool urgent, string user)
      {
         var request = new ProductStockRequest
         {
            ApprovedQuantity = requestedQuantity,
            CreatedBy = user,
            IsUrgent = urgent,
            LastModifiedBy = user,
            ProductId = productId,
            RequestLocationId = locationId,
            RequestedQuantity = requestedQuantity
         };
         Add(request);
         return request;
      }

      public void CloseRequests(OrderItem orderItem, string username, bool fulfilled)
      {
         var requests = orderItem.OrderItemSources.Select(ois => ois.ProductStockRequest);
         foreach (var request in requests.Where(r => r != null))
         {
            request.RequestStatus = RequestStatus.Closed;
            request.Fulfilled = fulfilled;
            request.LastModifiedBy = username;
            request.LastModifiedOn = DateTime.Now;
         }
      }

      public void Add(ProductStockRequest newEntity)
      {
         newEntity.LastModifiedOn = DateTime.Now;
         newEntity.RequestStatus = RequestStatus.Open;
         _context.ProductStockRequests.Add(newEntity);
      }

      public void Remove(ProductStockRequest stockRequest)
      {
         _context.ProductStockRequests.Remove(stockRequest);
      }
   }

   public interface IStockRequestRepository
   {
      ProductStockRequest Find(int id);
      void Commit();
      void Update(ProductStockRequest stockRequest);
      IQueryable<ProductStockRequest> FindCurrentRequests();
      IQueryable<ProductStockRequest> FindOpenRequests();
      void CloseRequests(OrderItem orderItem, string username, bool fulfilled);
      IQueryable<ProductStockRequest> Find(int[] ids);
      IQueryable<ProductStockRequest> FindApprovedRequests();
      ProductStockRequest CreateRequest(int productId, int requestedQuantity, int? locationId, bool urgent, string user);
      void Add(ProductStockRequest request);
      void Remove(ProductStockRequest stockRequest);
   }
}