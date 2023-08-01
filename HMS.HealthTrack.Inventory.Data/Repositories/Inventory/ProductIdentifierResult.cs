namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class ProductIdentifierResult
   {
      public ProductIdentifierResult(string value, ProductIdentifierType identifierType)
      {
         Value = value;
         IdentifierType = identifierType;
      }

      public string Value { get; set; }
      public ProductIdentifierType IdentifierType { get; set; }
   }

   public enum ProductIdentifierType
   {
      None,
      OrderChannel,
      SPC,
      UPN
   }
}