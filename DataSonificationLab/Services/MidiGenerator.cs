using System.Text;
using DataSonificationLab.Data;
using DataSonificationLab.Helpers;
using DataSonificationLab.Models;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace DataSonificationLab.Services;

public sealed record MidiGenerationResult(
    string FilePath,
    string ConversionLog);

public sealed class MidiGenerator
{
  public MidiGenerationResult Generate(
      IReadOnlyList<MidiNoteInput> notes,
      CompositionSettings settings,
      string outputPath)
  {
    ArgumentNullException.ThrowIfNull(notes);
    ArgumentNullException.ThrowIfNull(settings);

    if (string.IsNullOrWhiteSpace(outputPath))
    {
      throw new ArgumentException(
          "Шлях до MIDI-файлу не задано.",
          nameof(outputPath));
    }

    var objects = new List<ITimedObject>();
    var log = new StringBuilder();

    long microsecondsPerQuarterNote =
        60_000_000L / settings.TempoBpm;

    objects.Add(
        new TimedEvent(
            new SetTempoEvent(microsecondsPerQuarterNote),
            time: 0));

    var channel = (FourBitNumber)(byte)0;

    long currentTime = 0;

    for (int index = 0; index < notes.Count; index++)
    {
      MidiNoteInput input = notes[index];

      long durationTicks = MidiNoteHelper.BeatsToTicks(
          input.DurationBeats,
          settings.TicksPerQuarterNote);

      byte programNumber =
          (byte)(input.InstrumentNumber - 1);

      var programChange = new ProgramChangeEvent(
          (SevenBitNumber)programNumber)
      {
        Channel = channel
      };

      objects.Add(
          new TimedEvent(
              programChange,
              currentTime));

      long noteStartTime = currentTime + 1;

      var midiNote = new Note(
          noteNumber: (SevenBitNumber)(byte)input.Pitch,
          length: durationTicks,
          time: noteStartTime)
      {
        Velocity =
              (SevenBitNumber)(byte)input.Velocity,

        Channel = channel
      };

      objects.Add(midiNote);

      MidiInstrument instrument =
          GeneralMidiInstruments.All[
              input.InstrumentNumber - 1];

      log.AppendLine($"Нота {index + 1}:");
      log.AppendLine(
          $"  Pitch {input.Pitch} → " +
          MidiNoteHelper.GetNoteName(input.Pitch));

      log.AppendLine(
          $"  Velocity {input.Velocity} → " +
          MidiNoteHelper.GetVelocityDescription(
              input.Velocity));

      log.AppendLine(
          $"  Duration {input.DurationBeats} → " +
          $"{durationTicks} ticks");

      log.AppendLine(
          $"  Instrument {input.InstrumentNumber} → " +
          $"Program {programNumber}, {instrument.Name}");

      log.AppendLine();

      currentTime = noteStartTime + durationTicks;
    }

    var trackChunk = new TrackChunk();

    trackChunk.AddObjects(objects);

    var midiFile = new MidiFile(trackChunk)
    {
      TimeDivision =
            new TicksPerQuarterNoteTimeDivision(
                settings.TicksPerQuarterNote)
    };

    midiFile.Write(
        outputPath,
        overwriteFile: true,
        format: MidiFileFormat.SingleTrack);

    return new MidiGenerationResult(
        outputPath,
        log.ToString());
  }
}