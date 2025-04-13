using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Azure_Microservices.ValidationAttributes
{
    /// <summary>
    /// Validates that a phone number is in a valid North American format
    /// </summary>
    public class PhoneNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Phone number is required.");
            }

            string phoneNumber = value.ToString().Trim();

            // Check common North American formats: (555) 555-5555, 555-555-5555, 5555555555
            Regex regex = new Regex(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$");

            if (!regex.IsMatch(phoneNumber))
            {
                return new ValidationResult(ErrorMessage ?? "Invalid phone number format. Use formats like (555) 555-5555 or 555-555-5555.");
            }

            return ValidationResult.Success;
        }
    }
}
