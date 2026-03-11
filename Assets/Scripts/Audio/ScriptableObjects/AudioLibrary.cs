using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParameterBinding
{
    public AudioParameter parameter;
    public AudioBus targetBus;
    public ParameterTarget target;
    public AnimationCurve curve; // maps 0-1 normalized param value to output

    public enum ParameterTarget { Volume, Pitch }
}

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [SerializeField] private List<AudioEvent> events = new List<AudioEvent>();
    [SerializeField] private List<MusicTrack> tracks = new List<MusicTrack>();
    [SerializeField] private List<AudioBus> buses = new List<AudioBus>();
    [SerializeField] private List<AudioParameter> parameters = new List<AudioParameter>();
    [SerializeField] private List<ParameterBinding> parameterBindings = new List<ParameterBinding>();

    public IEnumerable<AudioEvent> Events => events;
    public IEnumerable<MusicTrack> Tracks => tracks;
    public IReadOnlyList<AudioBus> Buses => buses;
    public IReadOnlyList<AudioParameter> Parameters => parameters;
    public IReadOnlyList<ParameterBinding> ParameterBindings => parameterBindings;
}
