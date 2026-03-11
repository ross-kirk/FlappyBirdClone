using UnityEngine;

[CreateAssetMenu(fileName = "NewMusicTrack", menuName = "Audio/Music Track")]
public class MusicTrack : ScriptableObject
{
    [Header("Loop")]
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(-3f, 3f)] public float pitch = 1f;
    [Min(1f)] public float bpm = 120f;
    [Min(1)] public int beatsPerBar = 4;
    [Min(1)] public int barsPerLoop = 4;

    [Header("Stingers")]
    [Tooltip("Optional clip played before the loop starts when requested.")]
    public AudioClip intro;
    [Tooltip("Optional clip played when transitioning away from this track when requested.")]
    public AudioClip outro;

    [Header("Routing")]
    [Tooltip("Optional bus for DSP routing and runtime volume/pitch control.")]
    public AudioBus bus;

    [HideInInspector] public double beatDuration;
    [HideInInspector] public double barDuration;
    [HideInInspector] public double loopDuration;

    public void BuildGrid()
    {
        beatDuration = 60.0 / bpm;
        barDuration = beatDuration * beatsPerBar;
        loopDuration = barDuration * barsPerLoop;
    }

    public float EstimateMemoryMB()
    {
        float total = 0f;
        if (clip != null) total += (clip.samples * clip.channels * 4) / (1024f * 1024f);
        if (intro != null) total += (intro.samples * intro.channels * 4) / (1024f * 1024f);
        if (outro != null) total += (outro.samples * outro.channels * 4) / (1024f * 1024f);
        return total;
    }
}
