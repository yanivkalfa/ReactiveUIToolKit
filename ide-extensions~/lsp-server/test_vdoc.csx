using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Immutable;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.Roslyn;
using ReactiveUITK.Language.Lowering;

var filePath = @"c:\Yanivs\GameDev\UnityComponents\Assets\ReactiveUIToolKit\Samples\UITKX\Shared\ListViewStatefulDemoFunc.uitkx";
var text = File.ReadAllText(filePath);
var diags = new List<ReactiveUITK.Language.ParseDiagnostic>();
var directives = DirectiveParser.Parse(text, filePath, diags);
var nodes = UitkxParser.Parse(text, filePath, directives, diags);

// Apply canonical lowering (same as LSP does)
if (directives.IsFunctionStyle && !string.IsNullOrEmpty(directives.FunctionSetupCode))
{
    nodes = CanonicalLowering.Lower(directives, nodes, filePath);
}

var parseResult = new ParseResult(directives, nodes, ImmutableArray.CreateRange(diags));

Console.WriteLine($"IsFunctionStyle: {directives.IsFunctionStyle}");
Console.WriteLine($"FunctionSetupCode length: {directives.FunctionSetupCode?.Length}");
Console.WriteLine($"FunctionSetupStartLine: {directives.FunctionSetupStartLine}");

// Check CodeBlockNode ReturnMarkups
foreach (var n in nodes)
{
    if (n is ReactiveUITK.Language.Nodes.CodeBlockNode cb)
    {
        Console.WriteLine($"CodeBlockNode: ReturnMarkups={cb.ReturnMarkups.Length}, Code length={cb.Code?.Length}");
        foreach (var rm in cb.ReturnMarkups)
        {
            Console.WriteLine($"  RM: start={rm.StartOffsetInCodeBlock}, end={rm.EndOffsetInCodeBlock}, tag={rm.Element.TagName}");
        }
    }
}

// Generate virtual document
var gen = new VirtualDocumentGenerator();
var vdoc = gen.Generate(parseResult, text, filePath);

Console.WriteLine("\n=== VIRTUAL DOCUMENT ===\n");
Console.WriteLine(vdoc.Text);
