// Code samples for the Imports & Exports docs page (import/export grammar, leg 3).

export const EXAMPLE_GRAMMAR = `import { StatusChip } from "./components/StatusChip"
import { useCounter, CounterStyles } from "~/Shared/Counter.hooks"

export component Screen(int Start = 0) {
    var (count, setCount) = useCounter(Start);
    return (
        <Box>
            <StatusChip label={count.ToString()} />
        </Box>
    );
}

export hook useCounter(int start) { /* ... */ }
export module CounterStyles { /* ... */ }

component LocalHelper { /* ... */ }   // no export = file-private (strict-invisible)`

export const EXAMPLE_SPECIFIERS = `import { Card } from "./Card"              // same folder
import { Theme } from "../theme/Theme"     // parent folder
import { Icons } from "~/Shared/Icons"     // '~/' = UI source root (default Assets/)

// Extensionless — '.uitkx' is implied. Engine-native forms are NOT valid
// import specifiers (they never resolve → UITKX2300):
//   import { X } from "Assets/UI/X"       // ✗
//   import { X } from "Packages/p/X"      // ✗`

export const EXAMPLE_MIXED = `// A file is a SEQUENCE of declarations, any kind, any order:
import { Palette } from "./Palette"

export component Header { return (<Box style={Palette.Bar} />); }
export component Footer { return (<Box style={Palette.Bar} />); }
export hook useNav() { /* ... */ }
module LocalConst { public const int Gap = 8; }   // file-private`

export const EXAMPLE_STRICT = `// Screen references StatusChip but never imports it:
export component Screen {
    return (<StatusChip />);   // UITKX2305: 'StatusChip' is defined in
                               // StatusChip.uitkx but not imported —
                               // add: import { StatusChip } from "./StatusChip"
}`

export const EXAMPLE_CODEMOD = `# Dry run first — reports which files WOULD change, exits non-zero if any would.
dotnet run --project SourceGenerator~/Tools/UitkxMigrateImports -- Assets --check

# Then migrate in place (rewrites the .uitkx files under the given directory):
dotnet run --project SourceGenerator~/Tools/UitkxMigrateImports -- Assets

# Re-running --check afterwards should report 0 changes (the migration is idempotent).`

export const EXAMPLE_NAMESPACE = `// No @namespace? The default is derived from the file's path relative to the
// owning .asmdef:  Samples/Components/Board/Board.uitkx  (asmdef at Samples/)
//   → namespace ReactiveUITK.Uitkx.Components.Board
//
// @namespace is now an OPTIONAL interop override (for hand-written C# that
// references the generated type by a fixed namespace):
@namespace MyGame.UI
export component Board { /* ... */ }`
