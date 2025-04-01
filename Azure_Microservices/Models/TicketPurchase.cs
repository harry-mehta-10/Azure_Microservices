using System.ComponentModel.DataAnnotations;

namespace Azure_Microservices.Models
{
    public class TicketPurchase
    {
        [Required]
        public int ConcertId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Phone]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
            ErrorMessage = "Please enter a valid phone number format like (555) 555-5555 or 555-555-5555.")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10 tickets")]
        public int Quantity { get; set; }

        [Required]
        [CreditCard]
        public string CreditCard { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Expiration should be in format MM/YY")]
        public string Expiration { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Security code should be 3 or 4 digits")]
        public string SecurityCode { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Province { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$", ErrorMessage = "Invalid Canadian postal code format")]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;
    }
}