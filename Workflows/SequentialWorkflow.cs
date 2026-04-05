using AgentFramework.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace AgentFramework.Workflows;

internal class SequentialWorkflow
{
  private static Workflow? _instance;
  private static string? _endpoint;
  private static string? _deploymentName;

  public static async Task<Workflow> GetSequentialWorkflowAsync(string endpoint, string deploymentName)
  {
    _endpoint = endpoint;
    _deploymentName = deploymentName;
    return _instance ??= await CreateWorkflowAsync();
  }

  private static async Task<Workflow> CreateWorkflowAsync()
  {
    AIAgent handOffWorkflowAsAgent = await HandoffWorkflowAgent.GetContinentWorkflowAgentAsync(_endpoint!, _deploymentName!);
    AIAgent bookingAgent = await BookingExpert.GetAgentAsync(_endpoint!, _deploymentName!);

    return AgentWorkflowBuilder.BuildSequential([handOffWorkflowAsAgent, bookingAgent]);
  }
}
