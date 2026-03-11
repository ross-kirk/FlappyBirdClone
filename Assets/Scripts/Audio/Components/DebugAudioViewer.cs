using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioComponent))]
public class DebugAudioViewer : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebug = true;

    private AudioComponent _audio;
    private Vector2 _scrollPos;

    private readonly List<string> _eventNames = new List<string>();
    private readonly List<string> _trackNames = new List<string>();
    private int _selectedEventIndex;
    private int _selectedTrackIndex;
    private float _pitchOverride = 1f;
    private bool _loopMode;
    private bool _withIntro;
    private bool _withOutro;

    // Looping handles started from the debug viewer so we can stop them individually
    private readonly List<SourceHandle> _debugLoopingHandles = new List<SourceHandle>();

    private GUIStyle _headerStyle;
    private GUIStyle _boldLabel;
    private GUIStyle _dimLabel;
    private bool _stylesInitialised;

    // Beat flash
    private float _lastBeatTime = float.NegativeInfinity;

    // Lifecycle

    private void Start()
    {
        _audio = GetComponent<AudioComponent>();
        RefreshLists();

        if (_audio?.Conductor != null)
            _audio.Conductor.OnBeat += OnBeat;
    }

    private void OnDestroy()
    {
        if (_audio?.Conductor != null)
            _audio.Conductor.OnBeat -= OnBeat;
    }

    private void OnBeat(int beatIndex)
    {
        _lastBeatTime = Time.time;
    }

    private void RefreshLists()
    {
        if (_audio?.Indexer == null) return;

        _eventNames.Clear();
        _trackNames.Clear();

        foreach (var kvp in _audio.Indexer.Events) _eventNames.Add(kvp.Key);
        foreach (var kvp in _audio.Indexer.Tracks) _trackNames.Add(kvp.Key);

        _selectedEventIndex = Mathf.Clamp(_selectedEventIndex, 0, Mathf.Max(0, _eventNames.Count - 1));
        _selectedTrackIndex = Mathf.Clamp(_selectedTrackIndex, 0, Mathf.Max(0, _trackNames.Count - 1));
    }

    // IMGUI

    private void OnGUI()
    {
        if (!showDebug || _audio == null) return;

        InitStyles();

        float panelWidth = 430f;
        float margin = 10f;

        GUILayout.BeginArea(new Rect(margin, margin, panelWidth, Screen.height - margin * 2), GUI.skin.box);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        DrawMemory();
        Divider();
        DrawMusicConductor();
        Divider();
        DrawSources();
        Divider();
        DrawBuses();
        Divider();
        DrawParameters();
        Divider();
        DrawControls();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    // Sections

    private void DrawMemory()
    {
        GUILayout.Label("MEMORY", _headerStyle);

        if (_audio.Indexer == null)
        {
            GUILayout.Label("Not initialised", _dimLabel);
            return;
        }

        GUILayout.Label($"Estimated PCM footprint:  {_audio.Indexer.EstimatedMemoryMB:F2} MB");
        GUILayout.Label($"Events: {_audio.Indexer.Events.Count}    Tracks: {_audio.Indexer.Tracks.Count}");
        GUILayout.Label("(32-bit float uncompressed estimate)", _dimLabel);
    }

    private void DrawMusicConductor()
    {
        GUILayout.Label("MUSIC CONDUCTOR", _headerStyle);

        var c = _audio.Conductor;
        if (c == null || c.CurrentTrack == null)
        {
            GUILayout.Label("No track playing", _dimLabel);
            return;
        }

        var t = c.CurrentTrack;
        GUILayout.Label($"Track:  {t.name}");
        GUILayout.Label($"BPM: {t.bpm}    Time sig: {t.beatsPerBar}/4    Bars/loop: {t.barsPerLoop}");
        GUILayout.Label($"Bar {c.CurrentBar + 1} / {t.barsPerLoop}    Beat {c.CurrentBeatInBar + 1} / {t.beatsPerBar}");

        DrawBeatGrid(c, t);

        // Beat flash indicator
        float timeSinceBeat = Time.time - _lastBeatTime;
        float flashDuration = 0.15f;
        if (timeSinceBeat < flashDuration)
        {
            float alpha = 1f - (timeSinceBeat / flashDuration);
            Color prev = GUI.color;
            GUI.color = Color.Lerp(new Color(0.1f, 0.3f, 0.1f), Color.green, alpha);
            GUILayout.Box("BEAT", GUILayout.Width(50), GUILayout.Height(20));
            GUI.color = prev;
        }
        else
        {
            Color prev = GUI.color;
            GUI.color = new Color(0.1f, 0.3f, 0.1f);
            GUILayout.Box("BEAT", GUILayout.Width(50), GUILayout.Height(20));
            GUI.color = prev;
        }

        if (c.HasQueuedTrack)
        {
            double secondsUntil = c.QueuedPlayDspTime - AudioSettings.dspTime;
            GUILayout.Label($"Queued: {c.QueuedTrack?.name ?? "—"}  (in {secondsUntil:F2}s)", _boldLabel);
        }

        if (c.IsStingerActive)
            GUILayout.Label($"Stinger: {c.StingerClipName ?? "—"}", _boldLabel);
    }

    private void DrawBeatGrid(MusicConductor c, MusicTrack t)
    {
        GUILayout.BeginHorizontal();
        for (int beat = 0; beat < t.beatsPerBar; beat++)
        {
            bool active = beat == c.CurrentBeatInBar;
            GUI.color = active ? Color.green : new Color(0.35f, 0.35f, 0.35f);
            GUILayout.Box(active ? "■" : "□", GUILayout.Width(32), GUILayout.Height(32));
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        // Beat progress bar
        Rect barRect = GUILayoutUtility.GetRect(0, 8, GUILayout.ExpandWidth(true));
        barRect.x += 4;
        barRect.width -= 8;

        var prev = GUI.color;
        GUI.color = new Color(0.25f, 0.25f, 0.25f);
        GUI.DrawTexture(barRect, Texture2D.whiteTexture);

        GUI.color = Color.green;
        var fill = new Rect(barRect.x, barRect.y, barRect.width * (float)c.BeatProgress, barRect.height);
        GUI.DrawTexture(fill, Texture2D.whiteTexture);

        GUI.color = prev;
    }

    private void DrawSources()
    {
        GUILayout.Label("AUDIO SOURCES", _headerStyle);

        var sources = _audio.Sources;
        if (sources == null) { GUILayout.Label("Not initialised", _dimLabel); return; }

        GUILayout.Label("Music:", _boldLabel);
        foreach (var h in sources.MusicSources)
        {
            string label = h.IsActive ? $"[>] {h.EventName ?? "—"}" : "[_] idle";
            GUILayout.Label("  " + label, h.IsActive ? _boldLabel : _dimLabel);
        }

        // Stinger source
        var stinger = sources.StingerSource;
        if (stinger != null)
        {
            string stingerLabel = stinger.IsActive ? $"[>] stinger: {stinger.Source.clip?.name ?? "—"}" : "[_] idle";
            GUILayout.Label("  " + stingerLabel, stinger.IsActive ? _boldLabel : _dimLabel);
        }

        int active = 0;
        foreach (var h in sources.SfxPool) if (h.IsActive) active++;

        GUILayout.Label($"SFX pool  ({active} / {sources.SfxPool.Count} active):", _boldLabel);

        foreach (var h in sources.SfxPool)
        {
            if (!h.IsActive) continue;

            string variantTag = h.VariantIndex >= 0 ? $"v{h.VariantIndex}: {h.ClipName}" : (h.ClipName ?? "—");
            string loopTag = h.IsLooping ? " [loop]" : "";

            // Show playMode from indexer if available
            string modeTag = "";
            if (h.EventName != null && _audio.Indexer.TryGetEvent(h.EventName, out var evtAsset))
                modeTag = $" [{evtAsset.playMode}]";

            GUILayout.Label($"  [>] {h.EventName ?? "—"}  ({variantTag}){loopTag}{modeTag}  pri:{h.Priority}", _boldLabel);
        }

        if (active == 0)
            GUILayout.Label("  (none active)", _dimLabel);
    }

    private void DrawBuses()
    {
        GUILayout.Label("BUSES", _headerStyle);

        if (_audio.Library == null || _audio.Library.Buses == null || _audio.Library.Buses.Count == 0)
        {
            GUILayout.Label("No buses configured", _dimLabel);
            return;
        }

        foreach (var bus in _audio.Library.Buses)
        {
            if (bus == null) continue;

            GUILayout.Label(bus.name, _boldLabel);

            // Volume slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("Vol:", GUILayout.Width(30));
            float currentVol = _audio.Buses.GetVolume(bus);
            float newVol = GUILayout.HorizontalSlider(currentVol, 0f, 2f);
            GUILayout.Label($"{newVol:F2}", GUILayout.Width(35));
            GUILayout.EndHorizontal();

            if (!Mathf.Approximately(newVol, currentVol))
                _audio.Buses.SetVolume(bus, newVol);

            // Pitch slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pitch:", GUILayout.Width(35));
            float currentPitch = _audio.Buses.GetPitch(bus);
            float newPitch = GUILayout.HorizontalSlider(currentPitch, -3f, 3f);
            GUILayout.Label($"{newPitch:F2}", GUILayout.Width(35));
            GUILayout.EndHorizontal();

            if (!Mathf.Approximately(newPitch, currentPitch))
                _audio.Buses.SetPitch(bus, newPitch);

            // Duck button
            if (GUILayout.Button("Duck 0.5", GUILayout.Width(80)))
                _audio.Duck(bus, 0.5f, 0.1f, 0.5f, 0.3f);

            GUILayout.Space(4);
        }
    }

    private void DrawParameters()
    {
        GUILayout.Label("PARAMETERS", _headerStyle);

        if (_audio.Library == null || _audio.Library.Parameters == null || _audio.Library.Parameters.Count == 0)
        {
            GUILayout.Label("No parameters configured", _dimLabel);
            return;
        }

        foreach (var param in _audio.Library.Parameters)
        {
            if (param == null) continue;

            GUILayout.Label(param.name, _boldLabel);

            GUILayout.BeginHorizontal();
            // We track current value by reading it back; since AudioParameter doesn't store
            // runtime value, we use the default as initial and track via GUI state.
            // For simplicity, we store runtime values locally here.
            float currentVal = GetParameterDisplayValue(param);
            float newVal = GUILayout.HorizontalSlider(currentVal, param.minValue, param.maxValue);
            GUILayout.Label($"{newVal:F3}", GUILayout.Width(45));
            GUILayout.EndHorizontal();

            if (!Mathf.Approximately(newVal, currentVal))
            {
                SetParameterDisplayValue(param, newVal);
                _audio.SetParameter(param, newVal);
            }
        }
    }

    // Simple local storage for parameter display values (so slider doesn't snap to default each frame)
    private readonly Dictionary<AudioParameter, float> _parameterValues = new Dictionary<AudioParameter, float>();

    private float GetParameterDisplayValue(AudioParameter param)
    {
        if (!_parameterValues.TryGetValue(param, out float v))
        {
            v = param.defaultValue;
            _parameterValues[param] = v;
        }
        return v;
    }

    private void SetParameterDisplayValue(AudioParameter param, float value)
    {
        _parameterValues[param] = value;
    }

    private void DrawControls()
    {
        GUILayout.Label("CONTROLS", _headerStyle);

        // SFX
        GUILayout.Label("Sound Event:", _boldLabel);
        if (_eventNames.Count > 0)
        {
            DrawSelector(ref _selectedEventIndex, _eventNames);

            GUILayout.Label($"Pitch override: {_pitchOverride:F2}");
            _pitchOverride = GUILayout.HorizontalSlider(_pitchOverride, 0.1f, 3f);

            _loopMode = GUILayout.Toggle(_loopMode, "Loop mode");

            // Show cooldown if applicable
            string selectedEventName = _eventNames[_selectedEventIndex];
            float remaining = _audio.Player.GetCooldownRemaining(selectedEventName);
            if (remaining > 0f)
                GUILayout.Label($"Cooldown: {remaining:F1}s", _dimLabel);

            if (_loopMode)
            {
                if (GUILayout.Button("Play Looping"))
                {
                    var handle = _audio.Player.PlaySoundLooping(selectedEventName);
                    if (handle != null)
                    {
                        handle.Source.pitch = _pitchOverride;
                        _debugLoopingHandles.Add(handle);
                    }
                }

                // Remove any handles that were stopped externally
                _debugLoopingHandles.RemoveAll(h => !h.IsActive);

                if (_debugLoopingHandles.Count > 0)
                {
                    GUILayout.Label("Active loops:", _boldLabel);
                    for (int i = _debugLoopingHandles.Count - 1; i >= 0; i--)
                    {
                        var h = _debugLoopingHandles[i];
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"  {h.EventName ?? "—"}", GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("Stop", GUILayout.Width(50)))
                        {
                            _audio.Player.StopSound(h);
                            _debugLoopingHandles.RemoveAt(i);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Play Sound"))
                    _audio.Player.PlaySoundWithPitch(selectedEventName, _pitchOverride);
            }
        }
        else
        {
            GUILayout.Label("No events in library", _dimLabel);
        }

        GUILayout.Space(6);

        // Music
        GUILayout.Label("Music Track:", _boldLabel);
        if (_trackNames.Count > 0)
        {
            DrawSelector(ref _selectedTrackIndex, _trackNames);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Now"))
                _audio.Player.PlayMusicNow(_trackNames[_selectedTrackIndex], _withIntro);
            if (GUILayout.Button("Queue at Bar"))
                _audio.Player.QueueMusicAtNextBar(_trackNames[_selectedTrackIndex], _withOutro, _withIntro);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _withIntro = GUILayout.Toggle(_withIntro, "Intro");
            _withOutro = GUILayout.Toggle(_withOutro, "Outro");
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Stop"))
                _audio.Player.StopMusic();
            if (GUILayout.Button("Stop w/ Outro"))
                _audio.Player.StopMusicWithOutro();
            if (GUILayout.Button("Queue Stop"))
                _audio.Player.QueueStopMusicAtNextBar(_withOutro);
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("No tracks in library", _dimLabel);
        }

        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Stop All SFX")) _audio.Player.StopAllSounds();
        if (GUILayout.Button("Refresh Index")) RefreshLists();
        GUILayout.EndHorizontal();
    }

    // Helpers

    private void DrawSelector(ref int index, List<string> names)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("◀", GUILayout.Width(28)))
            index = (index - 1 + names.Count) % names.Count;
        GUILayout.Label(names[index], GUILayout.ExpandWidth(true));
        if (GUILayout.Button("▶", GUILayout.Width(28)))
            index = (index + 1) % names.Count;
        GUILayout.EndHorizontal();
    }

    private void Divider()
    {
        GUILayout.Space(4);
        Rect r = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
        r.x += 4;
        r.width -= 8;
        var prev = GUI.color;
        GUI.color = new Color(0.45f, 0.45f, 0.45f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = prev;
        GUILayout.Space(4);
    }

    private void InitStyles()
    {
        if (_stylesInitialised) return;

        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 13,
            normal = { textColor = new Color(0.4f, 0.85f, 1f) }
        };
        _boldLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
        _dimLabel = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.gray } };

        _stylesInitialised = true;
    }
}
