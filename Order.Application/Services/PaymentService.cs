using OrderDomain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Services
{
    public class PaymentService:IPaymentService
    {
        public async Task CheckPayment(int OrderId, Guid CustomerId, decimal TotalAmount)
        {
            //todo payment prosess
            Console.WriteLine("Ödeme alındı");
        }

    }
}
