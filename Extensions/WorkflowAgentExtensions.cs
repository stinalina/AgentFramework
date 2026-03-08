using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgentFramework.Extensions;

internal static class WorkflowAgentExtensions
{
  private static readonly List<ChatMessage>  debuggingHistory = new();

  extension(AIAgent workflowAgent)
  {
    public async Task<List<ChatMessage>> StartWorkflowAgentConversationAsync(string initialQuestion)
    {
      var messages = new List<ChatMessage>();
      try
      {
        AgentSession session = await workflowAgent.CreateSessionAsync();
        messages = await workflowAgent.ProcessWorkflowQuestionAsync(messages, initialQuestion, session); 

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

        Console.WriteLine(JsonSerializer.Serialize(debuggingHistory));
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
      try
      {
        var lastAgent = messages.LastOrDefault(m => m.Role == ChatRole.Assistant)?.AuthorName ?? string.Empty;

        messages.Add(new ChatMessage(ChatRole.User, userInput));
        debuggingHistory.Add(new ChatMessage(ChatRole.User, userInput));

        AgentResponse response = await workflowAgent.RunAsync(messages, session);
        var currAgent = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant)?.AuthorName ?? string.Empty;

        if (!String.IsNullOrEmpty(lastAgent) && lastAgent != currAgent)
        {
          Console.WriteLine("HandOff Occured");
        }

        debuggingHistory.AddRange(response.Messages);

        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (ChatMessage message in response.Messages)
        {
          if (!string.IsNullOrWhiteSpace(message.Text))
          {
            Console.WriteLine($"\n{message.AuthorName}: {message.Text}");
          }
        }
        Console.ResetColor();

        messages.AddRange(response.Messages.Where(x => x.Role == ChatRole.Assistant || x.Role == ChatRole.User));
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}\n");
      }

      return messages;
    }
  }
}