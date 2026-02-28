using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgentFramework.WorkflowSteps;

internal sealed partial class TravelAgencyExecutor() : Executor(nameof(TravelAgencyExecutor))
{
  protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder)
  {
    routeBuilder.AddHandler<string>(HandleAsync);

    return routeBuilder;
  }

  [MessageHandler]
  private async ValueTask HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken)
  {
    string msg = "Willkommen im Reisebüro!";
    await context.SendMessageAsync(msg, cancellationToken);
  }
}