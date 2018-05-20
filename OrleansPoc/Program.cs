using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;

namespace OrleansPoc
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var siloHostBuilder = new SiloHostBuilder()
                .ConfigureServices(collection =>
                {
                    collection.AddSingleton<ISomeService>(new ActualService());
                })
                .UseLocalhostClustering();

            using (var siloHost = siloHostBuilder.Build())
            {
                await siloHost.StartAsync();

                Console.WriteLine($"Started Silo at {stopwatch.ElapsedMilliseconds} ms.");

                var clientBuilder = new ClientBuilder()
                    .UseLocalhostClustering();

                using (var clusterClient = clientBuilder.Build())
                {
                    await clusterClient.Connect();

                    Console.WriteLine($"Connected client at {stopwatch.ElapsedMilliseconds} ms.");

                    var player = clusterClient.GetGrain<IPlayerGrain>(Guid.NewGuid());

                    Console.WriteLine(await player.Greet());

                    Console.WriteLine($"Referenced first grain in {stopwatch.ElapsedMilliseconds} ms.");

                    Console.ReadKey();

                    await clusterClient.Close();
                }

                await siloHost.StopAsync();
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
