using AgentFramework.WorkflowSteps;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Sprache;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AgentFramework.Workflows;

internal class MyWorkflow
{
  private static Workflow _instance;
  //TODO ctr mit entsprechenden Agenten

  //TravelAgentContact
  //ResearcherAgentPool
  //TravelAgentSales

  //AIAgent workflowAgent = await workflow.AsAgentAsync();

  //AgentRunResponse workflowResponse =
  //    await workflowAgent.RunAsync("Write a short story about a haunted house.");

  //Console.WriteLine(workflowResponse.Text);

  public static async Task<Workflow> GetWorkflowAsync()
  {
    if (_instance == null)
    {
      var welcomeMsg = new TravelAgencyExecutor();
      var travelAgencyAgent = await TravelAgency.GetEmployeeAsync();

      //Workflows are created by WorkflowBuilds. What about AgentWorkflowBuilder?

      Workflow workflow = AgentWorkflowBuilder.BuildSequential([travelAgencyAgent]);
      //AIAgent workflowAgent = workflow.AsAgent(
      //  id: "workflow-graph",
      //  name: "Wokflow Graph Agent",
      //  description: "A multi-agent workflow for a travel agency",
      //  checkpointManager: null,
      //  executionEnvironment: InProcessExecution.Default);

      Console.WriteLine(workflow.ToDotString());
      //To create an image file from the DOT format, you can use GraphViz tools with the following command
      // dotnet run | tail -n +20 | dot -Tpng -o workflow.png
      Console.WriteLine(workflow.ToMermaidString());

      //await foreach (var update in workflow..RunStreamingAsync("Guess a number between 1 and 10."))
      //{
      //  Console.Write(update);

      //  // Prompt the user for feedback before continuing
      //  Console.Write("\nYour feedback (higher/lower/correct): ");
      //  var feedback = Console.ReadLine();

      //  if (feedback?.Equals("correct", StringComparison.OrdinalIgnoreCase) == true)
      //  {
      //    Console.WriteLine("Guessed correctly!");
      //    break;
      //  }
      //}

      // Wrap the AIAgent in an AIAgentExecutor for TurnToken pattern support
      //var travelAgencyExecutor = new AIAgentExecutor("TravelAgencyEmployee", travelAgencyAgent);

      _instance = new WorkflowBuilder(welcomeMsg)
        .AddEdge(welcomeMsg, travelAgencyAgent)
        .Build();
    }
    return _instance;
  }

  //private async void ListenToEvents()
  //{
  //  await foreach (WorkflowEvent evt in run.WatchStreamAsync())
  //  {
  //    switch (evt)
  //    {
  //      case ExecutorInvokedEvent invoke:
  //        Console.WriteLine($"Starting {invoke.ExecutorId}");
  //        break;

  //      case ExecutorCompletedEvent complete:
  //        Console.WriteLine($"Completed {complete.ExecutorId}: {complete.Data}");
  //        break;

  //      case WorkflowOutputEvent output:
  //        Console.WriteLine($"Workflow output: {output.Data}");
  //        return;

  //      case WorkflowErrorEvent error:
  //        Console.WriteLine($"Workflow error: {error.Exception}");
  //        return;
  //    }
  //  }
  //}

  //  private static async void CreatingCheckpoint(string inputMessage)
  //  {
  //    var checkpointManager = new CheckpointManager();
  //    // List to store checkpoint info for later use
  //    var checkpoints = new List<CheckpointInfo>();

  //    // Run the workflow with checkpointing enabled
  //    Checkpointed<StreamingRun> checkpointedRun = await InProcessExecution
  //        .StreamAsync(workflow, input, checkpointManager)
  //        .ConfigureAwait(false);
  //    await foreach (WorkflowEvent evt in checkpointedRun.Run.WatchStreamAsync().ConfigureAwait(false))
  //    {
  //      if (evt is SuperStepCompletedEvent superStepCompletedEvt)
  //      {
  //        // Access the checkpoint and store it
  //        CheckpointInfo? checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;
  //        if (checkpoint != null)
  //        {
  //          checkpoints.Add(checkpoint);
  //        }
  //      }
  //    }
  //  }

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


  /**
   * User Workflow erklärt:
   * Input: User stellt eine Frage bzl Reisen
   * Workflow:
   *   1. FunctionExecutor. Es wird Beschrieben wo der User sich befinder. Sowas wie:
   *      Es ist ein wunderschöner morgen und sie betreten ein Reisebüro. Ein freundlicher Mitarbeiter begrüßt sie und fragt, wie er ihnen helfen kann.
   *   1. Agent, der die Frage an den zuständigen Agenten weiterleitet
   *      Der richtige Agent chattet mit dem User und stellt eine Reise (TripInfo) zusammen
   *      Diese Agenten agieren Handof und shiften die bisherige TripInfo an den nächsten Agenten (falls sich die Anforderung des Users plötzich ändert und ein neuer "Profi" notwendig ist, bis die TripInfo vollständig ist
   *   2. Agent, der mit der TripInfo eine anständige Mail an den User generiert
   *   
   *   # Executers
   *   Processing (Ausführende) Units innerhablb eines Workflows. Bei mir die Agenten
   *   
   *   # Edges
   *   Definieren die Verbindungen zw den Excuters. Bei mir die Übergabe der TripInfo von einem Agenten zum nächsten.
   *   Diese können auch Bedingenen unterliegen.
   *   
   *   # Events
   *   Provide observability into workflow execution, including lifecycle events, executor events, and custom events.
   *   
   *   # Workflow Builder & Execution
   *   Verbindet Executers mit Edges und bildet so einen Graphen.
   */
}