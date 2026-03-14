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

        string? lastExecutorId = null;
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
              if (update.ExecutorId != lastExecutorId) // inside orchestrator Agent
              {
                if (lastExecutorId is not null)
                {
                  Console.WriteLine();
                }

                Console.WriteLine($"- {update.ExecutorId}: ");
                lastExecutorId = update.ExecutorId;
              }

              if (update.Data is AgentResponseUpdate responseUpdate) // inside HandoffAgent
              {
                if (responseUpdate.AuthorName != lastAuthorName)
                {
                  if (lastAuthorName is not null)
                  {
                    Console.WriteLine();
                  }

                  Console.WriteLine($"- {responseUpdate.AuthorName}: ");
                    lastAuthorName = responseUpdate.AuthorName;
                }
                //Console.Write(responseUpdate.AuthorName + ": ");
                //if (responseUpdate.RawRepresentation is ExecutorCompletedEvent completedEvent)
                //{
                //  Console.WriteLine("ExecutorId: " + completedEvent.ExecutorId);
                //}
              }

              Console.Write(update.Update.Text);
                //Console.ForegroundColor = ConsoleColor.DarkYellow;
                ////          Console.Write(((AgentResponseUpdateEvent)evt).Update.Text);
                ////          Console.ResetColor();

                break;
            }

            case WorkflowOutputEvent output:
            {
              // Workflow completed
              var conversationHistory = output.As<List<ChatMessage>>()
                  .Where(x => x.Contents.Any(c => c is TextContent))
                  .ToList();

              //Console.WriteLine("\n=== Final Conversation ===");
              //foreach (var message in conversationHistory)
              //{
              //  Console.WriteLine($"{message.AuthorName}: {message.Text}");
              //}
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