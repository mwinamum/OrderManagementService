using OrderManagementService.Dtos;
using OrderManagementService.Enum;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OrderManagementServiceTests.IntegrationTests
{
    [TestFixture]
    public class AnalyticsControllerTests
    {
        private HttpClient? _client;
        private CustomWebApplicationFactory? _factory;

        [SetUp]
        public void SetUp()
        {
            try
            {
                _factory = new CustomWebApplicationFactory();
                _client = _factory.CreateClient();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to initialize test environment. Ensure project configuration is correct.\n" +
                                  $"Error: {ex.Message}\n" +
                                  $"StackTrace: {ex.StackTrace}\n" +
                                  $"Inner Exception: {ex.InnerException?.Message ?? "None"}\n" +
                                  $"Inner StackTrace: {ex.InnerException?.StackTrace ?? "None"}";
                Assert.Inconclusive(errorMessage);
            }
        }

        [Test]
        public async Task GetAnalytics_WithOrders_ReturnsCorrectAnalytics()
        {
            var orderDto = new OrderCreateDto { TotalAmount = 100m, CustomerName = "John Doe", CustomerEmail = "john@example.com", ShippingAddress = "123 Main St" };
            var content = new StringContent(JsonSerializer.Serialize(orderDto), Encoding.UTF8, "application/json");
            await _client!.PostAsync("/api/orders?customerId=1", content);

            var statusDto = new OrderStatusUpdateDto { Status = OrderStatus.Delivered };
            var statusContent = new StringContent(JsonSerializer.Serialize(statusDto), Encoding.UTF8, "application/json");
            await _client!.PutAsync("/api/orders/1/status", statusContent);

            var response = await _client!.GetAsync("/api/analytics/summary");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var analytics = JsonSerializer.Deserialize<AnalyticsDto>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.That(analytics, Is.Not.Null);
            Assert.That(analytics.AverageOrderValue, Is.EqualTo(85m)); // 100 - 15% discount
            Assert.That(analytics.AverageFulfillmentTime, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public async Task GetAnalytics_NoOrders_ReturnsZeroValues()
        {
            var response = await _client!.GetAsync("/api/analytics/summary");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var analytics = JsonSerializer.Deserialize<AnalyticsDto>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.That(analytics, Is.Not.Null);
            Assert.That(analytics.AverageOrderValue, Is.EqualTo(0m));
            Assert.That(analytics.AverageFulfillmentTime, Is.EqualTo(0.0));
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
            _client = null;
            _factory = null;
        }
    }
}