using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Azure_Microservices.ValidationAttributes
{
    /// <summary>
    /// Validates that a string is in the format of a Canadian postal code (A1A 1A1)
    /// </summary>
    public class CanadianPostalCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Postal code is required.");
            }

            string postalCode = value.ToString().Trim();

            // Canadian postal code pattern: A1A 1A1 or A1A1A1
            Regex regex = new Regex(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$");

            if (!regex.IsMatch(postalCode))
            {
                return new ValidationResult(ErrorMessage ?? "Invalid Canadian postal code format. Must be in format A1A 1A1.");
            }

            return ValidationResult.Success;
        }
    }
}