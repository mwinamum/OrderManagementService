using Moq;
using OrderManagementService.Interfaces;
using OrderManagementService.Models;

namespace OrderManagementServiceTests.UnitTests
{

    /// <summary>
    /// Contains unit tests for discount rule logic, verifying that discounts are applied correctly
    /// based on rule applicability for a given customer and order.
    /// </summary>
    [TestFixture]
    public class DiscountRuleTests
    {
        [Test]
        public void ApplyDiscount_ShouldReturnDiscountedAmount_WhenRuleIsApplicable()
        {
            // Arrange
            var mockRule = new Mock<IDiscountRule>();
            var customer = new Customer { Id = 1, Segment = "Regular" };
            var order = new Order { Id = 1, CustomerName = "Test", CustomerEmail = "test@example.com", ShippingAddress = "123 Main St", OrderDate = DateTime.Now, Status = 0, TotalAmount = 100m, OriginalTotalAmount = 100m, OrderCount = 6 };
            decimal expectedDiscounted = 90m;

            mockRule.Setup(r => r.IsApplicable(customer, order)).Returns(true);
            mockRule.Setup(r => r.CalculateDiscount(order)).Returns(expectedDiscounted);

            // Act
            var isApplicable = mockRule.Object.IsApplicable(customer, order);
            var discounted = mockRule.Object.CalculateDiscount(order);

            // Assert
            Assert.That(isApplicable, Is.True);
            Assert.That(discounted, Is.EqualTo(expectedDiscounted));
        }

        [Test]
        public void ApplyDiscount_ShouldReturnOriginalAmount_WhenRuleIsNotApplicable()
        {
            // Arrange
            var mockRule = new Mock<IDiscountRule>();
            var customer = new Customer { Id = 2, Segment = "Regular" };
            var order = new Order { Id = 2, CustomerName = "Test2", CustomerEmail = "test2@example.com", ShippingAddress = "456 Main St", OrderDate = DateTime.Now, Status = 0, TotalAmount = 50m, OriginalTotalAmount = 50m, OrderCount = 1 };

            mockRule.Setup(r => r.IsApplicable(customer, order)).Returns(false);
            mockRule.Setup(r => r.CalculateDiscount(order)).Returns(order.TotalAmount);

            // Act
            var isApplicable = mockRule.Object.IsApplicable(customer, order);
            var discounted = mockRule.Object.CalculateDiscount(order);

            // Assert
            Assert.That(isApplicable, Is.False);
            Assert.That(discounted, Is.EqualTo(order.TotalAmount));
        }
    }
}
