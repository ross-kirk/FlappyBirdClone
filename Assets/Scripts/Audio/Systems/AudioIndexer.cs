using System.Collections.Generic;

public class AudioIndexer
{
    private readonly Dictionary<string, AudioEvent> _events = new Dictionary<string, AudioEvent>();
    private readonly Dictionary<string, MusicTrack> _tracks = new Dictionary<string, MusicTrack>();

    public float EstimatedMemoryMB { get; private set; }

    public IReadOnlyDictionary<string, AudioEvent> Events => _events;
    public IReadOnlyDictionary<string, MusicTrack> Tracks => _tracks;

    public void Initialize(AudioLibrary library)
    {
        _events.Clear();
        _tracks.Clear();
        EstimatedMemoryMB = 0f;

        foreach (var e in library.Events)
        {
            if (e == null || e.clip == null) continue;
            _events[e.name] = e;
            EstimatedMemoryMB += (e.clip.samples * e.clip.channels * 4) / (1024f * 1024f);
        }

        foreach (var t in library.Tracks)
        {
            if (t == null || t.clip == null) continue;
            t.BuildGrid();
            _tracks[t.name] = t;
            EstimatedMemoryMB += t.EstimateMemoryMB();
        }
    }

    public bool TryGetEvent(string name, out AudioEvent audioEvent) =>
        _events.TryGetValue(name, out audioEvent);

    public bool TryGetTrack(string name, out MusicTrack track) =>
        _tracks.TryGetValue(name, out track);
}
