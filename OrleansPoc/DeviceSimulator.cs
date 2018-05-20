using System;
using System.Threading.Tasks;
using Orleans;

namespace OrleansPoc
{
    public interface IDeviceSimulator : IGrainWithStringKey
    {
        Task StartSimulation();
    }

    public class DeviceSimulator : Grain<DeviceSimulatorState>, IDeviceSimulator
    {
        public DeviceSimulator(ISomeService someService)
        {
            Console.WriteLine(someService.GetName());
        }

        private string Id => this.GetPrimaryKeyString();

        public Task StartSimulation()
        {
            RegisterTimer(
                asyncCallback: state => Simulate(),
                state: null,
                dueTime: TimeSpan.FromSeconds(1),
                period: TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        private Task Simulate()
        {
            State.CurrentStatus = NextStatus(State.CurrentStatus);

            Console.WriteLine($"Device {Id} is changing status to {State.CurrentStatus}.");

            return Task.CompletedTask;
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

    public enum Status
    {
        Occupied, Free
    }
}