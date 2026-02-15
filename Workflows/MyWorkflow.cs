//using Azure.AI.OpenAI;
//using Azure.Identity;
//using Microsoft.Agents.AI;
//using Microsoft.Agents.AI.Workflows;
//using Microsoft.Extensions.AI;

//namespace AgentFramework.Workflows;

//internal class MyWorkflow
//{
//  // Step 1: Convert text to uppercase
//  class UpperCase : Executor
//  {
//    [Handler]
//    public async Task ToUpperCase(string text, WorkflowContext<string> ctx)
//    {
//      await ctx.SendMessageAsync(text.ToUpper());
//    }
//  }

//  // Step 2: Reverse the string and yield output
//  [Executor(Id = "reverse_text")]
//  static async Task ReverseText(string text, WorkflowContext<Never, string> ctx)
//  {
//    var reversed = new string(text.Reverse().ToArray());
//    await ctx.YieldOutputAsync(reversed);
//  }
//}
