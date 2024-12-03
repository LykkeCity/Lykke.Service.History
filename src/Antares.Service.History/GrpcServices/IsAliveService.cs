using System.Threading.Tasks;
using Antares.Service.History.GrpcContract.Monitoring;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lykke.Common;

namespace Antares.Service.History.GrpcServices
{
    //TODO:
    public class IsAliveService : MonitoringService.MonitoringServiceBase
    {
        public override Task<IsAliveResponse> IsAlive(IsAliveRequest request, ServerCallContext context)
        {
            return Task.FromResult(new IsAliveResponse()
            {
                Name = AppEnvironment.Name,
                Version = AppEnvironment.Version,
                //StartedAt = Timestamp.FromDateTime(ApplicationInformation.StartedAt),
                Env = AppEnvironment.EnvInfo,
                //HostName = ApplicationEnvironment.HostName,
            });
        }
    }
}
