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
    if (history.Count == 0)
    {
      throw new InvalidOperationException("Conversation is empty; cannot select next speaker.");
    }

    //TODO check continent and then select the right agent based on that.
    AIAgent continentSpecialist = this._agents.First(a => a.Name == "DevOpsEngineer"); //name: $"{continent} Expert",
    return new ValueTask<AIAgent>(continentSpecialist);
  }

  protected override ValueTask<bool> ShouldTerminateAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default)
  {
    //var last = history.LastOrDefault();
    //bool shouldTerminate = last?.AuthorName == _orchestratorName &&
    //    last.Text?.Contains("approve", StringComparison.OrdinalIgnoreCase) == true;

    //return ValueTask.FromResult(shouldTerminate);

    return base.ShouldTerminateAsync(history, cancellationToken);
  }
}
