using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFramework.Agents;

internal sealed class TravelAgencyGroupChatManager : GroupChatManager
{
  private readonly IReadOnlyList<AIAgent> _agents;
  private readonly AIAgent _expert;

  public TravelAgencyGroupChatManager(IReadOnlyList<AIAgent> agents, AIAgent expert)
  {
    this._agents = agents;
    this._expert = expert;
  }

  protected override ValueTask<AIAgent> SelectNextAgentAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default)
  {
    if (history.Count == 0)
    {
      throw new InvalidOperationException("Conversation is empty; cannot select next speaker.");
    }

    //if (history.Count == 1)
    //{
    //  return new ValueTask<AIAgent>(this._expert);
    //}

    //TODO check continent and then select the right agent based on that.
    AIAgent continentSpecialist = this._agents[0];//.First(a => a.Name == "DevOpsEngineer"); //name: $"{continent} Expert",
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
