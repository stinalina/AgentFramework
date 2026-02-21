using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace AgentFramework;

//https://learn.microsoft.com/en-us/agent-framework/agents/middleware/?pivots=programming-language-csharp
internal class CustomMiddleware
{
  //The key change is replacing 
  //  CustomAgentRunMiddleware(IEnumerable<ChatMessage>, AgentSession?, AgentRunOptions?, AIAgent, CancellationToken) 
  //  with CustomChatClientMiddleware(IEnumerable<ChatMessage>, ChatOptions?, IChatClient, CancellationToken)
  //  since you're building a chat client middleware pipeline, not an agent middleware pipeline.

  // Shared state container that middleware instances can reference
  private static readonly Dictionary<string, object> SharedState = new() { ["callCount"] = 0 };

  //example of agent run middleware, that can inspect and/or modify the input and output from the agent run.
  public static async Task<AgentResponse> CustomAgentRunMiddleware( //that will get invoked for each agent run
    IEnumerable<ChatMessage> messages,
    AgentSession? session,
    AgentRunOptions? options,
    AIAgent innerAgent,
    CancellationToken cancellationToken)
  {
    Console.WriteLine(messages.Count());
    var response = await innerAgent.RunAsync(messages, session, options, cancellationToken).ConfigureAwait(false);
    Console.WriteLine(response.Messages.Count);
    return response;
  }

  public static async IAsyncEnumerable<AgentResponseUpdate> CustomAgentRunStreamingMiddleware( //that will get invoked for each agent run
    IEnumerable<ChatMessage> messages,
    AgentSession? session,
    AgentRunOptions? options,
    AIAgent innerAgent,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    Console.WriteLine(messages.Count());
    List<AgentResponseUpdate> updates = [];
    await foreach (var update in innerAgent.RunStreamingAsync(messages, session, options, cancellationToken))
    {
      updates.Add(update);
      yield return update;
    }

    Console.WriteLine(updates.ToAgentResponse().Messages.Count);
  }

  public static async ValueTask<object?> CustomFunctionCallingMiddleware( //gets called for each function tool that's invoked.
    AIAgent agent,
    FunctionInvocationContext context,
    Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
    CancellationToken cancellationToken)
  {
    Console.WriteLine($"Function Name: {context!.Function.Name}");
    var result = await next(context, cancellationToken);
    Console.WriteLine($"Function Call Result: {result}");

    return result;
  }

  //For agents that are built using IChatClient, you might want to intercept calls going from the agent to the IChatClient. In this case,
  public static async Task<ChatResponse> CustomChatClientMiddleware(
    IEnumerable<ChatMessage> messages,
    ChatOptions? options,
    IChatClient innerChatClient,
    CancellationToken cancellationToken)
  {
    Console.WriteLine("Inside middleware");
    //Console.WriteLine(messages.Count());
    var response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);
    //Console.WriteLine(response.Messages.Count);

