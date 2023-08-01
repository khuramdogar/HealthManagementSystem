using AutoMapper;
using HealthTrack.InventoryScanner.ScanCodeParsers;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ScanCode;
using System;
using System.Collections.Generic;
using System.Globalization;
using HMS.HealthTrack.Inventory.Common;

namespace HMS.HealthTrack.Web.Areas.Inventory.Utils
{
   public static class ScanCodeHelper
   {
      private const string Gs1ExpiryDateFormat = "yyMMdd";
      private static readonly CultureInfo Provider = CultureInfo.InvariantCulture;

      /// <summary>
      /// Attmepts to parse the provided code using the HIBC parser.
      /// Returns null if unable to extract a code.
      /// </summary>
      /// <param name="code"></param>
      /// <param name="logger"></param>
      /// <returns></returns>
      public static ScanCodeResult TryParseHibc(string code, ICustomLogger logger)
      {
         if (string.IsNullOrWhiteSpace(code)) return null;
         if (!code.StartsWith("+") && !code.StartsWith("$")) return null;

         ScanCodeResult result = null;
         HibcResultCode parseResult;

         logger.Debug("Attempting to parse code {Code} using HIBC parser", code);
         try
         {
            var parser = new HibcParser();
            parseResult = parser.Parse(code);
            if (parseResult == HibcResultCode.Success)
            {
               result = Mapper.Map<HibcParser, ScanCodeResult>(parser);
               if (string.IsNullOrWhiteSpace(result.UPN))
                  result.UPN = parser.Barcode;
            }
         }
         catch (Exception exception)
         {
            logger.Error(exception, "An error occurred while trying to parse the code {Code} using the HIBC parser", code);
            throw;
         }

         if (result == null)
            logger.Warning("Unable to parse the code {Code} due to {HibcResultCodeReason}", code, GetHibcResultMessage(parseResult));

         return result;
      }

      /// <summary>
      /// Attmepts to parse the provided code using the GS1 parser.
      /// Returns null if unable to extract a code.
      /// </summary>
      /// <param name="code"></param>
      /// <param name="logger"></param>
      /// <returns></returns>
      public static ScanCodeResult TryParseGs1(string code, ICustomLogger logger)
      {
         if (string.IsNullOrWhiteSpace(code)) return null;
         ScanCodeResult result = null;

         logger.Debug("Attempting to parse code {Code} using GS1 parser", code);
         try
         {
            var parser = new GS1Parser();
            var parseResult = parser.Parse(code);
            if (parseResult.Count > 0)
            {
               result = GetGs1ScanCodeResult(parser, parseResult, logger);
            }
            else if (code.Length >= 8)
            {
               // "Attempt EAN 8, 9, 11, 13 parse."
               var validUpn = parser.ValidateEANChecksum(code);
               if (!string.IsNullOrWhiteSpace(validUpn))
               {
                  result = new ScanCodeResult
                  {
                     UPN = GetUpnFromValidatedEan(validUpn)
                  };
               }
            }
            else
            {
               logger.Debug("Unable to parse the code {Code} using the GS1 parser", code);
            }
         }
         catch (Exception exception)
         {
            logger.Error(exception, "An error occured while attempting to parse the code {Code} using the GS1 parser", code);
         }

         return result;
      }

      private static ScanCodeResult GetGs1ScanCodeResult(GS1Parser parser, Dictionary<string, string> applicationIdentifierValues, ICustomLogger logger)
      {
         if (applicationIdentifierValues == null || applicationIdentifierValues.Count < 1 || parser == null)
         {
            return null;
         }

         var result = new ScanCodeResult();

         if (applicationIdentifierValues.ContainsKey(Gs1ApplicationIdentifiers.GTIN))
         {
            // GTIN exists, now validate it
            //var gtin = applicationIdentifierValues[Gs1ApplicationIdentifiers.GTIN];
            //result.UPN = parser.ValidateGS1Checksum("01" + gtin.Substring(0, gtin.Length - 1)); // TODO: Reimplement once ValidateCheckSum is fixed
            result.UPN = applicationIdentifierValues[Gs1ApplicationIdentifiers.GTIN];
         }

         if (applicationIdentifierValues.ContainsKey(Gs1ApplicationIdentifiers.LotNumber))
            result.LotNumber = applicationIdentifierValues[Gs1ApplicationIdentifiers.LotNumber];

         if (applicationIdentifierValues.ContainsKey(Gs1ApplicationIdentifiers.SerialNumber))
            result.SerialNumber = applicationIdentifierValues[Gs1ApplicationIdentifiers.SerialNumber];

         if (applicationIdentifierValues.ContainsKey(Gs1ApplicationIdentifiers.ExpiryDate))
         {
            var dateString = applicationIdentifierValues[Gs1ApplicationIdentifiers.ExpiryDate];
            if (dateString.Length == Gs1ExpiryDateFormat.Length)
            {
               dateString = dateString.Remove(dateString.Length - 1, 1) + 1;
               // because sometimes the DD part of the string is "00"
            }
            else
            {
               logger.Error("Unable to parse Expiry Date {ExpiryDate} from GS1 code", dateString);
               throw new Exception();
            }

            var date = DateTime.ParseExact(dateString, Gs1ExpiryDateFormat, Provider);
            result.ExpirationDate = date;
         }

         if (applicationIdentifierValues.ContainsKey(Gs1ApplicationIdentifiers.AdditionalId))
            result.SPC = applicationIdentifierValues[Gs1ApplicationIdentifiers.AdditionalId];

         if (applicationIdentifierValues.ContainsKey(Gs1ApplicationIdentifiers.Count))
            result.Quantity = applicationIdentifierValues[Gs1ApplicationIdentifiers.Count];

         return result;
      }

      private static string GetHibcResultMessage(HibcResultCode result)
      {
         switch (result)
         {
            case HibcResultCode.EmptyCheckCharacter:
               return "Code does not contain a check character.";
            case HibcResultCode.EmptyLinkCharacter:
               return "Code does not contain a link character.";
            case HibcResultCode.InvalidBarcode:
               return "Barcode is invalid.";
            case HibcResultCode.InvalidExpirationDate:
               return "Expiration date is invalid.";
            case HibcResultCode.InvalidQuantity:
               return "Quantity is invalid.";
         }
         return "Code parsed successfully";
      }

      private static string GetUpnFromValidatedEan(string value)
      {
         return !string.IsNullOrWhiteSpace(value) ? value.Substring(0, value.Length - 1) : null;
      }
   }

   /// <summary>
   /// As sourced from http://www.databar-barcode.info/application-identifiers/
   /// </summary>
   public static class Gs1ApplicationIdentifiers
   {
      public const string GTIN = "01"; // Global Trade Item Number - (UPN?)
      public const string LotNumber = "10"; // batch/lot number
      public const string BestBefore = "15"; // Best before/ Sell by
      public const string ExpiryDate = "17";
      public const string SerialNumber = "21";
      public const string QtyDateBatch = "22"; // "Secondary data for specific health industry products"
      public const string AdditionalId = "240";
      public const string Count = "37"; // Count of trade items contained in a logistic unit
      public const string ExpiryDateTime = "7003";
   }
}