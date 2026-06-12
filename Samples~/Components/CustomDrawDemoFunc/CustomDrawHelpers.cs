using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.UITKXComponents
{
    /// <summary>
    /// Plain-C# draw helpers for the Custom Drawing demo. The drawing bodies live
    /// here (not inline in the <c>.uitkx</c>) so the markup keeps simple
    /// single-expression lambdas and the component formats idempotently. Each
    /// method is a valid <c>Action&lt;MeshGenerationContext&gt;</c>-shaped target
    /// invoked from <c>onGenerateVisualContent</c>.
    /// </summary>
    public static class CustomDrawHelpers
    {
        // Painter2D vector drawing: an N-sided polygon centered in the element.
        public static void DrawPolygon(MeshGenerationContext ctx, int sides)
        {
            Rect r = ctx.visualElement.contentRect;
            if (r.width < 4f || r.height < 4f)
            {
                return;
            }
            Painter2D p = ctx.painter2D;
            p.lineWidth = 3f;
            p.strokeColor = Color.cyan;
            float cx = r.width * 0.5f;
            float cy = r.height * 0.5f;
            float radius = Mathf.Min(cx, cy) - 8f;
            int n = Mathf.Max(3, sides);
            p.BeginPath();
            for (int i = 0; i <= n; i++)
            {
                float a = ((float)i / n) * Mathf.PI * 2f - Mathf.PI * 0.5f;
                var pt = new Vector2(cx + Mathf.Cos(a) * radius, cy + Mathf.Sin(a) * radius);
                if (i == 0)
                {
                    p.MoveTo(pt);
                }
                else
                {
                    p.LineTo(pt);
                }
            }
            p.Stroke();
        }

        // Raw mesh via MeshGenerationContext.Allocate: a tinted quad inset by 8px.
        public static void DrawQuad(MeshGenerationContext ctx, bool blue)
        {
            Rect r = ctx.visualElement.contentRect;
            if (r.width < 4f || r.height < 4f)
            {
                return;
            }
            Color tint = blue ? new Color(0.2f, 0.5f, 1f, 1f) : new Color(1f, 0.5f, 0.2f, 1f);
            var verts = new Vertex[4];
            verts[0].position = new Vector3(8f, r.height - 8f, Vertex.nearZ);
            verts[1].position = new Vector3(8f, 8f, Vertex.nearZ);
            verts[2].position = new Vector3(r.width - 8f, 8f, Vertex.nearZ);
            verts[3].position = new Vector3(r.width - 8f, r.height - 8f, Vertex.nearZ);
            verts[0].tint = tint;
            verts[1].tint = tint;
            verts[2].tint = tint;
            verts[3].tint = tint;
            var indices = new ushort[] { 0, 1, 2, 2, 3, 0 };
            MeshWriteData mwd = ctx.Allocate(verts.Length, indices.Length);
            mwd.SetAllVertices(verts);
            mwd.SetAllIndices(indices);
        }

        // Stable callback target: scribbles a fresh random polyline on every
        // repaint, so a RedrawKey bump visibly redraws even though the delegate
        // reference never changes between renders.
        public static void Scatter(MeshGenerationContext ctx)
        {
            Rect r = ctx.visualElement.contentRect;
            if (r.width < 4f || r.height < 4f)
            {
                return;
            }
            Painter2D p = ctx.painter2D;
            p.lineWidth = 2f;
            p.strokeColor = new Color(0.3f, 0.9f, 1f, 1f);
            p.BeginPath();
            p.MoveTo(new Vector2(Random.value * r.width, Random.value * r.height));
            for (int i = 0; i < 16; i++)
            {
                p.LineTo(new Vector2(Random.value * r.width, Random.value * r.height));
            }
            p.Stroke();
        }
    }
}
