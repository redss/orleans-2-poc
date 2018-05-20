using System;
using System.Threading.Tasks;
using Orleans;

namespace OrleansPoc
{
    public interface IDeviceSimulator : IGrainWithStringKey
    {
        Task StartSimulation(TimeSpan interval);
    }

    public class DeviceSimulator : Grain<DeviceSimulatorState>, IDeviceSimulator
    {
        public DeviceSimulator(ISomeService someService)
        {
            Console.WriteLine(someService.GetName());
        }

        private string Id => this.GetPrimaryKeyString();

        public Task StartSimulation(TimeSpan interval)
        {
            RegisterTimer(
                asyncCallback: state => Simulate(),
                state: null,
                dueTime: TimeSpan.Zero,
                period: interval);

            return Task.CompletedTask;
        }

        private async Task Simulate()
        {
            State.CurrentStatus = NextStatus(State.CurrentStatus);

            var asyncStream = GetStreamProvider(DefaultStreamProvider.Name)
                .GetStream<DeviceStatusChanged>(Guid.Empty, "DeviceStatusChanged");

            await asyncStream.OnNextAsync(new DeviceStatusChanged
            {
                DeviceId = Id,
                NewStatus = State.CurrentStatus
            });
        }

        private Status NextStatus(Status status)
        {
            return status == Status.Occupied
                ? Status.Free
                : Status.Occupied;
        }
    }

    public class DeviceSimulatorState
    {
        public Status CurrentStatus { get; set; } = Status.Free;
    }

    public class DeviceStatusChanged
    {
        public string DeviceId { get; set; }
        public Status NewStatus { get; set; }
    }

    public enum Status
    {
        Occupied, Free
    }
}