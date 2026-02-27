using ECommerce.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderPersistence;
using System.Net;
using Xunit;

namespace ECommerce.IntegrationTests.OrderApi;

public class OrdersTests : IClassFixture<MsSqlContainerFixture>
{
    private readonly MsSqlContainerFixture _db;

    public OrdersTests(MsSqlContainerFixture db)
    {
        _db = db;
    }

    [Fact]
    public async Task Smoke_Test()
    {
        await using var factory = new OrderApiFactory(_db.ConnectionString);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); 
            await db.Database.MigrateAsync();
        }

        var client = factory.CreateClient();
        var resp = await client.GetAsync("/health");
        resp.EnsureSuccessStatusCode();
    }
}