using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderManagementService.Controllers;
using OrderManagementService.Dtos;
using OrderManagementService.Enum;
using OrderManagementService.Interfaces;
using OrderManagementService.Models;

namespace OrderManagementServiceTests.UnitTests
{
    [TestFixture]
    public class AnalyticsControllerUnitTests
    {
        private Mock<IOrderService> _orderServiceMock;
        private Mock<IMapper> _mapperMock;
        private AnalyticsController _controller;

        [SetUp]
        public void SetUp()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _mapperMock = new Mock<IMapper>();
            _controller = new AnalyticsController(_orderServiceMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task GetAnalytics_WithDeliveredOrders_ReturnsCorrectAnalytics()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { TotalAmount = 100m, OrderDate = DateTime.Now.AddDays(-1), DeliveredDate = DateTime.Now, Status = OrderStatus.Delivered },
                new Order { TotalAmount = 200m, OrderDate = DateTime.Now.AddDays(-1), DeliveredDate = DateTime.Now, Status = OrderStatus.Delivered }
            };
            _orderServiceMock.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetAnalytics();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var analytics = okResult.Value as AnalyticsDto;
            Assert.That(analytics, Is.Not.Null);
            Assert.That(analytics.AverageOrderValue, Is.EqualTo(150m).Within(0.01m)); // (100 + 200) / 2
            Assert.That(analytics.AverageFulfillmentTime, Is.EqualTo(24.0).Within(0.01)); // Average of 24 and 24 hours
        }

        [Test]
        public async Task GetAnalytics_NoOrders_ReturnsZeroValues()
        {
            // Arrange
            _orderServiceMock.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(new List<Order>());

            // Act
            var result = await _controller.GetAnalytics();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var analytics = okResult.Value as AnalyticsDto;
            Assert.That(analytics, Is.Not.Null);
            Assert.That(analytics.AverageOrderValue, Is.EqualTo(0m));
            Assert.That(analytics.AverageFulfillmentTime, Is.EqualTo(0.0));
        }

        [Test]
        public async Task GetAnalytics_NoDeliveredOrders_ReturnsZeroFulfillmentTime()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { TotalAmount = 100m, Status = OrderStatus.Pending, OrderDate = DateTime.Now },
                new Order { TotalAmount = 200m, Status = OrderStatus.Processing, OrderDate = DateTime.Now }
            };
            _orderServiceMock.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetAnalytics();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var analytics = okResult.Value as AnalyticsDto;
            Assert.That(analytics, Is.Not.Null);
            Assert.That(analytics.AverageOrderValue, Is.EqualTo(150m).Within(0.01m)); // (100 + 200) / 2
            Assert.That(analytics.AverageFulfillmentTime, Is.EqualTo(0.0));
        }
    }
}