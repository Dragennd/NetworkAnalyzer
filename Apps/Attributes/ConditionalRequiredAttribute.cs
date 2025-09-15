using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NetworkAnalyzer.Apps.Attributes
{
    internal class ConditionalRequiredAttribute : ValidationAttribute
    {
        private readonly string _booleanPropertyName;

        public ConditionalRequiredAttribute(string booleanPropertyName)
        {
            _booleanPropertyName = booleanPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Use reflection to get the boolean property value from the validation context
            PropertyInfo propertyInfo = validationContext.ObjectType.GetProperty(_booleanPropertyName);
            if (propertyInfo == null)
            {
                return new ValidationResult($"Unknown property: {_booleanPropertyName}");
            }

            // Ensure the property is an instance property
            if (propertyInfo.GetMethod.IsStatic)
            {
                return new ValidationResult($"The property: {_booleanPropertyName} should not be static.");
            }

            bool isRequired = (bool)propertyInfo.GetValue(validationContext.ObjectInstance);

            if (isRequired)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
