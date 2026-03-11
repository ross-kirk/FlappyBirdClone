using System;
using UnityEngine;

public class MusicConductor
{
    private readonly SourceController _sourceController;
    private SourceHandle _stingerHandle;

    private MusicTrack _currentTrack;
    private MusicTrack _queuedTrack;
    private SourceHandle _currentHandle;
    private SourceHandle _nextHandle;

    private double _musicStartDspTime;
    private double _queuedPlayDspTime;
    private double _queuedTrackingStartDspTime; // DSP time from which beat tracking starts for the queued track
    private bool _hasQueuedTrack;
    private int _currentMusicSlot;

    // Intro scheduled after an outro finishes (when both are requested in a queue)
    private bool _hasPendingIntro;
    private AudioClip _pendingIntroClip;
    private float _pendingIntroPitch;
    private double _pendingIntroScheduledTime;

    // DSP time the stinger was scheduled to start — used to avoid false cleanup
    // before the stinger has had a chance to begin playing
    private double _stingerScheduledStartTime;

    // CrossFade state
    private bool _isCrossfading;
    private double _crossfadeNewTrackStartDspTime;

    // Fade-out state
    private bool _isFadingOut;
    private double _fadingOutDspTime;

    // Pending stop state (stop-with-outro or queue-stop-at-next-bar)
    private bool _isPendingStop;
    private double _pendingStopDspTime;

    // Beat/bar event tracking
    private int _prevBeat = -1;
    private int _prevBar = -1;
    private int _loopCount = 0;

    // Public events

    /// <summary>Fires when the beat within the current bar changes. Passes the new beat index (0-based).</summary>
    public event Action<int> OnBeat;
    /// <summary>Fires when the current bar changes. Passes the new bar index (0-based).</summary>
    public event Action<int> OnBar;
    /// <summary>Fires when the track loops back to the start.</summary>
    public event Action OnLoop;

    // Public state for debug viewer

    public MusicTrack CurrentTrack => _currentTrack;
    public MusicTrack QueuedTrack => _queuedTrack;
    public bool IsPlaying => _currentHandle != null && _currentHandle.Source.isPlaying;
    public bool HasQueuedTrack => _hasQueuedTrack;
    public double QueuedPlayDspTime => _queuedPlayDspTime;
    public bool IsStingerActive => _stingerHandle != null && _stingerHandle.IsActive;
    public bool IsPendingStop => _isPendingStop;
    public string StingerClipName { get; private set; }

    public int CurrentBar { get; private set; }
    public int CurrentBeatInBar { get; private set; }
    public double BeatProgress { get; private set; }
    public double BarProgress { get; private set; }

    public MusicConductor(SourceController sourceController)
    {
        _sourceController = sourceController;
        _stingerHandle = sourceController.StingerSource;
    }

    // Playback

    /// <param name="withIntro">Play track.intro stinger before the loop begins.</param>
    public void PlayNow(MusicTrack track, bool withIntro = false)
    {
        StopAll();

        _currentMusicSlot = 0;
        _currentHandle = _sourceController.GetMusicSource(0);
        _currentTrack = track;

        double startTime = AudioSettings.dspTime + 0.1;
        double loopStartTime = startTime;

        if (withIntro && track.intro != null)
        {
            ScheduleStinger(track.intro, startTime, track.pitch);
            loopStartTime = startTime + track.intro.length / track.pitch;
        }

        // Beat tracking starts from the very beginning (intro or loop start) so
        // beats fire in sync throughout the intro and loop.
        _musicStartDspTime = startTime;

        _currentHandle.Source.clip = track.clip;
        _currentHandle.Source.volume = track.volume;
        _currentHandle.Source.pitch = track.pitch;
        _currentHandle.Source.loop = true;
        _currentHandle.Source.PlayScheduled(loopStartTime);

        _currentHandle.IsActive = true;
        _currentHandle.EventName = track.name;

        // Set bus fields on handle
        _currentHandle.BaseVolume = track.volume;
        _currentHandle.BasePitch = track.pitch;
        _currentHandle.Bus = track.bus;
        _currentHandle.FadeMultiplier = 1f;

        _prevBeat = -1;
        _prevBar = -1;
        _loopCount = 0;
    }

