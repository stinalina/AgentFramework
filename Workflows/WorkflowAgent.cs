using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace AgentFramework.Workflows;

internal class WorkflowAgent
{
  private static Workflow _instance;

  public static async Task<AIAgent> GetContinentWorkflowAgent()
  {
    _instance ??= await CreateWorkflowAsync();
    return _instance.AsAgent(
      id: "workflow-agent",
      name: "Continet Expert Handoff Workflow Agent",
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

    var allAgents = new List<AIAgent>() { africaExpert, americaExpert, asiaExpert, europeExpert, oceaniaExpert };

    return AgentWorkflowBuilder.CreateHandoffBuilderWith(africaExpert)
      .WithHandoffs(allAgents.Except([africaExpert]), africaExpert, "The question is related to the africa continent.")
      .WithHandoffs(allAgents.Except([americaExpert]), americaExpert, "The question is related to the america continent.")
      .WithHandoffs(allAgents.Except([asiaExpert]), asiaExpert, "The question is related to the asia continent.")
      .WithHandoffs(allAgents.Except([europeExpert]), europeExpert, "The question is related to the europe continent.")
      .WithHandoffs(allAgents.Except([oceaniaExpert]), oceaniaExpert, "The question is related to the oceania continent.")
      .Build();

    //Console.WriteLine("=== WORKFLOW STRUCTURE ===");
    //Console.WriteLine(workflow.ToDotString());
    //Console.WriteLine("=========================\n");
    //To create an image file from the DOT format, you can use GraphViz tools with the following command
    // dotnet run | tail -n +20 | dot -Tpng -o workflow.png
  }
}
