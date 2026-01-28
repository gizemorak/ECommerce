using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Orders.Queries.GetDueOrder
{
    using Bus.Shared;
    using Bus.Shared.Events;
    using Bus.Shared.Services;
    using global::OrderApplication.Orders.DTOs;
    using global::OrderApplication.Orders.Queries.GetOrder;
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


    namespace OrderApplication.Orders.Queries.GetOrder
    {
        public sealed class GeDueOrderCommandHandler : IRequestHandler<GetDueOrderCommand, ServiceResult>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger<GetByIdOrderCommandHandler> _logger;

            public GeDueOrderCommandHandler(
                IOrderRepository orderRepository,
                IUnitOfWork unitOfWork,
                ILogger<GetByIdOrderCommandHandler> logger)
            {
                _orderRepository = orderRepository;
                _unitOfWork = unitOfWork;
                _logger = logger;



            }

            public async Task<ServiceResult> Handle(GetDueOrderCommand request, CancellationToken cancellationToken)
            {


                var order = await _orderRepository.GetAllOrders();

                var nowUtc = DateTime.UtcNow;

                var list = order.Where(o => o.Status == OrderStatus.PendingPaymentDelay &&
                          o.PaymentDueAtUtc <= nowUtc)
              .OrderBy(o => o.PaymentDueAtUtc)
              .Take(100);

                var orderdto = from o in list
                               select new OrderDto
                               {
                                   OrderId = o.Id,
                                   OrderStatus = (OrderStatusDto)o.Status,
                                   PaymentDueAtUtc = o.PaymentDueAtUtc,
                                   BuyerId= o.BuyerId,
                                   TotalPrice = o.TotalPrice
                               };

                return ServiceResult<IEnumerable<OrderDto>>.SuccessAsOk(orderdto);
            }






        }

    }

}
