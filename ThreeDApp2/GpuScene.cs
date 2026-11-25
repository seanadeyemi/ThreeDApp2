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
        public float[] Positions = new float[0];   // x, y, z per vertex, two vertices per segment
        public byte[] Colors = new byte[0];        // r, g, b, a per vertex
        public int VertexCount;
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
        public float[] TriPositions = new float[0];
        public byte[] TriColors = new byte[0];
        public int TriVertexCount;
        public bool TrianglesTranslucent;
        public int GeometryStamp;                      // Changes whenever TriPositions holds different data
        public List<GpuLines> Lines = new List<GpuLines>();      // Drawn after the faces, depth-tested
        public int ShapeCount;
    }

    public static class GpuScene
    {
        /// <summary>
        /// Shapes turned into triangles and line segments. Rebuilt only when the shapes change; each
        /// frame just recolours it (shading depends on the view), which keeps orbiting fast.
        /// </summary>
        class Geometry
        {
            public List<MyPolygon> Layers;
            public int Version, Stamp;
            public float[] TriPos; public int[] TriLayer; public int TriVerts;        // TriLayer: per vertex
            public float[] EdgePos; public int[] EdgeLayer; public int EdgeVerts;     // Outlines of faces
            public float[] LinePos; public int[] LineLayer; public int LineVerts;     // Open polylines
            public float[] Normal;      // Per layer, model space, unit (0 if none)
            public float[] Center;      // Per layer centroid, for translucent ordering
            public bool[] Closed;
            public int Visible;
            public double[] Box = new double[6];   // Model-space bounds of everything
            public byte[] TriCol, EdgeCol, LineCol;
        }

        static Geometry sGeometry;
        static int sStamp;

        static Geometry GetGeometry(Scene scene)
        {
            Geometry g = sGeometry;
            if (g != null && ReferenceEquals(g.Layers, scene.Layers) && g.Version == scene.GeometryVersion && g.Normal.Length == scene.Layers.Count * 3)
                return g;
            g = new Geometry { Layers = scene.Layers, Version = scene.GeometryVersion, Stamp = ++sStamp };
            int n = scene.Layers.Count;
            List<float> tri = new List<float>(n * 18), edge = new List<float>(n * 24), line = new List<float>();
            List<int> triL = new List<int>(n * 6), edgeL = new List<int>(n * 8), lineL = new List<int>();
            g.Normal = new float[n * 3];
            g.Center = new float[n * 3];
            g.Closed = new bool[n];
            double[] box = { double.MaxValue, double.MaxValue, double.MaxValue, double.MinValue, double.MinValue, double.MinValue };
            for (int i = 0; i < n; i++)
            {
                MyPolygon layer = scene.Layers[i];
                if (layer.Hidden) continue;
                int count = Scene.VertexCount(layer);
                if (count < 2) continue;
                List<Point3D> pts = layer.RawPoints;
                Point3D off = layer.mPosition;
                if (off.X != 0 || off.Y != 0 || off.Z != 0) pts = Sketch.Vertices(layer);
                else if (pts.Count != count) pts = pts.GetRange(0, count);
                g.Visible++;
                bool closed = layer.IsClosedFace && pts.Count >= 3;
                g.Closed[i] = closed;
                double cx = 0, cy = 0, cz = 0;
                foreach (Point3D p in pts)
                {
                    cx += p.X; cy += p.Y; cz += p.Z;
                    box[0] = Math.Min(box[0], p.X); box[1] = Math.Min(box[1], p.Y); box[2] = Math.Min(box[2], p.Z);
                    box[3] = Math.Max(box[3], p.X); box[4] = Math.Max(box[4], p.Y); box[5] = Math.Max(box[5], p.Z);
                }
                g.Center[i * 3] = (float)(cx / pts.Count); g.Center[i * 3 + 1] = (float)(cy / pts.Count); g.Center[i * 3 + 2] = (float)(cz / pts.Count);
                if (closed)
                {
                    double nx = 0, ny = 0, nz = 0;   // Newell's method
                    for (int k = 0; k < pts.Count; k++)
                    {
                        Point3D a = pts[k], b = pts[(k + 1) % pts.Count];
                        nx += (a.Y - b.Y) * (a.Z + b.Z); ny += (a.Z - b.Z) * (a.X + b.X); nz += (a.X - b.X) * (a.Y + b.Y);
                    }
                    double nl = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    if (nl > 1e-12) { g.Normal[i * 3] = (float)(nx / nl); g.Normal[i * 3 + 1] = (float)(ny / nl); g.Normal[i * 3 + 2] = (float)(nz / nl); }
                    if (pts.Count == 3) { Add(tri, pts[0]); Add(tri, pts[1]); Add(tri, pts[2]); triL.Add(i); triL.Add(i); triL.Add(i); }
                    else if (pts.Count == 4 && IsConvexQuad(pts))
                    {
                        Add(tri, pts[0]); Add(tri, pts[1]); Add(tri, pts[2]); Add(tri, pts[0]); Add(tri, pts[2]); Add(tri, pts[3]);
                        for (int k = 0; k < 6; k++) triL.Add(i);
                    }
                    else
                        foreach (int[] t in ModelExchange.Triangulate(pts))
                            for (int k = 0; k < 3; k++) { Add(tri, pts[t[k]]); triL.Add(i); }
                }
                List<float> dst = closed ? edge : line;
                List<int> dstL = closed ? edgeL : lineL;
                int segs = closed ? pts.Count : pts.Count - 1;
                for (int k = 0; k < segs; k++) { Add(dst, pts[k]); Add(dst, pts[(k + 1) % pts.Count]); dstL.Add(i); dstL.Add(i); }
            }
            g.Box = box;
            g.TriPos = tri.ToArray(); g.TriLayer = triL.ToArray(); g.TriVerts = triL.Count;
            g.EdgePos = edge.ToArray(); g.EdgeLayer = edgeL.ToArray(); g.EdgeVerts = edgeL.Count;
            g.LinePos = line.ToArray(); g.LineLayer = lineL.ToArray(); g.LineVerts = lineL.Count;
            g.TriCol = new byte[g.TriVerts * 4]; g.EdgeCol = new byte[g.EdgeVerts * 4]; g.LineCol = new byte[g.LineVerts * 4];
            sGeometry = g;
            return g;
        }

        static bool IsConvexQuad(List<Point3D> q)
        {
            Point3D n = null;
            for (int i = 0; i < 4; i++)
            {
                Point3D c = Point3D.Cross(q[(i + 1) % 4] - q[i], q[(i + 2) % 4] - q[(i + 1) % 4]);
                if (n == null) n = c;
                else if (Point3D.Dot(n, c) <= 0) return false;
            }
            return true;
        }

        public static GpuFrame Build(Scene scene, int width, int height, RenderStyle style)
        {
            Geometry g = GetGeometry(scene);
            GpuFrame f = new GpuFrame { Width = Math.Max(1, width), Height = Math.Max(1, height), TrianglesTranslucent = style.Translucent && !style.Wireframe };
            f.ShapeCount = g.Visible;
            f.GeometryStamp = g.Stamp;
            int n = scene.Layers.Count;

            // Per-layer colours for this view (shading follows the view, like the standard renderer).
            Matrix3D o = scene.Orientation;
            double lx = Scene.LightDir[0], ly = Scene.LightDir[1], lz = Scene.LightDir[2];
            int[] fill = new int[n], outline = new int[n], lineCol = new int[n];
            for (int i = 0; i < n; i++)
            {
                if (scene.Layers[i].Hidden) continue;
                Color c = style.ColorOf(i);
                double nx = g.Normal[i * 3], ny = g.Normal[i * 3 + 1], nz = g.Normal[i * 3 + 2];
                double wx = o[0, 0] * nx + o[0, 1] * ny + o[0, 2] * nz, wy = o[1, 0] * nx + o[1, 1] * ny + o[1, 2] * nz, wz = o[2, 0] * nx + o[2, 1] * ny + o[2, 2] * nz;
                double len = Math.Sqrt(wx * wx + wy * wy + wz * wz);
                float shade = len < 1e-9 ? 1f : (float)(0.35 + 0.65 * Math.Abs((wx * lx + wy * ly + wz * lz) / len));
                fill[i] = Darken(c, shade, style.Translucent ? 150 : 255).ToArgb();
                outline[i] = (style.Wireframe ? c : Darken(c, 0.45f, 255)).ToArgb();
                lineCol[i] = Darken(c, 0.8f, 255).ToArgb();
            }

            if (!style.Wireframe)
            {
                if (f.TrianglesTranslucent)
                {
                    // Translucent faces must blend back to front: order whole faces by depth.
                    int[] order = SortedByDepth(scene, g);
                    List<float> pos = new List<float>(g.TriPos.Length);
                    List<byte> col = new List<byte>(g.TriCol.Length);
                    List<int>[] trisOf = new List<int>[n];
                    for (int v = 0; v < g.TriVerts; v += 3) { int L = g.TriLayer[v]; (trisOf[L] ?? (trisOf[L] = new List<int>())).Add(v); }
                    foreach (int L in order)
                    {
                        if (trisOf[L] == null) continue;
                        foreach (int v in trisOf[L])
                            for (int k = 0; k < 3; k++)
                            {
                                int q = (v + k) * 3;
                                pos.Add(g.TriPos[q]); pos.Add(g.TriPos[q + 1]); pos.Add(g.TriPos[q + 2]);
                                PutColor(col, fill[L]);
                            }
                    }
                    f.TriPositions = pos.ToArray();
                    f.TriColors = col.ToArray();
                    f.TriVertexCount = pos.Count / 3;
                    f.GeometryStamp = -1;   // Not the cached layout
                }
                else
                {
                    Paint(g.TriCol, g.TriLayer, g.TriVerts, fill);
                    f.TriPositions = g.TriPos;
                    f.TriColors = g.TriCol;
                    f.TriVertexCount = g.TriVerts;
                }
            }
            Paint(g.EdgeCol, g.EdgeLayer, g.EdgeVerts, outline);
            Paint(g.LineCol, g.LineLayer, g.LineVerts, lineCol);
            f.Lines.Add(new GpuLines { Width = 1.5f, Positions = g.EdgePos, Colors = g.EdgeCol, VertexCount = g.EdgeVerts });
            f.Lines.Add(new GpuLines { Width = 2.5f, Positions = g.LinePos, Colors = g.LineCol, VertexCount = g.LineVerts });

            // Selection and hover outlines: few shapes, built fresh.
            List<float> hp = new List<float>();
            List<byte> hc = new List<byte>();
            HashSet<int> selected = new HashSet<int>(style.Selected);
            foreach (int i in selected) Highlight(scene, g, i, Color.FromArgb(255, 140, 0), hp, hc);
            if (style.Hover >= 0 && !selected.Contains(style.Hover)) Highlight(scene, g, style.Hover, Color.FromArgb(160, 30, 144, 255), hp, hc);
            f.Lines.Add(new GpuLines { Width = 3f, Positions = hp.ToArray(), Colors = hc.ToArray(), VertexCount = hp.Count / 3 });

            // Depth range from the model's bounding box, so the depth buffer keeps its precision.
            double minDz = double.MaxValue, maxDz = 0;
            if (g.Box[0] <= g.Box[3])
                for (int c = 0; c < 8; c++)
                {
                    Point3D corner = new Point3D(g.Box[(c & 1) == 0 ? 0 : 3], g.Box[(c & 2) == 0 ? 1 : 4], g.Box[(c & 4) == 0 ? 2 : 5]);
                    double dz = scene.ToWorld(corner).Z - scene.Camera.Z;
                    minDz = Math.Min(minDz, dz);
                    maxDz = Math.Max(maxDz, dz);
                }
            if (scene.Orthographic)
            {
                // Parallel projection shows things behind the camera point too, so the range can go negative.
                double pad = Math.Max(1, (maxDz - minDz) * 0.02);
                f.Near = minDz == double.MaxValue ? -1 : minDz - pad;
                f.Far = minDz == double.MaxValue ? 1 : maxDz + pad;
            }
            else
            {
                f.Near = Math.Max(0.02, Math.Min(scene.NearPlane, minDz == double.MaxValue ? scene.NearPlane : minDz * 0.5));
                f.Far = Math.Max(f.Near * 10, maxDz * 1.05 + 1);
            }
            BuildMatrices(scene, f);
            return f;
        }

        static int[] SortedByDepth(Scene scene, Geometry g)
        {
            Matrix3D o = scene.Orientation;
            List<KeyValuePair<double, int>> order = new List<KeyValuePair<double, int>>();
            for (int i = 0; i < scene.Layers.Count; i++)
            {
                if (scene.Layers[i].Hidden) continue;
                double z = o[2, 0] * g.Center[i * 3] + o[2, 1] * g.Center[i * 3 + 1] + o[2, 2] * g.Center[i * 3 + 2];
                order.Add(new KeyValuePair<double, int>(z, i));
            }
            order.Sort((a, b) => b.Key != a.Key ? b.Key.CompareTo(a.Key) : a.Value.CompareTo(b.Value));
            int[] r = new int[order.Count];
            for (int i = 0; i < r.Length; i++) r[i] = order[i].Value;
            return r;
        }

        static void Paint(byte[] dst, int[] layerOf, int verts, int[] argb)
        {
            for (int v = 0, o = 0; v < verts; v++, o += 4)
            {
                int c = argb[layerOf[v]];
                dst[o] = (byte)(c >> 16); dst[o + 1] = (byte)(c >> 8); dst[o + 2] = (byte)c; dst[o + 3] = (byte)(c >> 24);
            }
        }

        static void PutColor(List<byte> col, int c)
        {
            col.Add((byte)(c >> 16)); col.Add((byte)(c >> 8)); col.Add((byte)c); col.Add((byte)(c >> 24));
        }

        static void Highlight(Scene scene, Geometry g, int i, Color c, List<float> pos, List<byte> col)
        {
            if (i < 0 || i >= scene.Layers.Count || scene.Layers[i].Hidden) return;
            List<Point3D> pts = Sketch.Vertices(scene.Layers[i]);
            if (pts.Count < 2) return;
            bool closed = g.Closed[i];
            int segs = closed ? pts.Count : pts.Count - 1;
            for (int k = 0; k < segs; k++)
            {
                Add(pos, pts[k]); PutColor(col, c.ToArgb());
                Add(pos, pts[(k + 1) % pts.Count]); PutColor(col, c.ToArgb());
            }
        }

        static void Add(List<float> pos, Point3D p) { pos.Add(p.X); pos.Add(p.Y); pos.Add(p.Z); }

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
            double[] pr = f.Projection;
            Array.Clear(pr, 0, 16);
            if (scene.Orthographic)
            {
                double s = scene.OrthoScale;
                pr[0] = s / (f.Width / 2.0);
                pr[5] = s / (f.Height / 2.0);
                pr[10] = -2 / (far - n);          // Eye z (OpenGL) is -depth, so nearer = smaller depth
                pr[14] = -(far + n) / (far - n);
                pr[15] = 1;
                return;
            }
            double right = f.Width / 2.0 * n / focal, top = f.Height / 2.0 * n / focal;
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

        public static Color Darken(Color c, float factor, int alpha)
        {
            factor = Math.Max(0f, Math.Min(1f, factor));
            return Color.FromArgb(alpha, (int)(c.R * factor), (int)(c.G * factor), (int)(c.B * factor));
        }
    }
}
