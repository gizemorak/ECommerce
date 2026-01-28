using Bus.Shared.Events;
using Bus.Shared.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderApplication.Orders.Commands.CreateOrder;
using OrderApplication.Orders.DTOs;
using OrderDomain.Orders;
using OrderDomain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Orders.Commands.UpdateOrder
{
    public sealed class MarkOrderPaymentRequestedCommandHandler : IRequestHandler<MarkOrderPaymentRequestedCommand, ServiceResult>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkOrderPaymentRequestedCommandHandler> _logger;
        private readonly KafkaService _busService;

        public MarkOrderPaymentRequestedCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<MarkOrderPaymentRequestedCommandHandler> logger, KafkaService busService)
        {
            this._orderRepository = orderRepository;
            this._unitOfWork = unitOfWork;
            this._logger = logger;
            this._busService = busService;


        }

        public async Task<ServiceResult> Handle(MarkOrderPaymentRequestedCommand request, CancellationToken cancellationToken)
        {

            var orderlist = await _orderRepository.GetAllOrders();

            var order = orderlist
            .Where(o => o.Id == request.OrderId
                     && o.Status == OrderStatus.PendingPaymentDelay).FirstOrDefault();

            order.Status = OrderStatus.PaymentRequested;

            bool result = _orderRepository.Update(order);


            await _unitOfWork.SaveChangesAsync(cancellationToken);


            return ServiceResult<bool>.SuccessAsOk(result);






        }

    }

}
