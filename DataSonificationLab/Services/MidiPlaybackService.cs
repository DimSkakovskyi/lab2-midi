using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace DataSonificationLab.Services;

public sealed class MidiPlaybackService : IDisposable
{
  private OutputDevice? _outputDevice;
  private Playback? _playback;
  private bool _disposed;

  public event EventHandler? PlaybackFinished;

  public void Play(string filePath)
  {
    ObjectDisposedException.ThrowIf(
        _disposed,
        this);

    if (!File.Exists(filePath))
    {
      throw new FileNotFoundException(
          "MIDI-файл не знайдено.",
          filePath);
    }

    Stop();

    if (OutputDevice.GetDevicesCount() == 0)
    {
      throw new InvalidOperationException(
          "У системі не знайдено MIDI output device.");
    }

    _outputDevice = OutputDevice.GetByIndex(0);

    MidiFile midiFile = MidiFile.Read(filePath);

    _playback = midiFile.GetPlayback(_outputDevice);

    _playback.Finished += OnPlaybackFinished;

    _playback.Start();
  }

  private void OnPlaybackFinished(
      object? sender,
      EventArgs e)
  {
    PlaybackFinished?.Invoke(this, EventArgs.Empty);
  }

  public void ValidateFile(string filePath)
  {
    ObjectDisposedException.ThrowIf(
        _disposed,
        this);

    if (!File.Exists(filePath))
    {
      throw new FileNotFoundException(
          "MIDI-файл не знайдено.",
          filePath);
    }

    // MidiFile.Read повністю перевіряє структуру файлу.
    // Якщо файл пошкоджений або не є MIDI, бібліотека викине виняток.
    _ = MidiFile.Read(filePath);
  }

  public void Stop()
  {
    try
    {
      if (_playback is not null)
      {
        _playback.Finished -= OnPlaybackFinished;
        _playback.Stop();
      }

      _outputDevice?.TurnAllNotesOff();
    }
    finally
    {
      _playback?.Dispose();
      _playback = null;

      _outputDevice?.Dispose();
      _outputDevice = null;
    }
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    Stop();

    _disposed = true;
    GC.SuppressFinalize(this);
  }
}
