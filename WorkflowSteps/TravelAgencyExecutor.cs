using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.RegularExpressions;

namespace AgentFramework.WorkflowSteps;

internal sealed partial class TravelAgencyExecutor() : Executor(nameof(TravelAgencyExecutor))
{
  private const string StateKey = "CustomExecutorState";

  private List<string> messages = new();

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

  protected override ValueTask OnCheckpointingAsync(IWorkflowContext context, CancellationToken cancellationToken = default)
  {
    //To ensure that the state of an executor is captured in a checkpoint,
    //the executor must override the OnCheckpointingAsync method and save its state to the workflow context.

    return context.QueueStateUpdateAsync(StateKey, this.messages);
    //return base.OnCheckpointingAsync(context, cancellationToken);
  }

  protected override async ValueTask OnCheckpointRestoredAsync(IWorkflowContext context, CancellationToken cancellation = default)
  {
    //Also, to ensure the state is correctly restored when resuming from a checkpoint, the executor must override the
    //OnCheckpointRestoredAsync method and load its state from the workflow context.
    this.messages = await context.ReadStateAsync<List<string>>(StateKey).ConfigureAwait(false);
  }
}