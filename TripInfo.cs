using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AgentFramework;

internal class TripInfo
{
  public required string Name { get; set; }
  public required string Destination { get; set; }
  public required string ShortDescription { get; set; }
  public required string Outline { get; set; }
  public int Cost { get; set; }
  public required string recommendedSession { get; set; }


  public static TripInfo Transform(string raw)
  {
    return JsonSerializer.Deserialize<TripInfo>(raw) ?? throw new InvalidOperationException("Could not deserialize TripInfo");
  }
}