using DataSonificationLab.Models;

namespace DataSonificationLab.Services;

public sealed class MidiInputValidator
{
  public IReadOnlyList<string> Validate(
      IReadOnlyList<MidiNoteInput> notes,
      CompositionSettings settings)
  {
    ArgumentNullException.ThrowIfNull(notes);
    ArgumentNullException.ThrowIfNull(settings);

    var errors = new List<string>();

    if (notes.Count == 0)
    {
      errors.Add("Додайте хоча б одну ноту.");
    }

    if (settings.TempoBpm is < 30 or > 300)
    {
      errors.Add("Темп повинен бути від 30 до 300 BPM.");
    }

    if (settings.TicksPerQuarterNote <= 0)
    {
      errors.Add(
          "Кількість ticks на чверть повинна бути більшою за нуль.");
    }

    for (int index = 0; index < notes.Count; index++)
    {
      MidiNoteInput note = notes[index];
      int rowNumber = index + 1;

      if (note.Pitch is < 0 or > 127)
      {
        errors.Add(
            $"Рядок {rowNumber}: pitch повинен бути від 0 до 127.");
      }

      if (note.Velocity is < 1 or > 127)
      {
        errors.Add(
            $"Рядок {rowNumber}: velocity повинна бути від 1 до 127.");
      }

      if (!double.IsFinite(note.DurationBeats) ||
          note.DurationBeats <= 0 ||
          note.DurationBeats > 16)
      {
        errors.Add(
            $"Рядок {rowNumber}: тривалість повинна бути " +
            "більшою за 0 і не перевищувати 16 долей.");
      }

      if (note.InstrumentNumber is < 1 or > 128)
      {
        errors.Add(
            $"Рядок {rowNumber}: номер інструмента " +
            "повинен бути від 1 до 128.");
      }
    }

    return errors;
  }
}