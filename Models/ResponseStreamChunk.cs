namespace AgentFramework.Models;

public record ResponseStreamChunk ( // Step 16
  string? AuthorName,
  string? Text,
  bool IsCompleted = false
);
