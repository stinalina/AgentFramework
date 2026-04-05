using AgentFramework.Extensions;
using AgentFramework.Models;
using AgentFramework.Workflows;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace AgentFramework.Entdpoints;

public static class TravelAgentEndpoints
{
  private static string _endpoint;
  private static string _deploymentName;

  public static void MapTravelAgentEndpoints(this WebApplication app, string endpoint, string deploymentName)
  {
    _endpoint = endpoint;
    _deploymentName = deploymentName;

    var group = app.MapGroup("api/travelagent/conversation");

    group.MapPost("stream", StreamConversation)
      .WithName(nameof(StreamConversation))
      .Produces(StatusCodes.Status400BadRequest, contentType: "application/json")
      .Produces(StatusCodes.Status200OK, contentType: "text/event-stream");
  }

  public static Results<BadRequest<string>, ServerSentEventsResult<ResponseStreamChunk>> StreamConversation(
    [FromBody] string question, CancellationToken cancellationToken)
  {
    return TypedResults.ServerSentEvents(GetChunks(question, cancellationToken), eventType: "agent-update");
  }

  private static async IAsyncEnumerable<ResponseStreamChunk> GetChunks(string question,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    Workflow? workflow = null;
    ResponseStreamChunk? errorChunk = null;

    try
    {
      workflow = await SequentialWorkflow.GetSequentialWorkflowAsync(_endpoint, _deploymentName);
    }
    catch (Exception ex)
    {
      errorChunk = new ResponseStreamChunk(null, $"Error creating the workflow: {ex.Message}", IsCompleted: true);
    }

    if (errorChunk is not null)
    {
      yield return errorChunk;
      yield break;
    }

    await foreach (ResponseStreamChunk chunk in workflow!.ExecuteWorkflowStreamAsync(question, cancellationToken))
    {
      yield return chunk;
    }
  }
}
