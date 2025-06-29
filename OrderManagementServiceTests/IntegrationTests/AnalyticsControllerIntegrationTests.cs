using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq; // For mocking IDiscountService and IDiscountRule
using OrderManagementService.Controllers;
using OrderManagementService.Data;
using OrderManagementService.Dtos;
using OrderManagementService.Enum;
using OrderManagementService.Interfaces;
using OrderManagementService.Models;
using OrderManagementService.Services;
using System.Net;
using System.Text.Json;

namespace OrderManagementServiceTests.IntegrationTests
{
    /// <summary>
    /// Integration tests for the AnalyticsController, verifying analytics summary endpoints
    /// using an in-memory SQLite database and mocked discount rules.
    /// </summary>
    [TestFixture]
    public class AnalyticsControllerIntegrationTests
    {
        private HttpClient? _client;
        private static TestServer? _server;
        private readonly SqliteConnection _connection;

        // Constants for hardcoded strings
        private const string TestDbPrefix = "DataSource=testdb.sqlite";
        private const string BaseAddress = "http://localhost:5174/";
        private const string AnalyticsSummaryEndpoint = "api/analytics/summary";
        private const string TestServerInitializedMessage = "Test server initialized successfully.";
        private const string HttpClientNotInitializedMessage = "HTTP client not initialized.";
        private const string FailedToInitializeTestEnvironmentMessage = "Failed to initialize test environment. Ensure project configuration is correct.\n";
        private const string endpoint = BaseAddress + AnalyticsSummaryEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsControllerIntegrationTests"/> class.
        /// Sets up the SQLite connection string.
        /// </summary>
        public AnalyticsControllerIntegrationTests()
        {
            _connection = new SqliteConnection(TestDbPrefix);
        }

        /// <summary>
        /// One-time setup for the test class.
        /// Configures the test server, dependency injection, and seeds initial data.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            try
            {
                _connection.Open();
                var builder = WebHost.CreateDefaultBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting(); // Add routing services

                        // Register OrderDbContext with SQLite in-memory
                        services.AddDbContext<OrderDbContext>(options =>
                            options.UseSqlite(_connection)
                                   .EnableSensitiveDataLogging());
                        services.AddControllers()
                            .AddApplicationPart(typeof(Program).Assembly)
                            .AddApplicationPart(typeof(AnalyticsController).Assembly);

                        // Seed initial data
                        using (var scope = services.BuildServiceProvider().CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                            dbContext.Database.EnsureCreated();
                            SeedData(dbContext);
                        }
                        // Mock IDiscountRule
                        var discountRuleMock = new Mock<IDiscountRule>();
                        discountRuleMock.Setup(r => r.IsApplicable(It.IsAny<Customer>(), It.IsAny<Order>())).Returns(false); // No discount applied
                        discountRuleMock.Setup(r => r.CalculateDiscount(It.IsAny<Order>())).Returns(0m); // No discount
                        var discountRules = new[] { discountRuleMock.Object };
                        services.AddSingleton<IEnumerable<IDiscountRule>>(discountRules);

                        // Register DiscountService as IDiscountService
                        services.AddScoped<IDiscountService, DiscountService>();

                        // Register OrderService
                        services.AddScoped<IOrderService, OrderService>();
                        services.AddAutoMapper(typeof(Program).Assembly); // If used in your app
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers(); // Map controller routes
                        });
                    });

                _server = new TestServer(builder);

                _client = _server.CreateClient();
                _client!.BaseAddress = new Uri(BaseAddress); // Set the base address before making the request

                Console.WriteLine(TestServerInitializedMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"{FailedToInitializeTestEnvironmentMessage}" +
                                  $"Error: {ex.Message}\n" +
                                  $"StackTrace: {ex.StackTrace}";
                Assert.Inconclusive(errorMessage);
            }
        }

        /// <summary>
        /// Seeds the test database with initial order data.
        /// </summary>
        /// <param name="dbContext">The database context to seed.</param>
        private void SeedData(OrderDbContext dbContext)
        {
            dbContext.Orders.AddRange(
                new Order
                {
                    OrderCount = 1,
                    TotalAmount = 100m,
                    OrderDate = DateTime.Now.AddDays(-1),
                    Status = OrderStatus.Delivered,
                    DeliveredDate = DateTime.Now
                },
                new Order
                {
                    OrderCount = 2,
                    TotalAmount = 200m,
                    OrderDate = DateTime.Now.AddDays(-1),
                    Status = OrderStatus.Delivered,
                    DeliveredDate = DateTime.Now
                }
            );
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Per-test setup to ensure the HTTP client is initialized.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            if (_client == null)
            {
                Assert.Inconclusive(HttpClientNotInitializedMessage);
            }
        }

        /// <summary>
        /// Tests that the analytics summary endpoint returns correct values
        /// when delivered orders exist in the database.
        /// </summary>
        [Test]
        public async Task GetAnalytics_WithDeliveredOrders_ReturnsCorrectAnalytics()
        {
            // Act
            var response = await _client!.GetAsync(AnalyticsSummaryEndpoint);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var analytics = JsonSerializer.Deserialize<AnalyticsDto>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.That(analytics, Is.Not.Null);
            Assert.That(analytics.AverageOrderValue, Is.EqualTo(150m).Within(0.01m)); // (100 + 200) / 2
            Assert.That(analytics.AverageFulfillmentTime, Is.EqualTo(24.0).Within(0.01)); // Average of 24 and 24 hours
        }

        /// <summary>
        /// Tests that the analytics summary endpoint returns zero values
        /// when there are no orders in the database.
        /// </summary>
        [Test]
        public async Task GetAnalytics_NoOrders_ReturnsZeroValues()
        {
            // Arrange: Clear seeded data for this test
            using var scope = _server!.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            dbContext.Orders.RemoveRange(dbContext.Orders);
            dbContext.SaveChanges();

            // Act
            var response = await _client!.GetAsync(endpoint);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var analytics = JsonSerializer.Deserialize<AnalyticsDto>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.That(analytics, Is.Not.Null);
            Assert.That(analytics.AverageOrderValue, Is.EqualTo(0m));
            Assert.That(analytics.AverageFulfillmentTime, Is.EqualTo(0.0));
        }

        /// <summary>
        /// One-time teardown to dispose of resources after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _client?.Dispose();
            _server?.Dispose();
            _connection?.Dispose();
            _client = null;
            _server = null;
        }
    }
}
