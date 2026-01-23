using Bus.Shared;
using Bus.Shared.Options;
using Microsoft.Extensions.Options;


namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);



            builder.Services.Configure<ServiceBusOption>(
                builder.Configuration.GetSection(nameof(ServiceBusOption)));
            builder.Services.AddSingleton<ServiceBusOption>(sp =>
            {
                IOptions<ServiceBusOption> optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();
                return optionsServiceBus.Value;

            });



            var host = builder.Build();
            host.Run();
        }
    }
}