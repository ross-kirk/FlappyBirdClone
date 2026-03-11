using System.Collections.Generic;
using UnityEngine;

public class BusMixer
{
    // Nested types

    public class Snapshot
    {
        public string Name;
        public Dictionary<AudioBus, float> Volumes;
        public Dictionary<AudioBus, float> Pitches;
    }

    private class VolumeFade
    {
        public float From;
        public float To;
        public float Duration;
        public float Elapsed;
    }

    private class DuckRoutine
    {
        public enum DuckState { Attack, Hold, Release, Done }

        public float BaseVolume;
        public float TargetVolume; // ducked level
        public float AttackTime;
        public float HoldTime;
        public float ReleaseTime;
        public float Elapsed;
        public DuckState State;

        public float Current(float elapsed)
        {
            switch (State)
            {
                case DuckState.Attack:
                    return Mathf.Lerp(BaseVolume, TargetVolume,
                        AttackTime > 0f ? Elapsed / AttackTime : 1f);
                case DuckState.Hold:
                    return TargetVolume;
                case DuckState.Release:
                    float releaseElapsed = Elapsed - AttackTime - HoldTime;
                    return Mathf.Lerp(TargetVolume, BaseVolume,
                        ReleaseTime > 0f ? releaseElapsed / ReleaseTime : 1f);
                default:
                    return BaseVolume;
            }
        }
    }

    private class SnapshotBlend
    {
        public Snapshot Target;
        public float Duration;
        public float Elapsed;
        public Dictionary<AudioBus, float> FromVolumes;
        public Dictionary<AudioBus, float> FromPitches;
    }

    // State

    private readonly AudioLibrary _library;
    private readonly SourceController _sources;

    private readonly Dictionary<AudioBus, float> _volumes = new Dictionary<AudioBus, float>();
    private readonly Dictionary<AudioBus, float> _pitches = new Dictionary<AudioBus, float>();
    private readonly Dictionary<AudioBus, VolumeFade> _fades = new Dictionary<AudioBus, VolumeFade>();
    private readonly Dictionary<AudioBus, DuckRoutine> _ducks = new Dictionary<AudioBus, DuckRoutine>();
    private readonly List<ParameterBinding> _bindings;

    private SnapshotBlend _activeBlend;

    // Properties

    public IReadOnlyDictionary<AudioBus, float> RuntimeVolumes => _volumes;
    public IReadOnlyDictionary<AudioBus, float> RuntimePitches => _pitches;

    public BusMixer(AudioLibrary library, SourceController sources)
    {
        _library = library;
        _sources = sources;
        _bindings = new List<ParameterBinding>();

        // Seed runtime volumes/pitches from ScriptableObject defaults
        foreach (var bus in library.Buses)
        {
            if (bus == null) continue;
            _volumes[bus] = bus.defaultVolume;
            _pitches[bus] = bus.defaultPitch;
        }

        // Cache bindings
        if (library.ParameterBindings != null)
        {
            foreach (var binding in library.ParameterBindings)
            {
                if (binding != null) _bindings.Add(binding);
            }
        }
    }

    // Public API

    public float GetVolume(AudioBus bus)
    {
        if (bus == null) return 1f;
        return _volumes.TryGetValue(bus, out float v) ? v : bus.defaultVolume;
    }

    public float GetPitch(AudioBus bus)
    {
        if (bus == null) return 1f;
        return _pitches.TryGetValue(bus, out float p) ? p : bus.defaultPitch;
    }

    public void SetVolume(AudioBus bus, float volume, float fadeDuration = 0f)
    {
        if (bus == null) return;

        // Cancel any active duck for this bus
        _ducks.Remove(bus);

        if (fadeDuration <= 0f)
        {
            _volumes[bus] = volume;
            _fades.Remove(bus);
        }
        else
        {
            float current = GetVolume(bus);
            _fades[bus] = new VolumeFade
            {
                From = current,
                To = volume,
                Duration = fadeDuration,
                Elapsed = 0f
            };
        }
    }

    public void SetPitch(AudioBus bus, float pitch)
    {
        if (bus == null) return;
        _pitches[bus] = pitch;
    }

    public void SetParameter(AudioParameter parameter, float value)
    {
        if (parameter == null) return;

        // Normalise value to 0-1 for curve evaluation
        float range = parameter.maxValue - parameter.minValue;
        float normalised = range > 0f
            ? Mathf.Clamp01((value - parameter.minValue) / range)
            : 0f;

        foreach (var binding in _bindings)
        {
            if (binding.parameter != parameter) continue;
            if (binding.targetBus == null) continue;

            float mapped = binding.curve != null ? binding.curve.Evaluate(normalised) : normalised;

            switch (binding.target)
            {
                case ParameterBinding.ParameterTarget.Volume:
                    SetVolume(binding.targetBus, mapped);
                    break;
                case ParameterBinding.ParameterTarget.Pitch:
                    SetPitch(binding.targetBus, mapped);
                    break;
            }
        }
    }

    public void Duck(AudioBus bus, float amount, float attackTime, float holdTime, float releaseTime)
    {
        if (bus == null) return;

        // Cancel any active volume fade for this bus
        _fades.Remove(bus);

        float baseVol = GetVolume(bus);
        _ducks[bus] = new DuckRoutine
        {
            BaseVolume = baseVol,
            TargetVolume = Mathf.Max(0f, baseVol - amount),
            AttackTime = attackTime,
            HoldTime = holdTime,
            ReleaseTime = releaseTime,
            Elapsed = 0f,
            State = DuckRoutine.DuckState.Attack
        };
    }

