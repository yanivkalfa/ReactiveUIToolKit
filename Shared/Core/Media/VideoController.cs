using System;
using UnityEngine;
using UnityEngine.Video;

namespace ReactiveUITK.Core.Media
{
    /// <summary>
    /// Imperative handle exposed by the <c>&lt;Video&gt;</c> element via its
    /// <c>Controller</c> ref. Wraps a single pooled <see cref="VideoPlayer"/>
    /// owned by <see cref="MediaHost"/>; created on mount, detached on
    /// unmount. Method calls on a detached controller no-op silently.
    /// </summary>
    public sealed class VideoController
    {
        private VideoPlayer _vp;

        internal VideoController(VideoPlayer vp)
        {
            _vp = vp;
        }

        /// <summary>True while the underlying VideoPlayer is still owned.</summary>
        public bool IsAttached => _vp != null;

        /// <summary>True if the underlying VideoPlayer is currently playing.</summary>
        public bool IsPlaying => _vp != null && _vp.isPlaying;

        /// <summary>True if the player is fully prepared and ready to play.</summary>
        public bool IsPrepared => _vp != null && _vp.isPrepared;

        /// <summary>Total clip duration in seconds (0 if not yet prepared).</summary>
        public double Duration => _vp != null ? _vp.length : 0d;

        /// <summary>Current playhead position in seconds.</summary>
        public double Time
        {
            get => _vp != null ? _vp.time : 0d;
            set
            {
                if (_vp != null)
                    _vp.time = Math.Max(0d, value);
            }
        }

        /// <summary>Audio volume in [0, 1] on channel 0 (DirectAudio mode).</summary>
        public float Volume
        {
            get => _vp != null ? _vp.GetDirectAudioVolume(0) : 0f;
            set
            {
                if (_vp != null)
                    _vp.SetDirectAudioVolume(0, UnityEngine.Mathf.Clamp01(value));
            }
        }

        /// <summary>Mute toggle on channel 0 (DirectAudio mode).</summary>
        public bool Muted
        {
            get => _vp != null && _vp.GetDirectAudioMute(0);
            set
            {
                if (_vp != null)
                    _vp.SetDirectAudioMute(0, value);
            }
        }

        /// <summary>Playback speed multiplier (1 = normal, 2 = double-speed, etc.).</summary>
        public float PlaybackSpeed
        {
            get => _vp != null ? _vp.playbackSpeed : 1f;
            set
            {
                if (_vp != null)
                    _vp.playbackSpeed = UnityEngine.Mathf.Max(0f, value);
            }
        }

        /// <summary>Current frame index (0-based).</summary>
        public long Frame
        {
            get => _vp != null ? _vp.frame : 0L;
            set
            {
                if (_vp != null)
                    _vp.frame = Math.Max(0L, value);
            }
        }

        /// <summary>Begin playback. No-op if already playing.</summary>
        public void Play()
        {
            if (_vp == null)
                return;
            if (_vp.isPlaying)
                return;
            _vp.Play();
        }

        /// <summary>Pause playback at the current frame.</summary>
        public void Pause()
        {
            if (_vp == null)
                return;
            _vp.Pause();
        }

        /// <summary>Stop playback and rewind to t=0.</summary>
        public void Stop()
        {
            if (_vp == null)
                return;
            _vp.Stop();
        }

        /// <summary>Seek to <paramref name="seconds"/> from clip start (clamped).</summary>
        public void Seek(double seconds)
        {
            if (_vp == null)
                return;
            _vp.time = Math.Clamp(seconds, 0d, _vp.length > 0 ? _vp.length : seconds);
        }

        // ── Internal — called by VideoFunc on cleanup ────────────────────
        internal void __Detach()
        {
            _vp = null;
        }
    }
}
