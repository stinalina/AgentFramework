using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AgentFramework.Extensions;

internal static class WorkflowAgentExtensions
{
    private static readonly List<ChatMessage> debuggingHistory = new();

    extension(AIAgent workflowAgent)
    {
        public async Task<List<ChatMessage>> StartWorkflowAgentConversationAsync(string initialQuestion)
        {
            var messages = new List<ChatMessage>();
            try
            {
                AgentSession session = await workflowAgent.CreateSessionAsync();

                if (!string.IsNullOrEmpty(initialQuestion))
                {
                    Console.WriteLine($"You: {initialQuestion}");
                    messages = await workflowAgent.ProcessWorkflowQuestionAsync(messages, initialQuestion, session);
                }

                while (true)
                {
                    Console.Write("\nYou: ");
                    string? userInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(userInput))
                        continue;

                    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                     userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Goodbye!");
                        break;
                    }

                    if (userInput.Equals("buchen", StringComparison.OrdinalIgnoreCase) ||
                     userInput.Equals("vollständig", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Ihre Reise wird nun erstellt. Bitte haben Sie einen Moment Geduld.");
                        break;
                    }

                    messages = await workflowAgent.ProcessWorkflowQuestionAsync(messages, userInput, session);
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(JsonSerializer.Serialize(messages));
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        Console.WriteLine($"Inner Inner Exception: {ex.InnerException.InnerException.Message}");
                    }
                }
                Console.WriteLine($"Stack Trace: {ex.StackTrace}\n");
            }
            return messages;
        }

        private async Task<List<ChatMessage>> ProcessWorkflowQuestionAsync(List<ChatMessage> messages, string userInput, AgentSession session)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            try
            {
                messages.Add(new ChatMessage(ChatRole.User, userInput));

                AgentResponse response = await workflowAgent.RunAsync(messages, session);

                foreach (ChatMessage message in response.Messages)
                {
                    if (!string.IsNullOrWhiteSpace(message.Text))
                    {
                        Console.WriteLine($"\n{message.AuthorName}: {message.Text}");
                    }
                }
                messages.AddRange(response.Messages);
                return messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}\n");
                return [];
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}