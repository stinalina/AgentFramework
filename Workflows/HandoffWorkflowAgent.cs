using AgentFramework.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace AgentFramework.Workflows;

internal class HandoffWorkflowAgent
{
  private static Workflow _instance;

  public static async Task<AIAgent> GetContinentWorkflowAgentAsync()
  {
    _instance ??= await CreateWorkflowAsync();
    return _instance.AsAgent(
      id: "workflow-agent",
      name: "Continent Expert Handoff Workflow Agent",
      description: "A multi-agent workflow for continent experts handoff",
      checkpointManager: CheckpointManager.CreateInMemory(),
      executionEnvironment: InProcessExecution.Default
    );
  }

  private static async Task<Workflow> CreateWorkflowAsync()
  {
    AIAgent africaExpert = await AgentFactory.GetContinentExpert(Continent.Africa);
    AIAgent americaExpert = await AgentFactory.GetContinentExpert(Continent.America);
    AIAgent asiaExpert = await AgentFactory.GetContinentExpert(Continent.Asia);
    AIAgent europeExpert = await AgentFactory.GetContinentExpert(Continent.Europe);
    AIAgent oceaniaExpert = await AgentFactory.GetContinentExpert(Continent.Oceania);
    AIAgent travelAgencyExpert = await TravelAgencyExpert.GetExpertAsync();

    var allContinentAgents = new List<AIAgent>() { africaExpert, americaExpert, asiaExpert, europeExpert, oceaniaExpert };

    return AgentWorkflowBuilder.CreateHandoffBuilderWith(travelAgencyExpert)
      .WithHandoff(travelAgencyExpert, africaExpert, "Das angefragte Land bezieht sich auf Afrika.")
      .WithHandoff(travelAgencyExpert, americaExpert, "Das angefragte Land bezieht sich auf Amerika.")
      .WithHandoff(travelAgencyExpert, asiaExpert, "Das angefragte Land bezieht sich auf Asien.")
      .WithHandoff(travelAgencyExpert, europeExpert, "Das angefragte Land bezieht sich auf Europa.")
      .WithHandoff(travelAgencyExpert, oceaniaExpert, "Das angefragte Land bezieht sich auf Oceanien.")
      .WithHandoffs(allContinentAgents.Except([africaExpert]), africaExpert, "Das angefragte Land bezieht sich auf Afrika.")
      .WithHandoffs(allContinentAgents.Except([americaExpert]), americaExpert, "Das angefragte Land bezieht sich auf Amerika.")
      .WithHandoffs(allContinentAgents.Except([asiaExpert]), asiaExpert, "Das angefragte Land bezieht sich auf Asien.")
      .WithHandoffs(allContinentAgents.Except([europeExpert]), europeExpert, "Das angefragte Land bezieht sich auf Europa.")
      .WithHandoffs(allContinentAgents.Except([oceaniaExpert]), oceaniaExpert, "Das angefragte Land bezieht sich auf Oceanien.")
      .Build();

    //Console.WriteLine("=== WORKFLOW STRUCTURE ===");
    //Console.WriteLine(workflow.ToDotString());
    //Console.WriteLine("=========================\n");
    //To create an image file from the DOT format, you can use GraphViz tools with the following command
    // dotnet run | tail -n +20 | dot -Tpng -o workflow.png
  }
}
