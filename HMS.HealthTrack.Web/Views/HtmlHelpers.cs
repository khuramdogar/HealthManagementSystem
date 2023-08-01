using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;

namespace HMS.HealthTrack.Web.Views
{
   public static class HtmlHelpers
   {

      public static IHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
      {
         var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
         var description = metadata.Description;
         return new HtmlString(description);
      }

      public static IHtmlString Separator(this HtmlHelper html)
      {
         return Separator(html, string.Empty);
      }

      public static IHtmlString Separator(this HtmlHelper html, string classes)
      {
         var classesToUse = !string.IsNullOrWhiteSpace(classes) ? string.Format(" {0}", classes) : null;
         return new HtmlString(string.Format("<span class='separator{0}'>&nbsp;</span>", classesToUse));
      }

      //http://stackoverflow.com/questions/9030763/how-to-make-html-displayfor-display-line-breaks
      public static MvcHtmlString DisplayWithBreaksFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
      {
         var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
         var model = html.Encode(metadata.Model).Replace("\r\n", "<br />\r\n");

         if (String.IsNullOrEmpty(model))
            return MvcHtmlString.Empty;

         return MvcHtmlString.Create(model);
      }

      public static MvcHtmlString MergeProductTableRow<TModel, TValue>(this HtmlHelper<TModel> html,
         Expression<Func<TModel, TValue>> expression, Expression<Func<TModel, TValue>> secondExpression)
      {
         var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
         var secondMetadata = ModelMetadata.FromLambdaExpression(secondExpression, html.ViewData);
         var identifier = string.Format("{0}{1}", InventoryConstants.MergeControlPrefix, metadata.PropertyName);
         return MvcHtmlString.Create(GetTableRow(metadata.DisplayName, metadata.PropertyName, identifier, metadata.Model,
            secondMetadata.Model));
      }

      private static string GetTableRow(string label, string propertyName, string identifier, object model,
         object otherModel)
      {
         return "<tr class='noselect'>" +
                                          "<td class='text-right'>" +
                                             "<label>" + label + "</label>" +
                                          "</td>" +
                                          "<td id='" + propertyName + "' class='productText'>" +
                                             model +
                                          "</td>" +
                                          "<td>" +
                                              "<span class='glyphicon glyphicon-arrow-left'></span>" +
                                                   "<span onclick='onCheckboxClick(this)' class='checkboxSpan'>" +
                                                      "<input class='mergeCheckbox' id='" + identifier + "' name='" + identifier + "' value='true' type='checkbox'/>" +
                                                      "<input name='" + identifier + "' value='false' type='hidden'/>" +
                                                   "</span>" +
                                          "</td>" +
                                          "<td>" +
                                             otherModel +
                                          "</td>" +
                                     "</tr>";
      }

      public static MvcHtmlString MergeProductPriceTableRows<TModel, TValue>(this HtmlHelper<TModel> html,
         Expression<Func<TModel, TValue>> expression, Expression<Func<TModel, TValue>> secondExpression)
      {
         var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
         var secondMetadata = ModelMetadata.FromLambdaExpression(secondExpression, html.ViewData);
         var pricesModel = metadata.Model as IEnumerable<ProductPriceViewModel>;
         var secondPricesModel = secondMetadata.Model as IList<ProductPriceViewModel>;

         if (pricesModel == null || secondPricesModel == null)
         {
            return new MvcHtmlString("<tr><td></td>" +
                                     "<td> Unable to display prices </td>" +
                                     "<td></td><td></td>" +
                                     "</tr>");
         }

         var prices = pricesModel.ToList();
         var pricesStringBuilder = new StringBuilder();

         var priceType = new ProductPriceViewModel().GetType();

         var buyPricePropertyName = Nameof<ProductPriceViewModel>.Property(p => p.BuyPrice);
         var buyPriceDisplayName = PropertyHelper.GetPropertyDisplayName(priceType.GetProperty(buyPricePropertyName));

         var buyCurrencyPropertyName = Nameof<ProductPriceViewModel>.Property(p => p.BuyCurrency);
         var buyCurrencyDisplayName = PropertyHelper.GetPropertyDisplayName(priceType.GetProperty(buyCurrencyPropertyName));

         var buyCurrencyRatePropertyName = Nameof<ProductPriceViewModel>.Property(p => p.BuyCurrencyRate);
         var buyCurrencyRateDisplayName = PropertyHelper.GetPropertyDisplayName(priceType.GetProperty(buyCurrencyRatePropertyName));

         var sellPricePropertyName = Nameof<ProductPriceViewModel>.Property(p => p.SellPrice);
         var sellPriceDisplayName = PropertyHelper.GetPropertyDisplayName(priceType.GetProperty(sellPricePropertyName));

         foreach (var priceTypeId in prices.Select(p => p.PriceTypeId))
         {
            var price = prices.Single(p => p.PriceTypeId == priceTypeId);
            var secondPrice = secondPricesModel.SingleOrDefault(p => p.PriceTypeId == priceTypeId);

            // buy price row
            var buyPriceName = string.Format("{0}_{1}", buyPricePropertyName, priceTypeId);

            pricesStringBuilder.Append(GetTableRow(string.Format("{0} {1}", price.PriceTypeName, buyPriceDisplayName), buyPriceName,
               GetPriceStringIdentifier(buyPriceName), price.BuyPrice.HasValue ? price.BuyPrice.Value.ToString("C") : string.Empty,
               secondPrice != null && secondPrice.BuyPrice.HasValue ? secondPrice.BuyPrice.Value.ToString("C") : null));

            // buy currency row
            var buyCurrencyName = string.Format("{0}_{1}", buyCurrencyPropertyName, priceTypeId);
            pricesStringBuilder.Append(GetTableRow(string.Format("{0} {1}", price.PriceTypeName, buyCurrencyDisplayName), buyCurrencyName,
               GetPriceStringIdentifier(buyCurrencyName), price.BuyCurrency,
               secondPrice != null ? secondPrice.BuyCurrency : null));

            // buy currency rate row
            var buyCurrencyRateName = string.Format("{0}_{1}", buyCurrencyRatePropertyName, priceTypeId);
            pricesStringBuilder.Append(GetTableRow(string.Format("{0} {1}", price.PriceTypeName, buyCurrencyRateDisplayName), buyCurrencyRateName,
               GetPriceStringIdentifier(buyCurrencyRateName), price.BuyCurrencyRate,
               secondPrice != null ? secondPrice.BuyCurrencyRate : null));

            // sell price row
            var sellPriceName = string.Format("{0}_{1}", sellPricePropertyName, priceTypeId);
            pricesStringBuilder.Append(GetTableRow(string.Format("{0} {1}", price.PriceTypeName, sellPriceDisplayName), sellPriceName,
               GetPriceStringIdentifier(sellPriceName), price.SellPrice.HasValue ? price.SellPrice.Value.ToString("C") : string.Empty,
               secondPrice != null && secondPrice.SellPrice.HasValue ? secondPrice.SellPrice.Value.ToString("C") : null));
         }
         return MvcHtmlString.Create(pricesStringBuilder.ToString());
      }

      private static string GetPriceStringIdentifier(string name)
      {
         return string.Format("{0}{1}{2}", InventoryConstants.MergeControlPrefix, InventoryConstants.MergePricePrefix, name);
      }
   }
}