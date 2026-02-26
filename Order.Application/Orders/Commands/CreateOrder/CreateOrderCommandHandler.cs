using Bus.Shared;
using Bus.Shared.Enums;
using Bus.Shared.Events;
using Bus.Shared.Publishers;
using MediatR;
using Microsoft.Extensions.Configuration;
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
    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ServiceResult>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateOrderCommandHandler> _logger;
        private readonly MessagePublisher _publisher;
        private readonly IConfiguration _configuration;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateOrderCommandHandler> logger,
            MessagePublisher publisher,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _publisher = publisher;
            _configuration = configuration;
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


            var busParameter = _configuration["BUS_TYPE"]; 
     

     
            if (!Enum.TryParse<BusType>(busParameter, ignoreCase: true, out var busType))
                busType = BusType.RabbitMQ;

            if (busType==BusType.Kafka)
            {
                await _publisher.PublishAsync(busType, "ordercreatedtopic", new OrderCreatedEvent(order.Id, request.UserId, order.TotalPrice));
            }
            else
            {
                await _publisher.PublishAsync(busType, null, new OrderCreatedEvent(order.Id, request.UserId, order.TotalPrice), cancellationToken);
            }

              

            return ServiceResult.SuccessAsNoContent();
        }


           
        }

   

     


    }
