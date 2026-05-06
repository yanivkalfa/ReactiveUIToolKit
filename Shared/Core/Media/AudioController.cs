using System;
using UnityEngine;
using UnityEngine.Audio;

namespace ReactiveUITK.Core.Media
{
    /// <summary>
    /// Imperative handle exposed by the <c>&lt;Audio&gt;</c> element via its
    /// <c>Controller</c> ref. Wraps a single pooled <see cref="AudioSource"/>
    /// owned by <see cref="MediaHost"/>; the controller is created when the
    /// component mounts and disposed when it unmounts.
    ///
    /// <para>
    /// Threading: Unity audio APIs are main-thread-only. All controller
    /// methods must be invoked from the main thread (which is the only
    /// thread render functions ever run on).
    /// </para>
    ///
    /// <para>
    /// Lifetime: a controller becomes inert once its underlying
    /// <see cref="AudioSource"/> is returned to the pool. Method calls on a
    /// disposed controller no-op; property reads return defaults.
    /// </para>
    /// </summary>
    public sealed class AudioController
    {
        private AudioSource _src;

        internal AudioController(AudioSource src)
        {
            _src = src;
        }

        /// <summary>True while the underlying AudioSource is still owned.</summary>
        public bool IsAttached => _src != null;

        /// <summary>True if the underlying AudioSource is currently playing.</summary>
        public bool IsPlaying => _src != null && _src.isPlaying;

        /// <summary>Currently-assigned clip (read-only — set via props).</summary>
        public AudioClip Clip => _src != null ? _src.clip : null;

        /// <summary>Length of the assigned clip in seconds (0 if no clip).</summary>
        public float Length => _src != null && _src.clip != null ? _src.clip.length : 0f;

        /// <summary>Current playback position in seconds.</summary>
        public float Time
        {
            get => _src != null ? _src.time : 0f;
            set
            {
                if (_src != null)
                    _src.time = Mathf.Max(0f, value);
            }
        }

        /// <summary>Volume in [0, 1].</summary>
        public float Volume
        {
            get => _src != null ? _src.volume : 0f;
            set
            {
                if (_src != null)
                    _src.volume = Mathf.Clamp01(value);
            }
        }

        /// <summary>Pitch multiplier (1.0 = normal). Negative values reverse playback.</summary>
        public float Pitch
        {
            get => _src != null ? _src.pitch : 1f;
            set
            {
                if (_src != null)
                    _src.pitch = value;
            }
        }

        /// <summary>Mute toggle.</summary>
        public bool Mute
        {
            get => _src != null && _src.mute;
            set
            {
                if (_src != null)
                    _src.mute = value;
            }
        }

        /// <summary>
        /// Begin playback. If the source is already playing this is a no-op.
        /// </summary>
        public void Play()
        {
            if (_src == null)
                return;
            if (_src.isPlaying)
                return;
            _src.Play();
        }

        /// <summary>Pause playback. <see cref="Play"/> will resume from the same position.</summary>
        public void Pause()
        {
            if (_src == null)
                return;
            _src.Pause();
        }

        /// <summary>Stop playback and rewind to t=0.</summary>
        public void Stop()
        {
            if (_src == null)
                return;
            _src.Stop();
        }

        /// <summary>
        /// Seek to <paramref name="seconds"/> from the start of the clip. Clamps
        /// to the valid range if the requested time exceeds clip length.
        /// </summary>
        public void Seek(float seconds)
        {
            if (_src == null)
                return;
            _src.time = Mathf.Clamp(seconds, 0f, _src.clip != null ? _src.clip.length : 0f);
        }

        // ── Internal — called by AudioFunc on cleanup ────────────────────
        internal void __Detach()
        {
            _src = null;
        }
    }
}
