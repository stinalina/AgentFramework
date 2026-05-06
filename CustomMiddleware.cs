using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFramework;

internal class CustomMiddleware
{
    public static async ValueTask<object?> FunctionMiddleware_LogUsedTool(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[Function] Invoking: {context.Function.Name}");
        var result = await next(context, cancellationToken);
        Console.WriteLine($"[Function] Result: {result}\n");
        Console.ResetColor();
        return result;
    }
}
