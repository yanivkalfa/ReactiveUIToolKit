export const AUDIO_BASIC = `@namespace MyApp.Pages
@using UnityEngine
@using ReactiveUITK.Core.Media

component MusicDemo {
  var music = Asset<AudioClip>("Assets/Resources/theme.mp3");
  var ctrl = useRef<AudioController>(null);
  var (playing, setPlaying) = useState(true);
  var (volume, setVolume) = useState(0.6f);

  return (
    <Box style={new Style { (FlexDirection, "row"), (Padding, 12f) }}>
      <Audio
        Clip={music}
        Loop={true}
        Autoplay={playing}
        Volume={volume}
        Controller={ctrl}
      />
      <Button
        text={playing ? "Pause" : "Play"}
        onClick={_ => {
          if (ctrl.Current == null) return;
          if (playing) ctrl.Current.Pause(); else ctrl.Current.Play();
          setPlaying(!playing);
        }}
      />
      <Button
        text="Vol -"
        style={new Style { (MarginLeft, 6f) }}
        onClick={_ => setVolume(Mathf.Clamp01(volume - 0.1f))}
      />
      <Button
        text="Vol +"
        style={new Style { (MarginLeft, 6f) }}
        onClick={_ => setVolume(Mathf.Clamp01(volume + 0.1f))}
      />
    </Box>
  );
}`

export const AUDIO_3D = `// Spatial 3D audio: blend = 1, position in world space.
<Audio
  Clip={engineLoop}
  Loop={true}
  Volume={0.8f}
  SpatialBlend={1f}
  RolloffMode={AudioRolloffMode.Logarithmic}
  MinDistance={2f}
  MaxDistance={40f}
  WorldPosition={new Vector3(0f, 0f, 5f)}
  MixerGroup={sfxGroup}
/>`

export const USE_SFX_BASIC = `@namespace MyApp.Pages
@using UnityEngine

component ClickyButton {
  var click = Asset<AudioClip>("Assets/Resources/click.wav");
  var playSfx = useSfx();

  return (
    <Button
      text="Click me"
      onClick={_ => playSfx(click, 1f)}
    />
  );
}`

export const USE_SFX_MIXER = `// Optional: route one-shots through a specific mixer group.
var playUiSfx = useSfx(uiMixerGroup);

// Same delegate identity across renders — safe in deps arrays.
useEffect(() => {
  playUiSfx(notify, 0.5f);
  return null;
}, playUiSfx);`
