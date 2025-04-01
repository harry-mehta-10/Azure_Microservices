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

        // POST api/tickets
        [HttpPost]
        public async Task<IActionResult> PurchaseTickets([FromBody] TicketPurchase purchase)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid ticket purchase request received");
                return BadRequest(new { message = "Invalid ticket purchase data", errors = ModelState });
            }

            // phone validation to ensure it catches all cases
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

            // custom validation for Canadian postal codes
            if (purchase.Country.ToLower() == "canada" && !IsValidCanadianPostalCode(purchase.PostalCode))
            {
                _logger.LogWarning("Invalid Canadian postal code format");
                return BadRequest(new { message = "Invalid postal code format for Canada" });
            }

            // Sends message to Azure Queue
            var result = await _queueService.SendMessageAsync(purchase);
            if (!result)
            {
                _logger.LogError("Failed to queue ticket purchase");
                return StatusCode(500, new { message = "Failed to process ticket purchase" });
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

        // GET api/tickets/health
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}