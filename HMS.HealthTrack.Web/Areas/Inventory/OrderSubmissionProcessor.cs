using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Hangfire;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Inventory.OrderingIntegration;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory
{
   [Queue(BackgroundQueues.Web)]
   public class OrderSubmissionProcessor
   {
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;
      private readonly IOrderSubmissionUnitOfWork _unitOfWork;
      private readonly ICollection<IOrderChannelSubmitter> _orderSubmissionProcessors;
      private readonly IOrderRepository _orderRepository;
      private readonly IOrderSubmissionRepository _orderSubmissionRepository;

      public OrderSubmissionProcessor(ICustomLogger logger, IPropertyProvider propertyProvider, IOrderSubmissionUnitOfWork unitOfWork)
      {
         _logger = logger;
         _propertyProvider = propertyProvider;
         _unitOfWork = unitOfWork;
         _orderRepository = _unitOfWork.OrderRepo;
         _orderSubmissionRepository = _unitOfWork.SubmissionRepo;

         var fmisOrderProcessor =
            new OracleOrderProcessor(_orderRepository,
            _unitOfWork.ConfigRepo,
            _logger,
            _unitOfWork.ProductRepo,
            _unitOfWork.OrderChannelRepo);
         _orderSubmissionProcessors = new Collection<IOrderChannelSubmitter>{fmisOrderProcessor};
      }

      [Queue(BackgroundQueues.Web)]
      public void ProcessOrders()
      {
         //Submission Status updates
         var ordersWithUnknownSubmissionStatus = _orderRepository.FindAll().Where(o => o.OrderSubmissionStatus == OrderSubmissionStatus.Unknown);
         PrepareUnknownOrders(ordersWithUnknownSubmissionStatus);
         _orderRepository.Commit();

         //Submit awaiting orders
         var ordersForSubmission = _orderRepository.FindAll().Include(o => o.Items.Select(i => i.Product.OrderChannelProducts))
            .Where(o => o.OrderSubmissionStatus == OrderSubmissionStatus.AwaitingSubmission);
         SubmitOrders(ordersForSubmission);
      }
      
      internal void PrepareUnknownOrders(IQueryable<Order> ordersToUpdate)
      {
         var ordersRequiringUpdate = ordersToUpdate.Where(o => o.OrderSubmissionStatus == OrderSubmissionStatus.Unknown).Include(o=>o.Items);

         foreach (var order in ordersRequiringUpdate)
         {
            order.OrderSubmissionStatus = 
               order.Items.Any() && order.Items.Any(i => i.Product.OrderChannelProducts.Any(oc=>oc.AutomaticOrder)) ?
            OrderSubmissionStatus.AwaitingSubmission //Has items with order channels
            : OrderSubmissionStatus.NotApplicable; //No items or order channels
         }
      }

      internal void SubmitOrders(IQueryable<Order> orders)
      {
         foreach (var order in orders)
         {
            try
            {
               var result = SubmitOrder(order);
               
               //Add submission result
               if (result.SubmissionStatus == SubmissionStatus.Error)
               {
                  _unitOfWork.SubmissionRepo.Add(result); //end of the line for this submission
                  _unitOfWork.Commit();
               }
            
               //update order
               order.OrderSubmissionStatus = OrderSubmissionStatus.Submitted;
               _unitOfWork.Commit();
               _logger.Information("Order {OrderId} submitted");
            }
            catch (Exception exception)
            {
               _logger.Error(exception, "Failed to process order {OrderId}", order.InventoryOrderId);

               //Save fail details
               var failedSubmission = new Submission
               {
                  AdditionalDetails = exception.Message,
                  OrderId = order.InventoryOrderId,
                  SubmissionStatus = SubmissionStatus.Error,
                  TimeStamp = DateTime.Now,
               };
               _orderSubmissionRepository.Add(failedSubmission);
               order.OrderSubmissionStatus = OrderSubmissionStatus.InError;
               _unitOfWork.Commit();
            }
         }
      }

      public Submission SubmitOrder(int orderId)
      {
         var order = _orderRepository.FindAll().Include(o => o.Items.Select(i => i.Product.OrderChannelProducts)).Where(o => o.InventoryOrderId == orderId);
         return SubmitOrder(order.SingleOrDefault());
      }

      internal Submission SubmitOrder(Order order)
      {
         var submission = new Submission {OrderId = order.InventoryOrderId, SubmissionStatus = SubmissionStatus.Error, TimeStamp = DateTime.Now};

         if (!_orderSubmissionProcessors.Any())
            submission.AdditionalDetails = "No processors available";

         foreach (var orderSubmissionProcessor in _orderSubmissionProcessors)
         {
            if (order.Items.Any(i => i.Product.OrderChannelProducts.Any(c => c.OrderChannel.Name == orderSubmissionProcessor.ChannelName && c.AutomaticOrder)))
            {
               var task = orderSubmissionProcessor.SendOrder(order.InventoryOrderId);
               _logger.Information("Order {OrderId} queued for submission on channel {ChannelName}", order.InventoryOrderId, orderSubmissionProcessor.ChannelName);
               submission.SubmissionStatus = SubmissionStatus.Queued;
               return submission;
            }
         }
         
         return submission;
      }
   }
}