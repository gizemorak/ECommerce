using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Services
{
    public interface IPaymentService
    {
        Task StartPaymentAsync(int OrderId);

    }
}
