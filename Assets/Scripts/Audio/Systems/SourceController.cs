using System.Collections.Generic;
using UnityEngine;

public class SourceController
{
    private const int SfxPoolSize = 16;

    private readonly List<SourceHandle> _sfxPool = new List<SourceHandle>();
    private readonly SourceHandle[] _musicSources = new SourceHandle[2];
    private SourceHandle _stingerSource;

    public IReadOnlyList<SourceHandle> SfxPool => _sfxPool;
    public SourceHandle[] MusicSources => _musicSources;
    public SourceHandle StingerSource => _stingerSource;

    public SourceController(GameObject parent)
    {
        for (int i = 0; i < 2; i++)
        {
            var src = parent.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.priority = 0;
            _musicSources[i] = new SourceHandle { Source = src, IsActive = false, IsMusic = true, Priority = 0 };
        }

        var stingerSrc = parent.AddComponent<AudioSource>();
        stingerSrc.playOnAwake = false;
        stingerSrc.priority = 0;
        _stingerSource = new SourceHandle { Source = stingerSrc, IsActive = false, IsMusic = true, Priority = 0 };

        for (int i = 0; i < SfxPoolSize; i++)
        {
            var src = parent.AddComponent<AudioSource>();
            src.playOnAwake = false;
            _sfxPool.Add(new SourceHandle { Source = src, IsActive = false, Priority = 256, VariantIndex = -1 });
        }
    }

    public SourceHandle GetMusicSource(int slot) => _musicSources[slot % 2];

    public SourceHandle GetAvailableSfxSource(int priority)
    {
        foreach (var handle in _sfxPool)
        {
            if (!handle.IsActive)
            {
                handle.IsActive = true;
                handle.Priority = priority;
                return handle;
            }
        }

        // Source stealing: take the lowest-priority (highest value) active source
        SourceHandle candidate = null;
        int lowestPriorityValue = -1;

        foreach (var handle in _sfxPool)
        {
            if (handle.Priority > lowestPriorityValue)
            {
                lowestPriorityValue = handle.Priority;
                candidate = handle;
            }
        }

        if (candidate != null && priority < lowestPriorityValue)
        {
            ResetHandle(candidate);
            candidate.IsActive = true;
            candidate.Priority = priority;
            return candidate;
        }

        return null;
    }

    public void ReleaseHandle(SourceHandle handle)
    {
        if (handle == null || handle.IsMusic) return;
        ResetHandle(handle);
    }

    public void Tick()
    {
        foreach (var handle in _sfxPool)
        {
            // Looping handles must be stopped explicitly — never auto-release them
            if (handle.IsActive && !handle.IsLooping && !handle.Source.isPlaying)
                ResetHandle(handle);
        }
    }

    private void ResetHandle(SourceHandle handle)
    {
        handle.Source.Stop();
        handle.Source.clip = null;
        handle.Source.loop = false;
        handle.Source.pitch = 1f;
        handle.Source.volume = 1f;
        handle.Source.priority = 128;
        handle.IsActive = false;
        handle.Priority = 256;
        handle.EventName = null;
        handle.IsLooping = false;
        handle.VariantIndex = -1;
        handle.ClipName = null;
        handle.BaseVolume = 1f;
        handle.BasePitch = 1f;
        handle.Bus = null;
        handle.FadeMultiplier = 1f;
    }
}
