using ReactiveUITK.SourceGenerator;
using ReactiveUITK.SourceGenerator.Formatter;

var fmt = new AstFormatter(FormatterOptions.Default);
string N(string s) => s.Replace("\r\n","\n").Replace("\r","\n");
string Format(string s) => N(fmt.Format(s));

// D10
var d10 = "component Foo {\n\tvar s = new Style\t{\n\t\t(StyleKeys.Padding, 4f),\n\t};\n\treturn (<Box />);\n}";
Console.WriteLine("=== D10 ===");
Console.WriteLine(Format(d10).Replace("\t","?TAB?"));

// E08
var e08 = "component Foo {\n  void Reset() {\nsetA(0);\nsetB(1);\n  }\n  return (<Box />);\n}";
Console.WriteLine("=== E08 ===");
Console.WriteLine(Format(e08));

// E11
var e11 = "component Foo {\n  useEffect(() => {\ndoSetup();\nreturn null;\n  }, Array.Empty<object>());\n  return (<Box />);\n}";
Console.WriteLine("=== E11 ===");
Console.WriteLine(Format(e11));

// G03
var g03 = "@namespace NS\n@using   static   ReactiveUITK.Props.Typed.StyleKeys\ncomponent Foo { return (<Box />); }";
Console.WriteLine("=== G03 ===");
Console.WriteLine(Format(g03));
