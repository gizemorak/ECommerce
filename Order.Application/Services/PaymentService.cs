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
  
        public async Task CheckPayment(int OrderId, Guid BuyerId, decimal TotalPrice)
        {
            //todo payment prosess
            Console.WriteLine("payment completed");
        }
    }

    }

