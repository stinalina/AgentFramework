using AgentFramework.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFramework.WorkflowSteps;

/// <summary>
/// Executor that creates an interactive loop within the workflow.
/// Allows user input collection while maintaining workflow structure.
/// </summary>
internal sealed class InteractiveConversationExecutor : Executor<string, List<ChatMessage>>
{
  private readonly AIAgent _agent;
  private List<ChatMessage> _conversationHistory;

  public InteractiveConversationExecutor(AIAgent agent)
    : base(nameof(InteractiveConversationExecutor))
  {
    _agent = agent ?? throw new ArgumentNullException(nameof(agent));
    _conversationHistory = new List<ChatMessage>();
  }

  [MessageHandler]
  public override async ValueTask<List<ChatMessage>> HandleAsync(
    string message,
    IWorkflowContext context,
    CancellationToken cancellationToken = default)
  {
    try
    {
      _conversationHistory = await _agent.StartWorkflowAgentConversationAsync(message);

      await context.SendMessageAsync(_conversationHistory, cancellationToken: cancellationToken);

      return _conversationHistory;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[{this.Id}] Error: {ex.Message}");
      throw;
    }
  }

  protected override ValueTask OnCheckpointingAsync(
    IWorkflowContext context,
    CancellationToken cancellationToken = default)
  {
    return context.QueueStateUpdateAsync("ConversationHistory", _conversationHistory, cancellationToken);
  }

  protected override async ValueTask OnCheckpointRestoredAsync(
    IWorkflowContext context,
    CancellationToken cancellationToken = default)
  {
    _conversationHistory.Clear();
    var restored = await context.ReadStateAsync<List<ChatMessage>>("ConversationHistory", cancellationToken);
    if (restored != null)
    {
      _conversationHistory.AddRange(restored);
    }
  }
}