using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.QueryHandlers.Products
{
   public class ProductSearchQueryHandler : IRequestHandler<ProductSearchQuery, ProductSearchResultDto>
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;
      private readonly IProductUnitOfWork _unitOfWork;
      public ProductSearchQueryHandler(ICustomLogger logger, IProductUnitOfWork unitOfWork,IDbContextInventoryContext context)
      {
         _logger = logger;
         _unitOfWork = unitOfWork;
         _context = context;
      }

      public async Task<ProductSearchResultDto> Handle(ProductSearchQuery request, CancellationToken cancellationToken)
      {
         if (request == null) return new ProductSearchResultDto {ErrorMessage = "No search parameters found"};
         ScanCodeResult codeResult = null;

         if (!string.IsNullOrWhiteSpace(request.ScannedCode))
            try
            {
               codeResult = ScanCodeHelper.TryParseGs1(request.ScannedCode, _logger) ?? ScanCodeHelper.TryParseHibc(request.ScannedCode, _logger);
               if (codeResult == null) return new ProductSearchResultDto { ErrorMessage = "Unable to interpret code provided." };
            }
            catch (Exception exception)
            {
               _logger.Error(exception, "An error occurred while trying to parse the code {Code} ", request.ScannedCode);
               return new ProductSearchResultDto {ErrorMessage = $"An error occurred while trying to parse the code {request.ScannedCode}"};
            }

         var productsQuery = codeResult != null
            ? _unitOfWork.ProductRepository.FindByCode(new ProductSearchCriteria {UPC = codeResult.UPN, SPC = codeResult.SPC})
            : _context.Products;
         
         if (!string.IsNullOrWhiteSpace(request.SPC))
            productsQuery = productsQuery.Where(p => p.SPC.Contains(request.SPC));

         if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            productsQuery = productsQuery.Where(p => p.Description.Contains(request.SearchTerm));

         if (!string.IsNullOrWhiteSpace(request.UPC))
            productsQuery = productsQuery.Where(p => p.ScanCodes.Any(upc => upc.Value.Contains(request.SPC)));

         var results = productsQuery.Include(p=>p.Prices.Select(pp=>pp.PriceType.PaymentClassMappings)).Include(p=>p.ScanCodes)
            .Select(HealthTrackProductExtension.Projection);

         return new ProductSearchResultDto {Data = results};
      }
   }
}