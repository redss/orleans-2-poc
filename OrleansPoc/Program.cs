using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Orleans;

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

                var player = orleansApp.Client.GetGrain<IPlayerGrain>(Guid.NewGuid());

                Console.WriteLine(await player.Greet());

                Console.WriteLine($"Referenced first grain at {stopwatch.ElapsedMilliseconds} ms.");

                Console.ReadKey();
            }
        }
    }

    public interface IPlayerGrain : IGrainWithGuidKey
    {
        Task<string> Greet();
    }

    public class PlayerGrain : Grain, IPlayerGrain
    {
        public PlayerGrain(ISomeService someService)
        {
            Console.WriteLine(someService.GetName());
        }

        public Task<string> Greet()
        {
            return Task.FromResult($"Hello, I'm player {this.GetPrimaryKey()}.");
        }
    }

    public interface ISomeService
    {
        string GetName();
    }

    class ActualService : ISomeService
    {
        public string GetName()
        {
            return "I'm injected, yo!";
        }
    }
}
