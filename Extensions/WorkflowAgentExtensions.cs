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

        Dictionary<string, List<AgentResponseUpdate>> buffer = [];
        await foreach (AgentResponseUpdate update in workflowAgent.RunStreamingAsync(messages, session))
        {
          if (update.MessageId is null || string.IsNullOrEmpty(update.Text))
          {
            // skip updates that don't have a message ID or text
            continue;
          }
          Console.Clear();

          if (!buffer.TryGetValue(update.MessageId, out List<AgentResponseUpdate>? value))
          {
            value = [];
            buffer[update.MessageId] = value;
          }
          value.Add(update);

          foreach (var (messageId, segments) in buffer)
          {
            string combinedText = string.Concat(segments);
            Console.WriteLine($"{segments[0].AuthorName}: {combinedText}");
            Console.WriteLine();
          }
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

        //AgentResponse response = await workflowAgent.RunAsync(messages, session);
        var currAgent = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant)?.AuthorName ?? string.Empty;

        var handoffMessage = response.Messages.FirstOrDefault(m => m.Role == ChatRole.Tool && m.Text?.Contains("Transferred") == true);
        if (handoffMessage != null && !String.IsNullOrEmpty(lastAgent) && lastAgent != currAgent)
        {
          // Nur die Handoff-Bestätigung anzeigen
          Console.WriteLine("HandOff Occured");
          debuggingHistory.AddRange(response.Messages.Where(m =>
            m.Role == ChatRole.Assistant && m.Contents?.Any(c => c.GetType().Name.Contains("FunctionCall")) == true ||
            m.Role == ChatRole.Tool
          ).ToList());

          messages.AddRange(response.Messages.Where(m =>
            m.Role == ChatRole.Assistant && m.Contents?.Any(c => c.GetType().Name.Contains("FunctionCall")) == true ||
            m.Role == ChatRole.Tool
          ).ToList());
        }
        else
        {
          debuggingHistory.AddRange(response.Messages);

          Console.ForegroundColor = ConsoleColor.Yellow;
          //foreach (ChatMessage message in response.Messages)
          //{
          //  if (!string.IsNullOrWhiteSpace(message.Text))
          //  {
          //    Console.WriteLine($"\n{message.AuthorName}: {message.Text}");
          //  }
          //}
          Console.ResetColor();

          messages.AddRange(response.Messages);
        }

        //messages.AddRange(response.Messages.Where(x => x.Role == ChatRole.Assistant || x.Role == ChatRole.User));
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}\n");
      }

      return messages;
    }
  }
}