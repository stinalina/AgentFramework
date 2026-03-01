using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;
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

        Console.WriteLine("You: " + initalQuestion);
        await workflowAgent.ProcessWorkflowQuestionAsync(messages, initalQuestion, session); 

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
            Console.WriteLine($"{message.AuthorName}: {message.Text}");
          }
        }
        messages.AddRange(response.Messages.Skip(messages.Count));
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}\n");
      }
    }
  }
}