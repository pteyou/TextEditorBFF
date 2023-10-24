using Grpc.Core;
using TextEditor.Messages.gRPC;
using TextEditorBFF.Clients;

namespace TextEditorBFF.Services;

public class SanityCheckService : CheckService.CheckServiceBase
{
    private readonly ILogger<SanityCheckService> _logger;

    public SanityCheckService(ILogger<SanityCheckService> logger)
    {
        _logger = logger;
    }

    public override async Task<CheckServiceReply> BackSanitiyCheck(BaseServiceInfo request, ServerCallContext context)
    {
        return await SanityChecker.Call();
    }
}