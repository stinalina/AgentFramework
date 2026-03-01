using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgentFramework.Extensions;

internal static class WorkflowAgentExtensions
{
  extension(AIAgent workflowAgent)
  {
    public async Task StartWorkflowAgentConversationAsync(string? initialQuestion = null)
    {
      try
      {

        Console.WriteLine("Starting workflow agent conversation...");

        Console.WriteLine("Creating session...");
        AgentSession session = await workflowAgent.CreateSessionAsync();
        Console.WriteLine("Session created successfully");

        var messages = new List<ChatMessage>
    {
      new(ChatRole.User, "Ich möchte eine Insel besuchen, auf der es warm ist mit Bergen zum Wandern.")
    };

        Console.WriteLine("Running agent...");
        AgentResponse response = await workflowAgent.RunAsync(messages, session);
        Console.WriteLine("Agent response received");

        Console.WriteLine($"Messages count: {response.Messages.Count}");
        Console.WriteLine(response.RawRepresentation);
        Console.WriteLine(response.Text);
        foreach (ChatMessage message in response.Messages)
        {
          Console.WriteLine($"{message.AuthorName}: {message.Text}");
        }
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

      //await foreach (AgentResponseUpdate update in workflowAgent.RunStreamingAsync(messages, session))
      //{
      //  // Process streaming updates from each agent in the workflow
      //  if (!string.IsNullOrEmpty(update.Text))
      //  {
      //    Console.Write(update.Text);
      //  }

      //  // Check for function call requests
      //  foreach (AIContent content in update.Contents)
      //  {
      //    if (content is FunctionCallContent functionCall)
      //    {
      //      // Handle the external input request
      //      Console.WriteLine($"Workflow requests input: {functionCall.Name}");
      //      Console.WriteLine($"Request data: {functionCall.Arguments}");

      //      // Provide the response in the next message
      //    }
      //  }
      //}

      //while (true)
      //{
      //  Console.Write("You: ");
      //  string? userInput = Console.ReadLine();

      //  if (string.IsNullOrWhiteSpace(userInput))
      //    continue;

      //  if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
      //      userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
      //  {
      //    Console.WriteLine("Goodbye!");
      //    break;
      //  }

      //  await workflowAgent.ProcessQuestionAsync(userInput, options, session);
      //}
    }

    private async Task ProcessQuestionAsync(string question, AgentRunOptions options, AgentSession session)
    {
      try
      {
        // Reset continuation token for new question
        //otherwise: An error occurred: Input messages are not allowed when continuing a background response using a continuation token.
        options.ContinuationToken = null;

        var response = await workflowAgent.RunAsync(question, session, options);
        // Continue to poll until the final response is received
        // The initial call may complete immediately (no continuation token) or start a background operation (with continuation token)
        while (response.ContinuationToken is not null)
        {
          // Wait before polling again.
          await Task.Delay(TimeSpan.FromSeconds(2));

          options.ContinuationToken = response.ContinuationToken; //store continuation tokens persistently for operations that may span user sessions
          response = await workflowAgent.RunAsync(session, options);
        }
        Console.WriteLine($"\nAgent: {response.Text}\n"); //TODO nicht Agent, sondern z.B. "Europe Expert" oder so, je nachdem welcher Agent antwortet
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}\n");
      }
    }
  }
}