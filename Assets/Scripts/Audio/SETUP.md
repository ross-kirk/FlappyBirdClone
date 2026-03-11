# AudioPackage — Setup Guide

## Drop-in

1. Copy the `AudioPackage` folder into your Unity project's `Assets/` directory.
2. Wait for Unity to compile (no errors expected).

---

## Create your data assets

Right-click in the Project window:

- **Audio > Audio Event** — one per sound effect. Assign a clip, set volume/pitch/priority.
- **Audio > Music Track** — one per music loop. Assign a clip, set BPM, beats per bar, bars per loop.
- **Audio > Audio Library** — one per project. Add your events and tracks to the lists.

---

## Scene setup

1. Create an empty GameObject (e.g. name it `AudioSystem`).
2. Add the **AudioComponent** script to it.
   - `DebugAudioViewer` is added automatically via RequireComponent.
3. Assign your **AudioLibrary** asset to the `Library` field on AudioComponent.
4. Press **Play**.

The debug panel appears in the top-left of the Game view automatically.
Toggle it on/off with the **Show Debug** checkbox on `DebugAudioViewer`.

---

## Debug panel sections

| Section | What it shows |
|---|---|
| MEMORY | Estimated uncompressed PCM footprint, event/track counts |
| MUSIC CONDUCTOR | Current track, BPM, bar/beat position, beat grid visualiser, queued track countdown |
| AUDIO SOURCES | Music source status, active SFX sources with priority |
| CONTROLS | Cycle & play sound events with pitch override slider; play/queue/stop music tracks |

---

## Calling audio from your own scripts

Get a reference to `AudioComponent` (find it, inject it, or use a singleton pattern of your choice), then:

```csharp
// Play a one-shot sound
audio.Player.PlaySound("MyEventAssetName");

// Play with a pitch override
audio.Player.PlaySoundWithPitch("MyEventAssetName", 1.5f);

// Play a looping sound (returns a handle to stop it later)
var handle = audio.Player.PlaySoundLooping("MyEventAssetName");
audio.Player.StopSound(handle);

// Music — play immediately
audio.Player.PlayMusicNow("MyTrackAssetName");

// Music — queue at next bar boundary
audio.Player.QueueMusicAtNextBar("MyTrackAssetName");

// Stop all music
audio.Player.StopMusic();
```

Asset names match the ScriptableObject filename in your Project window.

---

## Notes

- Memory estimate assumes 32-bit float decompressed audio. Actual Unity runtime usage depends on AudioClip load type (Compressed, Decompress on Load, Streaming).
- Music uses DSP-scheduled playback (`AudioSource.PlayScheduled`) for sample-accurate timing.
- Beat/bar sync trusts the BPM and time signature you configure on the MusicTrack asset. Drift over time is proportional to how closely the clip length matches the grid.
- SFX pool is 16 sources. Source stealing is by priority (0 = highest, 256 = lowest). Music sources are never stolen.
