using Bus.Shared.Enums;
using Bus.Shared.Events; // Ensure BaseEvent is accessible
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Shared.Publishers
{
    public class MessagePublisher
    {
        private readonly IServiceProvider _sp;

        public MessagePublisher(IServiceProvider sp)
        {
            _sp = sp;
        }

        public Task PublishAsync<T>(BusType busType, string topic,T message, CancellationToken ct = default)
            where T : BaseEvent
        {
            var bus = _sp.GetRequiredKeyedService<IBusService>(busType);
            return bus.PublishAsync(message, topic, null, ct);
        }
    }

}