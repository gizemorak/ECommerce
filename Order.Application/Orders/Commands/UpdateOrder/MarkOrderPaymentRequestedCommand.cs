using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Orders.Commands.UpdateOrder
{
    namespace OrderApplication.Orders.Commands.UpdateOrder
    {
        public sealed record MarkOrderPaymentRequestedCommand(int OrderId) : IRequestByServiceResult;
    }
}
