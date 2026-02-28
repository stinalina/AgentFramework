using System.Collections;
 using Microsoft.Agents.AI.Workflows;

namespace AgentFramework.WorkflowSteps;

internal sealed class CustomEvent(string message) : WorkflowEvent(message) { }

internal sealed partial class CustomExecutor() : Executor("CustomExecutor")
{
  protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder)
  {
    throw new NotImplementedException();
  }

  [MessageHandler]
  private async ValueTask HandleAsync(string message, IWorkflowContext context)
  {
    await context.AddEventAsync(new CustomEvent($"Processing message: {message}"));
    // Executor logic...
  }
}