    public Snapshot TakeSnapshot(string name, IEnumerable<AudioBus> buses)
    {
        var snap = new Snapshot
        {
            Name = name,
            Volumes = new Dictionary<AudioBus, float>(),
            Pitches = new Dictionary<AudioBus, float>()
        };

        foreach (var bus in buses)
        {
            if (bus == null) continue;
            snap.Volumes[bus] = GetVolume(bus);
            snap.Pitches[bus] = GetPitch(bus);
        }

        return snap;
    }

    public void RestoreSnapshot(Snapshot snap, float blendDuration = 0f)
    {
        if (snap == null) return;

        if (blendDuration <= 0f)
        {
            foreach (var kvp in snap.Volumes) _volumes[kvp.Key] = kvp.Value;
            foreach (var kvp in snap.Pitches) _pitches[kvp.Key] = kvp.Value;
            _activeBlend = null;
        }
        else
        {
            // Capture current state as blend start
            var fromVols = new Dictionary<AudioBus, float>();
            var fromPits = new Dictionary<AudioBus, float>();
            foreach (var kvp in snap.Volumes) fromVols[kvp.Key] = GetVolume(kvp.Key);
            foreach (var kvp in snap.Pitches) fromPits[kvp.Key] = GetPitch(kvp.Key);

            _activeBlend = new SnapshotBlend
            {
                Target = snap,
                Duration = blendDuration,
                Elapsed = 0f,
                FromVolumes = fromVols,
                FromPitches = fromPits
            };
        }
    }

    // Tick

    public void Tick(float deltaTime)
    {
        // Process volume fades
        var fadesToRemove = new List<AudioBus>();
        foreach (var kvp in _fades)
        {
            var fade = kvp.Value;
            fade.Elapsed += deltaTime;
            float t = fade.Duration > 0f ? Mathf.Clamp01(fade.Elapsed / fade.Duration) : 1f;
            _volumes[kvp.Key] = Mathf.Lerp(fade.From, fade.To, t);
            if (t >= 1f) fadesToRemove.Add(kvp.Key);
        }
        foreach (var bus in fadesToRemove) _fades.Remove(bus);

        // Process duck routines
        var ducksToRemove = new List<AudioBus>();
        foreach (var kvp in _ducks)
        {
            var duck = kvp.Value;
            duck.Elapsed += deltaTime;

            switch (duck.State)
            {
                case DuckRoutine.DuckState.Attack:
                    if (duck.Elapsed >= duck.AttackTime)
                    {
                        duck.State = DuckRoutine.DuckState.Hold;
                    }
                    break;
                case DuckRoutine.DuckState.Hold:
                    float holdElapsed = duck.Elapsed - duck.AttackTime;
                    if (holdElapsed >= duck.HoldTime)
                    {
                        duck.State = DuckRoutine.DuckState.Release;
                    }
                    break;
                case DuckRoutine.DuckState.Release:
                    float releaseElapsed = duck.Elapsed - duck.AttackTime - duck.HoldTime;
                    if (releaseElapsed >= duck.ReleaseTime)
                    {
                        duck.State = DuckRoutine.DuckState.Done;
                        _volumes[kvp.Key] = duck.BaseVolume;
                        ducksToRemove.Add(kvp.Key);
                        continue;
                    }
                    break;
                case DuckRoutine.DuckState.Done:
                    ducksToRemove.Add(kvp.Key);
                    continue;
            }

            if (duck.State != DuckRoutine.DuckState.Done)
                _volumes[kvp.Key] = duck.Current(duck.Elapsed);
        }
        foreach (var bus in ducksToRemove) _ducks.Remove(bus);

        // Process snapshot blend
        if (_activeBlend != null)
        {
            _activeBlend.Elapsed += deltaTime;
            float t = _activeBlend.Duration > 0f
                ? Mathf.Clamp01(_activeBlend.Elapsed / _activeBlend.Duration)
                : 1f;

            foreach (var kvp in _activeBlend.Target.Volumes)
            {
                float from = _activeBlend.FromVolumes.TryGetValue(kvp.Key, out float fv) ? fv : GetVolume(kvp.Key);
                _volumes[kvp.Key] = Mathf.Lerp(from, kvp.Value, t);
            }
            foreach (var kvp in _activeBlend.Target.Pitches)
            {
                float from = _activeBlend.FromPitches.TryGetValue(kvp.Key, out float fp) ? fp : GetPitch(kvp.Key);
                _pitches[kvp.Key] = Mathf.Lerp(from, kvp.Value, t);
            }

            if (t >= 1f) _activeBlend = null;
        }

        // Apply final volumes/pitches to all active handles
        ApplyToHandles(_sources.SfxPool);
        ApplyToHandles(_sources.MusicSources);
        ApplyToHandle(_sources.StingerSource);
    }

    // Helpers

    private void ApplyToHandles(IEnumerable<SourceHandle> handles)
    {
        foreach (var handle in handles)
            ApplyToHandle(handle);
    }

    private void ApplyToHandle(SourceHandle handle)
    {
        if (handle == null || !handle.IsActive || handle.Source == null) return;
        handle.Source.volume = handle.BaseVolume * GetVolume(handle.Bus) * handle.FadeMultiplier;
        handle.Source.pitch = handle.BasePitch * GetPitch(handle.Bus);
    }
}
