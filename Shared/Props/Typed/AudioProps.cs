using System;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Media;
using UnityEngine;
using UnityEngine.Audio;

namespace ReactiveUITK.Props.Typed
{
    /// <summary>
    /// Typed props for the <c>&lt;Audio&gt;</c> element. <c>&lt;Audio&gt;</c>
    /// renders nothing visible — it's a side-effect-only Func-Component that
    /// rents a pooled <see cref="AudioSource"/> from <see cref="MediaHost"/>,
    /// applies these settings on mount, and returns the source on unmount.
    /// </summary>
    public sealed class AudioProps : BaseProps
    {
        /// <summary>The clip to play.</summary>
        public AudioClip Clip { get; set; }

        /// <summary>Start playback automatically on mount. Default: true.</summary>
        public bool Autoplay { get; set; } = true;

        /// <summary>Loop the clip. Default: false.</summary>
        public bool Loop { get; set; }

        /// <summary>Volume in [0, 1]. Default: 1.</summary>
        public float Volume { get; set; } = 1f;

        /// <summary>Pitch multiplier. Default: 1. Negative values play in reverse.</summary>
        public float Pitch { get; set; } = 1f;

        /// <summary>Mute toggle. Default: false.</summary>
        public bool Mute { get; set; }

        /// <summary>
        /// Audio priority (0 = highest, 256 = lowest). Default: 128. Sources
        /// with higher priority numbers are virtualized first when Unity hits
        /// its real-voice cap.
        /// </summary>
        public int Priority { get; set; } = 128;

        /// <summary>Optional mixer group for routing.</summary>
        public AudioMixerGroup MixerGroup { get; set; }

        /// <summary>0 = pure 2D (UI sounds), 1 = pure 3D (positional). Default: 0.</summary>
        public float SpatialBlend { get; set; }

        /// <summary>Stereo pan in [-1, 1]. Default: 0 (centered).</summary>
        public float PanStereo { get; set; }

        /// <summary>3D distance attenuation curve. Default: Logarithmic.</summary>
        public AudioRolloffMode RolloffMode { get; set; } = AudioRolloffMode.Logarithmic;

        /// <summary>3D minimum-attenuation distance (full volume inside). Default: 1.</summary>
        public float MinDistance { get; set; } = 1f;

        /// <summary>3D maximum-attenuation distance (silent beyond). Default: 500.</summary>
        public float MaxDistance { get; set; } = 500f;

        /// <summary>
        /// Optional world-space position. When set, parents the AudioSource
        /// to the MediaHost root and places it here so 3D attenuation works.
        /// When null (default), the source sits at the host origin.
        /// </summary>
        public Vector3? WorldPosition { get; set; }

        /// <summary>Fired once when the audio actually starts playing.</summary>
        public Action OnStarted { get; set; }

        /// <summary>Fired when playback completes (single-shot only — not fired on loop).</summary>
        public Action OnEnded { get; set; }

        /// <summary>
        /// Optional ref to an <see cref="AudioController"/> for imperative
        /// access (Play / Pause / Stop / Seek). The controller's
        /// <c>IsAttached</c> flag turns false when the component unmounts.
        /// </summary>
        public Ref<AudioController> Controller { get; set; }

        internal override void __ResetFields()
        {
            Clip = null;
            Autoplay = true;
            Loop = false;
            Volume = 1f;
            Pitch = 1f;
            Mute = false;
            Priority = 128;
            MixerGroup = null;
            SpatialBlend = 0f;
            PanStereo = 0f;
            RolloffMode = AudioRolloffMode.Logarithmic;
            MinDistance = 1f;
            MaxDistance = 500f;
            WorldPosition = null;
            OnStarted = null;
            OnEnded = null;
            Controller = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<AudioProps>.Return(this);
        }
    }
}
