using System.ComponentModel;

namespace DataSonificationLab.Forms;

partial class MainForm
{
  private IContainer? components;

  private TableLayoutPanel rootLayout = null!;
  private Label titleLabel = null!;

  private DataGridView notesGrid = null!;
  private DataGridViewTextBoxColumn pitchColumn = null!;
  private DataGridViewTextBoxColumn velocityColumn = null!;
  private DataGridViewTextBoxColumn durationColumn = null!;
  private DataGridViewComboBoxColumn instrumentColumn = null!;

  private FlowLayoutPanel controlsPanel = null!;
  private Label tempoLabel = null!;
  private NumericUpDown tempoNumeric = null!;

  private Button addButton = null!;
  private Button removeButton = null!;
  private Button exampleButton = null!;
  private Button generateButton = null!;
  private Button playButton = null!;
  private Button stopButton = null!;

  private TextBox conversionLogTextBox = null!;
  private Label statusLabel = null!;

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      components?.Dispose();
    }

    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    components = new Container();

    rootLayout = new TableLayoutPanel();
    titleLabel = new Label();

    notesGrid = new DataGridView();
    pitchColumn = new DataGridViewTextBoxColumn();
    velocityColumn = new DataGridViewTextBoxColumn();
    durationColumn = new DataGridViewTextBoxColumn();
    instrumentColumn = new DataGridViewComboBoxColumn();

    controlsPanel = new FlowLayoutPanel();
    tempoLabel = new Label();
    tempoNumeric = new NumericUpDown();

    addButton = new Button();
    removeButton = new Button();
    exampleButton = new Button();
    generateButton = new Button();
    playButton = new Button();
    stopButton = new Button();

    conversionLogTextBox = new TextBox();
    statusLabel = new Label();

    SuspendLayout();

    rootLayout.Dock = DockStyle.Fill;
    rootLayout.Padding = new Padding(12);
    rootLayout.ColumnCount = 1;
    rootLayout.RowCount = 5;

    rootLayout.RowStyles.Add(
        new RowStyle(SizeType.AutoSize));

    rootLayout.RowStyles.Add(
        new RowStyle(SizeType.Percent, 55));

    rootLayout.RowStyles.Add(
        new RowStyle(SizeType.AutoSize));

    rootLayout.RowStyles.Add(
        new RowStyle(SizeType.Percent, 45));

    rootLayout.RowStyles.Add(
        new RowStyle(SizeType.AutoSize));

    titleLabel.AutoSize = true;
    titleLabel.Text = "Соніфікація даних у MIDI";
    titleLabel.Font = new Font(
        "Segoe UI",
        16,
        FontStyle.Bold);

    titleLabel.Margin = new Padding(0, 0, 0, 10);

    notesGrid.Dock = DockStyle.Fill;
    notesGrid.AutoGenerateColumns = false;
    notesGrid.AllowUserToAddRows = false;
    notesGrid.AllowUserToDeleteRows = false;
    notesGrid.SelectionMode =
        DataGridViewSelectionMode.FullRowSelect;

    notesGrid.MultiSelect = false;
    notesGrid.RowHeadersVisible = false;
    notesGrid.AutoSizeColumnsMode =
        DataGridViewAutoSizeColumnsMode.Fill;

    notesGrid.DataError += NotesGrid_DataError;

    pitchColumn.HeaderText = "Pitch (0–127)";
    pitchColumn.DataPropertyName =
        nameof(Models.MidiNoteInput.Pitch);

    velocityColumn.HeaderText = "Velocity (1–127)";
    velocityColumn.DataPropertyName =
        nameof(Models.MidiNoteInput.Velocity);

    durationColumn.HeaderText = "Тривалість";
    durationColumn.DataPropertyName =
        nameof(Models.MidiNoteInput.DurationBeats);

    instrumentColumn.HeaderText = "MIDI-інструмент";
    instrumentColumn.DataPropertyName =
        nameof(Models.MidiNoteInput.InstrumentNumber);

    instrumentColumn.DisplayStyle =
        DataGridViewComboBoxDisplayStyle.DropDownButton;

    notesGrid.Columns.AddRange(
        pitchColumn,
        velocityColumn,
        durationColumn,
        instrumentColumn);

    controlsPanel.Dock = DockStyle.Fill;
    controlsPanel.AutoSize = true;
    controlsPanel.WrapContents = true;
    controlsPanel.Padding = new Padding(0, 8, 0, 8);

    addButton.Text = "Додати ноту";
    addButton.AutoSize = true;
    addButton.Click += AddButton_Click;

    removeButton.Text = "Видалити";
    removeButton.AutoSize = true;
    removeButton.Click += RemoveButton_Click;

    exampleButton.Text = "Приклад";
    exampleButton.AutoSize = true;
    exampleButton.Click += ExampleButton_Click;

    tempoLabel.Text = "Темп:";
    tempoLabel.AutoSize = true;
    tempoLabel.Margin = new Padding(15, 8, 3, 0);

    tempoNumeric.Minimum = 30;
    tempoNumeric.Maximum = 300;
    tempoNumeric.Value = 120;
    tempoNumeric.Width = 70;

    generateButton.Text = "Згенерувати MIDI";
    generateButton.AutoSize = true;
    generateButton.Margin = new Padding(15, 3, 3, 3);
    generateButton.Click += GenerateButton_Click;

    playButton.Text = "Відтворити";
    playButton.AutoSize = true;
    playButton.Click += PlayButton_Click;

    stopButton.Text = "Стоп";
    stopButton.AutoSize = true;
    stopButton.Click += StopButton_Click;

    controlsPanel.Controls.Add(addButton);
    controlsPanel.Controls.Add(removeButton);
    controlsPanel.Controls.Add(exampleButton);
    controlsPanel.Controls.Add(tempoLabel);
    controlsPanel.Controls.Add(tempoNumeric);
    controlsPanel.Controls.Add(generateButton);
    controlsPanel.Controls.Add(playButton);
    controlsPanel.Controls.Add(stopButton);

    conversionLogTextBox.Dock = DockStyle.Fill;
    conversionLogTextBox.Multiline = true;
    conversionLogTextBox.ReadOnly = true;
    conversionLogTextBox.ScrollBars =
        ScrollBars.Vertical;

    conversionLogTextBox.Font = new Font(
        "Consolas",
        10);

    statusLabel.AutoSize = true;
    statusLabel.Text = "Готово.";
    statusLabel.Padding = new Padding(0, 8, 0, 0);

    rootLayout.Controls.Add(titleLabel, 0, 0);
    rootLayout.Controls.Add(notesGrid, 0, 1);
    rootLayout.Controls.Add(controlsPanel, 0, 2);
    rootLayout.Controls.Add(conversionLogTextBox, 0, 3);
    rootLayout.Controls.Add(statusLabel, 0, 4);

    Controls.Add(rootLayout);

    Text = "Data Sonification Lab";
    StartPosition = FormStartPosition.CenterScreen;
    ClientSize = new Size(1000, 650);
    MinimumSize = new Size(800, 500);

    ResumeLayout(false);
  }
}