//using System.Text;
//using System.Text.Json;
//using Azure.AI.OpenAI;
//using Azure.Identity;
//using Microsoft.Agents.AI;
//using Microsoft.Extensions.AI;
//using Microsoft.Rest;
//using OpenAI.Chat;
//using System.Activities.Statements;

//namespace AgentFramework;

///**
// * An AIContextProvider instance is attached to an agent and the same instance would be used for all sessions. 
// * This means that the AIContextProvider should not store any session specific state in the provider instance.
// */
//internal class CustomContextProvidor : AIContextProvider //https://learn.microsoft.com/en-us/agent-framework/agents/conversations/context-providers?pivots=programming-language-csharp#advanced-aicontextprovider-implementation
//{
//  private readonly ProviderSessionState<State> _sessionState;
//  private readonly ServiceClient _client;

//  internal class MyCustomState
//  {
//    public string? MemoryId { get; set; }

//  }


//  public CustomContextProvidor()
//  {
//    var sessionStateHelper = new InMemoryAgentSession ProviderSessionState<MyCustomState>(
//  // stateInitializer is called when there is no state in the session for this AIContextProvider yet
//  stateInitializer: currentSession => new MyCustomState() { MemoryId = Guid.NewGuid().ToString() },
//  // The key under which to store state in the session for this provider. Make sure it does not clash with the keys of other providers.
//  stateKey: this.GetType().Name,
//  // An optional jsonSerializerOptions to control the serialization/deserialization of the custom state object
//  jsonSerializerOptions: myJsonSerializerOptions);

//    // Using the helper you can read state:
//    MyCustomState state = sessionStateHelper.GetOrInitializeState(session);
//    Console.WriteLine(state.MemoryId);

//    // And write state:
//    sessionStateHelper.SaveState(session, state);
//  }

//  protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = default)
//  {
//    throw new NotImplementedException();
//  }

//  protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = default)
//  {
//    return base.InvokedCoreAsync(context, cancellationToken);
//  }
//}
