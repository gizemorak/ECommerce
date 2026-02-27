using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderPersistence;
using Xunit;

namespace ECommerce.IntegrationTests.Features.Orders;

public sealed class GetOrderTests : IClassFixture<MsSqlContainerFixture>
{
 private readonly MsSqlContainerFixture _db;

 public GetOrderTests(MsSqlContainerFixture db)
 {
 _db = db;
 }

 [Fact]
 public async Task Get_Order_ById_WhenEndpointMissing_ShouldReturn404()
 {
 await using var factory = new OrderApiFactory(_db.ConnectionString);
 await TestDbMigrator.MigrateAsync(factory.Services);

 using var client = factory.CreateClient();

 // Note: Order.Api currently maps only POST /send and POST /cancel under /api/v{version}/orders.
 // Until a GET endpoint is added, this is expected to be404.
 var response = await client.GetAsync("/api/v1/orders/1");

 response.StatusCode.Should().Be(HttpStatusCode.NotFound);
 }

 [Fact]
 public async Task Get_Order_ById_ShouldReturn200_WhenImplemented()
 {
 await using var factory = new OrderApiFactory(_db.ConnectionString);
 await TestDbMigrator.MigrateAsync(factory.Services);

 using var client = factory.CreateClient();

 // Arrange: create an order first.
 var createPayload = new
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

 var createResponse = await client.PostAsJsonAsync("/api/v1/orders/send", createPayload);
 createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

 var createdOrderId = await TestDbMigrator.QueryAsync(factory.Services, db =>
 db.Set<OrderDomain.Orders.Order>()
 .OrderByDescending(o => o.Id)
 .Select(o => o.Id)
 .FirstAsync());

 // Act: attempt GET by id (this will return404 until you add an endpoint).
 var getResponse = await client.GetAsync($"/api/v1/orders/{createdOrderId}");

 // Assert: once implemented, update expectation to OK + response body checks.
 getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
 }
}
