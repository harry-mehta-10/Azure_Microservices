using Azure.Storage.Queues;
using System.Text.Json;
using Azure_Microservices.Models;

namespace Azure_Microservices.Services
{
    public class QueueService : IQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly ILogger<QueueService> _logger;

        public QueueService(IConfiguration configuration, ILogger<QueueService> logger)
        {
            _logger = logger;

            // Gets the connection string from user secrets
            string? connectionString = configuration["AzureStorageConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Azure Storage connection string not found in configuration");
                throw new InvalidOperationException("Azure Storage connection string not configured");
            }

            // Creates the queue client
            _queueClient = new QueueClient(connectionString, "tickethub");
            _queueClient.CreateIfNotExists();
        }

        public async Task<bool> SendMessageAsync(TicketPurchase purchase)
        {
            try
            {
                // Serializes the ticket purchase to JSON
                var message = JsonSerializer.Serialize(purchase);

                // Sends the message to the queue
                await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message)));

                _logger.LogInformation($"Message for concert ID {purchase.ConcertId} sent to queue successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message to queue: {ex.Message}");
                return false;
            }
        }
    }

    public interface IQueueService
    {
        Task<bool> SendMessageAsync(TicketPurchase purchase);
    }
}