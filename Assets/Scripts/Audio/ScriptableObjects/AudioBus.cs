using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewAudioBus", menuName = "Audio/Audio Bus")]
public class AudioBus : ScriptableObject
{
    [Range(0f, 2f)] public float defaultVolume = 1f;
    [Range(-3f, 3f)] public float defaultPitch = 1f;
    [Tooltip("Optional DSP routing target. When assigned, all sources on this bus will output through this mixer group.")]
    public AudioMixerGroup mixerGroup;
}
