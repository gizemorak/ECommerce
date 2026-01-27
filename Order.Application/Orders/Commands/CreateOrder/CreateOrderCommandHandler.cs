using Bus.Shared;
using Bus.Shared.Events;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderDomain.Orders;
using OrderDomain.Repositories;
using RedisApp.Servives;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace OrderApplication.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand,ServiceResult>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateOrderCommandHandler> _logger;
        private readonly RedisService _redisService;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateOrderCommandHandler> logger, RedisService redisService)
        {
            this._orderRepository = orderRepository;
            this._unitOfWork = unitOfWork;
            this._logger = logger;
            this._redisService = redisService;


        }

        public async Task<ServiceResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var orderItems = request.OrderItems.Select(oi =>
                new OrderItem(             
                    oi.ProductName,
                    oi.ProductId,
                    oi.Price
                )
            ).ToList();

            var order = OrderDomain.Orders.Order.CreateNewOrder(
                request.UserId,
                new Address(request.adressdto.Street, request.adressdto.City, request.adressdto.State, request.adressdto.Country, request.adressdto.ZipCode),
                orderItems
            );

            _orderRepository.Add(order);

      


            await _unitOfWork.SaveChangesAsync(cancellationToken);


            var db =_redisService.GetDatabase();

            var maxRetries = 3;
            var retryCount = 0;
            RedisValue messageId = RedisValue.Null;

            var eventId = Guid.NewGuid().ToString("N");

            var message = new NameValueEntry[]

 {
            new("order", JsonSerializer.Serialize(new OrderCreatedEvent(order.Id, request.UserId, order.TotalPrice))),
            new("eventId", eventId),
            new("created_date", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
 };

            while (retryCount < maxRetries)
            {
                try
                {
                    messageId = await db.StreamAddAsync("ordercreated", message, null, null, false);

                    if (messageId.HasValue)
                    {
                        break;
                    }

                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new Exception("message can not send redis stream");
                    }
                }
                catch (Exception)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw;
                    }

                    await Task.Delay(100 * retryCount);
                }
            }


            return ServiceResult.SuccessAsNoContent();
        }

   

     


    }

}
