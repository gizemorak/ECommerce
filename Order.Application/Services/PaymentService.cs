using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Services
{
    public class PaymentService:IPaymentService
    {
        public async Task StartPaymentAsync(int OrderId)
        {
            //todo payment prosess
            Console.WriteLine("Ödeme alındı");
        }

    }
}
