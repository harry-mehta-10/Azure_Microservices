using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Azure_Microservices.ValidationAttributes
{
    /// <summary>
    /// Validates that a credit card security code (CVV/CVC) is 3-4 digits
    /// </summary>
    public class CreditCardSecurityCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Security code is required.");
            }

            string securityCode = value.ToString().Trim();

            // CVV/CVC should be 3-4 digits
            Regex regex = new Regex(@"^[0-9]{3,4}$");

            if (!regex.IsMatch(securityCode))
            {
                return new ValidationResult(ErrorMessage ?? "Security code must be 3 or 4 digits.");
            }

            return ValidationResult.Success;
        }
    }
}
