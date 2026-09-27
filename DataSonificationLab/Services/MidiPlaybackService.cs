using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace DataSonificationLab.Services;

public sealed class MidiPlaybackService : IDisposable
{
  private OutputDevice? _outputDevice;
  private Playback? _playback;
  private bool _disposed;

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

    _playback.Start();
  }

  public void Stop()
  {
    try
    {
      _playback?.Stop();
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