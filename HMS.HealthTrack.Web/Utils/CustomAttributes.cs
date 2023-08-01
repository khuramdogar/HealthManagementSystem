using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Utils
{
   [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
   public class NumericStringAttribute : ValidationAttribute, IClientValidatable
   {
      protected override ValidationResult IsValid(object value, ValidationContext validationContext)
      {
         if (value == null)
         {
            return null;
         }
         int result;
         //return int.TryParse(value.ToString(), out result) ? null : new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
         return int.TryParse(value.ToString(), out result) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
      }

      public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
      {
         var rule = new ModelClientValidationRule
         {
            ErrorMessage = ErrorMessage,
            ValidationType = "numericstring"
         };
         yield return rule;
      }
   }

   [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
   public class DecimalStringAttribute : ValidationAttribute, IClientValidatable
   {
      protected override ValidationResult IsValid(object value, ValidationContext validationContext)
      {
         if (value == null)
         {
            return null;
         }
         decimal result;
         return decimal.TryParse(value.ToString(), out result) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
      }

      public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
      {
         var rule = new ModelClientValidationRule
         {
            ErrorMessage = ErrorMessage,
            ValidationType = "decimalstring"
         };
         yield return rule;
      }
   }

   [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
   public class BoolStringAttribute : ValidationAttribute, IClientValidatable
   {
      protected override ValidationResult IsValid(object value, ValidationContext validationContext)
      {
         if (value == null)
            return null;

         var valid =
            ProductImport.ProductImportValidationRules.BooleanTextValues.ContainsKey(value.ToString().ToLower().Trim());

         return valid ? ValidationResult.Success : new ValidationResult(ErrorMessage);

      }

      public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
      {
         var rule = new ModelClientValidationRule
         {
            ErrorMessage = ErrorMessage,
            ValidationType = "boolstring"
         };
         yield return rule;
      }
   }

   public class IgnoreAttribute : Attribute
   {

   }

   // http://stackoverflow.com/a/15975880
   [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
   public class RequiredIfAttribute : ValidationAttribute, IClientValidatable
   {
      private String PropertyName { get; set; }
      private Object DesiredValue { get; set; }
      private readonly RequiredAttribute _innerAttribute;

      public RequiredIfAttribute(String propertyName, Object desiredvalue)
      {
         PropertyName = propertyName;
         DesiredValue = desiredvalue;
         _innerAttribute = new RequiredAttribute();
      }

      protected override ValidationResult IsValid(object value, ValidationContext context)
      {
         var dependentValue = context.ObjectInstance.GetType().GetProperty(PropertyName).GetValue(context.ObjectInstance, null);

         if ((dependentValue == null && DesiredValue == null) ||
            (dependentValue != null && DesiredValue != null && dependentValue.ToString() == DesiredValue.ToString()))
         {
            if (!_innerAttribute.IsValid(value))
            {
               return new ValidationResult(FormatErrorMessage(context.DisplayName), new[] { context.MemberName });
            }
         }

         return ValidationResult.Success;
      }

      public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
      {
         var rule = new ModelClientValidationRule
         {
            ErrorMessage = ErrorMessageString,
            ValidationType = "requiredif",
         };
         rule.ValidationParameters["dependentproperty"] = (context as ViewContext).ViewData.TemplateInfo.GetFullHtmlFieldId(PropertyName);
         rule.ValidationParameters["desiredvalue"] = DesiredValue is bool ? DesiredValue.ToString().ToLower() : DesiredValue;

         yield return rule;
      }
   }


}