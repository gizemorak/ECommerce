using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderApplication.Orders.Queries.GetOrder;
using OrderDomain.Repositories;
using Xunit;

namespace ECommerce.Tests.OrderApplication;

public sealed class GetByIdOrderCommandHandlerTests
{
 [Fact]
 public async Task Handle_WhenOrderExists_ShouldReturnSuccessWithDto()
 {
 var orderRepository = new Mock<IOrderRepository>();
 var unitOfWork = new Mock<IUnitOfWork>();
 var logger = Mock.Of<ILogger<GetByIdOrderCommandHandler>>();

 var order = global::OrderDomain.Orders.Order.CreateNewOrder(
 Guid.NewGuid(),
 new global::OrderDomain.Orders.Address("Street", "City", "State", "Country", "12345"),
 [new global::OrderDomain.Orders.OrderItem("Product", "prod-1",10m)]);
 order.Id =1;

 orderRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

 var handler = new GetByIdOrderCommandHandler(orderRepository.Object, unitOfWork.Object, logger);

 var result = await handler.Handle(new GetByIdOrderCommand(1), CancellationToken.None);

 result.IsSuccess.Should().BeTrue();
 result.Data.Should().NotBeNull();
 result.Data!.OrderId.Should().Be(1);
 }

 [Fact]
 public async Task Handle_WhenOrderMissing_ShouldReturnNotFound()
 {
 var orderRepository = new Mock<IOrderRepository>();
 var unitOfWork = new Mock<IUnitOfWork>();
 var logger = Mock.Of<ILogger<GetByIdOrderCommandHandler>>();

 orderRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
 .ReturnsAsync((global::OrderDomain.Orders.Order?)null);

 var handler = new GetByIdOrderCommandHandler(orderRepository.Object, unitOfWork.Object, logger);

 var result = await handler.Handle(new GetByIdOrderCommand(999), CancellationToken.None);

 result.IsSuccess.Should().BeFalse();
 result.Fail.Should().NotBeNull();
 result.Status.Should().Be(System.Net.HttpStatusCode.NotFound);
 }
}
