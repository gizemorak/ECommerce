using Bus.Shared;
using Bus.Shared.Enums;
using Bus.Shared.Events;
using Bus.Shared.Publishers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrderApplication.Orders.Commands.CreateOrder;
using OrderApplicationOrders.Commands.CreateOrder;
using OrderDomain.Repositories;
using RabbitMQ.Client;
using StackExchange.Redis;
using Xunit;

namespace ECommerce.Tests.Order.Application;

public sealed class CreateOrderCommandHandlerTests
{
 private sealed class FakeBusService : IBusService
 {
 public Task Init() => Task.CompletedTask;
 public Task PublishAsync<T>(T message, string? topic = null, Dictionary<string, object>? headers = null, CancellationToken ct = default) where T : BaseEvent
 => Task.CompletedTask;
 public Task Publish<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent
 => Task.CompletedTask;
 public Task<IChannel> CreateChannel() => Task.FromResult<IChannel>(null!);
 public IDatabase GetDatabase(int db =0) => throw new NotImplementedException();
 }

 [Fact]
 public async Task Handle_ShouldAddOrder_SaveChanges_AndReturnSuccess()
 {
 // Arrange
 var orderRepository = new Mock<IOrderRepository>();
 var unitOfWork = new Mock<IUnitOfWork>();
 var logger = Mock.Of<ILogger<CreateOrderCommandHandler>>();

 var services = new ServiceCollection();
 services.AddKeyedSingleton<IBusService>(BusType.Kafka, new FakeBusService());
 var sp = services.BuildServiceProvider();

 var publisher = new MessagePublisher(sp);

 var configuration = new Mock<IConfiguration>();
 configuration.Setup(c => c["BUS_TYPE"]).Returns("Kafka");

 var handler = new CreateOrderCommandHandler(
 orderRepository.Object,
 unitOfWork.Object,
 logger,
 publisher,
 configuration.Object);

 var address = new AdressDto
 {
 Street = "Street",
 City = "City",
 State = "State",
 Country = "Country",
 ZipCode = "12345"
 };

 var command = new CreateOrderCommand(
 UserId: Guid.NewGuid(),
 adressdto: address,
 OrderItems: [new global::OrderApplication.Orders.DTOs.OrderItemDto { ProductId = "prod-1", ProductName = "Product", Price =10 }],
 Payment: new PaymentDto("4532015112830366", "John Doe", "12/25", "123",10));

 // Act
 var result = await handler.Handle(command, CancellationToken.None);

 // Assert
 result.IsSuccess.Should().BeTrue();
 orderRepository.Verify(r => r.Add(It.IsAny<OrderDomain.Orders.Order>()), Times.Once);
 unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
 }
}
