using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using FunctionApp66.Models;


namespace FunctionApp66.Functions
{

    public class ProcessSalesOrdersQueue
    {
        private readonly ILogger _logger;

        public ProcessSalesOrdersQueue(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessSalesOrdersQueue>();
        }

        [Function("ProcessSalesOrdersQueue")]
        public async Task<ServiceBusOrdersOutput> Run(
            [QueueTrigger("kamqueue", Connection = "AzureWebJobsStorage")] string queueItem)
        {
            var order = JsonSerializer.Deserialize<SalesOrder>(queueItem, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (order == null)
            {
                _logger.LogError("Failed to deserialize SalesOrder message.");
                return new ServiceBusOrdersOutput(); // empty list
            }

            _logger.LogInformation($"Published order {order.Id} to Service Bus.");

            return new ServiceBusOrdersOutput
            {
                Messages = new List<SalesOrder> { order }
            };
        }

        public class ServiceBusOrdersOutput
        {
            [ServiceBusOutput("kamtopic", Connection = "ServiceBusConnectionString")]
            public List<SalesOrder> Messages { get; set; } = new();
        }
    }

}