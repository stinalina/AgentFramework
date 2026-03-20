using AgentFramework.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace AgentFramework.Workflows;

internal class OrchestratorWorkflowAgent
{
  public static async Task<Workflow> CreateWorkflowAsync(string endpoint, string deploymentName)
  {
    AIAgent handOffWorkflowAsAgent = await HandoffWorkflowAgent.GetContinentWorkflowAgentAsync(endpoint, deploymentName);
    AIAgent bookingAgent = await BookingExpert.GetAgentAsync(endpoint, deploymentName);

    return AgentWorkflowBuilder
      .CreateGroupChatBuilderWith(participants => new TravelAgencyGroupChatManager(participants)
      {
        MaximumIterationCount = 2,
      })
      .AddParticipants(handOffWorkflowAsAgent, bookingAgent)
      .Build();

    //Console.WriteLine("=== WORKFLOW STRUCTURE ===");
    //Console.WriteLine(workflow.ToDotString());
    //Console.WriteLine("=========================\n");
    //To create an image file from the DOT format, you can use GraphViz tools with the following command
    // dotnet run | tail -n +20 | dot -Tpng -o workflow.png
  }
}
