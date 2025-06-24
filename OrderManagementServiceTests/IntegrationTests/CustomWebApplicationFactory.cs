using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using OrderManagementService.Constants;
using OrderManagementService.Data;
using OrderManagementService.Models;

namespace OrderManagementService.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Navigate up two levels from bin/Debug/net8.0 to the parent directory (D:\Projects)
            builder.UseSolutionRelativeContentRoot("../../OrderManagementService");

            // Add debug logging to verify the content root
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var contentRoot = context.HostingEnvironment.ContentRootPath;
                Console.WriteLine($"Resolved Content Root: {contentRoot}");
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<OrderDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

                // Seed data
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                SeedData(dbContext);
            });
        }

        private void SeedData(OrderDbContext dbContext)
        {
            dbContext.Customers.AddRange(
                new Customer { Id = 1, Segment = CustomerSegments.GoldSegment, OrderCount = 5 },
                new Customer { Id = 2, Segment = CustomerSegments.PremiumSegment, OrderCount = 6 },
                new Customer { Id = 3, Segment = CustomerSegments.RegularSegment, OrderCount = 5 }
            );
            dbContext.SaveChanges();
        }
    }
}