    /// <param name="withOutro">Play the current track's outro before switching.</param>
    /// <param name="withIntro">Play the incoming track's intro before its loop begins.</param>
    public void QueueAtNextBar(MusicTrack track, bool withOutro = false, bool withIntro = false)
    {
        if (_currentTrack == null)
        {
            PlayNow(track, withIntro);
            return;
        }

        double elapsed = AudioSettings.dspTime - _musicStartDspTime;
        int barsElapsed = (int)(elapsed / _currentTrack.barDuration);
        double transitionTime = _musicStartDspTime + (barsElapsed + 1) * _currentTrack.barDuration;

        double loopStartTime = transitionTime;

        if (withOutro && _currentTrack.outro != null)
        {
            // Stop current loop at the bar boundary and play outro over it
            _currentHandle.Source.SetScheduledEndTime(transitionTime);
            double outroLength = _currentTrack.outro.length / _currentTrack.pitch;
            ScheduleStinger(_currentTrack.outro, transitionTime, _currentTrack.pitch);
            loopStartTime = transitionTime + outroLength;

            if (withIntro && track.intro != null)
            {
                // Outro finishes, then intro plays, then loop — schedule intro via pending state
                double introLength = track.intro.length / track.pitch;
                _hasPendingIntro = true;
                _pendingIntroClip = track.intro;
                _pendingIntroPitch = track.pitch;
                _pendingIntroScheduledTime = loopStartTime;
                loopStartTime += introLength;
            }
        }
        else if (withIntro && track.intro != null)
        {
            // No outro: intro plays at the bar boundary, loop deferred
            double introLength = track.intro.length / track.pitch;
            ScheduleStinger(track.intro, transitionTime, track.pitch);
            loopStartTime = transitionTime + introLength;
        }

        _queuedTrack = track;
        _queuedPlayDspTime = loopStartTime;
        // Beat tracking for the new track starts from the bar boundary (covers outro/intro in tempo)
        _queuedTrackingStartDspTime = transitionTime;
        _hasQueuedTrack = true;
        _isCrossfading = false;

        _nextHandle = _sourceController.GetMusicSource(1 - _currentMusicSlot);
        _nextHandle.Source.Stop();
        _nextHandle.Source.clip = track.clip;
        _nextHandle.Source.volume = track.volume;
        _nextHandle.Source.pitch = track.pitch;
        _nextHandle.Source.loop = true;
        _nextHandle.Source.PlayScheduled(loopStartTime);
        _nextHandle.IsActive = true;
        _nextHandle.EventName = track.name;

        _nextHandle.BaseVolume = track.volume;
        _nextHandle.BasePitch = track.pitch;
        _nextHandle.Bus = track.bus;
        _nextHandle.FadeMultiplier = 1f;
    }

    /// <summary>
    /// Initiates a cross-fade from the current track to <paramref name="track"/> over
    /// <paramref name="crossfadeDuration"/> seconds.
    /// </summary>
    public void CrossFadeTo(MusicTrack track, float crossfadeDuration, FadeManager fadeManager)
    {
        if (track == null) return;

        double dspTime = AudioSettings.dspTime;
        double startDspTime = dspTime + 0.05;

        // Clear any queued state
        _hasQueuedTrack = false;
        _queuedTrack = null;
        _hasPendingIntro = false;

        int nextSlot = 1 - _currentMusicSlot;
        _nextHandle = _sourceController.GetMusicSource(nextSlot);
        _nextHandle.Source.Stop();
        _nextHandle.Source.clip = track.clip;
        _nextHandle.Source.loop = true;
        _nextHandle.Source.PlayScheduled(startDspTime);
        _nextHandle.IsActive = true;
        _nextHandle.EventName = track.name;

        _nextHandle.BaseVolume = track.volume;
        _nextHandle.BasePitch = track.pitch;
        _nextHandle.Bus = track.bus;
        _nextHandle.FadeMultiplier = 0f;

        // Set volume to 0 initially so BusMixer picks up the correct base
        _nextHandle.Source.volume = 0f;
        _nextHandle.Source.pitch = track.pitch;

        fadeManager.FadeIn(_nextHandle, crossfadeDuration);

        if (_currentHandle != null && _currentHandle.IsActive)
            fadeManager.FadeOut(_currentHandle, crossfadeDuration, releaseOnComplete: false);

        _queuedTrack = track;
        _queuedPlayDspTime = dspTime + crossfadeDuration;
        _hasQueuedTrack = true;
        _isCrossfading = true;
        _crossfadeNewTrackStartDspTime = startDspTime;
    }

