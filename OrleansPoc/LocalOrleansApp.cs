using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;

namespace OrleansPoc
{
    public class LocalOrleansApp : IDisposable
    {
        private readonly ISiloHost _siloHost;
        private readonly IClusterClient _clusterClient;

        private ILifetimeScope _siloScope;

        public LocalOrleansApp(ILifetimeScope parentScope)
        {
            _siloHost = new SiloHostBuilder()
                .UseServiceProviderFactory(collection =>
                {
                    _siloScope = parentScope.BeginLifetimeScope(builder => builder.Populate(collection));

                    return new AutofacServiceProvider(_siloScope);
                })
                .UseLocalhostClustering()
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddSimpleMessageStreamProvider(DefaultStreamProvider.Name)
                .AddMemoryGrainStorage("PubSubStore") // for streams
                .Build();

            _clusterClient = new ClientBuilder()
                .UseLocalhostClustering()
                .AddSimpleMessageStreamProvider(DefaultStreamProvider.Name)
                .Build();
        }

        public async Task Start()
        {
            await _siloHost.StartAsync();
            await _clusterClient.Connect();
        }

        public IClusterClient Client => _clusterClient ?? throw new InvalidOperationException("Orleans is not started.");

        public void Dispose()
        {
            _clusterClient?.Close();
            _clusterClient?.Dispose();

            _siloHost?.StopAsync().Wait();
            _siloHost?.Dispose();
            
            _siloScope?.Dispose();
        }
    }
}