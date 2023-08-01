using System;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public enum StockTakeType
   {
      Standard = 0,
      [Obsolete] NewProduct = 1
   }
}