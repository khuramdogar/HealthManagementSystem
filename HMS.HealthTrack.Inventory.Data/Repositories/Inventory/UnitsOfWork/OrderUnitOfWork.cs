using System;
using HMS.HealthTrack.Web.Data.Model.Clinical;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Clinical;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class OrderUnitOfWork : IOrderUnitOfWork
   {
      private readonly IDbContextClinicalContext _clinicalContext;
      private readonly IDbContextInventoryContext _inventoryContext;
      private readonly IMedicalRecordRepository _medicalRecordRepository;
      private readonly IPropertyProvider _propertyProvider;
      private ICompanyRepository _companyRepository;
      private IGeneralLedgerRepository _generalLedgerRepository;
      private IGeneralLedgerTierRepository _generalLedgerTierRepository;
      private IOrderItemRepository _orderItemRepository;
      private IOrderRepository _orderRepository;
      private IProductRepository _productRepo;
      private IStockAdjustmentRepository _stockAdjustmentRepository;
      private IStockLocationRepository _stockLocationRepository;
      private IStockRequestRepository _stockRequestRepo;
      private IStockSetRepository _stockSetRepository;

      public OrderUnitOfWork(IDbContextInventoryContext inventoryContext, IDbContextClinicalContext clinicalContext,
         IPropertyProvider propertyProvider)
      {
         _inventoryContext = inventoryContext;
         _clinicalContext = clinicalContext;
         _propertyProvider = propertyProvider;
         _productRepo = new ProductRepository(_inventoryContext, propertyProvider);
         _orderRepository = new OrderRepository(_inventoryContext);
         _orderItemRepository = new OrderItemRepository(_inventoryContext, propertyProvider);
         _medicalRecordRepository = new MedicalRecordRepository(clinicalContext);
         _stockRequestRepo = new StockRequestRepository(inventoryContext);
         _stockSetRepository = new StockSetRepository(inventoryContext);
         _companyRepository = new CompanyRepository(inventoryContext);
         _stockAdjustmentRepository = new StockAdjustmentRepository(inventoryContext);
         _stockLocationRepository = new StockLocationRepository(inventoryContext);
         _generalLedgerRepository = new GeneralLedgerRepository(inventoryContext);
         _generalLedgerTierRepository = new GeneralLedgerTierRepository(inventoryContext);
      }

      public IProductRepository ProductRepo => _productRepo ?? (_productRepo = new ProductRepository(_inventoryContext, _propertyProvider));

      public IOrderRepository OrderRepository => _orderRepository ?? (_orderRepository = new OrderRepository(_inventoryContext));

      public IOrderItemRepository OrderItemRepository => _orderItemRepository ??
                                                         (_orderItemRepository = new OrderItemRepository(_inventoryContext, _propertyProvider));

      public IStockRequestRepository StockRequestRepository => _stockRequestRepo ?? (_stockRequestRepo = new StockRequestRepository(_inventoryContext));

      public IStockSetRepository StockSetRepository => _stockSetRepository ?? (_stockSetRepository = new StockSetRepository(_inventoryContext));

      public ICompanyRepository CompanyRepository => _companyRepository ?? (_companyRepository = new CompanyRepository(_inventoryContext));

      public IStockAdjustmentRepository StockConsumptionRepository => _stockAdjustmentRepository ??
                                                                      (_stockAdjustmentRepository = new StockAdjustmentRepository(_inventoryContext));

      public IStockLocationRepository StockLocationRepository => _stockLocationRepository ?? (_stockLocationRepository = new StockLocationRepository(_inventoryContext));

      public IGeneralLedgerRepository GeneralLedgerRepository => _generalLedgerRepository ??
                                                                 (_generalLedgerRepository = new GeneralLedgerRepository(_inventoryContext));

      public IGeneralLedgerTierRepository GeneralLedgerTierRepository => _generalLedgerTierRepository ??
                                                                         (_generalLedgerTierRepository = new GeneralLedgerTierRepository(_inventoryContext));

      public void Commit()
      {
         _inventoryContext.ObjectContext.SaveChanges();
      }

      public void ReverseOrder(Order order, string username)
      {
         foreach (var item in order.Items)
         {
            // set item to reversed
            item.Status = OrderItemStatus.Reversed;

            foreach (var orderItemSource in item.OrderItemSources)
               // disassociate requests from order
               if (orderItemSource.StockRequestId.HasValue)
               {
                  var request = orderItemSource.ProductStockRequest;
                  request.RequestStatus = RequestStatus.Approved;
                  request.Fulfilled = false;
                  request.LastModifiedBy = username;
                  request.LastModifiedOn = DateTime.Now;
               }
         }
      }
   }

   public interface IOrderUnitOfWork
   {
      IProductRepository ProductRepo { get; }
      IOrderRepository OrderRepository { get; }
      IOrderItemRepository OrderItemRepository { get; }
      IStockRequestRepository StockRequestRepository { get; }
      IStockSetRepository StockSetRepository { get; }
      ICompanyRepository CompanyRepository { get; }
      IStockAdjustmentRepository StockConsumptionRepository { get; }
      IStockLocationRepository StockLocationRepository { get; }
      IGeneralLedgerRepository GeneralLedgerRepository { get; }
      IGeneralLedgerTierRepository GeneralLedgerTierRepository { get; }
      void Commit();
      void ReverseOrder(Order order, string username);
   }
}