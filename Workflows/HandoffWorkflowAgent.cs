using AgentFramework.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace AgentFramework.Workflows;

internal class HandoffWorkflowAgent
{
    public static AIAgent GetContinentWorkflowAgentAsync()
    {
        Workflow instance = CreateWorkflow();

        return instance
          .AsAIAgent(
            id: "workflow-agent",
            name: "Continent Expert Handoff Workflow Agent",
            description: "A multi-agent workflow for continent experts handoff",
            includeExceptionDetails: true,
            executionEnvironment: InProcessExecution.Default);
    }

    private static Workflow CreateWorkflow()
    {
        AIAgent travelAgencyExpert = TravelAgencyExpert.Create();

        Dictionary<string, AIAgent> continentExperts = AgentFactory.CreateAgentExperts();
        AIAgent africaExpert = continentExperts[Continent.Africa.ToString()];
        AIAgent americaExpert = continentExperts[Continent.America.ToString()];
        AIAgent asiaExpert = continentExperts[Continent.Asia.ToString()];
        AIAgent europeExpert = continentExperts[Continent.Europe.ToString()];
        AIAgent oceaniaExpert = continentExperts[Continent.Oceania.ToString()];

        var allContinentAgents = new List<AIAgent>() { africaExpert, americaExpert, asiaExpert, europeExpert, oceaniaExpert };

        return AgentWorkflowBuilder.CreateHandoffBuilderWith(travelAgencyExpert) // Step 8
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
