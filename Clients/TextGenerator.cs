using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using TextEditor.Messages.gRPC;
using TextEditor.Configuration;

namespace TextEditorBFF.Clients;

public static class TextGenerator
{
    private static readonly string CoreHostname; 
    private static readonly int GrpcCorePort;
    private static readonly int TimeoutInSeconds;

    
    static TextGenerator()
    {
        var config = Hosting.Instance.Config;
        GrpcCorePort = config.GrpcCorePort;
        CoreHostname = config.Hostname;
        TimeoutInSeconds = config.TextGeneratorTimeoutInSeconds;
    }

    public static async Task<TextGenerationReply> Call(
        string inputText, 
        int requiredExtraSize, 
        int numReturnedSequences)
    {
        var channel = GrpcChannel.ForAddress($"http://{CoreHostname}:{GrpcCorePort}");
        var client = new NLPTasks.NLPTasksClient(channel);
        TextGenerationReply response;
        try
        {
            response = await client.GenerateTextAsync(new TextGenerationRequest {
                ServiceInfo = new BaseServiceInfo {
                    CallDateTime =  Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                    ServiceName = nameof(TextGenerator),
                    Ok = true,
                    RequestTimeoutInSeconds = TimeoutInSeconds
                },
                InputText = inputText,
                RequiredExtraSize = requiredExtraSize,
                NumReturnedSequences = numReturnedSequences
            }, deadline: DateTime.UtcNow.AddSeconds(TimeoutInSeconds));
        }
        catch (RpcException ex)
        {
            response = new TextGenerationReply {
                ServiceInfo = new BaseServiceInfo {
                    CallDateTime =  Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                    ServiceName = nameof(TextGenerator),
                    Ok = false,
                    StatusMessage = ex.Status.Detail
                }
            };
        }
        return response;
    } 
}