using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Collections;
using AgentFramework.WorkflowSteps;

namespace AgentFramework.Extensions;

public static class WorkflowExtensions
{

  extension(Workflow workflow)
  {
    public async Task StartWorkflowAsync()
    {
      try { 
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, "Hello World!"));

        // Must send the turn token to trigger the agents.
        // The agents are wrapped as executors. When they receive messages,
        // they will cache the messages and only start processing when they receive a TurnToken.
        await run.TrySendMessageAsync(new Microsoft.Agents.AI.Workflows.TurnToken(emitEvents: true));
        await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
        {
          if (evt is Microsoft.Agents.AI.Workflows.AgentResponseUpdateEvent executorComplete)
          {
            Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
          }
        }
      } catch (Exception e)
      {
        Console.WriteLine($"An error occurred: {e}");
      }

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