    /// <summary>Plays the current track's outro stinger then stops. Falls back to immediate stop if no outro is set.</summary>
    public void StopWithOutro()
    {
        if (_currentTrack == null) return;
        if (_currentTrack.outro == null) { StopAll(); return; }

        double startTime = AudioSettings.dspTime + 0.05;
        _currentHandle?.Source.SetScheduledEndTime(startTime);
        ScheduleStinger(_currentTrack.outro, startTime, _currentTrack.pitch);
        _pendingStopDspTime = startTime + _currentTrack.outro.length / _currentTrack.pitch;
        _isPendingStop = true;
    }

    /// <summary>Schedules a stop at the next bar boundary, optionally playing the current track's outro.</summary>
    public void QueueStopAtNextBar(bool withOutro = false)
    {
        if (_currentTrack == null) { StopAll(); return; }

        double elapsed = AudioSettings.dspTime - _musicStartDspTime;
        int barsElapsed = (int)(elapsed / _currentTrack.barDuration);
        double nextBarTime = _musicStartDspTime + (barsElapsed + 1) * _currentTrack.barDuration;

        if (withOutro && _currentTrack.outro != null)
        {
            _currentHandle?.Source.SetScheduledEndTime(nextBarTime);
            ScheduleStinger(_currentTrack.outro, nextBarTime, _currentTrack.pitch);
            _pendingStopDspTime = nextBarTime + _currentTrack.outro.length / _currentTrack.pitch;
        }
        else
        {
            _pendingStopDspTime = nextBarTime;
        }

        _isPendingStop = true;
    }

    /// <summary>Fades the currently playing music out over <paramref name="duration"/> seconds.</summary>
    public void FadeMusicOut(float duration, FadeManager fadeManager)
    {
        if (_currentHandle == null || !_currentHandle.IsActive) return;
        fadeManager.FadeOut(_currentHandle, duration, releaseOnComplete: false);
        _fadingOutDspTime = AudioSettings.dspTime + duration;
        _isFadingOut = true;
    }

    public void StopAll()
    {
        for (int i = 0; i < 2; i++)
        {
            var h = _sourceController.GetMusicSource(i);
            h.Source.Stop();
            h.Source.clip = null;
            h.IsActive = false;
            h.EventName = null;
        }

        if (_stingerHandle != null)
        {
            _stingerHandle.Source.Stop();
            _stingerHandle.Source.clip = null;
            _stingerHandle.IsActive = false;
        }

        _currentTrack = null;
        _queuedTrack = null;
        _hasQueuedTrack = false;
        _currentHandle = null;
        _nextHandle = null;
        _hasPendingIntro = false;
        _pendingIntroClip = null;
        _isCrossfading = false;
        _isFadingOut = false;
        _isPendingStop = false;
        StingerClipName = null;
        CurrentBar = 0;
        CurrentBeatInBar = 0;
        BeatProgress = 0;
        BarProgress = 0;
    }

    // Tick (called from MonoBehaviour.Update)

