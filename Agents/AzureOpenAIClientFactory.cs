using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;

namespace AgentFramework.Agents;

internal static class AzureOpenAIClientFactory
{
  /// <summary>
  /// Creates an AzureOpenAIClient using:
  /// - API Key (AZURE_OPENAI_API_KEY env var) → container / CI
  /// - AzureCliCredential → local development (az login)
  /// - ManagedIdentityCredential → hosted in Azure (ACA, AKS, ...)
  /// </summary>
  public static AzureOpenAIClient Create(string endpoint)
  {
    var uri = new Uri(endpoint);

    // Container / CI: API Key hat Vorrang
    if (Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") is { Length: > 0 } apiKey)
      return new AzureOpenAIClient(uri, new Azure.AzureKeyCredential(apiKey));

    string? tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

    TokenCredential credential = new ChainedTokenCredential(
        new AzureCliCredential(new AzureCliCredentialOptions { TenantId = tenantId }),
        new ManagedIdentityCredential()
    );

    return new AzureOpenAIClient(uri, credential);
  }
}
