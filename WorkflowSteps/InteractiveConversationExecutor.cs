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
  private AgentSession? _session;
  private readonly List<ChatMessage> _conversationHistory;

  public InteractiveConversationExecutor(AIAgent agent)
    : base(nameof(InteractiveConversationExecutor))
  {
    _agent = agent ?? throw new ArgumentNullException(nameof(agent));
    _conversationHistory = new List<ChatMessage>();
  }

  //protected override async ValueTask OnInitializingAsync(
  //  IWorkflowContext context,
  //  CancellationToken cancellationToken = default)
  //{
  //  _session = await _agent.CreateSessionAsync(cancellationToken: cancellationToken);
  //  await base.OnInitializingAsync(context, cancellationToken);
  //}

  [MessageHandler]
  public override async ValueTask<List<ChatMessage>> HandleAsync(
    string message,
    IWorkflowContext context,
    CancellationToken cancellationToken = default)
  {
    try
    {
      await _agent.StartWorkflowAgentConversationAsync(message);
      // Erste Frage hinzufügen
      //_conversationHistory.Add(new ChatMessage(ChatRole.User, initialInput));
      //Console.WriteLine($"\nYou: {initialInput}");

      //// Interaktive Loop für Konversation
      //while (true)
      //{
      //  // Agent antwortet
      //  AgentResponse response = await _agent.RunAsync(_conversationHistory, _session, cancellationToken: cancellationToken);

      //  foreach (var message in response.Messages)
      //  {
      //    if (!string.IsNullOrWhiteSpace(message.Text))
      //    {
      //      Console.WriteLine($"\n{message.AuthorName}: {message.Text}");
      //    }
      //    _conversationHistory.Add(message);
      //  }

      //  // User Input für Follow-up Fragen
      //  Console.Write("\nYou: ");
      //  string? userInput = Console.ReadLine();

      //  if (string.IsNullOrWhiteSpace(userInput))
      //    continue;

      //  // Exit-Bedingung
      //  if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
      //      userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
      //  {
      //    Console.WriteLine("Goodbye!");
      //    break;
      //  }

      //  // Check ob die Reise vollständig ist (z.B. über Agent-Response)
      //  if (IsReiseVollstaendig(response))
      //  {
      //    Console.WriteLine("\n✓ Ihre Reiseinformationen sind vollständig!");
      //    break;
      //  }

      //  // Neue User-Eingabe zur History hinzufügen
      //  _conversationHistory.Add(new ChatMessage(ChatRole.User, userInput));
      //}

      // Konversation als Output weitergeben
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