    public void Tick()
    {
        double now = AudioSettings.dspTime;

        // Handle fade-out completion
        if (_isFadingOut && now >= _fadingOutDspTime)
        {
            StopAll();
            _isFadingOut = false;
            return;
        }

        // Handle pending stop (stop-with-outro / queue-stop-at-next-bar)
        if (_isPendingStop && now >= _pendingStopDspTime)
        {
            _isPendingStop = false;
            StopAll();
            return;
        }

        // When an outro is playing and an intro is pending, schedule the intro
        // once we're close enough for PlayScheduled to accept it
        if (_hasPendingIntro && now >= _pendingIntroScheduledTime - 0.2)
        {
            ScheduleStinger(_pendingIntroClip, _pendingIntroScheduledTime, _pendingIntroPitch);
            _hasPendingIntro = false;
            _pendingIntroClip = null;
        }

        // Clean up stinger source once the clip has finished playing.
        // We wait until past the scheduled start time to avoid a false cleanup
        // during the brief window between PlayScheduled and actual playback.
        if (_stingerHandle != null && _stingerHandle.IsActive &&
            now > _stingerScheduledStartTime + 0.05 &&
            !_stingerHandle.Source.isPlaying &&
            !_hasPendingIntro)
        {
            _stingerHandle.IsActive = false;
            _stingerHandle.Source.clip = null;
            StingerClipName = null;
        }

        if (_currentTrack == null || _currentTrack.loopDuration <= 0) return;

        double elapsed = now - _musicStartDspTime;
        if (elapsed < 0.0) elapsed = 0.0; // clamp: can be slightly negative right after swap

        double loopDuration = _currentTrack.loopDuration;
        double posInLoop = elapsed % loopDuration;

        double currentBeat = posInLoop / _currentTrack.beatDuration;
        int totalBeat = (int)currentBeat;

        int newBeatInBar = totalBeat % _currentTrack.beatsPerBar;
        int newBar = totalBeat / _currentTrack.beatsPerBar;

        CurrentBeatInBar = newBeatInBar;
        CurrentBar = newBar;
        BeatProgress = currentBeat - (int)currentBeat;
        BarProgress = (currentBeat % _currentTrack.beatsPerBar) / _currentTrack.beatsPerBar;

        // Fire beat event
        if (newBeatInBar != _prevBeat)
        {
            _prevBeat = newBeatInBar;
            OnBeat?.Invoke(newBeatInBar);
        }

        // Fire bar event
        if (newBar != _prevBar)
        {
            _prevBar = newBar;
            OnBar?.Invoke(newBar);
        }

        // Detect loop
        int newLoopCount = loopDuration > 0 ? (int)(elapsed / loopDuration) : 0;
        if (newLoopCount > _loopCount)
        {
            _loopCount = newLoopCount;
            OnLoop?.Invoke();
        }

        // Swap tracking to queued track once its loop start time arrives
        if (_hasQueuedTrack && now >= _queuedPlayDspTime - 0.01)
        {
            if (!_isCrossfading)
            {
                _currentHandle.Source.Stop();
                _currentHandle.IsActive = false;
                // Use tracking start (bar boundary) so beats fired through outro/intro are contiguous
                _musicStartDspTime = _queuedTrackingStartDspTime;
            }
            else
            {
                // For crossfade: the new track already started at _crossfadeNewTrackStartDspTime
                _currentHandle.IsActive = false;
                _musicStartDspTime = _crossfadeNewTrackStartDspTime;
                _isCrossfading = false;
            }

            _currentHandle = _nextHandle;
            _currentTrack = _queuedTrack;
            _currentMusicSlot = 1 - _currentMusicSlot;

            _queuedTrack = null;
            _hasQueuedTrack = false;
            _nextHandle = null;

            _prevBeat = -1;
            _prevBar = -1;
            _loopCount = 0;
        }
    }

    // Helpers

    private void ScheduleStinger(AudioClip clip, double dspTime, float pitch = 1f)
    {
        if (_stingerHandle == null || clip == null) return;

        _stingerHandle.Source.Stop();
        _stingerHandle.Source.clip = clip;
        _stingerHandle.Source.loop = false;
        _stingerHandle.Source.volume = 1f;
        _stingerHandle.Source.pitch = pitch;
        _stingerHandle.Source.PlayScheduled(dspTime);
        _stingerHandle.IsActive = true;
        _stingerScheduledStartTime = dspTime;
        StingerClipName = clip.name;
    }
}
