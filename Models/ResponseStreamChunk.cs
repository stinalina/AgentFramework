namespace AgentFramework.Models;

public record ResponseStreamChunk(
  string? AuthorName,
  string? Text,
  bool IsCompleted = false
);
