using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderApplication;
using OrderApplication.Orders.Commands.UpdateOrder;
using OrderApplication.Orders.DTOs;
using OrderApplication.Services;
using OrderDomain.Orders;
using OrderPersistence;

public sealed class PaymentSchedulerWorker(IServiceProvider serviceProvider)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = serviceProvider.CreateScope();

            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var getOrderQuery = new OrderApplication.Orders.Queries.GetDueOrder.GetDueOrderCommand();



            try
            {


                var dueOrders = await mediator.Send(getOrderQuery, stoppingToken) as ServiceResult<IEnumerable<OrderDto>>;
                if (dueOrders?.Data == null) continue;

                var orders = dueOrders.Data.ToList();

                if (orders.Count == 0) continue;


                foreach (var order in dueOrders.Data)
                {
                    try
                    {

                        var claimed = await mediator.Send(
                                                   new MarkOrderPaymentRequestedCommand(order.OrderId),
                                                   stoppingToken);

                        if (!claimed.IsSuccess)
                            continue; 


                        if (order.OrderStatus == (OrderStatusDto)OrderStatus.Cancelled)
                            continue;

                        await paymentService.CheckPayment(order.OrderId, order.BuyerId, order.TotalPrice);

                        Console.WriteLine("send request to payment gateway for order " + order.OrderId);

                    }
                    catch
                    {
      
                        //todo order.OrderStatus = (OrderStatusDto)OrderStatus.Failed;
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }

     
        }
    }
}
