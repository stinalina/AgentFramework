using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFramework.Agents;

internal enum Continent
{
    Africa,
    America,
    Europe,
    Asia,
    Oceania,
}

internal static class AgentFactory
{
    public static Dictionary<string, AIAgent> CreateAgentExperts()
    {
        var continentExperts = new Dictionary<string, AIAgent>();

        foreach (var continent in Enum.GetValues<Continent>())
        {
            AIAgent agent = CreateContinentExpert(continent);
            continentExperts.Add(continent.ToString(), agent);
        }

        return continentExperts;
    }

    private static AIAgent CreateContinentExpert(Continent continent)
    {
        Console.WriteLine($"Creating {continent} Expert...");

        string instructions = File
          .ReadAllText(Path.Combine(AppContext.BaseDirectory, $"instructions/continent_agent.instructions.txt"))
          .Replace("<continent>", continent.ToString());

        var wikipediaTools = Tools.WikipediaAIFunctions();

        return LocalAgent.Create()
           .AsBuilder()
           .ConfigureOptions(options =>
           {
               options.Temperature = 0.3f;
               options.TopP = 0.8f;
               options.MaxOutputTokens = 4096;
               options.ToolMode = ChatToolMode.Auto;
           })
           .Build()
          .AsAIAgent(
            instructions,
            name: $"{continent} Expert",
            description: $"An expert in all questions related to {continent}.",
            tools: [
            ..wikipediaTools,
             ]
           )
          .AsBuilder()
          .Use(CustomMiddleware.FunctionMiddleware_LogUsedTool)
          .Build();
    }
}
