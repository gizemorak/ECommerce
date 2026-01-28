using System;


namespace OrderDomain.Orders
{
    public enum OrderStatus
    {
        PendingPaymentDelay = 0, // created, waiting 10 minutes
        PaymentRequested = 1, // sent to payment service
        Completed = 2, // paid
        Failed = 3, // payment failed
        Cancelled = 4  // user cancelled

    }

}
