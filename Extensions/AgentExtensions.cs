using Microsoft.Agents.AI;

namespace AgentFramework.Extensions;

public static class AgentExtensions
{
    private static AgentSession? SharedSession;

    extension(AIAgent agent)
    {
        public async Task StartConversationAsync(string? initialQuestion = null)
        {
            // Persist and restore later
            //Treat AgentSession as an opaque state object and restore it with the same agent/provider configuration that created it.
            SharedSession ??= await agent.CreateSessionAsync();
            //var serialized = await agent.SerializeSessionAsync(session); //Idea. Session are passed between the agents
            //Console.WriteLine("Serialized session: " + serialized); // Serialized session: {"conversationId":"resp_04196b0246fa0d0700699951f1bc1c8194884b3a5d9f506c53"}
            //AgentSession resumed = await agent.DeserializeSessionAsync(serialized);
            //ChatClientAgentSession typedSession = (ChatClientAgentSession)session;
            //Console.WriteLine(typedSession.ConversationId);

            AgentRunOptions options = new() //pass run level middleware here
            {
                AllowBackgroundResponses = false, // Deaktiviert, um Continuation-Fehler zu vermeiden
            };

            // Process initial question if provided
            if (!string.IsNullOrEmpty(initialQuestion))
            {
                Console.WriteLine($"You: {initialQuestion}");
                await ProcessQuestionAsync(agent, initialQuestion, options);
            }

            // Interactive loop
            while (true)
            {
                Console.Write("You: ");
                string? userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                    continue;

                if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                await agent.ProcessQuestionAsync(userInput, options);
            }
        }

        private async Task ProcessQuestionAsync(string question, AgentRunOptions options)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            try
            {
                // Reset continuation token for new question
                //otherwise: An error occurred: Input messages are not allowed when continuing a background response using a continuation token.
                options.ContinuationToken = null;

                Console.WriteLine("Thinking...");
                int currentLineCursor = Console.CursorTop;

                var response = await agent.RunAsync(question, SharedSession, options);
                Console.SetCursorPosition(0, currentLineCursor - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor - 1);

                // Continue to poll until the final response is received
                // The initial call may complete immediately (no continuation token) or start a background operation (with continuation token)
                while (response.ContinuationToken is not null)
                {
                    // Wait before polling again.
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    options.ContinuationToken = response.ContinuationToken; //store continuation tokens persistently for operations that may span user sessions
                    response = await agent.RunAsync(SharedSession, options);
                }
                Console.WriteLine($"\nAgent: {response.Text}\n"); //TODO nicht Agent, sondern z.B. "Europe Expert" oder so, je nachdem welcher Agent antwortet
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}\n");
            }
            Console.ResetColor();
        }
    }
}