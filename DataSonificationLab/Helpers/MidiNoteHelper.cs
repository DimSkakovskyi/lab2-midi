namespace DataSonificationLab.Helpers;

public static class MidiNoteHelper
{
  private static readonly string[] NoteNames =
  [
      "C", "C#", "D", "D#", "E", "F",
        "F#", "G", "G#", "A", "A#", "B"
  ];

  public static string GetNoteName(int pitch)
  {
    if (pitch is < 0 or > 127)
    {
      throw new ArgumentOutOfRangeException(
          nameof(pitch),
          "Висота MIDI-ноти повинна бути від 0 до 127.");
    }

    string noteName = NoteNames[pitch % 12];
    int octave = pitch / 12 - 1;

    return $"{noteName}{octave}";
  }

  public static long BeatsToTicks(
      double durationBeats,
      short ticksPerQuarterNote)
  {
    if (!double.IsFinite(durationBeats) || durationBeats <= 0)
    {
      throw new ArgumentOutOfRangeException(
          nameof(durationBeats),
          "Тривалість повинна бути додатним числом.");
    }

    if (ticksPerQuarterNote <= 0)
    {
      throw new ArgumentOutOfRangeException(
          nameof(ticksPerQuarterNote));
    }

    long ticks = (long)Math.Round(
        durationBeats * ticksPerQuarterNote,
        MidpointRounding.AwayFromZero);

    return Math.Max(1, ticks);
  }

  public static string GetVelocityDescription(int velocity)
  {
    return velocity switch
    {
      <= 40 => "тихо",
      <= 80 => "середньо",
      <= 110 => "голосно",
      _ => "дуже голосно"
    };
  }
}