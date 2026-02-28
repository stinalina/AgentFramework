using AgentFramework.WorkflowSteps;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace AgentFramework.Workflows;

internal class MyWorkflow
{
  private static Workflow _instance;
  //TODO ctr mit entsprechenden Agenten
  //Workflow workflow =  AgentWorkflowBuilder.CreateHandoffBuilderWith;

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

  private static async void RunWorkflow(string inputMessage)
  {
    // Execute the workflow
    
  }

}


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
