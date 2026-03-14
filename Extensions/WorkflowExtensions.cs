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

      //Console.Write("You: ");
      //string? userQuestion = Console.ReadLine();
      //if (string.IsNullOrEmpty(userQuestion))
      //{
      //  Console.WriteLine("Keine Frage eingegeben. Beende das Reisebüro.");
      //  return;
      //}

      var userQuestion = "Erstelle mir eine Reise nach Italien";
      Console.WriteLine("You: " + userQuestion);

      await workflow.RunGroupChatAsync(userQuestion);
   }

    private async Task RunGroupChatAsync(string initialQuestion)
    {
      try
      {
        var messages = new List<ChatMessage> { new(ChatRole.User, initialQuestion) };

        StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        string? lastAuthorName = null;
        await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
        {
          switch (evt)
          {
            case SuperStepStartedEvent superStepStarted:
              //Console.WriteLine("Super Step started");
              break;

            case AgentResponseUpdateEvent update:
            {
              if (update.Data is AgentResponseUpdate responseUpdate)
              {
                if (responseUpdate.AuthorName != lastAuthorName)
                {
                  if (lastAuthorName is not null)
                  {
                    Console.WriteLine();
                  }

                  Console.WriteLine($"{responseUpdate.AuthorName}: ");
                    lastAuthorName = responseUpdate.AuthorName;
                }
                //Console.Write(responseUpdate.AuthorName + ": ");
                //if (responseUpdate.RawRepresentation is ExecutorCompletedEvent completedEvent)
                //{
                //  Console.WriteLine("ExecutorId: " + completedEvent.ExecutorId);
                //}
              }

              Console.Write(update.Update.Text);
              break;
            }

            case WorkflowOutputEvent output:
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("Reisebüro Center: Vielen Dank für Ihre Anfrage! Ihre Reise wurde erfolgreich erstellt.");
                Console.ResetColor();
                break;
            }

            case SuperStepCompletedEvent superStep:
              Console.WriteLine();
              break;
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}");
      }
    }
  }
}