using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgentFramework.Extensions;

internal static class WorkflowAgentExtensions
{
  extension(AIAgent workflowAgent)
  {
    public async Task StartWorkflowAgentConversationAsync(string? initialQuestion = null)
    {
      try
      {
        AgentSession session = await workflowAgent.CreateSessionAsync();

        var initalQuestion = "Ich möchte eine Insel besuchen, auf der es warm ist mit Bergen zum Wandern.";
        var messages = new List<ChatMessage> { };

        Console.WriteLine("\nYou: " + initalQuestion);
        await workflowAgent.ProcessWorkflowQuestionAsync(messages, initalQuestion, session); 

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

          await workflowAgent.ProcessWorkflowQuestionAsync(messages, userInput, session);
        }

        Console.WriteLine(JsonSerializer.Serialize(messages));
      } catch (Exception ex)
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
    }

    private async Task ProcessWorkflowQuestionAsync(List<ChatMessage> messages, string userInput, AgentSession session)
    {
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
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}\n");
      }
    }

    private bool IsReiseVollstaendig(AgentResponse response)
    {
      // Beispiel: Prüfe ob Agent eine "Buchung bestätigt"-Nachricht gibt
      return response.Messages.Any(m =>
        m.Text?.Contains("vollständig", StringComparison.OrdinalIgnoreCase) == true ||
        m.Text?.Contains("buchen", StringComparison.OrdinalIgnoreCase) == true);
    }
  }
}