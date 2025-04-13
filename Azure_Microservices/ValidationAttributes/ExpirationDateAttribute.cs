using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Azure_Microservices.ValidationAttributes
{
    /// <summary>
    /// Validates that a credit card expiration date is in the correct format (MM/YY) and not expired
    /// </summary>
    public class ExpirationDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Expiration date is required.");
            }

            string expirationDate = value.ToString().Trim();

            // MM/YY format validation
            Regex regex = new Regex(@"^(0[1-9]|1[0-2])\/([0-9]{2})$");
            if (!regex.IsMatch(expirationDate))
            {
                return new ValidationResult(ErrorMessage ?? "Expiration date must be in MM/YY format.");
            }

            // Check if the card is not expired
            string[] parts = expirationDate.Split('/');
            int month = int.Parse(parts[0]);
            int year = 2000 + int.Parse(parts[1]); // Assuming 20xx

            DateTime expirationDateTime = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            if (expirationDateTime < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "The credit card has expired.");
            }

            return ValidationResult.Success;
        }
    }
}