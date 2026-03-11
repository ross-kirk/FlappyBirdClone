using UnityEngine;

public class SourceHandle
{
    public AudioSource Source;
    public bool IsActive;
    public int Priority;
    public string EventName;
    public bool IsMusic;
    public bool IsLooping;
    public int VariantIndex; // -1 = primary clip
    public string ClipName;

    /// <summary>Original event volume before bus scaling and fade.</summary>
    public float BaseVolume = 1f;
    /// <summary>Original event pitch before bus scaling.</summary>
    public float BasePitch = 1f;
    /// <summary>Bus this handle is routed through. Null means no bus (multiplier = 1).</summary>
    public AudioBus Bus;
    /// <summary>0–1 multiplier applied by FadeManager.</summary>
    public float FadeMultiplier = 1f;
}
