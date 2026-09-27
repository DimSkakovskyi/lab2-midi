using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Standards;

namespace DataSonificationLab.Data;

public sealed record MidiInstrument(
    int Number,
    byte ProgramNumber,
    string Name)
{
  public string DisplayName => $"{Number} — {Name}";
}

public static class GeneralMidiInstruments
{
  public static IReadOnlyList<MidiInstrument> All { get; } =
      Enum.GetValues<GeneralMidiProgram>()
          .OrderBy(program => (byte)program)
          .Select(program =>
          {
            byte programNumber = (byte)program;

            return new MidiInstrument(
                  Number: programNumber + 1,
                  ProgramNumber: programNumber,
                  Name: Humanize(program.ToString()));
          })
          .ToArray();

  private static string Humanize(string value)
  {
    return Regex.Replace(
        value,
        @"(?<=[a-z])(?=[A-Z0-9])|(?<=[0-9])(?=[A-Za-z])",
        " ");
  }
}