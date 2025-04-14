using System;
using System.Text.Json;
using System.Data.SqlClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TicketHub.Functions
{
    public class ProcessTicketPurchase
    {
        private readonly ILogger _logger;

        public ProcessTicketPurchase(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessTicketPurchase>();
        }

        [Function("ProcessTicketPurchase")]
        public void Run(
            [QueueTrigger("process-ticket-purchase", Connection = "AzureStorageConnectionString")] string queueItem)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {queueItem}");

            try
            {
                // 1. Deserialize the message from JSON
                var decodedMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(queueItem));
                var ticketPurchase = JsonSerializer.Deserialize<TicketPurchase>(decodedMessage);

                if (ticketPurchase == null)
                {
                    _logger.LogError("Failed to deserialize the ticket purchase data");
                    return;
                }

                // 2. Generate a unique order reference
                string orderReference = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{ticketPurchase.ConcertId}-{Guid.NewGuid().ToString().Substring(0, 8)}";

                // 3. Insert the data into the SQL database
                SaveToDatabase(ticketPurchase, orderReference);

                _logger.LogInformation($"Successfully processed ticket purchase for {ticketPurchase.Email}, Order Reference: {orderReference}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing ticket purchase: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            }
        }

        private void SaveToDatabase(TicketPurchase purchase, string orderReference)
        {
            // Get the connection string from app settings
            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        INSERT INTO TicketPurchases (
                            ConcertId, Email, Name, Phone, Quantity, 
                            CreditCard, Expiration, SecurityCode, 
                            Address, City, Province, PostalCode, Country, 
                            OrderReference, ProcessStatus)
                        VALUES (
                            @ConcertId, @Email, @Name, @Phone, @Quantity, 
                            @CreditCard, @Expiration, @SecurityCode, 
                            @Address, @City, @Province, @PostalCode, @Country, 
                            @OrderReference, 'Completed')";

                    command.Parameters.AddWithValue("@ConcertId", purchase.ConcertId);
                    command.Parameters.AddWithValue("@Email", purchase.Email);
                    command.Parameters.AddWithValue("@Name", purchase.Name);
                    command.Parameters.AddWithValue("@Phone", purchase.Phone);
                    command.Parameters.AddWithValue("@Quantity", purchase.Quantity);
                    command.Parameters.AddWithValue("@CreditCard", purchase.CreditCard);
                    command.Parameters.AddWithValue("@Expiration", purchase.Expiration);
                    command.Parameters.AddWithValue("@SecurityCode", purchase.SecurityCode);
                    command.Parameters.AddWithValue("@Address", purchase.Address);
                    command.Parameters.AddWithValue("@City", purchase.City);
                    command.Parameters.AddWithValue("@Province", purchase.Province);
                    command.Parameters.AddWithValue("@PostalCode", purchase.PostalCode);
                    command.Parameters.AddWithValue("@Country", purchase.Country);
                    command.Parameters.AddWithValue("@OrderReference", orderReference);

                    int rowsAffected = command.ExecuteNonQuery();
                    _logger.LogInformation($"Inserted {rowsAffected} row(s) into database");
                }
            }
        }
    }

    // Model class matching the JSON structure coming from the queue
    public class TicketPurchase
    {
        public int ConcertId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string CreditCard { get; set; } = string.Empty;
        public string Expiration { get; set; } = string.Empty;
        public string SecurityCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}