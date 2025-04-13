using System.ComponentModel.DataAnnotations;
using Azure_Microservices.ValidationAttributes;

namespace Azure_Microservices.Models
{
    public class TicketPurchase
    {
        [Required(ErrorMessage = "Concert ID is required.")]
        public int ConcertId { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [PhoneNumber(ErrorMessage = "Please enter a valid phone number format like (555) 555-5555 or 555-555-5555.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ticket quantity is required.")]
        [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10 tickets.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Credit card number is required.")]
        [CreditCard(ErrorMessage = "Please enter a valid credit card number.")]
        public string CreditCard { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiration date is required.")]
        [ExpirationDate(ErrorMessage = "Please enter a valid expiration date in MM/YY format that is not expired.")]
        public string Expiration { get; set; } = string.Empty;

        [Required(ErrorMessage = "Security code is required.")]
        [CreditCardSecurityCode(ErrorMessage = "Security code should be 3 or 4 digits.")]
        public string SecurityCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province/State is required.")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required.")]
        [CanadianPostalCode(ErrorMessage = "Invalid Canadian postal code format. Must be in format A1A 1A1.")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        public string Country { get; set; } = string.Empty;
    }
}