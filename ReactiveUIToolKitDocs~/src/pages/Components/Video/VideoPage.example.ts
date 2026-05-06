export const VIDEO_BASIC = `@namespace MyApp.Pages
@using UnityEngine.Video
@using ReactiveUITK.Core.Media

component VideoDemo {
  var clip = Asset<VideoClip>("Assets/Resources/intro.mp4");
  var ctrl = useRef<VideoController>(null);
  var (playing, setPlaying) = useState(true);

  return (
    <Box style={new Style { (Padding, 12f) }}>
      <Video
        Clip={clip}
        Loop={true}
        Autoplay={playing}
        ScaleMode="scaleToFit"
        Controller={ctrl}
        style={new Style { (Width, 480f), (Height, 270f) }}
      />
      <Button
        text={playing ? "Pause" : "Play"}
        onClick={_ => {
          if (ctrl.Current == null) return;
          if (playing) ctrl.Current.Pause(); else ctrl.Current.Play();
          setPlaying(!playing);
        }}
      />
    </Box>
  );
}`

export const VIDEO_URL = `// Streaming URL wins over Clip when both are set.
<Video
  Url="https://example.com/clip.mp4"
  Loop={false}
  OnPrepared={() => Debug.Log("ready")}
  OnEnded={() => Debug.Log("done")}
  OnError={msg => Debug.LogError(msg)}
  style={new Style { (Width, 640f), (Height, 360f) }}
/>`

export const VIDEO_CONTROLLER = `// Imperative access via Ref<VideoController>
var ctrl = useRef<VideoController>(null);

useEffect(() => {
  if (ctrl.Current == null || !ctrl.Current.IsPrepared) return null;
  ctrl.Current.Seek(2.5d);          // seconds
  ctrl.Current.PlaybackSpeed = 1.5f;
  return null;
}, ctrl.Current?.IsPrepared);`
