using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFramework.Agents;

internal class TravelAgencyExpert
{
    public static AIAgent Create()
    {
        Console.WriteLine($"Creating Travel Agency Employee...");

        string instructions = File.ReadAllText(
          Path.Combine(AppContext.BaseDirectory, $"instructions/travel_agency_employee.instructions.txt"));


        return LocalAgent.Create()
         .AsAIAgent(
            instructions,
            name: "Empfangsdame Heidi",
            description: "Erster Ansprechpartner im Reisebüro"
            )
         .AsBuilder() //TODO was ist AsBuilder und was Build()
         .Build();
    }
}
