using AgentFramework.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgentFramework.Workflows;

internal class OrchestratorWorkflowAgent
{
  //private static Workflow _instance;

  //public static async Task<AIAgent> GetContinentWorkflowAgent()
  //{
  //  _instance ??= await CreateWorkflowAsync();
  //  return _instance.AsAgent(
  //    id: "workflow-agent",
  //    name: "Continent Expert Handoff Workflow Agent",
  //    description: "A multi-agent workflow for continent experts handoff",
  //    checkpointManager: CheckpointManager.CreateInMemory(),
  //    executionEnvironment: InProcessExecution.Default
  //  );
  //}

  public static async Task<Workflow> CreateWorkflowAsync()
  {
    //AIAgent africaExpert = await AgentFactory.GetContinentExpert(Continent.Africa);
    //AIAgent americaExpert = await AgentFactory.GetContinentExpert(Continent.America);
    //AIAgent asiaExpert = await AgentFactory.GetContinentExpert(Continent.Asia);
    //AIAgent europeExpert = await AgentFactory.GetContinentExpert(Continent.Europe);
    //AIAgent oceaniaExpert = await AgentFactory.GetContinentExpert(Continent.Oceania);

    //var continentAgents = new List<AIAgent>() { africaExpert, americaExpert, asiaExpert, europeExpert, oceaniaExpert };
    AIAgent travelAgencyExpert = await TravelAgencyExpert.GetExpertAsync();

    AIAgent handOffWorkflowAsAgent = await HandoffWorkflowAgent.GetContinentWorkflowAgentAsync();

    TravelAgencyGroupChatManager manager = new(new List<AIAgent>() { handOffWorkflowAsAgent }, travelAgencyExpert)
    {
      MaximumIterationCount = 3
    };

    return AgentWorkflowBuilder
      .CreateGroupChatBuilderWith(_ => manager)
      .AddParticipants(handOffWorkflowAsAgent)
      .Build();


    //Console.WriteLine("=== WORKFLOW STRUCTURE ===");
    //Console.WriteLine(workflow.ToDotString());
    //Console.WriteLine("=========================\n");
    //To create an image file from the DOT format, you can use GraphViz tools with the following command
    // dotnet run | tail -n +20 | dot -Tpng -o workflow.png
  }
}
