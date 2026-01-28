using Bus.Shared;
using Bus.Shared.Events;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderApplication.Orders.DTOs;
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


namespace OrderApplication.Orders.Queries.GetOrder
{
    public sealed class GetByIdOrderCommandHandler : IRequestHandler<GetByIdOrderCommand,ServiceResult>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetByIdOrderCommandHandler> _logger;

        public GetByIdOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<GetByIdOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;



        }

        public async Task<ServiceResult> Handle(GetByIdOrderCommand request, CancellationToken cancellationToken)
        {
          

            var order=await _orderRepository.GetByIdAsync(request.OrderId);

            var orderdto = new OrderDto();

            orderdto.OrderId = order.Id;
            orderdto.OrderStatus = (OrderStatusDto)order.Status;
            orderdto.BuyerId = order.BuyerId;
            orderdto.TotalPrice = order.TotalPrice;



            return  ServiceResult<OrderDto>.SuccessAsOk(orderdto);
        }

   

     


    }

}