    return response;
  }

  // IChatClient middleware that logs requests and responses
  public static async Task<ChatResponse> LoggingChatMiddleware(
    IEnumerable<ChatMessage> messages,
    ChatOptions? options,
    IChatClient innerChatClient,
    CancellationToken cancellationToken)
  {
    Console.WriteLine($"[ChatLog] Sending {messages.Count()} messages to model...");
    foreach (var msg in messages)
    {
      Console.WriteLine($"[ChatLog]   {msg.Role}: {msg.Text?.Substring(0, Math.Min(msg.Text.Length, 80))}");
    }

    var response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);

    Console.WriteLine($"[ChatLog] Received {response.Messages.Count} response messages.");
    return response;
  }

  // // Agent-level middleware: applied to ALL runs
  public static async Task<AgentResponse> SecurityMiddleware(
    IEnumerable<ChatMessage> messages,
    AgentSession? session,
    AgentRunOptions? options,
    AIAgent innerAgent,
    CancellationToken cancellationToken)
  {
    Console.WriteLine("[Security] Validating request...");
    var response = await innerAgent.RunAsync(messages, session, options, cancellationToken);
    return response;
  }

  // // Run-level middleware: applied to a specific run only
  public static async Task<AgentResponse> DebugMiddleware(
    IEnumerable<ChatMessage> messages,
    AgentSession? session,
    AgentRunOptions? options,
    AIAgent innerAgent,
    CancellationToken cancellationToken)
  {
    Console.WriteLine($"[Debug] Input messages: {messages.Count()}");
    var response = await innerAgent.RunAsync(messages, session, options, cancellationToken);
    Console.WriteLine($"[Debug] Output messages: {response.Messages.Count}");
    return response;
  }

  // // Guardrail middleware that checks input and can return early without calling the agent
  public static async Task<AgentResponse> GuardrailMiddleware(
    IEnumerable<ChatMessage> messages,
    AgentSession? session,
    AgentRunOptions? options,
    AIAgent innerAgent,
    CancellationToken cancellationToken)
  {
    // Pre-execution check: block requests containing sensitive words
    var lastMessage = messages.LastOrDefault()?.Text?.ToLower() ?? "";
    string[] blockedWords = ["password", "secret", "credentials"];

    foreach (var word in blockedWords)
    {
      if (lastMessage.Contains(word))
      {
        Console.WriteLine($"[Guardrail] Blocked request containing '{word}'.");
        return new AgentResponse([new ChatMessage(ChatRole.Assistant,
                $"Sorry, I cannot process requests containing '{word}'.")]);
      }
    }

    // Input passed validation — proceed with agent execution
    var response = await innerAgent.RunAsync(messages, session, options, cancellationToken);

    // Post-execution check: validate the output
    var responseText = response.Messages.LastOrDefault()?.Text ?? "";
    if (responseText.Length > 5000)
    {
      Console.WriteLine("[Guardrail] Response too long, truncating.");
      return new AgentResponse([new ChatMessage(ChatRole.Assistant,
            responseText.Substring(0, 5000) + "... [truncated]")]);
    }

    return response;
  }

  // // Middleware that modifies the AgentResponse after the agent completes
  public static async Task<AgentResponse> ResultOverrideMiddleware(
    IEnumerable<ChatMessage> messages,
    AgentSession? session,
    AgentRunOptions? options,
    AIAgent innerAgent,
    CancellationToken cancellationToken)
  {
    var response = await innerAgent.RunAsync(messages, session, options, cancellationToken);

    // Post-process: append a disclaimer to every assistant message
    var modifiedMessages = response.Messages.Select(msg =>
    {
      if (msg.Role == ChatRole.Assistant && msg.Text is not null)
      {
        return new ChatMessage(ChatRole.Assistant,
            msg.Text + "\n\n_Disclaimer: This information is AI-generated._");
      }
      return msg;
    }).ToList();

    return new AgentResponse(modifiedMessages);
  }

  // Middleware that catches exceptions and provides graceful fallback response
  public static async Task<AgentResponse> ExceptionHandlingMiddleware(
    IEnumerable<ChatMessage> messages,
    AgentSession? session,
    AgentRunOptions? options,
    AIAgent innerAgent,
    CancellationToken cancellationToken)
  {
    try
    {
      Console.WriteLine("[ExceptionHandler] Executing agent run...");
      return await innerAgent.RunAsync(messages, session, options, cancellationToken);
    }
    catch (TimeoutException ex)
    {
      Console.WriteLine($"[ExceptionHandler] Caught timeout: {ex.Message}");
      return new AgentResponse([new ChatMessage(ChatRole.Assistant,
            "Sorry, the request timed out. Please try again later.")]);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[ExceptionHandler] Caught error: {ex.Message}");
      return new AgentResponse([new ChatMessage(ChatRole.Assistant,
            "An error occurred while processing your request.")]);
    }
  }

  // Middleware that increments a shared call counter
  public static async Task<AgentResponse> CounterMiddleware(
      IEnumerable<ChatMessage> messages,
      AgentSession? session,
      AgentRunOptions? options,
      AIAgent innerAgent,
      CancellationToken cancellationToken)
  {
    var count = (int)SharedState["callCount"] + 1;
    SharedState["callCount"] = count;
    Console.WriteLine($"[Counter] Call #{count}");

    return await innerAgent.RunAsync(messages, session, options, cancellationToken);
  }

  // Middleware that reads shared state to enrich output
  public static async Task<AgentResponse> EnrichMiddleware(
      IEnumerable<ChatMessage> messages,
      AgentSession? session,
      AgentRunOptions? options,
      AIAgent innerAgent,
      CancellationToken cancellationToken)
  {
    var response = await innerAgent.RunAsync(messages, session, options, cancellationToken);
    var count = (int)SharedState["callCount"];
    Console.WriteLine($"[Enrich] Total calls so far: {count}");
    return response;
  }

}
