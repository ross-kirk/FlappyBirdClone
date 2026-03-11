using System.Collections.Generic;

public class FadeManager
{
    private class SourceFade
    {
        public float From;
        public float To;
        public float Duration;
        public float Elapsed;
        public bool ReleaseOnComplete;
    }

    private readonly SourceController _sourceController;
    private readonly Dictionary<SourceHandle, SourceFade> _fades
        = new Dictionary<SourceHandle, SourceFade>();

    public FadeManager(SourceController sourceController)
    {
        _sourceController = sourceController;
    }

    /// <summary>Fades the handle's FadeMultiplier from 0 to 1 over <paramref name="duration"/> seconds.</summary>
    public void FadeIn(SourceHandle handle, float duration)
    {
        if (handle == null) return;
        handle.FadeMultiplier = 0f;
        _fades[handle] = new SourceFade
        {
            From = 0f,
            To = 1f,
            Duration = duration,
            Elapsed = 0f,
            ReleaseOnComplete = false
        };
    }

    /// <summary>Fades the handle's FadeMultiplier from its current value to 0 over <paramref name="duration"/> seconds.</summary>
    public void FadeOut(SourceHandle handle, float duration, bool releaseOnComplete = true)
    {
        if (handle == null) return;
        _fades[handle] = new SourceFade
        {
            From = handle.FadeMultiplier,
            To = 0f,
            Duration = duration,
            Elapsed = 0f,
            ReleaseOnComplete = releaseOnComplete
        };
    }

    public bool IsFading(SourceHandle handle)
    {
        return handle != null && _fades.ContainsKey(handle);
    }

    public void CancelFade(SourceHandle handle)
    {
        if (handle != null) _fades.Remove(handle);
    }

    /// <summary>
    /// Advances all active fades. Must be called before BusMixer.Tick() each frame.
    /// </summary>
    public void Tick(float deltaTime)
    {
        var completed = new List<SourceHandle>();

        foreach (var kvp in _fades)
        {
            var handle = kvp.Key;
            var fade = kvp.Value;

            fade.Elapsed += deltaTime;
            float t = fade.Duration > 0f
                ? UnityEngine.Mathf.Clamp01(fade.Elapsed / fade.Duration)
                : 1f;

            handle.FadeMultiplier = UnityEngine.Mathf.Lerp(fade.From, fade.To, t);

            if (t >= 1f)
                completed.Add(handle);
        }

        foreach (var handle in completed)
        {
            bool release = _fades[handle].ReleaseOnComplete;
            _fades.Remove(handle);
            if (release)
                _sourceController.ReleaseHandle(handle);
        }
    }
}
