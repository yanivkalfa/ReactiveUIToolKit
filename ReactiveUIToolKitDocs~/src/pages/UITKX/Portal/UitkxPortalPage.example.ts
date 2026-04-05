export const PORTAL_BASIC = `component ModalDemo {
  var modalRoot = useContext<VisualElement>(PortalContextKeys.ModalRoot);
  var (showModal, setShowModal) = useState(false);

  return (
    <VisualElement>
      <Button text="Open Modal" onClick={_ => setShowModal(true)} />

      @if (showModal) {
        return (
          <Portal target={modalRoot}>
            <VisualElement style={new Style {
              Position = PosAbsolute,
              Left = 0f, Top = 0f, Right = 0f, Bottom = 0f,
              BackgroundColor = Rgba(0, 0, 0, 0.5f),
              JustifyContent = JustifyCenter,
              AlignItems = AlignCenter,
            }}>
              <Label text="I am a modal!" />
              <Button text="Close" onClick={_ => setShowModal(false)} />
            </VisualElement>
          </Portal>
        );
      }
    </VisualElement>
  );
}`

export const PORTAL_CONTEXT_KEYS = `// In your root component, register the portal target:
provideContext(PortalContextKeys.ModalRoot, myOverlayElement);

// In a child component, resolve it:
var modalRoot = useContext<VisualElement>(PortalContextKeys.ModalRoot);`
