using System;
using System.Collections.Generic;
using System.Drawing;

namespace ThreeDApp2
{
    /// <summary>How the shapes should look; mirrors the options of the standard renderer.</summary>
    public class RenderStyle
    {
        public bool Wireframe;
        public bool Translucent;
        public List<int> Selected = new List<int>();   // Layer indices drawn with the orange outline
        public int Hover = -1;                         // Layer index drawn with the blue outline
        public Func<int, Color> ColorOf = i => Color.SteelBlue;
    }

    /// <summary>Lines of one width.</summary>
    public class GpuLines
    {
        public float Width;
        public List<float> Positions = new List<float>();   // x, y, z per vertex, two vertices per segment
        public List<byte> Colors = new List<byte>();        // r, g, b, a per vertex
    }

    /// <summary>
    /// Everything the GPU needs for one frame: triangle and line vertex arrays (model space) and the
    /// two matrices that reproduce the app's own perspective projection exactly, so the GPU picture
    /// lines up pixel for pixel with the overlays (handles, snap markers) drawn on top.
    /// </summary>
    public class GpuFrame
    {
        public int Width, Height;
        public double[] ModelView = new double[16];   // Column-major, as OpenGL expects
        public double[] Projection = new double[16];
        public double Near, Far;
        public List<float> TriPositions = new List<float>();
        public List<byte> TriColors = new List<byte>();
        public bool TrianglesTranslucent;
        public List<GpuLines> Lines = new List<GpuLines>();      // Drawn after the faces, depth-tested
        public int ShapeCount;
    }

    public static class GpuScene
    {
        public static GpuFrame Build(Scene scene, int width, int height, RenderStyle style)
        {
            GpuFrame f = new GpuFrame { Width = Math.Max(1, width), Height = Math.Max(1, height), TrianglesTranslucent = style.Translucent && !style.Wireframe };
            GpuLines outlines = new GpuLines { Width = 1.5f };
            GpuLines polylines = new GpuLines { Width = 2.5f };
            GpuLines highlights = new GpuLines { Width = 3f };
            f.Lines.Add(outlines);
            f.Lines.Add(polylines);
            f.Lines.Add(highlights);

            // Back to front, so translucent faces blend in the same order as the standard renderer.
            List<KeyValuePair<double, int>> order = new List<KeyValuePair<double, int>>();
            double minDz = double.MaxValue, maxDz = 0;
            for (int i = 0; i < scene.Layers.Count; i++)
            {
                MyPolygon layer = scene.Layers[i];
                if (layer.Hidden) continue;
                int count = Scene.VertexCount(layer);
                if (count < 2) continue;
                double depth = 0;
                for (int j = 0; j < count; j++)
                {
                    double dz = scene.ToWorld(layer.Absolute(j)).Z - scene.Camera.Z;
                    depth += dz;
                    minDz = Math.Min(minDz, dz);
                    maxDz = Math.Max(maxDz, dz);
                }
                order.Add(new KeyValuePair<double, int>(depth / count, i));
            }
            order.Sort((a, b) => b.Key != a.Key ? b.Key.CompareTo(a.Key) : a.Value.CompareTo(b.Value));

            foreach (var item in order)
            {
                int i = item.Value;
                MyPolygon layer = scene.Layers[i];
                List<Point3D> pts = Sketch.Vertices(layer);
                bool closed = layer.IsClosedFace && pts.Count >= 3;
                Color baseColor = style.ColorOf(i);
                f.ShapeCount++;

                if (closed)
                {
                    if (!style.Wireframe)
                    {
                        List<Point3D> world = new List<Point3D>();
                        foreach (Point3D p in pts) world.Add(scene.ToWorld(p));
                        Color fill = Darken(baseColor, (float)Scene.Shade(world), style.Translucent ? 150 : 255);
                        foreach (int[] t in ModelExchange.Triangulate(pts))
                            for (int k = 0; k < 3; k++) Add(f.TriPositions, f.TriColors, pts[t[k]], fill);
                    }
                    AddLoop(outlines, pts, true, style.Wireframe ? baseColor : Darken(baseColor, 0.45f, 255));
                }
                else
                    AddLoop(polylines, pts, false, Darken(baseColor, 0.8f, 255));

                if (style.Selected.Contains(i)) AddLoop(highlights, pts, closed, Color.FromArgb(255, 140, 0));
                else if (i == style.Hover) AddLoop(highlights, pts, closed, Color.FromArgb(160, 30, 144, 255));
            }

            // Depth range: hug the model so the depth buffer keeps its precision.
            f.Near = Math.Max(0.02, Math.Min(scene.NearPlane, minDz == double.MaxValue ? scene.NearPlane : minDz * 0.5));
            f.Far = Math.Max(f.Near * 10, maxDz * 1.05 + 1);
            BuildMatrices(scene, f);
            return f;
        }

