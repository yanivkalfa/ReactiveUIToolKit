using System;
using ReactiveUITK.Core.Media;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace ReactiveUITK.Elements
{
    /// <summary>
    /// Custom <see cref="VisualElement"/> subclass that hosts the
    /// <c>&lt;Video&gt;</c> Pattern-A element. Owns the pooled
    /// <see cref="VideoPlayer"/>, the pooled <see cref="RenderTexture"/>,
    /// and a child <see cref="UnityEngine.UIElements.Image"/> sink that
    /// samples the RT every repaint.
    ///
    /// <para>
    /// Lifecycle:
    /// <list type="bullet">
    /// <item><c>AttachToPanelEvent</c> rents and configures the player +
    ///       RenderTexture; subscribes to <c>prepareCompleted</c>; calls
    ///       <see cref="VideoPlayer.Prepare"/>; on prepared, autoplays if
    ///       requested.</item>
    /// <item><c>DetachFromPanelEvent</c> tears everything down and returns
    ///       peers to the pool.</item>
    /// <item><c>GeometryChangedEvent</c> swaps the RT to a new
    ///       16-px-bucketed size when the element resizes.</item>
    /// </list>
    /// </para>
    /// <para>
    /// JSX overlay children (e.g. play/pause buttons positioned on top of
    /// the video) are added via the framework's normal child-reconciliation
    /// into the inner content container exposed by
    /// <see cref="VideoElementAdapter.ResolveChildHost"/>. That container
    /// is layered ABOVE the sink, so overlays render on top.
    /// </para>
    /// </summary>
    internal sealed class VideoVisualElement : VisualElement
    {
        private readonly Image _sink;
        private readonly VisualElement _childHost;

        private VideoProps _props;
        private VideoPlayer _vp;
        private RenderTexture _rt;
        private VideoController _ctrl;
#if UNITY_EDITOR
        // Editor-only minimal pump that only ticks the engine player loop.
        // Repaint is still driven event-style by VideoPlayer.frameReady.
        // Without this, edit-mode VideoPlayer wakes up lazily and Play()
        // takes ~100-300ms to produce its first frame.
        private UnityEditor.EditorApplication.CallbackFunction _editorPump;
#endif

        private bool _attached;
        private bool _prepared;

        // Wired-once handler refs so we can `-=` on teardown.
        private VideoPlayer.EventHandler _onPrepared;
        private VideoPlayer.EventHandler _onLoopPoint;
        private VideoPlayer.ErrorEventHandler _onError;
        private VideoPlayer.EventHandler _onSeek;
        private VideoPlayer.FrameReadyEventHandler _onFrameReady;

        public VideoVisualElement()
        {
            // Background sink — receives the live RT.
            _sink = new Image
            {
                name = "__uitkx_video_sink",
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore,
            };
            _sink.style.position = Position.Absolute;
            _sink.style.left = 0f;
            _sink.style.top = 0f;
            _sink.style.right = 0f;
            _sink.style.bottom = 0f;
            hierarchy.Add(_sink);

            // Foreground child host — anything in JSX <Video>...</Video>
            // lands here, on top of the sink.
            _childHost = new VisualElement { name = "__uitkx_video_overlay" };
            _childHost.style.position = Position.Absolute;
            _childHost.style.left = 0f;
            _childHost.style.top = 0f;
            _childHost.style.right = 0f;
            _childHost.style.bottom = 0f;
            // Default flex layout so children stack naturally; consumer can
            // override via per-child styles.
            hierarchy.Add(_childHost);

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            RegisterCallback<GeometryChangedEvent>(OnGeometry);
        }

        internal VisualElement ChildHost => _childHost;

        /// <summary>
        /// Adapter entrypoint. Captures the latest props and either rebuilds
        /// the player (when re-init-relevant fields change) or pushes live
        /// values (volume / speed / muted / scale-mode) without renting a
        /// new player.
        /// </summary>
        internal void ApplyProps(VideoProps p)
        {
            if (p == null)
                return;

            bool needRebuild =
                _props == null
                || !ReferenceEquals(_props.Clip, p.Clip)
                || !string.Equals(_props.Url ?? string.Empty, p.Url ?? string.Empty)
                || _props.Loop != p.Loop
                || _props.AudioOutputMode != p.AudioOutputMode
                || !ReferenceEquals(_props.MixerGroup, p.MixerGroup)
                || _props.RenderTextureFormat != p.RenderTextureFormat;

            _props = p;

            // Push the controller ref out to the consumer immediately, even
            // before mount, so binding via prop ref Just Works.
            if (p.Controller != null && _ctrl != null)
                p.Controller.Current = _ctrl;

            _sink.scaleMode = MapScaleMode(p.ScaleMode);

            if (!_attached)
            {
                // Setup happens in OnAttach.
                return;
            }

            if (needRebuild)
            {
                Teardown();
                Setup();
            }
            else
            {
                PushLiveValues();
            }
        }

        private void OnAttach(AttachToPanelEvent _)
        {
            _attached = true;
            if (_props != null)
                Setup();
        }

        private void OnDetach(DetachFromPanelEvent _)
        {
            _attached = false;
#if UNITY_EDITOR
            _cachedOwnerWindow = null;
#endif
            Teardown();
        }

        private void OnGeometry(GeometryChangedEvent evt)
        {
            if (_vp == null || _props == null)
                return;
            int w = Mathf.Max(16, Mathf.RoundToInt(evt.newRect.width));
            int h = Mathf.Max(16, Mathf.RoundToInt(evt.newRect.height));
            int rW = ((w + 15) / 16) * 16;
            int rH = ((h + 15) / 16) * 16;
            if (_rt != null && _rt.width == rW && _rt.height == rH)
                return;
            var host = MediaHost.Instance;
            var newRt = host.RentRenderTexture(rW, rH, _props.RenderTextureFormat);
            _vp.targetTexture = newRt;
            host.ReturnRenderTexture(_rt);
            _rt = newRt;
            _sink.image = _rt;
        }

        private void Setup()
        {
            if (_vp != null || _props == null)
                return;

            var host = MediaHost.Instance;
            _vp = host.RentVideoPlayer();

            // Source.
            if (!string.IsNullOrEmpty(_props.Url))
            {
                _vp.source = VideoSource.Url;
                _vp.url = _props.Url;
            }
            else
            {
                _vp.source = VideoSource.VideoClip;
                _vp.clip = _props.Clip;
            }

            _vp.isLooping = _props.Loop;
            _vp.playbackSpeed = Mathf.Max(0f, _props.PlaybackSpeed);
            _vp.audioOutputMode = _props.AudioOutputMode;
            _vp.SetDirectAudioMute(0, _props.Muted);
            _vp.SetDirectAudioVolume(0, Mathf.Clamp01(_props.Volume));

            // RT sized to current layout (or a sane default until first
            // GeometryChangedEvent fires).
            int initW = 256;
            int initH = 256;
            if (resolvedStyle.width > 0)
                initW = Mathf.Max(16, Mathf.RoundToInt(resolvedStyle.width));
            if (resolvedStyle.height > 0)
                initH = Mathf.Max(16, Mathf.RoundToInt(resolvedStyle.height));
            _rt = host.RentRenderTexture(initW, initH, _props.RenderTextureFormat);
            _vp.targetTexture = _rt;
            // Critical: APIOnly does NOT write to targetTexture.
            _vp.renderMode = VideoRenderMode.RenderTexture;
            _sink.image = _rt;

            _ctrl = new VideoController(_vp);
            if (_props.Controller != null)
                _props.Controller.Current = _ctrl;

            // Event wiring (one-shot prepared semantics).
            _prepared = false;
            _onPrepared = src =>
            {
                if (_prepared)
                    return;
                _prepared = true;
                if (_props?.OnPrepared != null)
                    _props.OnPrepared.Invoke();
                if (_props != null && _props.Autoplay)
                {
                    src.Play();
                }
            };
            _onLoopPoint = _ =>
            {
                if (_props != null && !_props.Loop && _props.OnEnded != null)
                    _props.OnEnded.Invoke();
            };
            _onError = (_, message) =>
            {
                if (_props?.OnError != null)
                    _props.OnError.Invoke(message);
            };
            _onSeek = _ =>
            {
                if (_props?.OnSeekCompleted != null)
                    _props.OnSeekCompleted.Invoke();
            };

            // frameReady fires AFTER VideoPlayer has actually uploaded a
            // decoded frame to its targetTexture. This is the only signal
            // that's both (a) raised on the main thread and (b) guarantees
            // the GPU texture content is fresh. Without enabling
            // sendFrameReadyEvents, the VideoPlayer in edit mode will
            // advance vp.frame / vp.time in its internal counters but the
            // render path that actually blits decoded data into the RT is
            // skipped — which is exactly what the diagnostic "vp.frame
            // increments but the Image stays black" symptom showed.
            // Calling vp.Pause() forces a synchronous blit, which is why
            // the user sees "click pause makes the picture appear".
            //
            _onFrameReady = (_, __) =>
            {
                _sink.MarkDirtyRepaint();
#if UNITY_EDITOR
                var win = ResolveOwningEditorWindow();
                if (win != null)
                    win.Repaint();
#endif
            };
            _vp.sendFrameReadyEvents = true;
            _vp.frameReady += _onFrameReady;
            _vp.prepareCompleted += _onPrepared;
            _vp.loopPointReached += _onLoopPoint;
            _vp.errorReceived += _onError;
            _vp.seekCompleted += _onSeek;

            // Per-frame repaint pump.
            //
            // Repaints are event-driven via `VideoPlayer.frameReady` (wired
            // above): fires when a new frame is uploaded to the RT, then
            // calls `MarkDirtyRepaint()` + `EditorWindow.Repaint()`. Zero
            // idle cost, no per-tick overhead.
            //
            // The one piece that's NOT event-driven is the engine player
            // loop itself. In edit mode it wakes lazily, so without an
            // explicit `QueuePlayerLoopUpdate()` each editor frame the
            // VideoPlayer takes 100-300ms after `Play()` to produce its
            // first frame (the "play-button lag" symptom). Runtime
            // doesn't need this — the player loop ticks on its own.
#if UNITY_EDITOR
            _editorPump = () =>
            {
                if (_vp == null)
                    return;
                // Defensive: if the panel went away without DetachFromPanelEvent
                // firing (rare but happens on hard-close), tear down so the
                // pooled VideoPlayer doesn't keep running.
                if (panel == null)
                {
                    Teardown();
                    return;
                }
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            };
            UnityEditor.EditorApplication.update += _editorPump;
#endif

            if (_props.Clip != null || !string.IsNullOrEmpty(_props.Url))
                _vp.Prepare();
        }

        private void Teardown()
        {
            if (_vp == null)
                return;

#if UNITY_EDITOR
            if (_editorPump != null)
            {
                try
                {
                    UnityEditor.EditorApplication.update -= _editorPump;
                }
                catch { }
                _editorPump = null;
            }
#endif

            try
            {
                if (_onPrepared != null)
                    _vp.prepareCompleted -= _onPrepared;
                if (_onLoopPoint != null)
                    _vp.loopPointReached -= _onLoopPoint;
                if (_onError != null)
                    _vp.errorReceived -= _onError;
                if (_onSeek != null)
                    _vp.seekCompleted -= _onSeek;
                if (_onFrameReady != null)
                {
                    _vp.frameReady -= _onFrameReady;
                    _vp.sendFrameReadyEvents = false;
                }
                _vp.Stop();
            }
            catch
            {
                // VideoPlayer destroyed externally — drop on the floor.
            }

            _onPrepared = null;
            _onLoopPoint = null;
            _onError = null;
            _onFrameReady = null;
            _onSeek = null;

            _ctrl?.__Detach();
            if (_props?.Controller != null && _props.Controller.Current == _ctrl)
                _props.Controller.Current = null;
            _ctrl = null;

            var host = MediaHost.Instance;
            if (_rt != null)
            {
                host.ReturnRenderTexture(_rt);
                _rt = null;
            }
            host.ReturnVideoPlayer(_vp);
            _vp = null;

            _sink.image = null;
            _prepared = false;
        }

        private void PushLiveValues()
        {
            if (_ctrl == null || !_ctrl.IsAttached || _props == null)
                return;
            _ctrl.Volume = _props.Volume;
            _ctrl.PlaybackSpeed = _props.PlaybackSpeed;
            _ctrl.Muted = _props.Muted;
        }

        private static ScaleMode MapScaleMode(string scaleMode)
        {
            switch (scaleMode)
            {
                case "scaleAndCrop":
                    return ScaleMode.ScaleAndCrop;
                case "stretchToFill":
                    return ScaleMode.StretchToFill;
                case "scaleToFit":
                default:
                    return ScaleMode.ScaleToFit;
            }
        }

#if UNITY_EDITOR
        // Cache so we don't enumerate every editor window on every pump tick.
        private UnityEditor.EditorWindow _cachedOwnerWindow;

        /// <summary>
        /// Locate the <see cref="UnityEditor.EditorWindow"/> that owns the
        /// panel this element lives in. Used by the per-frame pump to
        /// trigger an explicit repaint — editor panels do not auto-repaint
        /// just because a child element was marked dirty.
        ///
        /// Cached once found; invalidated automatically on detach.
        /// </summary>
        private UnityEditor.EditorWindow ResolveOwningEditorWindow()
        {
            if (_cachedOwnerWindow != null)
                return _cachedOwnerWindow;
            var thisPanel = panel;
            if (thisPanel == null)
                return null;
            var windows = Resources.FindObjectsOfTypeAll<UnityEditor.EditorWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                var w = windows[i];
                if (w == null)
                    continue;
                var root = w.rootVisualElement;
                if (root != null && ReferenceEquals(root.panel, thisPanel))
                {
                    _cachedOwnerWindow = w;
                    return w;
                }
            }
            return null;
        }
#endif
    }

    /// <summary>
    /// Pattern-A element adapter for <c>&lt;Video&gt;</c>. Owns a
    /// <see cref="VideoVisualElement"/> instance and forwards typed-prop
    /// updates to it. Children are routed to the element's overlay container
    /// via <see cref="ResolveChildHost"/> so they render above the video.
    /// </summary>
    public sealed class VideoElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create() => new VideoVisualElement();

        public override VisualElement ResolveChildHost(VisualElement element) =>
            element is VideoVisualElement vve ? vve.ChildHost : element;

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is VideoVisualElement vve && props is VideoProps vp)
                vve.ApplyProps(vp);
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is VideoVisualElement vve && next is VideoProps vp)
                vve.ApplyProps(vp);
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
