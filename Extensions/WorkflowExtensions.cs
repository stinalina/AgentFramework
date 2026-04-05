using AgentFramework.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Runtime.CompilerServices;

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

      Console.Write("You: ");
      string? userQuestion = Console.ReadLine();
      if (string.IsNullOrEmpty(userQuestion))
      {
        Console.WriteLine("Keine Frage eingegeben. Beende das Reisebüro.");
        return;
      }

      await workflow.RunGroupChatAsync(userQuestion);
    }

    public async IAsyncEnumerable<ResponseStreamChunk> ExecuteWorkflowStreamAsync(
    string initialQuestion,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
      var messages = new List<ChatMessage> { new(ChatRole.User, initialQuestion) };

      StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
      await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

      string? lastAuthorName = null;

      await foreach (WorkflowEvent evt in run.WatchStreamAsync().WithCancellation(cancellationToken))
      {
        switch (evt)
        {
          case AgentResponseUpdateEvent update:
            {
              string? newAuthor = null;
              if (update.Data is AgentResponseUpdate responseUpdate)
              {
                if (responseUpdate.RawRepresentation is ExecutorFailedEvent failedEvent)
                {
                  yield return new ResponseStreamChunk(null, "Error: " + failedEvent.Data.Message, IsCompleted: true);
                  yield break;
                }

                if (responseUpdate.AuthorName != lastAuthorName)
                {
                  lastAuthorName = responseUpdate.AuthorName;
                  newAuthor = lastAuthorName;
                }
              }

              // Debug: Auch in Stream-Response auf Fehler prüfen
              if (update.Data is AgentResponseUpdate responseUpdate2 && 
                  responseUpdate2.RawRepresentation is ChatResponseUpdate chatResponseUpdate2)
              {
                var hasErrorCode = chatResponseUpdate2.Contents?.Any(c => 
                  c is not null && 
                  c.GetType().GetProperty("ErrorCode")?.GetValue(c) is not null) ?? false;
                
                if (hasErrorCode)
                {
                  Console.ForegroundColor = ConsoleColor.Red;
                  Console.WriteLine("\n[STREAM] ErrorCode in Contents!");
                  Console.WriteLine(JsonSerializer.Serialize(chatResponseUpdate2.Contents, new JsonSerializerOptions { WriteIndented = true }));
                  Console.ResetColor();
                }
              }

              if (update.Update.Text is not null)
                yield return new ResponseStreamChunk(newAuthor, update.Update.Text);

              break;
            }

          case WorkflowOutputEvent:
            yield return new ResponseStreamChunk(null, null, IsCompleted: true);
            break;
        }
      }
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
            case AgentResponseUpdateEvent update:
              {
                if (update.Data is AgentResponseUpdate responseUpdate)
                {
                  if (responseUpdate.RawRepresentation is ExecutorFailedEvent failedEvent)
                  {
                    Console.WriteLine("Error: " + failedEvent.Data.Message);
                    throw new Exception("Workflow stopped");
                  }

                  if (responseUpdate.RawRepresentation is ChatResponseUpdate chatResponseUpdate)
                  {
                    var hasErrorCode = chatResponseUpdate.Contents?.Any(c => 
                      c is not null && 
                      c.GetType().GetProperty("ErrorCode")?.GetValue(c) is not null) ?? false;
                    
                    if (hasErrorCode)
                    {
                      Console.ForegroundColor = ConsoleColor.Red;
                      Console.WriteLine("\n[DEBUG] ErrorCode in Contents gefunden!");
                      Console.WriteLine(JsonSerializer.Serialize(chatResponseUpdate.Contents, new JsonSerializerOptions { WriteIndented = true }));
                      Console.ResetColor();
                    }
                  }

                  if (responseUpdate.AuthorName != lastAuthorName)
                  {
                    lastAuthorName = responseUpdate.AuthorName;
                    if (lastAuthorName is not null)
                    {
                      Console.WriteLine();
                      Console.ForegroundColor = ConsoleColor.Green;
                      Console.WriteLine($"\n{responseUpdate.AuthorName}: ");
                      Console.ResetColor();
                    }
                  }
                }

                Console.Write(update.Update.Text);
                break;
              }

            case WorkflowOutputEvent output:
              {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("\nReisebüro Center: Vielen Dank für Ihre Anfrage! Ihre Reise wurde erfolgreich erstellt.");
                Console.ResetColor();
                break;
              }
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