using Bus.Shared;
using Bus.Shared.Events;
using Bus.Shared.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderApplication.Orders.Commands.CreateOrder;
using OrderDomain.Orders;
using OrderDomain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Orders.Commands.CancelOrder
{
    public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ServiceResult>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateOrderCommandHandler> _logger;


        public CancelOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateOrderCommandHandler> logger)
        {
            this._orderRepository = orderRepository;
            this._unitOfWork = unitOfWork;
            this._logger = logger;



        }

        public async Task<ServiceResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
         
             var order = await _orderRepository.GetByIdAsync(request.OrderId);

            if (order is null)
                return ServiceResult.ErrorAsNotFound();

             order.Status= OrderStatus.Cancelled;

        
             _orderRepository.Update(order);



            await _unitOfWork.SaveChangesAsync(cancellationToken);

       

            return ServiceResult.SuccessAsNoContent();
        }






    }
}
