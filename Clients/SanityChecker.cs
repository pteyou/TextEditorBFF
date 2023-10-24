using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using TextEditor.Messages.gRPC;
using TextEditor.Configuration;

namespace TextEditorBFF.Clients;

public static class SanityChecker
{
    private static readonly string CoreHostname; 
    private static readonly int GrpcCorePort;
    private static readonly int TimeoutInSeconds;

    
    static SanityChecker()
    {
        var config = Hosting.Instance.Config;
        GrpcCorePort = config.GrpcCorePort;
        CoreHostname = config.Hostname;
        TimeoutInSeconds = config.SanityCheckTimeoutInSeconds;
    } 

    public static async Task<CheckServiceReply> Call()
    {
        var channel = GrpcChannel.ForAddress($"http" + $"://{CoreHostname}:{GrpcCorePort}");
        var client = new CheckService.CheckServiceClient(channel);
        CheckServiceReply response;
        try
        {
            response = await client.BackSanitiyCheckAsync(new BaseServiceInfo {
                CallDateTime =  Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                ServiceName = nameof(SanityChecker),
                Ok = true,
                RequestTimeoutInSeconds = TimeoutInSeconds
            }, deadline: DateTime.UtcNow.AddSeconds(TimeoutInSeconds));
        }
        catch (RpcException ex)
        {
            response = new CheckServiceReply {
                Ok = false,
                ServiceInfo = new BaseServiceInfo {
                    CallDateTime =  Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                    ServiceName = nameof(SanityChecker),
                    Ok = false,
                    StatusMessage = ex.Status.Detail
                }
            };
        }
        return response;
    }

}