namespace DataSonificationLab.Models;

public sealed class MidiNoteInput
{
  public int Pitch { get; set; } = 60;

  public int Velocity { get; set; } = 100;

  public double DurationBeats { get; set; } = 1.0;

  public int InstrumentNumber { get; set; } = 1;
}