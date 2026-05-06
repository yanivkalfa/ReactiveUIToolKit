using System;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Media;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;

namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Typed props for the <c>&lt;Video&gt;</c> element. Renders as a
    /// real positionable <c>VisualElement</c> whose
    /// <c>style.backgroundImage</c> is a pooled
    /// <see cref="UnityEngine.RenderTexture"/> updated each frame by a
    /// pooled <see cref="VideoPlayer"/>.
    ///
    /// <para>
    /// Provide either <see cref="Clip"/> (preferred — Asset-imported VideoClip)
    /// or <see cref="Url"/> (HTTP/file path for streaming). If both are set,
    /// <see cref="Url"/> wins.
    /// </para>
    /// </summary>
    public sealed class VideoProps : BaseProps
    {
        /// <summary>The asset-imported video clip to play.</summary>
        public VideoClip Clip { get; set; }

        /// <summary>
        /// Streaming URL alternative to <see cref="Clip"/> (e.g. HTTP, RTMP,
        /// file://). Wins over <see cref="Clip"/> when both are set.
        /// </summary>
        public string Url { get; set; }

        /// <summary>Start playback automatically once prepared. Default: true.</summary>
        public bool Autoplay { get; set; } = true;

        /// <summary>Loop the clip. Default: false.</summary>
        public bool Loop { get; set; }

        /// <summary>Mute the audio track. Default: false.</summary>
        public bool Muted { get; set; }

        /// <summary>Audio volume in [0, 1]. Default: 1.</summary>
        public float Volume { get; set; } = 1f;

        /// <summary>Playback speed multiplier. Default: 1.</summary>
        public float PlaybackSpeed { get; set; } = 1f;

        /// <summary>
        /// How the video texture is fitted into the host VisualElement.
        /// Mirrors <c>ImageProps.ScaleMode</c>: <c>scaleToFit</c> (default),
        /// <c>scaleAndCrop</c>, <c>stretchToFill</c>.
        /// </summary>
        public string ScaleMode { get; set; } = "scaleToFit";

        /// <summary>How VideoPlayer outputs audio. Default: Direct.</summary>
        public VideoAudioOutputMode AudioOutputMode { get; set; } = VideoAudioOutputMode.Direct;

        /// <summary>Optional mixer group when AudioOutputMode is AudioSource.</summary>
        public AudioMixerGroup MixerGroup { get; set; }

        /// <summary>RenderTexture format. Default: ARGB32.</summary>
        public RenderTextureFormat RenderTextureFormat { get; set; } = RenderTextureFormat.ARGB32;

        /// <summary>Fired once the player has finished preparing.</summary>
        public Action OnPrepared { get; set; }

        /// <summary>Fired when playback completes (single-shot only — not fired on loop).</summary>
        public Action OnEnded { get; set; }

        /// <summary>Fired with the error message if VideoPlayer reports one.</summary>
        public Action<string> OnError { get; set; }

        /// <summary>Fired after a successful seek completes.</summary>
        public Action OnSeekCompleted { get; set; }

        /// <summary>
        /// Optional ref to a <see cref="VideoController"/> for imperative
        /// access (Play / Pause / Stop / Seek).
        /// </summary>
        public Ref<VideoController> Controller { get; set; }

        internal override void __ResetFields()
        {
            Clip = null;
            Url = null;
            Autoplay = true;
            Loop = false;
            Muted = false;
            Volume = 1f;
            PlaybackSpeed = 1f;
            ScaleMode = "scaleToFit";
            AudioOutputMode = VideoAudioOutputMode.Direct;
            MixerGroup = null;
            RenderTextureFormat = RenderTextureFormat.ARGB32;
            OnPrepared = null;
            OnEnded = null;
            OnError = null;
            OnSeekCompleted = null;
            Controller = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<VideoProps>.Return(this);
        }
    }
}
