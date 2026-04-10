export const SUSPENSE_CALLBACK = `component DataView {
  var (data, setData) = useState<string[]>(null);

  useEffect(() => {
    // simulate async load
    MyService.LoadDataAsync().ContinueWith(t => setData(t.Result));
    return null;
  });

  var fallback = <Label text="Loading…" />;

  return (
    <Suspense isReady={() => data != null} fallback={fallback}>
      <VisualElement>
        @foreach (var item in data) {
          return (<Label key={item} text={item} />);
        }
      </VisualElement>
    </Suspense>
  );
}`

export const SUSPENSE_TASK = `component AsyncView {
  var loadTask = useMemo(() => MyService.LoadDataAsync());

  return (
    <Suspense pendingTask={loadTask} fallback={<Label text="Loading…" />}>
      <Label text="Data loaded!" />
    </Suspense>
  );
}`
