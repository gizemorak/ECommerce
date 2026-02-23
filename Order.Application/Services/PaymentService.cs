using OrderDomain.Orders;
using OrderDomain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Services
{
    public class PaymentService:IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            this._orderRepository = orderRepository;
            this._unitOfWork = unitOfWork;
        }
        public async Task CheckPayment(int OrderId, Guid BuyerId, decimal TotalPrice)
        {
            // TODO: Implement payment processing logic
            Console.WriteLine("Payment received");

            var order = await _orderRepository.GetByIdAsync(OrderId);

            order.Status= OrderStatus.Completed;

            _orderRepository.Update(order);

            await  _unitOfWork.SaveChangesAsync();

        }

    }
}
