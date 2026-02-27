using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderPersistence;
using Xunit;

namespace ECommerce.IntegrationTests.Features.Orders;

public sealed class CreateOrderTests : IClassFixture<MsSqlContainerFixture>
{
 private readonly MsSqlContainerFixture _db;

 public CreateOrderTests(MsSqlContainerFixture db)
 {
 _db = db;
 }

 [Fact]
 public async Task Post_SendOrder_ShouldCreateOrder_AndReturnSuccess()
 {
 await using var factory = new OrderApiFactory(_db.ConnectionString);
 await TestDbMigrator.MigrateAsync(factory.Services);

 using var client = factory.CreateClient();

 var payload = new
 {
 userId = Guid.NewGuid(),
 adressdto = new
 {
 street = "123 Main St",
 city = "Istanbul",
 state = "TR",
 country = "TR",
 zipCode = "34000"
 },
 orderItems = new[]
 {
 new { productId = "prod-1", productName = "Item1", price =10.5m }
 },
 payment = new
 {
 cardNumber = "4532015112830366",
 cardHolderName = "John Doe",
 expiration = "12/30",
 cvc = "123",
 amount =10.5m
 }
 };

 var response = await client.PostAsJsonAsync("/api/v1/orders/send", payload);
 response.StatusCode.Should().Be(HttpStatusCode.OK);

 var count = await TestDbMigrator.QueryAsync(factory.Services,
 db => db.Set<OrderDomain.Orders.Order>().CountAsync());

 count.Should().BeGreaterThan(0);
 }
}
