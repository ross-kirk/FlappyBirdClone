using System;
using UnityEngine;

[RequireComponent(typeof(DebugAudioViewer))]
public class AudioComponent : MonoBehaviour
{
    public static AudioComponent Instance { get; private set; }
    
    [SerializeField] private AudioLibrary library;

    public AudioLibrary Library { get; private set; }
    public AudioPlayer Player { get; private set; }
    public MusicConductor Conductor { get; private set; }
    public AudioIndexer Indexer { get; private set; }
    public SourceController Sources { get; private set; }
    public BusMixer Buses { get; private set; }
    public FadeManager Fades { get; private set; }

    private void Awake()
    {
        if (library == null)
        {
            Debug.LogError("[AudioComponent] No AudioLibrary assigned. Assign one in the Inspector.");
            enabled = false;
            return;
        }

        Sources = new SourceController(gameObject);
        Indexer = new AudioIndexer();
        Indexer.Initialize(library);
        Conductor = new MusicConductor(Sources);
        Fades = new FadeManager(Sources);
        Buses = new BusMixer(library, Sources);
        Player = new AudioPlayer(Sources, Indexer, Conductor, Buses, Fades);
        Library = library;

        Debug.Log($"[AudioComponent] Ready — {Indexer.Events.Count} events, {Indexer.Tracks.Count} tracks, ~{Indexer.EstimatedMemoryMB:F2} MB");
    }

    private void Start()
    {
        if(Instance == null || Instance == this)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[AudioComponent] Multiple instances detected. Destroying duplicate.");
            Destroy(this);
        }
    }

    private void Update()
    {
        Sources?.Tick();
        Conductor?.Tick();
        Fades?.Tick(Time.deltaTime);
        Buses?.Tick(Time.deltaTime);
    }

    // Convenience API

    public void SetParameter(AudioParameter parameter, float value) =>
        Buses.SetParameter(parameter, value);

    public void Duck(AudioBus bus, float amount, float attack, float hold, float release) =>
        Buses.Duck(bus, amount, attack, hold, release);

    public BusMixer.Snapshot TakeSnapshot(string name) =>
        Buses.TakeSnapshot(name, Library.Buses);

    public void RestoreSnapshot(BusMixer.Snapshot snap, float blend = 0f) =>
        Buses.RestoreSnapshot(snap, blend);
    
    public void PlaySound(string eventName) => Player.PlaySound(eventName);
}
