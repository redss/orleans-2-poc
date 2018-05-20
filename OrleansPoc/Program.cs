using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;

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

                await orleansApp.Client.GetGrain<IDeviceSimulator>("1").StartSimulation();

                await Task.Delay(TimeSpan.FromMilliseconds(250));

                await orleansApp.Client.GetGrain<IDeviceSimulator>("2").StartSimulation();

                await Task.Delay(TimeSpan.FromMilliseconds(250));

                await orleansApp.Client.GetGrain<IDeviceSimulator>("3").StartSimulation();

                Console.ReadKey();
            }
        }
    }
}
