using System.Collections.Generic;
using UnityEngine;

public enum VariantPlayMode
{
    Random,
    Sequence,
    Shuffle
}

[System.Serializable]
public struct AudioVariant
{
    public AudioClip clip;
    [Min(0f)] public float weight;
}

[CreateAssetMenu(fileName = "NewAudioEvent", menuName = "Audio/Audio Event")]
public class AudioEvent : ScriptableObject
{
    [Header("Core")]
    [Tooltip("Primary clip — used as fallback when no variants are defined.")]
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(-3f, 3f)] public float pitch = 1f;
    [Range(0, 256)] public int priority = 128;
    [TextArea] public string notes;

    [Header("Variants")]
    [Tooltip("Optional weighted variants. When any variant is present, the primary clip is ignored and a clip is chosen according to playMode.")]
    public List<AudioVariant> variants = new List<AudioVariant>();
    public VariantPlayMode playMode = VariantPlayMode.Random;

    [Header("Variation")]
    [Range(0f, 0.5f)] public float pitchVariance;
    [Range(0f, 0.5f)] public float volumeVariance;

    [Header("Timing")]
    [Min(0f)] public float cooldown;
    [Min(0f)] public float fadeIn;

    [Header("Routing")]
    [Tooltip("Optional bus for DSP routing and runtime volume/pitch control.")]
    public AudioBus bus;

    /// <summary>
    /// Returns a clip selected by weighted random from variants if any are defined,
    /// otherwise returns the primary clip. variantIndex is -1 for primary clip.
    /// </summary>
    public AudioClip SelectClip(out int variantIndex)
    {
        variantIndex = -1;

        if (variants == null || variants.Count == 0)
            return clip;

        float totalWeight = 0f;
        for (int i = 0; i < variants.Count; i++)
            if (variants[i].clip != null) totalWeight += variants[i].weight;

        if (totalWeight <= 0f)
            return clip;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < variants.Count; i++)
        {
            if (variants[i].clip == null) continue;
            cumulative += variants[i].weight;
            if (roll <= cumulative)
            {
                variantIndex = i;
                return variants[i].clip;
            }
        }

        return clip;
    }
}
