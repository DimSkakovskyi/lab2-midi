using System.ComponentModel;
using DataSonificationLab.Data;
using DataSonificationLab.Models;
using DataSonificationLab.Services;

namespace DataSonificationLab.Forms;

public partial class MainForm : Form
{
  private readonly BindingList<MidiNoteInput> _notes = [];

  private readonly MidiInputValidator _validator = new();
  private readonly MidiGenerator _generator = new();
  private readonly MidiPlaybackService _playbackService = new();

  private string? _lastMidiFilePath;

  public MainForm()
  {
    InitializeComponent();

    instrumentColumn.DataSource =
        GeneralMidiInstruments.All.ToList();

    instrumentColumn.DisplayMember =
        nameof(MidiInstrument.DisplayName);

    instrumentColumn.ValueMember =
        nameof(MidiInstrument.Number);

    notesGrid.DataSource = _notes;

    LoadExample();
  }

  private void AddButton_Click(
      object? sender,
      EventArgs e)
  {
    _notes.Add(new MidiNoteInput());
  }

  private void RemoveButton_Click(
      object? sender,
      EventArgs e)
  {
    if (notesGrid.CurrentRow?.DataBoundItem
        is MidiNoteInput selectedNote)
    {
      _notes.Remove(selectedNote);
    }
  }

  private void ExampleButton_Click(
      object? sender,
      EventArgs e)
  {
    LoadExample();
  }

  private void GenerateButton_Click(
      object? sender,
      EventArgs e)
  {
    notesGrid.EndEdit();

    List<MidiNoteInput> notes = _notes.ToList();

    var settings = new CompositionSettings
    {
      TempoBpm = (int)tempoNumeric.Value,
      TicksPerQuarterNote = 480
    };

    IReadOnlyList<string> errors =
        _validator.Validate(notes, settings);

    if (errors.Count > 0)
    {
      MessageBox.Show(
          string.Join(Environment.NewLine, errors),
          "Помилка введення",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning);

      return;
    }

    using var saveDialog = new SaveFileDialog
    {
      Title = "Зберегти MIDI-композицію",
      Filter = "MIDI files (*.mid)|*.mid",
      DefaultExt = "mid",
      AddExtension = true,
      FileName = "composition.mid"
    };

    if (saveDialog.ShowDialog(this) != DialogResult.OK)
    {
      return;
    }

    try
    {
      MidiGenerationResult result =
          _generator.Generate(
              notes,
              settings,
              saveDialog.FileName);

      _lastMidiFilePath = result.FilePath;
      conversionLogTextBox.Text =
          result.ConversionLog;

      statusLabel.Text =
          $"Створено: {Path.GetFileName(result.FilePath)}";
    }
    catch (Exception exception)
    {
      MessageBox.Show(
          exception.Message,
          "Помилка створення MIDI",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
    }
  }

  private void PlayButton_Click(
      object? sender,
      EventArgs e)
  {
    if (string.IsNullOrWhiteSpace(_lastMidiFilePath) ||
        !File.Exists(_lastMidiFilePath))
    {
      MessageBox.Show(
          "Спочатку згенеруйте MIDI-файл.",
          "Файл відсутній",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information);

      return;
    }

    try
    {
      _playbackService.Play(_lastMidiFilePath);
      statusLabel.Text = "Відтворення...";
    }
    catch (Exception exception)
    {
      MessageBox.Show(
          exception.Message,
          "Помилка відтворення",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
    }
  }

  private void StopButton_Click(
      object? sender,
      EventArgs e)
  {
    _playbackService.Stop();
    statusLabel.Text = "Відтворення зупинено.";
  }

  private void NotesGrid_DataError(
      object? sender,
      DataGridViewDataErrorEventArgs e)
  {
    e.ThrowException = false;

    statusLabel.Text =
        "Перевірте формат значення у таблиці.";
  }

  private void LoadExample()
  {
    _notes.Clear();

    _notes.Add(new MidiNoteInput
    {
      Pitch = 60,
      Velocity = 100,
      DurationBeats = 1,
      InstrumentNumber = 1
    });

    _notes.Add(new MidiNoteInput
    {
      Pitch = 64,
      Velocity = 90,
      DurationBeats = 0.5,
      InstrumentNumber = 1
    });

    _notes.Add(new MidiNoteInput
    {
      Pitch = 67,
      Velocity = 110,
      DurationBeats = 1.5,
      InstrumentNumber = 41
    });
  }

  protected override void OnFormClosed(
      FormClosedEventArgs e)
  {
    _playbackService.Dispose();
    base.OnFormClosed(e);
  }
}