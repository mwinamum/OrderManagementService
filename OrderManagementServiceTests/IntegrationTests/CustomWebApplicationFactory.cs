using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using OrderManagementService.Constants;
using OrderManagementService.Data;
using OrderManagementService.Models;

namespace OrderManagementServiceTests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Path relative to bin\Debug\net8.0
            builder.UseSolutionRelativeContentRoot("../OrderManagement");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                var contentRoot = context.HostingEnvironment.ContentRootPath;
                var applicationBasePath = AppContext.BaseDirectory;
                var solutionPath = Path.Combine(contentRoot, "OrderManagement.sln");
                Console.WriteLine($"Resolved Content Root: {contentRoot}");
                Console.WriteLine($"Application Base Path: {applicationBasePath}");
                Console.WriteLine($"Expected Solution Path: {solutionPath}");
                if (!File.Exists(solutionPath))
                {
                    Console.WriteLine($"Warning: Solution file not found at {solutionPath}");
                }
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<OrderDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                SeedData(dbContext);
            });
        }

        private void SeedData(OrderDbContext dbContext)
        {
            dbContext.Customers.AddRange(
                new Customer { Id = 1, Segment = CustomerSegments.GoldSegment },
                new Customer { Id = 2, Segment = CustomerSegments.PremiumSegment },
                new Customer { Id = 3, Segment = CustomerSegments.RegularSegment }
            );
            dbContext.SaveChanges();
        }
    }
}