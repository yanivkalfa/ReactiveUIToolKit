import type { FC } from 'react'
import {
  Box,
  Chip,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'

const styles = {
  root: { display: 'flex', flexDirection: 'column', gap: 2 },
  section: { mt: 2 },
} as const

type HookSig = {
  name: string
  signature: string
  returns: string
  category: string
  note?: string
}

const hooks: HookSig[] = [
  // State
  {
    name: 'UseState<T>',
    signature: '(T initial = default)',
    returns: '(T value, StateSetter<T> set)',
    category: 'State',
  },
  {
    name: 'UseReducer<TState, TAction>',
    signature: '(Func<TState, TAction, TState> reducer, TState initialState)',
    returns: '(TState state, Action<TAction> dispatch)',
    category: 'State',
  },
  // Effects
  {
    name: 'UseEffect',
    signature: '(Func<Action> effectFactory, params object[] dependencies)',
    returns: 'void',
    category: 'Effects',
  },
  {
    name: 'UseLayoutEffect',
    signature: '(Func<Action> effectFactory, params object[] dependencies)',
    returns: 'void',
    category: 'Effects',
    note: 'Synchronous — runs before paint',
  },
  // Memoization
  {
    name: 'UseMemo<T>',
    signature: '(Func<T> factory, params object[] dependencies)',
    returns: 'T',
    category: 'Memoization',
  },
  {
    name: 'UseCallback<T>',
    signature: '(Func<T> callback, params object[] dependencies)',
    returns: 'Func<T>',
    category: 'Memoization',
    note: 'Returns Func<T>, not original delegate type',
  },
  {
    name: 'UseDeferredValue<T>',
    signature: '(T value, params object[] dependencies)',
    returns: 'T',
    category: 'Memoization',
  },
  // Refs
  {
    name: 'UseRef<T>',
    signature: '(T initial = default)',
    returns: 'Ref<T>',
    category: 'Refs',
  },
  {
    name: 'UseRef',
    signature: '()',
    returns: 'VisualElement',
    category: 'Refs',
    note: 'Element ref overload',
  },
  {
    name: 'UseImperativeHandle<THandle>',
    signature: '(Func<THandle> factory, params object[] dependencies)',
    returns: 'THandle',
    category: 'Refs',
    note: 'where THandle : class',
  },
  // Context
  {
    name: 'UseContext<T>',
    signature: '(string key)',
    returns: 'T',
    category: 'Context',
    note: 'Does not consume a hook slot',
  },
  {
    name: 'ProvideContext<T>',
    signature: '(string key, T value)',
    returns: 'void',
    category: 'Context',
  },
  {
    name: 'ProvideContext',
    signature: '(string key, object value)',
    returns: 'void',
    category: 'Context',
    note: 'Untyped overload',
  },
  // Stable functions
  {
    name: 'UseStableFunc<T>',
    signature: '(Func<T> function)',
    returns: 'Func<T>',
    category: 'Stable functions',
    note: 'Identity never changes',
  },
  {
    name: 'UseStableAction<T>',
    signature: '(Action<T> action)',
    returns: 'Action<T>',
    category: 'Stable functions',
    note: 'Identity never changes',
  },
  {
    name: 'UseStableCallback',
    signature: '(Action callback)',
    returns: 'Action',
    category: 'Stable functions',
    note: 'Identity never changes',
  },
  // Signals
  {
    name: 'UseSignal<T>',
    signature: '(Signal<T> signal)',
    returns: 'T',
    category: 'Signals',
  },
  {
    name: 'UseSignal<T, TSlice>',
    signature: '(Signal<T> signal, Func<T, TSlice> selector, IEqualityComparer<TSlice>? comparer = null)',
    returns: 'TSlice',
    category: 'Signals',
  },
  {
    name: 'UseSignal<T>',
    signature: '(string key, T initialValue = default)',
    returns: 'T',
    category: 'Signals',
    note: 'Key-based overload',
  },
  {
    name: 'UseSignal<T, TSlice>',
    signature: '(string key, Func<T, TSlice> selector, IEqualityComparer<TSlice>? comparer = null, T initialValue = default)',
    returns: 'TSlice',
    category: 'Signals',
    note: 'Key-based + selector',
  },
  // Animation
  {
    name: 'UseAnimate',
    signature: '(IReadOnlyList<AnimateTrack> tracks, bool autoplay = true, params object[] dependencies)',
    returns: 'void',
    category: 'Animation',
  },
  {
    name: 'UseTweenFloat',
    signature: '(float from, float to, float duration, Ease ease, float delay, Action<float> onUpdate, Action onComplete, params object[] dependencies)',
    returns: 'void',
    category: 'Animation',
  },
  // Utilities
  {
    name: 'UseSafeArea',
    signature: '(float tolerance = 0.5f)',
    returns: 'SafeAreaInsets',
    category: 'Utilities',
  },
]

const categories = [...new Set(hooks.map((h) => h.category))]

const categoryColors: Record<string, 'primary' | 'secondary' | 'success' | 'warning' | 'info' | 'error'> = {
  State: 'primary',
  Effects: 'secondary',
  Memoization: 'success',
  Refs: 'info',
  Context: 'warning',
  'Stable functions': 'error',
  Signals: 'primary',
  Animation: 'secondary',
  Utilities: 'info',
}

export const HooksAPIPage: FC = () => (
  <Box sx={styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Hooks API Reference
    </Typography>
    <Typography variant="body1" paragraph>
      Complete reference of every hook exposed by{' '}
      <code>ReactiveUITK.Hooks</code>. All hooks are called as lowercase
      functions in <code>.uitkx</code> markup (e.g.,{' '}
      <code>{'useState<int>(0)'}</code>). In the C# runtime API they are
      static methods on the <code>Hooks</code> class.
    </Typography>

    {categories.map((cat) => (
      <Box key={cat} sx={styles.section}>
        <Typography variant="h5" component="h2" gutterBottom>
          {cat}
        </Typography>
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell><strong>Hook</strong></TableCell>
                <TableCell><strong>Parameters</strong></TableCell>
                <TableCell><strong>Returns</strong></TableCell>
                <TableCell><strong>Notes</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {hooks
                .filter((h) => h.category === cat)
                .map((h, i) => (
                  <TableRow key={`${h.name}-${i}`}>
                    <TableCell>
                      <code>{h.name}</code>
                    </TableCell>
                    <TableCell><code>{h.signature}</code></TableCell>
                    <TableCell><code>{h.returns}</code></TableCell>
                    <TableCell>
                      {h.note && (
                        <Chip
                          label={h.note}
                          size="small"
                          color={categoryColors[cat] ?? 'default'}
                          variant="outlined"
                        />
                      )}
                    </TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
    ))}

    {/* ── Supporting types ─────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Supporting types
      </Typography>

      <Typography variant="h6" gutterBottom>
        {'StateSetter<T> & StateUpdate<T>'}
      </Typography>
      <CodeBlock
        language="jsx"
        code={`public delegate T StateSetter<T>(StateUpdate<T> update);

public struct StateUpdate<T>
{
    // Implicit conversion from a direct value
    public static implicit operator StateUpdate<T>(T value);

    // Implicit conversion from a functional updater
    public static implicit operator StateUpdate<T>(Func<T, T> updater);
}`}
      />

      <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
        StateSetterExtensions
      </Typography>
      <CodeBlock
        language="jsx"
        code={`public static T Set<T>(this StateSetter<T> setter, StateUpdate<T> update);
public static T Set<T>(this StateSetter<T> setter, Func<T, T> updater);
public static Action<T> ToValueAction<T>(this StateSetter<T> setter);
// ToValueAction is useful for binding: onInput={setName.ToValueAction()}`}
      />

      <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
        {'Ref<T>'}
      </Typography>
      <CodeBlock
        language="jsx"
        code={`public class Ref<T>
{
    public T Current { get; set; }
}`}
      />

      <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
        SafeAreaInsets
      </Typography>
      <CodeBlock
        language="jsx"
        code={`public struct SafeAreaInsets
{
    public float Top, Bottom, Left, Right;
}`}
      />
    </Box>

    {/* ── Configuration ────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Configuration
      </Typography>
      <CodeBlock
        language="jsx"
        code={`public static bool Hooks.EnableHookValidation { get; set; }
public static bool Hooks.EnableStrictDiagnostics { get; set; }
public static bool Hooks.EnableHookAutoRealign { get; set; }`}
      />
    </Box>
  </Box>
)
