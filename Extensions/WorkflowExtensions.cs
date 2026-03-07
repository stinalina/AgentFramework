using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Sprache;

namespace AgentFramework.Extensions;

public static class WorkflowExtensions
{

  extension(Workflow workflow)
  {
    public async Task ExecuteWorkflowAsync()
    {
      Console.ForegroundColor = ConsoleColor.DarkMagenta;
      Console.WriteLine("Willkommen im Reisebüro! Wie kann ich Ihnen behilflich sein?");
      Console.ResetColor();

      // Configure whether to show agent thinking in real-time
      const bool ShowAgentThinking = true;

      Console.WriteLine("User: ");
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
  }
}