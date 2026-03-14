using AgentFramework.WorkflowSteps;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
namespace AgentFramework.Workflows;

internal class MyWorkflow
{
  private static WorkflowBuilder _instance;

  public static async Task<Workflow> GetWorkflowAsync()
  {
    _instance ??= await CreateWorkflowBuilderAsync();
    return _instance.Build();
  }

  private static async Task<WorkflowBuilder> CreateWorkflowBuilderAsync()
  {
    UserInputExecutor userInput = new();
    TripInfoOutputExecutor outputMsg = new();
    AIAgent workflowAgent = await HandoffWorkflowAgent.GetContinentWorkflowAgentAsync();

    InteractiveConversationExecutor interactiveLoop = new(workflowAgent);

    WorkflowBuilder workflowBuilder = new WorkflowBuilder(userInput)
      .AddEdge(userInput, interactiveLoop)
      .AddEdge(interactiveLoop, outputMsg)
      .WithOutputFrom(outputMsg);
  
    return workflowBuilder;
  }

  //  private static async void ResumeFromCheckpointOnTheSameRun() ///e.g. going back
  //  {
  //    // Assume we want to resume from the 6th checkpoint
  //    CheckpointInfo savedCheckpoint = checkpoints[5];
  //    // Note that we are restoring the state directly to the same run instance.
  //    await checkpointedRun.RestoreCheckpointAsync(savedCheckpoint, CancellationToken.None).ConfigureAwait(false);
  //    await foreach (WorkflowEvent evt in checkpointedRun.Run.WatchStreamAsync().ConfigureAwait(false))
  //    {
  //      if (evt is WorkflowOutputEvent workflowOutputEvt)
  //      {
  //        Console.WriteLine($"Workflow completed with result: {workflowOutputEvt.Data}");
  //      }
  //    }
  //  }

  //  private static async void RehydrateFromCheckpoint() //createing a new workflow based on a checkpoint
  //  {
  //    // Assume we want to resume from the 6th checkpoint
  //    CheckpointInfo savedCheckpoint = checkpoints[5];
  //    Checkpointed<StreamingRun> newCheckpointedRun = await InProcessExecution
  //        .ResumeStreamAsync(newWorkflow, savedCheckpoint, checkpointManager)
  //        .ConfigureAwait(false);
  //    await foreach (WorkflowEvent evt in newCheckpointedRun.Run.WatchStreamAsync().ConfigureAwait(false))
  //    {
  //      if (evt is WorkflowOutputEvent workflowOutputEvt)
  //      {
  //        Console.WriteLine($"Workflow completed with result: {workflowOutputEvt.Data}");
  //      }
  //    }
  //  }
  //}
}