namespace DataSonificationLab.Models;

public sealed class CompositionSettings
{
  public int TempoBpm { get; set; } = 120;

  public short TicksPerQuarterNote { get; set; } = 480;
}