        /// <summary>
        /// The app projects model point m to world w = O(m - P) + P and then to the screen with
        /// x = f (w - c).x / dz, y = -f (w - c).y / dz around the window centre. Here that becomes a
        /// modelview matrix (eye space, looking down -Z as OpenGL does) and a matching frustum.
        /// </summary>
        public static void BuildMatrices(Scene scene, GpuFrame f)
        {
            Matrix3D o = scene.Orientation;
            Point3D p = scene.Pivot, c = scene.Camera;
            Point3D op = o.Transform(p);
            double[] t = { p.X - op.X - c.X, p.Y - op.Y - c.Y, p.Z - op.Z - c.Z };
            double[] mv = f.ModelView;
            for (int r = 0; r < 3; r++)
            {
                double sign = r == 2 ? -1 : 1;   // Flip Z: the app looks along +Z, OpenGL along -Z
                for (int col = 0; col < 3; col++) mv[col * 4 + r] = sign * o[r, col];
                mv[12 + r] = sign * t[r];
            }
            mv[3] = mv[7] = mv[11] = 0;
            mv[15] = 1;

            double focal = scene.FocalLength;
            double n = f.Near, far = f.Far;
            double right = f.Width / 2.0 * n / focal, top = f.Height / 2.0 * n / focal;
            double[] pr = f.Projection;
            Array.Clear(pr, 0, 16);
            pr[0] = n / right;
            pr[5] = n / top;
            pr[10] = -(far + n) / (far - n);
            pr[11] = -1;
            pr[14] = -2 * far * n / (far - n);
        }

        /// <summary>Where the matrices put a model point, in window pixels (y down), for checking.</summary>
        public static bool ProjectToWindow(GpuFrame f, Point3D m, out PointF pt, out double depth)
        {
            double[] e = new double[4], cl = new double[4];
            double[] v = { m.X, m.Y, m.Z, 1 };
            for (int r = 0; r < 4; r++) { e[r] = 0; for (int k = 0; k < 4; k++) e[r] += f.ModelView[k * 4 + r] * v[k]; }
            for (int r = 0; r < 4; r++) { cl[r] = 0; for (int k = 0; k < 4; k++) cl[r] += f.Projection[k * 4 + r] * e[k]; }
            pt = PointF.Empty;
            depth = 0;
            if (cl[3] <= 1e-9) return false;
            double nx = cl[0] / cl[3], ny = cl[1] / cl[3];
            depth = cl[2] / cl[3];
            pt = new PointF((float)((nx + 1) / 2 * f.Width), (float)((1 - (ny + 1) / 2) * f.Height));
            return true;
        }

        static void Add(List<float> pos, List<byte> col, Point3D p, Color c)
        {
            pos.Add(p.X); pos.Add(p.Y); pos.Add(p.Z);
            col.Add(c.R); col.Add(c.G); col.Add(c.B); col.Add(c.A);
        }

        static void AddLoop(GpuLines lines, List<Point3D> pts, bool closed, Color c)
        {
            int segs = closed ? pts.Count : pts.Count - 1;
            for (int k = 0; k < segs; k++)
            {
                Add(lines.Positions, lines.Colors, pts[k], c);
                Add(lines.Positions, lines.Colors, pts[(k + 1) % pts.Count], c);
            }
        }

        public static Color Darken(Color c, float factor, int alpha)
        {
            factor = Math.Max(0f, Math.Min(1f, factor));
            return Color.FromArgb(alpha, (int)(c.R * factor), (int)(c.G * factor), (int)(c.B * factor));
        }
    }
}
