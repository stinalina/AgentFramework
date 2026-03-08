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

        Console.WriteLine(JsonSerializer.Serialize(messages));
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
      Dictionary<string, List<AgentResponseUpdate>> buffer = [];
      try
      {
        messages.Add(new ChatMessage(ChatRole.User, userInput));

        await foreach (AgentResponseUpdate update in workflowAgent.RunStreamingAsync(messages, session))
        {
          if (update.Contents is not null)
          {
            foreach (AIContent content in update.Contents)
            {
              if (content is FunctionCallContent functionCall &&
                  functionCall.Name.StartsWith("handoff_to_", StringComparison.OrdinalIgnoreCase))
              {
                string reason = functionCall.Arguments?.TryGetValue("reasonForHandoff", out object? reasonValue) == true
                  ? reasonValue?.ToString() ?? "No reason provided"
                  : "No reason provided";

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[Handoff] Reason: {reason}");
                Console.ResetColor();
              }
            }
          }

          if (update.MessageId is null || string.IsNullOrEmpty(update.Text))
          {
            continue;
          }

          if (!buffer.TryGetValue(update.MessageId, out List<AgentResponseUpdate>? value))
          {
            value = [];
            buffer[update.MessageId] = value;
          }
          value.Add(update);
        }

        AgentResponse response = buffer.Values
           .SelectMany(segments => segments)
           .ToAgentResponse();

        var correctedMessages = response.Messages.Select(msg =>
        {
          if (!string.IsNullOrEmpty(msg.AuthorName) &&
              msg.AuthorName != "User" &&
              msg.Role == ChatRole.User &&
              !string.IsNullOrWhiteSpace(msg.Text))
          {
            return new ChatMessage(ChatRole.Assistant, msg.Text) { AuthorName = msg.AuthorName };
          }
          return msg;
        }).ToList();

        response = new AgentResponse(correctedMessages);

        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (ChatMessage message in response.Messages)
        {
          if (!string.IsNullOrWhiteSpace(message.Text))
          {
            Console.WriteLine($"\n{message.AuthorName}: {message.Text}");
          }
        }

        Console.ResetColor();
        messages.AddRange(response.Messages);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}\n");
      }

      return messages;
    }
  }
}