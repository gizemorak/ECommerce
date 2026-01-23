using System;


namespace OrderDomain.Orders
{
    public enum OrderStatus
    {
        Pending = 0,
        Processed = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 3

    }

}
