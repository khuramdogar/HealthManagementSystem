using System;
using System.Data.SqlTypes;
using MediatR;
using FluentValidation;
using FluentValidation.Attributes;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Consumption
{
   [Validator(typeof(ConsumptionSubmissionValidator))]
   public class ConsumptionSubmission : IRequest<ConsumptionSubmissionResult>
   {
      public int ConsumptionReference { get; set; }
      public int Quantity { get; set; }
      public string SPC { get; set; }
      public string UPC { get; set; }
      public string ProductName { get; set; }
      public string Serial { get; set; }
      public string BatchNumber { get; set; }
      public DateTime ConsumedOn { get; set; }
      public int LocationId { get; set; }
      public string LocationName { get; set; }
      public string Consumer { get; set; }
      public string ApplicationId { get; set; }
      public string Description { get; set; }
      public string RebateCode { get; set; }
      public int? ProductId { get; set; }
   }

   public class ConsumptionSubmissionValidator : AbstractValidator<ConsumptionSubmission>
   {
      public ConsumptionSubmissionValidator()
      {
         RuleFor(submission => submission.ConsumptionReference).NotEmpty().WithMessage("A consumption reference is required");
         RuleFor(submission => submission.Quantity).NotEmpty().GreaterThan(0);
         RuleFor(submission => submission.ConsumedOn).NotEmpty().LessThan(DateTime.Now).WithMessage("Consumption date must be in the past").GreaterThan(SqlDateTime.MinValue.Value);
         RuleFor(submission => submission.ApplicationId).NotEmpty().WithMessage("Your application ID is required");
         RuleFor(submission => submission.LocationId).Must(BeAValidLocation).WithMessage("Please specify either a location id or location name");
      }
      
      public bool BeAValidLocation(ConsumptionSubmission submission, int locationId)
      {
         return submission.LocationId > 0 || !string.IsNullOrWhiteSpace(submission.LocationName);
      }
   }
}
