using Microsoft.Agents.AI;

namespace AgentFramework;

public static class AIAgentContinentExtension
{
  private static AgentSession? SharedSession;

  extension(AIAgent agent)
  {
    public async Task StartConversationAsync(string question)
    {
      // Persist and restore later
      //Treat AgentSession as an opaque state object and restore it with the same agent/provider configuration that created it.
      SharedSession ??= await agent.CreateSessionAsync();
      //var serialized = await agent.SerializeSessionAsync(session); //Idea. Session are passed between the agents
      //Console.WriteLine("Serialized session: " + serialized); // Serialized session: {"conversationId":"resp_04196b0246fa0d0700699951f1bc1c8194884b3a5d9f506c53"}
      //AgentSession resumed = await agent.DeserializeSessionAsync(serialized);
      //ChatClientAgentSession typedSession = (ChatClientAgentSession)session;
      //Console.WriteLine(typedSession.ConversationId);

      //var runOptions = new AgentRunOptions { RunMiddleware = DebugMiddleware };
      AgentRunOptions options = new() //pass run level middleware here
      {
        AllowBackgroundResponses = true,
      };

      try
      {
        Console.WriteLine(question);
        var response = await agent.RunAsync(question, SharedSession, options);
        //var response = await agent.RunAsync("What country should I vist when I fly to Oceania? Please make a research with wikipedia.", session, options);
        // Continue to poll until the final response is received
        // The initial call may complete immediately (no continuation token) or start a background operation (with continuation token)
        while (response.ContinuationToken is not null)
        {
          // Wait before polling again.
          await Task.Delay(TimeSpan.FromSeconds(2));

          options.ContinuationToken = response.ContinuationToken; //store continuation tokens persistently for operations that may span user sessions
          response = await agent.RunAsync(SharedSession, options);
        }
        Console.WriteLine(response.Text);
        //Console.WriteLine("Usage Details: " + JsonSerializer.Serialize(response.Usage));
        //Console.WriteLine(JsonSerializer.Serialize(response.Messages));
      }
      catch (Exception ex)
      {
        Console.WriteLine("An error occurred: " + ex.Message);
      }

      //Stream the response; Cant stream when output schmea format erzwungen ist, da die Agenten in diesem Fall die Antwort erst komplett generieren müssen, um sie gegen das Schema zu validieren.
      //AgentResponseUpdate? latestReceivedUpdate = null;
      //
      //Console.WriteLine(question);
      //await foreach (var update in agent.RunStreamingAsync(question, SharedSession, options))
      //{
      //  Console.Write(update);
      //}
      //await foreach (var update in agent.RunStreamingAsync("What country should I vist when I fly to Oceania?", session, options))
      //{
      //  Console.Write(update.Text);

      //  latestReceivedUpdate = update;

      //  // Simulate an interruption
      //  break;
      //}
    }
  }
}
