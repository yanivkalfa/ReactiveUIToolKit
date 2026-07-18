/* ------------------------------------------------------------------ */
/*  Custom Rendering page - .uitkx + companion-C# code examples       */
/* ------------------------------------------------------------------ */

// Plain-C# companion class holding the draw bodies. Keeping the bodies out of
// the markup lets the .uitkx use simple single-expression lambdas (which also
// keeps the formatter idempotent).
export const CUSTOM_RENDERING_HELPERS_EXAMPLE = `using UnityEngine;
using UnityEngine.UIElements;

public static class DrawHelpers
{
    // Painter2D vector drawing: an N-sided polygon outline, centered.
    public static void Polygon(MeshGenerationContext ctx, int sides)
    {
        var r = ctx.visualElement.contentRect;
        var p = ctx.painter2D;
        p.lineWidth = 3f;
        p.strokeColor = Color.cyan;
        float cx = r.width * 0.5f, cy = r.height * 0.5f;
        float radius = Mathf.Min(cx, cy) - 8f;
        int n = Mathf.Max(3, sides);
        p.BeginPath();
        for (int i = 0; i <= n; i++)
        {
            float a = (float)i / n * Mathf.PI * 2f - Mathf.PI * 0.5f;
            var pt = new Vector2(cx + Mathf.Cos(a) * radius, cy + Mathf.Sin(a) * radius);
            if (i == 0) p.MoveTo(pt); else p.LineTo(pt);
        }
        p.Stroke();
    }

    // Raw mesh via ctx.Allocate: a solid tinted quad inset 8px from the edges.
    public static void Quad(MeshGenerationContext ctx, Color tint)
    {
        var r = ctx.visualElement.contentRect;
        var verts = new Vertex[4];
        verts[0].position = new Vector3(8f, r.height - 8f, Vertex.nearZ);
        verts[1].position = new Vector3(8f, 8f, Vertex.nearZ);
        verts[2].position = new Vector3(r.width - 8f, 8f, Vertex.nearZ);
        verts[3].position = new Vector3(r.width - 8f, r.height - 8f, Vertex.nearZ);
        for (int i = 0; i < 4; i++) verts[i].tint = tint;

        var indices = new ushort[] { 0, 1, 2, 2, 3, 0 };
        var mwd = ctx.Allocate(verts.Length, indices.Length);
        mwd.SetAllVertices(verts);
        mwd.SetAllIndices(indices);
    }

    // A stable target that scribbles a fresh random polyline on every repaint,
    // so a redrawKey bump visibly redraws even though the delegate is unchanged.
    public static void Scatter(MeshGenerationContext ctx)
    {
        var r = ctx.visualElement.contentRect;
        var p = ctx.painter2D;
        p.lineWidth = 2f;
        p.strokeColor = new Color(0.3f, 0.9f, 1f, 1f);
        p.BeginPath();
        p.MoveTo(new Vector2(Random.value * r.width, Random.value * r.height));
        for (int i = 0; i < 16; i++)
            p.LineTo(new Vector2(Random.value * r.width, Random.value * r.height));
        p.Stroke();
    }
}`

// Vector drawing with Painter2D, driven by component state.
export const CUSTOM_RENDERING_PAINTER_EXAMPLE = `import "@UnityEngine.UIElements"

export VirtualNode PolygonCanvas() {
  var (sides, setSides) = useState(3);

  var canvas = new Style {
    (StyleKeys.Height, 130f),
    (StyleKeys.BackgroundColor, new UnityEngine.Color(0.12f, 0.12f, 0.14f, 1f)),
  };

  return (
    <VisualElement>
      <VisualElement
        style={canvas}
        onGenerateVisualContent={ctx => DrawHelpers.Polygon(ctx, sides)} />
      <Button text="Add side" onClick={_ => setSides(sides + 1)} />
    </VisualElement>
  );
}`

// Raw mesh via MeshGenerationContext.Allocate.
export const CUSTOM_RENDERING_RAW_MESH_EXAMPLE = `import "@UnityEngine.UIElements"

export VirtualNode QuadCanvas() {
  var (blue, setBlue) = useState(true);

  var canvas = new Style { (StyleKeys.Height, 130f) };

  return (
    <VisualElement>
      <VisualElement
        style={canvas}
        onGenerateVisualContent={ctx =>
          DrawHelpers.Quad(ctx, blue ? Color.blue : new Color(1f, 0.5f, 0.2f))} />
      <Button text="Toggle color" onClick={_ => setBlue(!blue)} />
    </VisualElement>
  );
}`

// Stable callback + redrawKey: repaint on demand without changing the callback.
export const CUSTOM_RENDERING_REDRAW_KEY_EXAMPLE = `import "@UnityEngine.UIElements"

export VirtualNode ScatterCanvas() {
  var (tick, setTick) = useState(0);

  // Stable delegate: its reference never changes between renders, so the
  // element does NOT repaint every render. Bumping redrawKey forces a repaint.
  var draw = useMemo<Action<MeshGenerationContext>>(
    () => DrawHelpers.Scatter,
    Array.Empty<object>()
  );

  var canvas = new Style { (StyleKeys.Height, 130f) };

  return (
    <VisualElement>
      <VisualElement
        style={canvas}
        onGenerateVisualContent={draw}
        redrawKey={tick} />
      <Button text="Shuffle" onClick={_ => setTick(tick + 1)} />
    </VisualElement>
  );
}`

// The underlying Unity field and how the attributes map to it.
export const CUSTOM_RENDERING_SIGNATURE_EXAMPLE = `// onGenerateVisualContent maps directly to Unity's delegate field on
// every VisualElement:
public Action<MeshGenerationContext> generateVisualContent;

// In .uitkx the attribute accepts any Action<MeshGenerationContext>:
onGenerateVisualContent={ctx => /* draw with ctx.painter2D or ctx.Allocate */}

// redrawKey is a plain int. Changing it forces a repaint WITHOUT changing
// the callback reference - pair it with a stable callback (useMemo /
// useStableCallback):
redrawKey={someIntState}`
