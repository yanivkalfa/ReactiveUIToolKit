using System;
using System.IO;
using System.Collections.Immutable;
using ReactiveUITK.Language.IntelliSense;
using ReactiveUITK.Language.Parser;

AstCursorContext.DebugLog = msg => Console.WriteLine(msg);

var filePath = @"c:\Yanivs\GameDev\UnityComponents\Assets\ReactiveUIToolKit\Samples\UITKX\Components\UitkxCounterFunc\UitkxCounterFunc.uitkx";
var text = File.ReadAllText(filePath);
var diags = new System.Collections.Generic.List<ReactiveUITK.Language.ParseDiagnostic>();
var directives = DirectiveParser.Parse(text, filePath, diags);
var nodes = UitkxParser.Parse(text, filePath, directives, diags);
var parseResult = new ParseResult(directives, nodes, ImmutableArray.CreateRange(diags));

Console.WriteLine($"IsFunctionStyle: {directives.IsFunctionStyle}");
Console.WriteLine($"FunctionSetupStartLine: {directives.FunctionSetupStartLine}");
Console.WriteLine($"ReturnMarkups count: {nodes.Length}");

// Check all CodeBlockNodes for ReturnMarkups
void WalkForCodeBlocks(ImmutableArray<ReactiveUITK.Language.Nodes.AstNode> ns, string path) {
    foreach (var n in ns) {
        if (n is ReactiveUITK.Language.Nodes.CodeBlockNode cb) {
            Console.WriteLine($"  CodeBlock at line {cb.SourceLine}: {cb.ReturnMarkups.Length} ReturnMarkups");
            foreach (var rm in cb.ReturnMarkups) {
                Console.WriteLine($"    RM: {rm.Element.GetType().Name} tag={((ReactiveUITK.Language.Nodes.ElementNode)rm.Element).TagName} line={rm.Element.SourceLine}");
                var el = (ReactiveUITK.Language.Nodes.ElementNode)rm.Element;
                foreach (var attr in el.Attributes)
                    Console.WriteLine($"      attr: {attr.Name} line={attr.SourceLine}");
                // recurse children
                foreach (var child in el.Children) {
                    if (child is ReactiveUITK.Language.Nodes.ElementNode cel) {
                        Console.WriteLine($"      child: {cel.TagName} line={cel.SourceLine}");
                        foreach (var ca in cel.Attributes)
                            Console.WriteLine($"        attr: {ca.Name} line={ca.SourceLine}");
                    }
                }
            }
        }
    }
}
WalkForCodeBlocks(nodes, "root");

// Now test Find() at specific positions
int[] testLines = { 373, 374, 375, 358, 359 }; // 1-based
foreach (var line in testLines) {
    // Try col=8 (typical indent + a few chars)
    var ctx = AstCursorContext.Find(parseResult, text, line, 8);
    Console.WriteLine($"Find(line={line}, col=8) => Kind={ctx.Kind} Tag={ctx.TagName ?? "(null)"} Attr={ctx.AttributeName ?? "(null)"} Prefix='{ctx.Prefix}'");
}
