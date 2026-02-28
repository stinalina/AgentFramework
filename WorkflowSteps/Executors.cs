using Microsoft.Agents.AI.Workflows;

namespace AgentFramework.WorkflowSteps;

// The class must be marked partial to enable source generation.
internal sealed partial class PassInfoExecutor() : Executor(nameof(PassInfoExecutor))
{
  protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder)
  {
    //// Map incoming string messages to the string message handler
    //routeBuilder.MapInput<string>(HandleAsync);

    //// Map incoming TripInfo objects to the TripInfo handler
    //routeBuilder.MapInput<TripInfo>(HandleTripInfoAsync);

    return routeBuilder;
  }

  [MessageHandler]
  private ValueTask<string> HandleAsync(string message, IWorkflowContext context) //Den RückgabTyp suchst du aus
  {
    string result = message.ToUpperInvariant();
    return ValueTask.FromResult(result); // Return value is automatically sent to connected executors
  }

  //[MessageHandler]
  //private async ValueTask HandleAsync(string message, IWorkflowContext context)
  //{
  //  string result = message.ToUpperInvariant();
  //  await context.SendMessageAsync(result); // Manually send messages to connected executors
  //}

  [MessageHandler]
  private ValueTask<TripInfo> HandleTripInfoAsync(string message, IWorkflowContext context)
  {
    //TODO transform message to trip info and pass it to the next agent
    TripInfo result = TripInfo.Transform(message);
    return ValueTask.FromResult(result);
  }
}
