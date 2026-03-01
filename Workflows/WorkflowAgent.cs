using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFramework.Workflows;

internal class WorkflowAgent
{
  //public static async Task<IList<AIAgent>> GetContinentWorkflowAgent()
  public static async Task<Workflow> GetContinentWorkflowAgent()

  {
    AIAgent africaExpert = await AgentFactory.GetContinentExpert(Continent.Africa);
    AIAgent americaExpert = await AgentFactory.GetContinentExpert(Continent.America);
    AIAgent asiaExpert = await AgentFactory.GetContinentExpert(Continent.Asia);
    AIAgent europeExpert = await AgentFactory.GetContinentExpert(Continent.Europe);
    AIAgent oceaniaExpert = await AgentFactory.GetContinentExpert(Continent.Oceania);

    var allAgents = new List<AIAgent>() { africaExpert, americaExpert, asiaExpert, europeExpert, oceaniaExpert };

    Workflow workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(africaExpert)
      .WithHandoffs(africaExpert, allAgents.Except([africaExpert])) //Handoff Rules. Welcher Agent an welchen delegieren kann
      .WithHandoffs(americaExpert, allAgents.Except([americaExpert]))
      .WithHandoffs(asiaExpert, allAgents.Except([asiaExpert]))
      .WithHandoffs(europeExpert, allAgents.Except([europeExpert]))
      .WithHandoffs(oceaniaExpert, allAgents.Except([oceaniaExpert]))
      .Build();

    //Console.WriteLine(workflow.ToDotString());
    //To create an image file from the DOT format, you can use GraphViz tools with the following command
    // dotnet run | tail -n +20 | dot -Tpng -o workflow.png
    //Console.WriteLine(workflow.ToMermaidString());

    return workflow;
    //Console.WriteLine("=== WORKFLOW STRUCTURE ===");
    //Console.WriteLine(workflow.ToDotString());
    //Console.WriteLine("=========================\n");

    //return workflow.AsAgent(
    //  id: "workflow-agent",
    //  name: "Continet Expert Handoff Workflow Agent",
    //  description: "A multi-agent workflow for continent experts handoff",
    //  checkpointManager: CheckpointManager.CreateInMemory(),
    //  executionEnvironment: InProcessExecution.Default
    //);
  }
}
