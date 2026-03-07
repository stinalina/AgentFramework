using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFramework.WorkflowSteps;
// erwartet ChatMessages und retuned einen string...
internal sealed partial class TripInfoOutputExecutor() : Executor<List<ChatMessage>, string>(nameof(TripInfoOutputExecutor))
{
  private const string StateKey = "TripInfoOutputState";

  private List<string> messages = new();

  [MessageHandler]
  public override async ValueTask<string> HandleAsync(List<ChatMessage> message, IWorkflowContext context, CancellationToken cancellationToken = default)
  {
    var msg = "Ihre Reise wurde erfolgreich gebucht.";

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[{this.Id}] {msg}");
    Console.ResetColor();

    await context.SendMessageAsync(msg, cancellationToken);
    return msg;
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