namespace AgentFramework.Models;

internal class TravelAgencyBookingFormat
{
  public required string Destination { get; set; }
  public required string Reisedaten { get; set; }
  public required string Unterkunft { get; set; }
  public required string Abreise { get; set; }
  public required string EmpfohleneAktivitaeten { get; set; }
  public int KostenInEuro { get; set; }
  public int KostenInFremdwährung { get; set; }
  public required string RecommendedSession { get; set; }
}