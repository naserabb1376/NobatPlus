using System.ComponentModel.DataAnnotations;
namespace NobatPlusAPI.Tools
{
    public class ConditionalRegularExpressionAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        private readonly string _pattern;

        public ConditionalRegularExpressionAttribute(string comparisonProperty, string pattern)
        {
            _comparisonProperty = comparisonProperty;
            _pattern = pattern;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
                throw new ArgumentException("Property with this name not found");

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);

            if (comparisonValue is int intValue && intValue == 1)
            {
                if (value is string stringValue && !System.Text.RegularExpressions.Regex.IsMatch(stringValue, _pattern))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
