using ReactiveUITK.Language.Formatter;
var source = System.IO.File.ReadAllText(@"c:\Yanivs\GameDev\UnityComponents\Assets\ReactiveUIToolKit\Samples\UITKX\Components\UitkxCounterFunc\UitkxCounterFunc.uitkx");
try {
    var f = new AstFormatter(FormatterOptions.Default);
    var result = f.Format(source, "UitkxCounterFunc.uitkx");
    System.Console.WriteLine($"SUCCESS: {result.Length} chars, first 200: {result.Substring(0, System.Math.Min(200, result.Length))}");
} catch (System.Exception ex) {
    System.Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
    System.Console.WriteLine(ex.StackTrace?.Substring(0, System.Math.Min(500, ex.StackTrace?.Length ?? 0)));
}
