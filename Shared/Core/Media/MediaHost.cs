using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;

namespace ReactiveUITK.Core.Media
{
    /// <summary>
    /// Hidden, lazy, <c>DontDestroyOnLoad</c> <see cref="GameObject"/> that owns
    /// pooled <see cref="VideoPlayer"/>, <see cref="AudioSource"/> and
    /// <see cref="RenderTexture"/> peers used by <c>&lt;Video&gt;</c>,
    /// <c>&lt;Audio&gt;</c> and the <c>useSfx</c> hook.
    ///
    /// <para>
    /// Lifecycle:
    ///   - The host <see cref="GameObject"/> is created on first access and
    ///     hidden via <see cref="HideFlags.HideAndDontSave"/>.
    ///   - <see cref="UnityEngine.Object.DontDestroyOnLoad"/> keeps it alive
    ///     across scene changes.
    ///   - <see cref="ResetForPlayMode"/> (<c>RuntimeInitializeOnLoadMethod</c>
    ///     at <c>SubsystemRegistration</c>) clears the pools when entering
    ///     play-mode in the Editor — matches <c>UitkxAssetRegistry</c>'s
    ///     cache-reset semantics.
    /// </para>
    ///
    /// <para>
    /// All pools are simple LIFO stacks. Renting is O(1) (pop or new+attach),
    /// returning is O(1) (push). RenderTextures additionally bucket by
    /// <c>(width, height, format, depth)</c> so a resized <c>&lt;Video&gt;</c>
    /// can swap textures without per-frame GPU memory churn.
    /// </para>
    ///
    /// <para>
    /// This class is purely runtime — it deliberately does not depend on
    /// <c>UnityEditor</c> so that Player builds and standalone runtime tests
    /// can exercise the full pool lifecycle.
    /// </para>
    /// </summary>
    public sealed class MediaHost
    {
        // ── Singleton ─────────────────────────────────────────────────────
        private static MediaHost s_instance;

