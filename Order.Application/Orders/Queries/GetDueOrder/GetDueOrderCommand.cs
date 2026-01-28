using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Orders.Queries.GetDueOrder
{


    public sealed record GetDueOrderCommand() : IRequestByServiceResult;
}
