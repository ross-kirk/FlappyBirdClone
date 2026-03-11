using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer
{
    private class EventState
    {
        public int SequenceIndex;
        public List<int> ShuffleQueue = new List<int>();
        public float LastPlayTime = float.NegativeInfinity;
    }
    
    private readonly SourceController _sourceController;
    private readonly AudioIndexer _indexer;
    private readonly MusicConductor _conductor;
    private readonly BusMixer _busMixer;
    private readonly FadeManager _fadeManager;
    private readonly Dictionary<string, EventState> _eventStates = new Dictionary<string, EventState>();

    public AudioPlayer(SourceController sourceController, AudioIndexer indexer,
                       MusicConductor conductor, BusMixer busMixer, FadeManager fadeManager)
    {
        _sourceController = sourceController;
        _indexer = indexer;
        _conductor = conductor;
        _busMixer = busMixer;
        _fadeManager = fadeManager;
    }
    
    public void PlaySound(string eventName)
    {
        if (_indexer.TryGetEvent(eventName, out var e))
            PlaySound(e);
    }

    public void PlaySoundWithPitch(string eventName, float pitch)
    {
        if (_indexer.TryGetEvent(eventName, out var e))
            PlaySound(e, pitch);
    }

    public void PlaySound(AudioEvent audioEvent, float pitchOverride = -1f)
    {
        if (audioEvent == null) return;

        if (IsCoolingDown(audioEvent)) return;

        var clip = SelectClipForEvent(audioEvent, out int variantIndex);
        if (clip == null) return;

        var handle = _sourceController.GetAvailableSfxSource(audioEvent.priority);
        if (handle == null) return;

        float finalVolume = audioEvent.volume
            + Random.Range(-audioEvent.volumeVariance, audioEvent.volumeVariance);
        finalVolume = Mathf.Clamp(finalVolume, 0f, 2f);

        float basePitch = pitchOverride < 0f ? audioEvent.pitch : pitchOverride;
        float finalPitch = basePitch
            + Random.Range(-audioEvent.pitchVariance, audioEvent.pitchVariance);

        // Configure handle
        handle.BaseVolume = finalVolume;
        handle.BasePitch = finalPitch;
        handle.Bus = audioEvent.bus;
        handle.FadeMultiplier = audioEvent.fadeIn > 0f ? 0f : 1f;

        // Configure source
        handle.Source.clip = clip;
        handle.Source.volume = finalVolume;
        handle.Source.pitch = finalPitch;
        handle.Source.priority = audioEvent.priority;
        handle.Source.loop = false;

        handle.EventName = audioEvent.name;
        handle.VariantIndex = variantIndex;
        handle.ClipName = clip.name;
        handle.IsLooping = false;

        // Route through mixer group if bus specifies one
        if (audioEvent.bus != null && audioEvent.bus.mixerGroup != null)
            handle.Source.outputAudioMixerGroup = audioEvent.bus.mixerGroup;

        handle.Source.Play();

        // Fade in if requested
        if (audioEvent.fadeIn > 0f)
            _fadeManager.FadeIn(handle, audioEvent.fadeIn);

        RecordPlay(audioEvent);
    }

    public SourceHandle PlaySoundLooping(string eventName)
    {
        if (!_indexer.TryGetEvent(eventName, out var e)) return null;
        return PlaySoundLooping(e);
    }

    public SourceHandle PlaySoundLooping(AudioEvent audioEvent, float pitchOverride = -1f)
    {
        if (audioEvent == null) return null;

        if (IsCoolingDown(audioEvent)) return null;

        var clip = SelectClipForEvent(audioEvent, out int variantIndex);
        if (clip == null) return null;

        var handle = _sourceController.GetAvailableSfxSource(audioEvent.priority);
        if (handle == null) return null;

        float finalVolume = audioEvent.volume
            + Random.Range(-audioEvent.volumeVariance, audioEvent.volumeVariance);
        finalVolume = Mathf.Clamp(finalVolume, 0f, 2f);

        float basePitch = pitchOverride < 0f ? audioEvent.pitch : pitchOverride;
        float finalPitch = basePitch
            + Random.Range(-audioEvent.pitchVariance, audioEvent.pitchVariance);

        handle.BaseVolume = finalVolume;
        handle.BasePitch = finalPitch;
        handle.Bus = audioEvent.bus;
        handle.FadeMultiplier = audioEvent.fadeIn > 0f ? 0f : 1f;

        handle.Source.clip = clip;
        handle.Source.volume = finalVolume;
        handle.Source.pitch = finalPitch;
        handle.Source.priority = audioEvent.priority;
        handle.Source.loop = true;

        handle.EventName = audioEvent.name;
        handle.VariantIndex = variantIndex;
        handle.ClipName = clip.name;
        handle.IsLooping = true;

        if (audioEvent.bus != null && audioEvent.bus.mixerGroup != null)
            handle.Source.outputAudioMixerGroup = audioEvent.bus.mixerGroup;

        handle.Source.Play();

        if (audioEvent.fadeIn > 0f)
            _fadeManager.FadeIn(handle, audioEvent.fadeIn);

        RecordPlay(audioEvent);
        return handle;
    }

    public void StopSound(SourceHandle handle) =>
        _sourceController.ReleaseHandle(handle);

    public void StopAllSounds()
    {
        foreach (var handle in _sourceController.SfxPool)
            if (handle.IsActive)
                _sourceController.ReleaseHandle(handle);
    }

    // Music

    public void PlayMusicNow(string trackName, bool withIntro = false)
    {
        if (_indexer.TryGetTrack(trackName, out var t))
            _conductor.PlayNow(t, withIntro);
    }

    public void QueueMusicAtNextBar(string trackName, bool withOutro = false, bool withIntro = false)
    {
        if (_indexer.TryGetTrack(trackName, out var t))
            _conductor.QueueAtNextBar(t, withOutro, withIntro);
    }

    public void CrossFadeMusicTo(string trackName, float duration)
    {
        if (_indexer.TryGetTrack(trackName, out var t))
            _conductor.CrossFadeTo(t, duration, _fadeManager);
    }

    public void FadeMusicOut(float duration)
    {
        _conductor.FadeMusicOut(duration, _fadeManager);
    }

    public void StopMusic() => _conductor.StopAll();

    public void StopMusicWithOutro() => _conductor.StopWithOutro();

    public void QueueStopMusicAtNextBar(bool withOutro = false) => _conductor.QueueStopAtNextBar(withOutro);

    // Cooldown

    public float GetCooldownRemaining(string eventName)
    {
        if (!_eventStates.TryGetValue(eventName, out var state)) return 0f;
        if (!_indexer.TryGetEvent(eventName, out var e)) return 0f;

        float elapsed = Time.time - state.LastPlayTime;
        float remaining = e.cooldown - elapsed;
        return Mathf.Max(0f, remaining);
    }

    // Private helpers

    private bool IsCoolingDown(AudioEvent audioEvent)
    {
        if (audioEvent.cooldown <= 0f) return false;
        if (!_eventStates.TryGetValue(audioEvent.name, out var state)) return false;
        return (Time.time - state.LastPlayTime) < audioEvent.cooldown;
    }

    private void RecordPlay(AudioEvent audioEvent)
    {
        if (!_eventStates.TryGetValue(audioEvent.name, out var state))
        {
            state = new EventState();
            _eventStates[audioEvent.name] = state;
        }
        state.LastPlayTime = Time.time;
    }

    private EventState GetOrCreateState(AudioEvent audioEvent)
    {
        if (!_eventStates.TryGetValue(audioEvent.name, out var state))
        {
            state = new EventState();
            _eventStates[audioEvent.name] = state;
        }
        return state;
    }

    private AudioClip SelectClipForEvent(AudioEvent audioEvent, out int variantIndex)
    {
        variantIndex = -1;

        // No variants — always use primary
        if (audioEvent.variants == null || audioEvent.variants.Count == 0)
            return audioEvent.SelectClip(out variantIndex);

        int count = audioEvent.variants.Count;

        switch (audioEvent.playMode)
        {
            case VariantPlayMode.Random:
                return audioEvent.SelectClip(out variantIndex);

            case VariantPlayMode.Sequence:
            {
                var state = GetOrCreateState(audioEvent);
                // Wrap index
                int idx = state.SequenceIndex % count;
                state.SequenceIndex = (idx + 1) % count;

                var v = audioEvent.variants[idx];
                if (v.clip != null)
                {
                    variantIndex = idx;
                    return v.clip;
                }
                // Fallback to primary if variant clip is null
                return audioEvent.SelectClip(out variantIndex);
            }

            case VariantPlayMode.Shuffle:
            {
                var state = GetOrCreateState(audioEvent);
                if (state.ShuffleQueue == null || state.ShuffleQueue.Count == 0)
                    state.ShuffleQueue = GenerateShuffleQueue(count);

                int idx = state.ShuffleQueue[0];
                state.ShuffleQueue.RemoveAt(0);

                var v = audioEvent.variants[idx];
                if (v.clip != null)
                {
                    variantIndex = idx;
                    return v.clip;
                }
                return audioEvent.SelectClip(out variantIndex);
            }

            default:
                return audioEvent.SelectClip(out variantIndex);
        }
    }

    private List<int> GenerateShuffleQueue(int count)
    {
        var list = new List<int>(count);
        for (int i = 0; i < count; i++) list.Add(i);

        for (int i = list.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }
}