        /// <summary>
        /// Lazily-instantiated singleton. Safe to call from any thread; the
        /// underlying <see cref="GameObject"/> creation is deferred to the
        /// next call from the main thread (Unity APIs are main-thread-only).
        /// </summary>
        public static MediaHost Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new MediaHost();
                return s_instance;
            }
        }

        // ── Hosted GameObject (lazy) ──────────────────────────────────────
        private GameObject _hostGo;
        private GameObject HostGo
        {
            get
            {
                if (_hostGo == null)
                {
                    _hostGo = new GameObject("__ReactiveUITK_MediaHost")
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                    if (Application.isPlaying)
                        UnityEngine.Object.DontDestroyOnLoad(_hostGo);
                }
                return _hostGo;
            }
        }

        // ── VideoPlayer pool ──────────────────────────────────────────────
        private readonly Stack<VideoPlayer> _videoPool = new Stack<VideoPlayer>();

        /// <summary>
        /// Rent a <see cref="VideoPlayer"/> with default neutral settings
        /// (<c>playOnAwake = false</c>, <c>waitForFirstFrame = true</c>,
        /// <c>renderMode = APIOnly</c>). Caller is responsible for
        /// configuring <c>clip</c>/<c>url</c>/<c>targetTexture</c>/etc. and
        /// for calling <see cref="ReturnVideoPlayer"/> on cleanup.
        /// </summary>
        public VideoPlayer RentVideoPlayer()
        {
            VideoPlayer vp;
            if (_videoPool.Count > 0)
            {
                vp = _videoPool.Pop();
                if (vp == null) // GameObject destroyed externally — recurse to get a fresh one.
                    return RentVideoPlayer();
                vp.enabled = true;
            }
            else
            {
                var go = new GameObject("__ReactiveUITK_VideoPeer")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                go.transform.SetParent(HostGo.transform, worldPositionStays: false);
                vp = go.AddComponent<VideoPlayer>();
            }

            vp.playOnAwake = false;
            vp.waitForFirstFrame = true;
            // Default neutral render mode. Callers that want a RenderTexture
            // sink MUST set `vp.targetTexture` *and* flip `vp.renderMode`
            // back to `VideoRenderMode.RenderTexture` — `APIOnly` does not
            // write to `targetTexture` (per Unity docs: "Don't draw the
            // video content anywhere, but still make it available via the
            // VideoPlayer's texture property"). VideoFunc handles this
            // explicitly when it rents a player.
            vp.renderMode = VideoRenderMode.APIOnly;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;
            vp.isLooping = false;
            vp.playbackSpeed = 1f;
            return vp;
        }

        /// <summary>
        /// Return a previously-rented <see cref="VideoPlayer"/> to the pool.
        /// Detaches event handlers, stops playback, clears the
        /// <c>targetTexture</c> reference, and disables the component until
        /// the next rent.
        /// </summary>
        public void ReturnVideoPlayer(VideoPlayer vp)
        {
            if (vp == null)
                return;
            try
            {
                vp.Stop();
                vp.clip = null;
                vp.url = null;
                vp.targetTexture = null;
                vp.audioOutputMode = VideoAudioOutputMode.Direct;
                vp.SetTargetAudioSource(0, null);
                // NOTE: VideoPlayer event delegates (prepareCompleted,
                // loopPointReached, errorReceived, seekCompleted, frameReady,
                // frameDropped, started) cannot be cleared from outside the
                // declaring type — C# only allows `+=` / `-=` on events.
                // Callers MUST `-=` every handler they `+=` before returning
                // the player. VideoFunc honours this contract in its effect
                // cleanup; any future renter must do the same.
                vp.enabled = false;
            }
            catch
            {
                // VideoPlayer already destroyed — drop on the floor.
                return;
            }
            _videoPool.Push(vp);
        }

        // ── AudioSource pool ──────────────────────────────────────────────
        private readonly Stack<AudioSource> _audioPool = new Stack<AudioSource>();

        /// <summary>
        /// Rent an <see cref="AudioSource"/> with neutral defaults
        /// (<c>playOnAwake = false</c>, <c>spatialBlend = 0</c> for 2D UI sounds).
        /// </summary>
        public AudioSource RentAudioSource()
        {
            AudioSource src;
            if (_audioPool.Count > 0)
            {
                src = _audioPool.Pop();
                if (src == null)
                    return RentAudioSource();
                src.enabled = true;
            }
            else
            {
                var go = new GameObject("__ReactiveUITK_AudioPeer")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                go.transform.SetParent(HostGo.transform, worldPositionStays: false);
                src = go.AddComponent<AudioSource>();
            }

            src.playOnAwake = false;
            src.loop = false;
            src.volume = 1f;
            src.pitch = 1f;
            src.mute = false;
            src.priority = 128;
            src.spatialBlend = 0f;
            src.panStereo = 0f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.minDistance = 1f;
            src.maxDistance = 500f;
            src.outputAudioMixerGroup = null;
            src.clip = null;
            return src;
        }

        /// <summary>
        /// Return a previously-rented <see cref="AudioSource"/> to the pool.
        /// </summary>
        public void ReturnAudioSource(AudioSource src)
        {
            if (src == null)
                return;
            try
            {
                src.Stop();
                src.clip = null;
                src.outputAudioMixerGroup = null;
                src.transform.localPosition = Vector3.zero;
                src.transform.SetParent(HostGo.transform, worldPositionStays: false);
                src.enabled = false;
            }
            catch
            {
                return;
            }
            _audioPool.Push(src);
        }

        // ── Shared SFX source (single, never returned) ────────────────────
        private AudioSource _sfxSource;

        /// <summary>
        /// Single shared <see cref="AudioSource"/> dedicated to fire-and-forget
        /// <see cref="AudioSource.PlayOneShot(AudioClip, float)"/> calls from
        /// the <c>useSfx</c> hook. Survives across mounts; never pooled.
        /// </summary>
        public AudioSource SfxSource
        {
            get
            {
                if (_sfxSource == null)
                {
                    var go = new GameObject("__ReactiveUITK_Sfx")
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                    go.transform.SetParent(HostGo.transform, worldPositionStays: false);
                    _sfxSource = go.AddComponent<AudioSource>();
                    _sfxSource.playOnAwake = false;
                    _sfxSource.loop = false;
                    _sfxSource.spatialBlend = 0f;
                    _sfxSource.priority = 128;
                }
                return _sfxSource;
            }
        }

        // ── RenderTexture pool ────────────────────────────────────────────
        // Bucketed by (width, height, format, depth). Conservative sizing —
        // a resized <Video> rounds dimensions to the nearest 16 px to limit
        // bucket explosion under live drag-resize.

        private readonly struct RTKey : IEquatable<RTKey>
        {
            public readonly int W;
            public readonly int H;
            public readonly RenderTextureFormat Fmt;
            public readonly int Depth;

            public RTKey(int w, int h, RenderTextureFormat fmt, int depth)
            {
                W = w;
                H = h;
                Fmt = fmt;
                Depth = depth;
            }

            public bool Equals(RTKey other) =>
                W == other.W && H == other.H && Fmt == other.Fmt && Depth == other.Depth;

            public override bool Equals(object obj) => obj is RTKey k && Equals(k);

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = 17;
                    h = h * 31 + W;
                    h = h * 31 + H;
                    h = h * 31 + (int)Fmt;
                    h = h * 31 + Depth;
                    return h;
                }
            }
        }

        private readonly Dictionary<RTKey, Stack<RenderTexture>> _rtPool =
            new Dictionary<RTKey, Stack<RenderTexture>>();

        /// <summary>
        /// Rent a <see cref="RenderTexture"/> sized at least
        /// <paramref name="width"/> × <paramref name="height"/>. Dimensions
        /// are clamped to ≥16 px and rounded up to a multiple of 16 to limit
        /// bucket explosion during live resize.
        /// </summary>
        public RenderTexture RentRenderTexture(
            int width,
            int height,
            RenderTextureFormat format = RenderTextureFormat.ARGB32,
            int depth = 0
        )
        {
            int w = RoundUp(Mathf.Max(16, width));
            int h = RoundUp(Mathf.Max(16, height));
            var key = new RTKey(w, h, format, depth);

            if (_rtPool.TryGetValue(key, out var stack) && stack.Count > 0)
            {
                var rt = stack.Pop();
                if (rt != null && rt.IsCreated())
                    return rt;
            }

            var fresh = new RenderTexture(w, h, depth, format)
            {
                name = $"__ReactiveUITK_RT_{w}x{h}",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            fresh.Create();
            return fresh;

            static int RoundUp(int v) => ((v + 15) / 16) * 16;
        }

        /// <summary>
        /// Return a previously-rented <see cref="RenderTexture"/> to the
        /// pool. Safe to pass <c>null</c>.
        /// </summary>
        public void ReturnRenderTexture(RenderTexture rt)
        {
            if (rt == null)
                return;
            var key = new RTKey(rt.width, rt.height, rt.format, rt.depth);
            if (!_rtPool.TryGetValue(key, out var stack))
            {
                stack = new Stack<RenderTexture>();
                _rtPool[key] = stack;
            }
            stack.Push(rt);
        }

        // ── Reset (Editor play-mode + tests) ──────────────────────────────

        /// <summary>
        /// Destroys all pooled peers and resets the singleton. Called on
        /// play-mode enter to mirror <c>UitkxAssetRegistry</c>'s cache-reset
        /// semantics; also exposed for unit tests.
        /// </summary>
        internal static void ResetForTests()
        {
            if (s_instance == null)
                return;
            try
            {
                if (s_instance._ticker != null)
                    s_instance._ticker.Tick = null;
                foreach (var vp in s_instance._videoPool)
                    if (vp != null)
                        UnityEngine.Object.Destroy(vp.gameObject);
                foreach (var src in s_instance._audioPool)
                    if (src != null)
                        UnityEngine.Object.Destroy(src.gameObject);
                foreach (var stack in s_instance._rtPool.Values)
                foreach (var rt in stack)
                    if (rt != null)
                        UnityEngine.Object.Destroy(rt);
                if (s_instance._sfxSource != null)
                    UnityEngine.Object.Destroy(s_instance._sfxSource.gameObject);
                if (s_instance._hostGo != null)
                    UnityEngine.Object.Destroy(s_instance._hostGo);
            }
            catch
            { /* ignore */
            }
            s_instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForPlayMode()
        {
            ResetForTests();
        }

        // ── Per-frame ticker (for OnEnded / OnTimeUpdate etc.) ────────────

        private MediaHostTicker _ticker;

        /// <summary>
        /// Subscribe a per-frame callback for media-component polling
        /// (AudioSource has no native end-of-clip event; VideoPlayer's
        /// callbacks all dispatch from the main loop too). Returns an
        /// unsubscribe action that the caller MUST invoke on cleanup.
        /// </summary>
        public Action SubscribeTick(Action onTick)
        {
            if (onTick == null)
                return static () => { };
            if (_ticker == null)
            {
                _ticker = HostGo.AddComponent<MediaHostTicker>();
            }
            _ticker.Tick += onTick;
            return () =>
            {
                if (_ticker != null)
                    _ticker.Tick -= onTick;
            };
        }

        /// <summary>
        /// MonoBehaviour anchored on the MediaHost GO. Fires <see cref="Tick"/>
        /// once per Update (main-thread). Used by Audio/Video render functions
        /// to poll <c>isPlaying</c>/<c>time</c> for OnEnded/OnTimeUpdate
        /// callbacks since AudioSource has no equivalent of VideoPlayer's
        /// <c>loopPointReached</c>.
        /// </summary>
        internal sealed class MediaHostTicker : MonoBehaviour
        {
            internal Action Tick;

            private void Update()
            {
                Tick?.Invoke();
            }
        }
    }
}
