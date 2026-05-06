using System.Collections.Generic;
using ReactiveUITK.Core.Media;
using ReactiveUITK.Props.Typed;
using UnityEngine;

namespace ReactiveUITK.Core.Media
{
    /// <summary>
    /// Render function for the <c>&lt;Audio&gt;</c> Func-Component. Renders an
    /// empty <see cref="VirtualNode"/> (Fragment) — <c>&lt;Audio&gt;</c> has
    /// no visual output. All work happens in <see cref="Hooks.UseEffect"/>:
    ///
    /// <para>
    /// Heavy effect (re-runs only when re-init-relevant deps change):
    /// <list type="number">
    ///   <item>Rent <see cref="UnityEngine.AudioSource"/> from <see cref="MediaHost"/>.</item>
    ///   <item>Configure all source fields from <see cref="AudioProps"/>.</item>
    ///   <item>Construct <see cref="AudioController"/> and assign to optional ref.</item>
    ///   <item>If <c>Autoplay</c>, call <see cref="UnityEngine.AudioSource.Play"/>.</item>
    ///   <item>Subscribe a per-frame ticker to detect end-of-clip and fire <c>OnEnded</c>.</item>
    ///   <item>Cleanup: stop, unsubscribe, detach controller, return to pool.</item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// Cheap effect (re-runs whenever Volume/Pitch/Mute change but never
    /// rents/re-creates): pushes the live values onto the still-attached
    /// AudioSource.
    /// </para>
    /// </summary>
    public static class AudioFunc
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            var p = rawProps as AudioProps;

            // Stable per-instance controller ref so the cheap-effect can
            // mutate the live source without re-rendering the heavy effect.
            var ctrlRef = Hooks.UseRef<AudioController>(null);

            // Heavy effect: rent + configure + autoplay + cleanup. Runs on
            // mount, on unmount, and whenever any of the listed deps change
            // (clip swap, loop toggle, mixer-group change, spatial-blend
            // change). Pure mutation of in-place fields like Volume/Pitch
            // does NOT re-run this — see the cheap effect below.
            Hooks.UseEffect(
                () =>
                {
                    if (p == null)
                    {
                        return null;
                    }

                    var host = MediaHost.Instance;
                    var src = host.RentAudioSource();

                    // Apply all settings (including those handled by the
                    // cheap effect — initial values must be correct from
                    // frame 1).
                    ApplyAll(src, p);

                    var ctrl = new AudioController(src);
                    ctrlRef.Current = ctrl;
                    if (p.Controller != null)
                        p.Controller.Current = ctrl;

                    bool started = false;
                    if (p.Autoplay && p.Clip != null)
                    {
                        src.Play();
                        started = true;
                        p.OnStarted?.Invoke();
                    }

                    // Per-frame poll for OnEnded — AudioSource has no
                    // native event. We track playing → not-playing
                    // transitions for non-looping sources.
                    System.Action unsubTick = null;
                    if (p.OnEnded != null && !p.Loop)
                    {
                        bool wasPlaying = started;
                        unsubTick = host.SubscribeTick(() =>
                        {
                            if (src == null)
                                return;
                            bool nowPlaying = src.isPlaying;
                            if (wasPlaying && !nowPlaying)
                            {
                                wasPlaying = false;
                                p.OnEnded?.Invoke();
                            }
                            else if (!wasPlaying && nowPlaying)
                            {
                                wasPlaying = true;
                            }
                        });
                    }

                    return () =>
                    {
                        if (unsubTick != null)
                            unsubTick();
                        try
                        {
                            src.Stop();
                        }
                        catch
                        { /* destroyed */
                        }
                        ctrl.__Detach();
                        if (p.Controller != null && p.Controller.Current == ctrl)
                            p.Controller.Current = null;
                        host.ReturnAudioSource(src);
                    };
                },
                p?.Clip,
                p?.Loop ?? false,
                p?.MixerGroup,
                p?.SpatialBlend ?? 0f,
                p?.WorldPosition ?? (object)null
            );

            // Cheap effect: push live values onto the existing source
            // without renting a new one.
            Hooks.UseEffect(
                () =>
                {
                    if (p == null)
                        return null;
                    var ctrl = ctrlRef.Current;
                    if (ctrl != null && ctrl.IsAttached)
                    {
                        ctrl.Volume = p.Volume;
                        ctrl.Pitch = p.Pitch;
                        ctrl.Mute = p.Mute;
                    }
                    return null;
                },
                p?.Volume ?? 1f,
                p?.Pitch ?? 1f,
                p?.Mute ?? false
            );

            // <Audio> has no visual representation.
            return ReactiveUITK.V.Fragment();
        }

        private static void ApplyAll(AudioSource src, AudioProps p)
        {
            src.clip = p.Clip;
            src.loop = p.Loop;
            src.volume = Mathf.Clamp01(p.Volume);
            src.pitch = p.Pitch;
            src.mute = p.Mute;
            src.priority = Mathf.Clamp(p.Priority, 0, 256);
            src.outputAudioMixerGroup = p.MixerGroup;
            src.spatialBlend = Mathf.Clamp01(p.SpatialBlend);
            src.panStereo = Mathf.Clamp(p.PanStereo, -1f, 1f);
            src.rolloffMode = p.RolloffMode;
            src.minDistance = Mathf.Max(0f, p.MinDistance);
            src.maxDistance = Mathf.Max(src.minDistance + 0.01f, p.MaxDistance);
            if (p.WorldPosition.HasValue)
            {
                src.transform.localPosition = p.WorldPosition.Value;
            }
            else
            {
                src.transform.localPosition = Vector3.zero;
            }
        }
    }
}
