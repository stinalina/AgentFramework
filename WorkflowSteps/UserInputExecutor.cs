using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgentFramework.WorkflowSteps;

/// <summary>
/// Executor that accepts user input and passes it through the workflow.
/// </summary>
/// /// <summary>
/// Executor that converts a string message to a ChatMessage and triggers agent processing.
/// This demonstrates the adapter pattern needed when connecting string-based executors to agents.
/// Agents in workflows use the Chat Protocol, which requires:
/// 1. Sending ChatMessage(s)
/// 2. Sending a TurnToken to trigger processing
/// </summary>
internal sealed class UserInputExecutor() : Executor<string, string>("UserInput")
{
  public override async ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
  {
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"[{this.Id}] Received question: \"{message}\"");
    Console.ResetColor();

    // Store the original question in workflow state for later use by JailbreakSyncExecutor
    await context.QueueStateUpdateAsync("OriginalQuestion", message, cancellationToken);

    // Convert the string to a ChatMessage that the agent can understand
    // The agent expects messages in a conversational format with a User role
    ChatMessage chatMessage = new(ChatRole.User, message);

    // Send the chat message to the agent executor
    await context.SendMessageAsync(chatMessage, cancellationToken: cancellationToken);

    // Send a turn token to signal the agent to process the accumulated messages
    await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken: cancellationToken);

    return message;
  }
}