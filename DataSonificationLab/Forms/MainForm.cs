using System.ComponentModel;
using System.Globalization;
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
  private bool _hasInvalidCellInput;
  private bool _showingCellValidationMessage;

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

    _playbackService.PlaybackFinished +=
        PlaybackService_PlaybackFinished;

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
    if (!TryCommitGridInput())
    {
      return;
    }

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
          "Спочатку згенеруйте або відкрийте MIDI-файл.",
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

  private void OpenMidiButton_Click(
      object? sender,
      EventArgs e)
  {
    using var openDialog = new OpenFileDialog
    {
      Title = "Відкрити MIDI-файл",
      Filter = "MIDI files (*.mid;*.midi)|*.mid;*.midi|All files (*.*)|*.*",
      CheckFileExists = true,
      CheckPathExists = true,
      Multiselect = false
    };

    if (openDialog.ShowDialog(this) != DialogResult.OK)
    {
      return;
    }

    try
    {
      _playbackService.Stop();
      _playbackService.ValidateFile(openDialog.FileName);

      _lastMidiFilePath = Path.GetFullPath(
          openDialog.FileName);

      conversionLogTextBox.Text =
          $"Відкрито MIDI-файл:{Environment.NewLine}" +
          $"{Path.GetFileName(_lastMidiFilePath)}" +
          $"{Environment.NewLine}{Environment.NewLine}" +
          "Натисніть «Відтворити», щоб прослухати файл.";

      statusLabel.Text =
          $"Відкрито: {Path.GetFileName(_lastMidiFilePath)}";
    }
    catch (Exception exception)
    {
      _lastMidiFilePath = null;

      MessageBox.Show(
          exception.Message,
          "Не вдалося відкрити MIDI",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);

      statusLabel.Text = "Файл не відкрито.";
    }
  }

  private void StopButton_Click(
      object? sender,
      EventArgs e)
  {
    _playbackService.Stop();
    statusLabel.Text = "Відтворення зупинено.";
  }

  private void PlaybackService_PlaybackFinished(
      object? sender,
      EventArgs e)
  {
    if (IsDisposed || !IsHandleCreated)
    {
      return;
    }

    BeginInvoke(new Action(() =>
    {
      if (!IsDisposed)
      {
        statusLabel.Text = "Відтворення закінчено.";
      }
    }));
  }

  private void NotesGrid_DataError(
      object? sender,
      DataGridViewDataErrorEventArgs e)
  {
    e.ThrowException = false;

    _hasInvalidCellInput = true;

    if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
    {
      notesGrid.Rows[e.RowIndex]
          .Cells[e.ColumnIndex]
          .ErrorText =
              "Значення не відповідає типу або формату колонки.";
    }

    statusLabel.Text =
        "Некоректне значення: введіть число у допустимому діапазоні.";
  }

  private void NotesGrid_CellValidating(
      object? sender,
      DataGridViewCellValidatingEventArgs e)
  {
    if (e.RowIndex < 0 || e.ColumnIndex < 0)
    {
      return;
    }

    DataGridViewColumn column =
        notesGrid.Columns[e.ColumnIndex];

    DataGridViewCell cell =
        notesGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];

    string text =
        Convert.ToString(
            e.FormattedValue,
            CultureInfo.CurrentCulture)?.Trim() ??
        string.Empty;

    string? error = column switch
    {
      _ when column == pitchColumn =>
          ValidateIntegerCell(
              text,
              "Pitch",
              minimum: 0,
              maximum: 127),

      _ when column == velocityColumn =>
          ValidateIntegerCell(
              text,
              "Velocity",
              minimum: 1,
              maximum: 127),

      _ when column == durationColumn =>
          ValidateDurationCell(text),

      _ when column == instrumentColumn &&
             string.IsNullOrWhiteSpace(text) =>
          "Виберіть MIDI-інструмент.",

      _ => null
    };

    if (error is not null)
    {
      e.Cancel = true;
      _hasInvalidCellInput = true;
      cell.ErrorText = error;
      ShowCellValidationError(
          rowNumber: e.RowIndex + 1,
          cell,
          error);
      return;
    }

    cell.ErrorText = string.Empty;
    _hasInvalidCellInput = false;
    statusLabel.Text = "Готово.";
  }

  private void ShowCellValidationError(
      int rowNumber,
      DataGridViewCell cell,
      string error)
  {
    string message =
        $"Рядок {rowNumber}: {error}{Environment.NewLine}{Environment.NewLine}" +
        "Виправте значення в поточній клітинці.";

    statusLabel.Text = message.Replace(
        Environment.NewLine,
        " ");

    if (_showingCellValidationMessage)
    {
      return;
    }

    try
    {
      _showingCellValidationMessage = true;

      MessageBox.Show(
          this,
          message,
          "Помилка введення",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning);
    }
    finally
    {
      _showingCellValidationMessage = false;
    }

    BeginInvoke(new Action(() =>
    {
      if (IsDisposed || cell.DataGridView is null)
      {
        return;
      }

      notesGrid.CurrentCell = cell;
      notesGrid.BeginEdit(selectAll: true);

      if (notesGrid.EditingControl is TextBox editingTextBox)
      {
        editingTextBox.SelectAll();
      }
    }));
  }

  private void NotesGrid_CellParsing(
      object? sender,
      DataGridViewCellParsingEventArgs e)
  {
    if (e.RowIndex < 0 || e.ColumnIndex < 0)
    {
      return;
    }

    DataGridViewColumn column =
        notesGrid.Columns[e.ColumnIndex];

    string text =
        Convert.ToString(
            e.Value,
            CultureInfo.CurrentCulture)?.Trim() ??
        string.Empty;

    if ((column == pitchColumn ||
         column == velocityColumn) &&
        int.TryParse(
            text,
            NumberStyles.Integer,
            CultureInfo.CurrentCulture,
            out int integerValue))
    {
      e.Value = integerValue;
      e.ParsingApplied = true;
      return;
    }

    if (column == durationColumn &&
        TryParseDuration(text, out double durationValue))
    {
      e.Value = durationValue;
      e.ParsingApplied = true;
    }
  }

  private bool TryCommitGridInput()
  {
    bool committed = notesGrid.EndEdit();

    if (committed && !_hasInvalidCellInput)
    {
      return true;
    }

    string message =
        "Виправте некоректне значення у таблиці. " +
        "Числові поля не можуть містити лише пробіли або слова.";

    MessageBox.Show(
        this,
        message,
        "Помилка введення",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning);

    statusLabel.Text = message;
    return false;
  }

  private static string? ValidateIntegerCell(
      string text,
      string fieldName,
      int minimum,
      int maximum)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      return $"Поле {fieldName} не може бути порожнім.";
    }

    if (!int.TryParse(
            text,
            NumberStyles.Integer,
            CultureInfo.CurrentCulture,
            out int value))
    {
      return $"Поле {fieldName} повинно містити ціле число.";
    }

    if (value < minimum || value > maximum)
    {
      return $"Поле {fieldName} повинно бути від {minimum} до {maximum}.";
    }

    return null;
  }

  private static string? ValidateDurationCell(
      string text)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      return "Тривалість не може бути порожньою.";
    }

    if (!TryParseDuration(text, out double value))
    {
      return "Тривалість повинна бути числом, наприклад 0,5 або 1.";
    }

    if (!double.IsFinite(value) || value <= 0 || value > 16)
    {
      return "Тривалість повинна бути більшою за 0 і не перевищувати 16 долей.";
    }

    return null;
  }

  private static bool TryParseDuration(
      string text,
      out double value)
  {
    bool parsed = double.TryParse(
        text,
        NumberStyles.Float,
        CultureInfo.CurrentCulture,
        out value);

    if (!parsed)
    {
      parsed = double.TryParse(
          text.Replace(',', '.'),
          NumberStyles.Float,
          CultureInfo.InvariantCulture,
          out value);
    }

    return parsed;
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
    _playbackService.PlaybackFinished -=
        PlaybackService_PlaybackFinished;

    _playbackService.Dispose();
    base.OnFormClosed(e);
  }
}
