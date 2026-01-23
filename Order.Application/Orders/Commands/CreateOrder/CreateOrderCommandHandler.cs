using Bus.Shared;
using Bus.Shared.Events;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderDomain.Orders;
using OrderDomain.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace OrderApplication.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand,ServiceResult>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateOrderCommandHandler> _logger;
        private readonly IBusService _busService;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateOrderCommandHandler> logger, IBusService busService)
        {
            this._orderRepository = orderRepository;
            this._unitOfWork = unitOfWork;
            this._logger = logger;
            this._busService=busService;


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

            await _busService.Publish(new OrderCreatedEvent(order.Id, request.UserId, order.TotalPrice));

            return ServiceResult.SuccessAsNoContent();
        }

   

     


    }

}
