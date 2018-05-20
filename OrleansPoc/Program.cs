using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Orleans.Streams;

namespace OrleansPoc
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var containerBuilder = new ContainerBuilder();

            containerBuilder
                .RegisterType<ActualService>()
                .As<ISomeService>()
                .SingleInstance();

            containerBuilder
                .RegisterType<LocalOrleansApp>()
                .AsSelf();

            using (var container = containerBuilder.Build())
            {
                var orleansApp = container.Resolve<LocalOrleansApp>();

                await orleansApp.Start();

                Console.WriteLine($"Started Silo at {stopwatch.ElapsedMilliseconds} ms.");

                await orleansApp.Client.GetGrain<IDeviceSimulator>("1").StartSimulation(TimeSpan.FromMilliseconds(200));

                await Task.Delay(TimeSpan.FromMilliseconds(250));

                await orleansApp.Client.GetGrain<IDeviceSimulator>("2").StartSimulation(TimeSpan.FromMilliseconds(250));

                await Task.Delay(TimeSpan.FromMilliseconds(250));

                await orleansApp.Client.GetGrain<IDeviceSimulator>("3").StartSimulation(TimeSpan.FromMilliseconds(300));

                var asyncStream = orleansApp.Client
                    .GetStreamProvider(DefaultStreamProvider.Name)
                    .GetStream<DeviceStatusChanged>(Guid.Empty, "DeviceStatusChanged");

                StreamSubscriptionHandle<DeviceStatusChanged> subscribeAsync = null;

                try
                {
                    subscribeAsync = await asyncStream.SubscribeAsync(OnDeviceStatusChanged);

                    Console.ReadKey();
                }
                finally
                {
                    if (subscribeAsync != null)
                    {
                        await subscribeAsync.UnsubscribeAsync();
                    }
                }
            }
        }

        private static Task OnDeviceStatusChanged(DeviceStatusChanged change, StreamSequenceToken token)
        {
            Console.WriteLine($"Device {change.DeviceId} changed status to {change.NewStatus}.");

            return Task.CompletedTask;
        }
    }
}
