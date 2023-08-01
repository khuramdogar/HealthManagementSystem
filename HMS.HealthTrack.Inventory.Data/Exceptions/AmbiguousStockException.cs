namespace HMS.HealthTrack.Web.Data.Exceptions
{
   public class AmbiguousStockException : StockException
   {
      public AmbiguousStockException(string message, params object[] args)
         : base(message, args)
      {
      }
   }
}