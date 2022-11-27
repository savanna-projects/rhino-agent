using Microsoft.AspNetCore.SignalR.Client;

using Rhino.Controllers.Models;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IWorkerRepository
    {
        HubConnection Connection { get; }
        void StopWorker();
        void RestartWorker();
        void StartWorker();
        string GetWorkerStatus();
    }
}
