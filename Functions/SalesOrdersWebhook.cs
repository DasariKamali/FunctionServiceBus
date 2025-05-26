using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using FunctionApp66.Models;

namespace FunctionApp66.Functions
{

    public class SalesOrdersWebhook
    {
        private readonly ILogger _logger;

        public SalesOrdersWebhook(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SalesOrdersWebhook>();
        }

        [Function("SalesOrdersWebhook")]
        public async Task<OrdersOutput> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var salesOrders = JsonSerializer.Deserialize<List<SalesOrder>>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var response = req.CreateResponse();

            if (salesOrders == null || salesOrders.Count == 0)
            {
                _logger.LogWarning("No sales orders found in request.");
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                await response.WriteStringAsync("No sales orders provided.");
                return new OrdersOutput { HttpResponse = response };
            }

            _logger.LogInformation($"Enqueued {salesOrders.Count} sales order(s).");

            response.StatusCode = System.Net.HttpStatusCode.OK;
            await response.WriteStringAsync($"Enqueued {salesOrders.Count} sales order(s).");

            return new OrdersOutput
            {
                HttpResponse = response,
                QueueOutput = requestBody
            };
        }

        public class OrdersOutput
        {
            [QueueOutput("kamqueue", Connection = "AzureWebJobsStorage")]
            public string? QueueOutput { get; set; }

            public HttpResponseData? HttpResponse { get; set; }
        }
    }
}