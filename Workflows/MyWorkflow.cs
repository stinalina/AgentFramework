using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFramework.Workflows;

internal class MyWorkflow
{
  //TODO ctr mit entsprechenden Agenten
  //Workflow workflow =  AgentWorkflowBuilder.BuildSequential(writer, editor);

  //AIAgent workflowAgent = await workflow.AsAgentAsync();

  //AgentRunResponse workflowResponse =
  //    await workflowAgent.RunAsync("Write a short story about a haunted house.");

  //Console.WriteLine(workflowResponse.Text);
}


/**
 * User Workflow erklärt:
 * Input: User stellt eine Frage bzl Reisen
 * Workflow:
 *   1. Agent, der die Frage an den zuständigen Agenten weiterleitet
 *      Der richtige Agent chattet mit dem User und stellt eine Reise (TripInfo) zusammen
 *      Diese Agenten agieren Handof und shiften die bisherige TripInfo an den nächsten Agenten (falls sich die Anforderung des Users plötzich ändert und ein neuer "Profi" notwendig ist, bis die TripInfo vollständig ist
 *   2. Agent, der mit der TripInfo eine anständige Mail an den User generiert
 */
