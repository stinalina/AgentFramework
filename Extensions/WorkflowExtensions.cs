using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Sprache;
using System.Text.Json;
using System.ComponentModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFramework.Extensions;

public static class WorkflowExtensions
{

  extension(Workflow workflow)
  {
    public async Task ExecuteWorkflowAsync()
    {
      Console.ForegroundColor = ConsoleColor.DarkMagenta;
      Console.WriteLine("Reisebüro Center: Willkommen im Reisebüro! Wie kann ich Ihnen behilflich sein?");
      Console.ResetColor();

      // Configure whether to show agent thinking in real-time
      const bool ShowAgentThinking = true;

      Console.Write("You: ");
      string ? userQuestion = Console.ReadLine();
      if (string.IsNullOrEmpty(userQuestion))
      {
        Console.WriteLine("Keine Frage eingegeben. Beende das Reisebüro.");
        return;
      }

      try
      {
        // Execute in streaming mode to see real-time progress; new ChatMessage(ChatRole.User, "Hallöchen!")
        //await using Checkpointed<StreamingRun> run = await InProcessExecution.StreamAsync(workflow, CheckpointManager.Default);
        await using StreamingRun runn = await InProcessExecution.StreamAsync(workflow, userQuestion);

        // Watch the workflow events
        //await foreach (WorkflowEvent evt in run.Run.WatchStreamAsync()) //.ConfigureAwait(false);
        await foreach (WorkflowEvent evt in runn.WatchStreamAsync()) //.ConfigureAwait(false);
          {
          switch (evt)
          {
            case ExecutorCompletedEvent executorComplete when executorComplete.Data is not null:
              // Don't print internal executor outputs, let them handle their own printing
              break;

            case AgentResponseUpdateEvent:
              // Show agent thinking in real-time (optional)
              if (ShowAgentThinking && !string.IsNullOrEmpty(((AgentResponseUpdateEvent)evt).Update.Text))
              {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(((AgentResponseUpdateEvent)evt).Update.Text);
                Console.ResetColor();
              }
              break;

            case WorkflowOutputEvent:
              // Workflow completed - final output already printed by FinalOutputExecutor
              break;
          }
        }
      } catch (Exception e)
      {
        Console.WriteLine($"An error occurred: {e}");
      }

     // await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, "Hello World!"));

      //  // Must send the turn token to trigger the agents.
      //  // The agents are wrapped as executors. When they receive messages,
      //  // they will cache the messages and only start processing when they receive a TurnToken.
      //  await run.TrySendMessageAsync(new Microsoft.Agents.AI.Workflows.TurnToken(emitEvents: true));
      //  await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
      //  {
      //    if (evt is Microsoft.Agents.AI.Workflows.AgentResponseUpdateEvent executorComplete)
      //    {
      //      Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
      //    }
      //  }
      //} catch (Exception e)
      //{
      //  Console.WriteLine($"An error occurred: {e}");
      //}



      //// Streaming execution — get events as they happen
      //StreamingRun run = await InProcessExecution.StreamAsync(_instance, inputMessage);
      //await foreach (WorkflowEvent evt in run.WatchStreamAsync())
      //{
      //  if (evt is ExecutorCompleteEvent executorComplete)
      //  {
      //    Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
      //  }

      //  if (evt is WorkflowOutputEvent outputEvt)
      //  {
      //    Console.WriteLine($"Workflow completed: {outputEvt.Data}");
      //  }
      //}

      //// Non-streaming execution — wait for completion
      //Run result = await InProcessExecution.RunAsync(_instance, inputMessage);
      //foreach (WorkflowEvent evt in result.NewEvents)
      //{
      //  if (evt is WorkflowOutputEvent outputEvt)
      //  {
      //    Console.WriteLine($"Final result: {outputEvt.Data}");
      //  }
      //}
    }

    public async Task RunGroupChatAsync(string initialQuestion)
    {
      try
      {
        var messages = new List<ChatMessage> { new(ChatRole.User, initialQuestion) };

        StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        string? lastExecutorId = null;
        await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
        {
          switch (evt)
          {
            case RequestInfoEvent e:
            {
              if (e.Request.DataAs<FunctionApprovalRequestContent>() is FunctionApprovalRequestContent approvalRequestContent)
              {
                Console.WriteLine();
                Console.WriteLine($"[APPROVAL REQUIRED] From agent: {e.Request.PortInfo.PortId}");
                Console.WriteLine($"  Tool: {approvalRequestContent.FunctionCall.Name}");
                Console.WriteLine($"  Arguments: {JsonSerializer.Serialize(approvalRequestContent.FunctionCall.Arguments)}");
                Console.WriteLine();

                // Approve the tool call request
                Console.WriteLine($"Tool: {approvalRequestContent.FunctionCall.Name} approved");
                await run.SendResponseAsync(e.Request.CreateResponse(approvalRequestContent.CreateResponse(approved: true)));
              }

              break;
            }

            case AgentResponseUpdateEvent update:
            {
              //// Process streaming agent responses
              //AgentResponse response = update.AsResponse();
              //foreach (ChatMessage message in response.Messages)
              //{
              //  //Console.WriteLine($"[{update.ExecutorId}]: {message.Text}");
              //}
              //break;
              if (update.ExecutorId != lastExecutorId)
              {
                if (lastExecutorId is not null)
                {
                  Console.WriteLine();
                }

                Console.WriteLine($"- {update.ExecutorId}: ");
                lastExecutorId = update.ExecutorId;
              }

              Console.Write(update.Update.Text);

              break;
            }

            case WorkflowOutputEvent output:
            {
              // Workflow completed
              var conversationHistory = output.As<List<ChatMessage>>()
                  .Where(x => x.Contents.Any(c => c is TextContent))
                  .ToList();

              Console.WriteLine("\n=== Final Conversation ===");
              foreach (var message in conversationHistory)
              {
                Console.WriteLine($"{message.AuthorName}: {message.Text}");
              }
              break;
            }
          }
        }
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
    }
  }
}