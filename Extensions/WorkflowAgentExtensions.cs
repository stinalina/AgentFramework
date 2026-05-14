using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFramework.Extensions;

internal static class WorkflowAgentExtensions
{
	extension(AIAgent workflowAgent)
	{
		public async Task<List<ChatMessage>> StartWorkflowAgentConversationAsync()
		{
			List<ChatMessage> messages = [];
			AgentSession session = await workflowAgent.CreateSessionAsync();

			while (true)
			{
				Console.Write("\nYou: ");
				string userInput = Console.ReadLine()!;
				messages.Add(new ChatMessage(ChatRole.User, userInput));

				if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
					userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
				{
					Console.WriteLine("Goodbye!");
					break;
				}

				if (userInput.Contains("buchen", StringComparison.OrdinalIgnoreCase) ||
					userInput.Contains("vollständig", StringComparison.OrdinalIgnoreCase))
				{
					Console.WriteLine("Ihre Reise wird nun erstellt. Bitte haben Sie einen Moment Geduld.");
					break;
				}

				messages = await workflowAgent.ProcessWorkflowQuestionAsync(messages, userInput, session);
			}
			
			return messages;
		}

		private async Task<List<ChatMessage>> ProcessWorkflowQuestionAsync(List<ChatMessage> messages, string userInput, AgentSession session)
		{
			try
			{
				string currAuthor = string.Empty;
				string agentResponseText = string.Empty;

				Console.ForegroundColor = ConsoleColor.DarkYellow;

				await foreach (AgentResponseUpdate update in workflowAgent.RunStreamingAsync(messages, session))
				{
					if (!string.IsNullOrEmpty(update.AuthorName) && currAuthor != update.AuthorName)
					{
						currAuthor = update.AuthorName;
						Console.Write($"\n{currAuthor}: ");
					}
					if (!string.IsNullOrEmpty(update.Text))
					{
						agentResponseText += update.Text;
						Console.Write(update.Text);
					}
				}
				Console.ResetColor();

				messages.AddRange(new ChatMessage(ChatRole.Assistant, agentResponseText));
				return messages;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}\n");
				return [];
			}
		}
	}
}