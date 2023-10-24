using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using TextEditor.Messages.gRPC;
using TextEditorBFF.Clients;
using TextEditorBFF.DataStructures;

namespace TextEditorBFF.Services;

public class TextGenerationService : NLPTasks.NLPTasksBase
{
    private readonly ILogger<TextGenerationService> _logger;
    private TernarySearchTree _innerTree;

    public TextGenerationService(ILogger<TextGenerationService> logger)
    {
        _logger = logger;
        _innerTree = new TernarySearchTree();
    }

    public override async Task<TextGenerationReply> GenerateText(TextGenerationRequest request, ServerCallContext context)
    {
        var result = new TextGenerationReply();
        var proposals = _innerTree.GetSentencesForCompletion(request.InputText, request.RequiredExtraSize).ToList();
        if( proposals.Count >= 0 )
        {
            proposals.Sort(delegate (Tuple<string[], decimal> a, Tuple<string[], decimal> b)
                {
                    return a.Item2 >= b.Item2 ? 1 : -1;
                });
            result.ServiceInfo = new BaseServiceInfo 
                {
                    CallDateTime =  Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                    ServiceName = nameof(TextGenerator),
                    Ok = true,
                };
            result.OuptutText.Add(proposals.Select(p => string.Join(" ", p.Item1)).Take(Math.Min(request.NumReturnedSequences, proposals.Count)));
        }
        if(proposals.Count < request.NumReturnedSequences)
        {
            ThreadPool.QueueUserWorkItem(async (state) => await FeedTreeAsync(request));
        }
        return await Task.FromResult(result);
    }

    private async Task FeedTreeAsync(TextGenerationRequest request)
    {
        var reply = await TextGenerator.Call(request.InputText, request.RequiredExtraSize, request.NumReturnedSequences);
        foreach (var sentence in reply.OuptutText)
        {
            _innerTree.InsertSentence(sentence, 0.0m);
        }
    }
}