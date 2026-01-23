using Bus.Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Shared.Services
{
    public interface IBusService
    {

        Task CreateTopic(string topicName);

        Task SendMessage(string topic, OrderCreatedEvent orderCreatedEvent);
    }
}
