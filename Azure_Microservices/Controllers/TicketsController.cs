using Microsoft.AspNetCore.Mvc;
using Azure_Microservices.Models;
using Azure_Microservices.Services;
using System.Text.RegularExpressions;

namespace Azure_Microservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly IQueueService _queueService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(IQueueService queueService, ILogger<TicketsController> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseTickets([FromBody] TicketPurchase purchase)
        {
            // If the model state is invalid, return detailed validation errors
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid ticket purchase request received");
                
                var errors = ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                
                return BadRequest(new 
                { 
                    message = "Invalid ticket purchase data", 
                    errors = errors 
                });
            }

            // Additional phone validation to ensure it catches all cases
            var phonePattern = new Regex(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$");
            if (!phonePattern.IsMatch(purchase.Phone))
            {
                _logger.LogWarning("Invalid phone number format");
                return BadRequest(new
                {
                    message = "Invalid phone number format",
                    errors = new
                    {
                        Phone = new[] { "Please enter a valid phone number format like (555) 555-5555 or 555-555-5555." }
                    }
                });
            }

            // Custom validation for Canadian postal codes when country is Canada
            if (!string.IsNullOrEmpty(purchase.Country) && 
                purchase.Country.Trim().Equals("Canada", StringComparison.OrdinalIgnoreCase) && 
                !IsValidCanadianPostalCode(purchase.PostalCode))
            {
                _logger.LogWarning("Invalid Canadian postal code format");
                return BadRequest(new 
                { 
                    message = "Invalid postal code format for Canada",
                    errors = new
                    {
                        PostalCode = new[] { "Please enter a valid Canadian postal code format like A1A 1A1 or A1A1A1." }
                    }
                });
            }

            // Validate credit card expiration (check that it's not expired)
            if (!IsValidExpirationDate(purchase.Expiration))
            {
                _logger.LogWarning("Expired credit card");
                return BadRequest(new
                {
                    message = "Invalid credit card expiration",
                    errors = new
                    {
                        Expiration = new[] { "Credit card has expired. Please use a valid card." }
                    }
                });
            }

            // Sends message to Azure Queue
            var result = await _queueService.SendMessageAsync(purchase);
            if (!result)
            {
                _logger.LogError("Failed to queue ticket purchase");
                return StatusCode(500, new { message = "Failed to process ticket purchase. Please try again later." });
            }

            _logger.LogInformation($"Ticket purchase for concert ID {purchase.ConcertId} successfully processed");

            return Ok(new
            {
                message = "Your ticket purchase has been successfully processed! Thank you for your order.",
                ticketCount = purchase.Quantity,
                orderReference = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{purchase.ConcertId}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                estimatedProcessingTime = "5 minutes"
            });
        }

        private bool IsValidCanadianPostalCode(string postalCode)
        {
            // Canadian postal code format: A1A 1A1 or A1A1A1
            return Regex.IsMatch(
                postalCode,
                @"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$"
            );
        }

        private bool IsValidExpirationDate(string expiration)
        {
            if (!Regex.IsMatch(expiration, @"^(0[1-9]|1[0-2])\/([0-9]{2})$"))
                return false;

            var parts = expiration.Split('/');
            var month = int.Parse(parts[0]);
            var year = int.Parse(parts[1]) + 2000; // Convert YY to 20YY

            var expirationDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            
            return expirationDate >= DateTime.Today;
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
