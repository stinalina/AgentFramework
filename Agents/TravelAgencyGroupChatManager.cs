using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFramework.Agents;

internal sealed class TravelAgencyGroupChatManager : GroupChatManager
{
  private readonly IReadOnlyList<AIAgent> _agents;

  public TravelAgencyGroupChatManager(IReadOnlyList<AIAgent> agents)
  {
    this._agents = agents;
  }

  protected override ValueTask<AIAgent> SelectNextAgentAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default)
  {
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"\nIteration {this.IterationCount + 1} of {this.MaximumIterationCount}");
    Console.ResetColor();

    if (history.Count == 0)
    {
      throw new InvalidOperationException("Conversation is empty; cannot select next speaker.");
    }

    if (this.IterationCount == this.MaximumIterationCount - 1)
    {
      var finalAgent = this._agents.FirstOrDefault(a => a.Name == "Reisebüroangestellter 2");
      ArgumentNullException.ThrowIfNull(finalAgent, "Reisebüroangestellter 2 agent not found in agents list.");
      return new ValueTask<AIAgent>(finalAgent);
    }

    return new ValueTask<AIAgent>(this._agents[0]);
  }

  protected override ValueTask<bool> ShouldTerminateAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default)
  {
    //bool shouldTerminate = history.LastOrDefault()?.AuthorName == "Reisebüroangestellter 2";
    //if (shouldTerminate)
    //{
    //  Console.ForegroundColor = ConsoleColor.Green;
    //  Console.WriteLine("Reisebüro Center: Vielen Dank für Ihre Anfrage! Ihre Reise wurde erfolgreich erstellt.");
    //  Console.ResetColor();
    //}
    //return ValueTask.FromResult(shouldTerminate);
    //TODO print session?!
    return base.ShouldTerminateAsync(history, cancellationToken);
  }
}
