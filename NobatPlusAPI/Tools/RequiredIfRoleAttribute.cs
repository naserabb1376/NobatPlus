using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Tools
{
    public class RequiredIfRoleAttribute : ValidationAttribute
    {
        private readonly string _roleProperty;
        private readonly int[] _requiredRoles;

        public RequiredIfRoleAttribute(string roleProperty, params int[] requiredRoles)
        {
            _roleProperty = roleProperty;
            _requiredRoles = requiredRoles;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var rolePropertyInfo = validationContext.ObjectType.GetProperty(_roleProperty);
            if (rolePropertyInfo == null)
                return new ValidationResult($"Property '{_roleProperty}' not found");

            var roleValue = rolePropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (roleValue == null)
                return ValidationResult.Success;

            if (int.TryParse(roleValue.ToString(), out int roleId))
            {
                if (Array.Exists(_requiredRoles, r => r == roleId))
                {
                    if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} الزامی است");
                }
            }

            return ValidationResult.Success;
        }
    }
}
