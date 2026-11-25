using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace ThreeDApp2
{
    /// <summary>What an import produced.</summary>
    public class ImportResult
    {
        public List<MyPolygon> Shapes = new List<MyPolygon>();
        public int Faces;
        public int Lines;
        public List<string> Warnings = new List<string>();
    }

    /// <summary>
    /// Import and export of common 3D formats: Wavefront OBJ (+ MTL colours), STL (binary and text),
    /// PLY (text and binary) and glTF 2.0 (.gltf and .glb).
    ///
    /// Coordinates: the app is Y-up and left-handed (Z goes into the screen). OBJ, PLY and glTF are
    /// Y-up right-handed, so Z is flipped. STL files are usually Z-up (3D printing and CAD), so Z becomes
    /// the vertical axis. Both mappings are proper rotations, so nothing is mirrored, and export applies
    /// the exact inverse, so a file survives a round trip.
    /// </summary>
    public static class ModelExchange
    {
        public const string ImportFilter =
            "3D models (*.obj;*.stl;*.ply;*.gltf;*.glb)|*.obj;*.stl;*.ply;*.gltf;*.glb|" +
            "Wavefront OBJ (*.obj)|*.obj|STL (*.stl)|*.stl|PLY (*.ply)|*.ply|glTF 2.0 (*.gltf;*.glb)|*.gltf;*.glb|All files (*.*)|*.*";

        public const string ExportFilter =
            "Wavefront OBJ with colours (*.obj)|*.obj|STL binary (*.stl)|*.stl|PLY with colours (*.ply)|*.ply|" +
            "glTF 2.0 (*.gltf)|*.gltf|glTF 2.0 binary (*.glb)|*.glb";

        /// <summary>Colour given to imported faces that have none.</summary>
        public static readonly Color DefaultColor = Color.FromArgb(205, 205, 210);

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        #region Coordinate conversion

        static Point3D FromYUp(double x, double y, double z) { return new Point3D(x, y, -z); }
        static Point3D FromZUp(double x, double y, double z) { return new Point3D(x, z, y); }
        static double[] ToYUp(Point3D p) { return new double[] { p.X, p.Y, -p.Z }; }
        static double[] ToZUp(Point3D p) { return new double[] { p.X, p.Z, p.Y }; }

        #endregion

        #region Shared geometry

        /// <summary>One face or polyline read from a file, already in app coordinates.</summary>
        class RawShape
        {
            public List<Point3D> Pts;
            public bool Closed;
            public Color Color;
            public string Object;
        }

        /// <summary>
        /// Splits a polygon into triangles (ear clipping on its best-fit plane). Returns index triples
        /// in the polygon's own winding. Works for concave outlines; falls back to a fan if the outline
        /// is degenerate.
        /// </summary>
        public static List<int[]> Triangulate(IList<Point3D> pts)
        {
            List<int[]> tris = new List<int[]>();
            int n = pts.Count;
            if (n < 3) return tris;
            if (n == 3) { tris.Add(new[] { 0, 1, 2 }); return tris; }
            Plane3D plane = Plane3D.FromPoints(pts);
            if (plane == null) { for (int i = 1; i + 1 < n; i++) tris.Add(new[] { 0, i, i + 1 }); return tris; }

            double[] u = new double[n], v = new double[n];
            double area = 0;
            for (int i = 0; i < n; i++)
            {
                Point3D d = pts[i] - plane.Origin;
                u[i] = Point3D.Dot(d, plane.U);
                v[i] = Point3D.Dot(d, plane.V);
            }
            for (int i = 0; i < n; i++) { int j = (i + 1) % n; area += u[i] * v[j] - u[j] * v[i]; }
            bool flip = area < 0;

            List<int> idx = new List<int>();
            for (int i = 0; i < n; i++) idx.Add(flip ? n - 1 - i : i);
            double scale = Math.Max(1e-12, Math.Abs(area));
            int guard = 0;
            while (idx.Count > 3 && guard++ < n * n)
            {
                bool found = false;
                for (int k = 0; k < idx.Count; k++)
                {
                    int a = idx[(k + idx.Count - 1) % idx.Count], b = idx[k], c = idx[(k + 1) % idx.Count];
                    double cross = (u[b] - u[a]) * (v[c] - v[b]) - (v[b] - v[a]) * (u[c] - u[b]);
                    if (cross <= 1e-9 * scale) continue;   // Reflex or flat corner
                    bool inside = false;
                    foreach (int p in idx)
                    {
                        if (p == a || p == b || p == c) continue;
                        if (InTriangle(u[p], v[p], u[a], v[a], u[b], v[b], u[c], v[c])) { inside = true; break; }
                    }
                    if (inside) continue;
                    tris.Add(flip ? new[] { c, b, a } : new[] { a, b, c });
                    idx.RemoveAt(k);
                    found = true;
                    break;
                }
                if (!found) break;
            }
            if (idx.Count >= 3)
                for (int i = 1; i + 1 < idx.Count; i++)
                    tris.Add(flip ? new[] { idx[i + 1], idx[i], idx[0] } : new[] { idx[0], idx[i], idx[i + 1] });
            return tris;
        }

        static bool InTriangle(double px, double py, double ax, double ay, double bx, double by, double cx, double cy)
        {
            double d1 = (px - bx) * (ay - by) - (ax - bx) * (py - by);
            double d2 = (px - cx) * (by - cy) - (bx - cx) * (py - cy);
            double d3 = (px - ax) * (cy - ay) - (cx - ax) * (py - ay);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }

        static Point3D Normal(IList<Point3D> pts)
        {
            Plane3D p = Plane3D.FromPoints(pts);
            return p == null ? new Point3D(0, 0, 0) : p.Normal;
        }

        static bool Same(Point3D a, Point3D b, float eps)
        {
            return Math.Abs(a.X - b.X) <= eps && Math.Abs(a.Y - b.Y) <= eps && Math.Abs(a.Z - b.Z) <= eps;
        }

        /// <summary>
        /// Most exporters write each quad as two triangles one after the other. Joins such pairs back into
        /// one quad when they share an edge, lie flat and make a convex shape, so a cube shows 6 faces, not 12.
        /// </summary>
        static List<RawShape> MergeTrianglePairs(List<RawShape> shapes, float eps)
        {
            List<RawShape> result = new List<RawShape>();
            for (int i = 0; i < shapes.Count; i++)
            {
                RawShape a = shapes[i];
                if (i + 1 < shapes.Count && a.Closed && a.Pts.Count == 3)
                {
                    RawShape b = shapes[i + 1];
                    List<Point3D> quad = b.Closed && b.Pts.Count == 3 && b.Color == a.Color && b.Object == a.Object
                        ? TryMergeQuad(a.Pts, b.Pts, eps) : null;
                    if (quad != null)
                    {
                        result.Add(new RawShape { Pts = quad, Closed = true, Color = a.Color, Object = a.Object });
                        i++;
                        continue;
                    }
                }
                result.Add(a);
            }
            return result;
        }

        static List<Point3D> TryMergeQuad(List<Point3D> a, List<Point3D> b, float eps)
        {
            int unique = -1, sharedCount = 0;
            for (int j = 0; j < 3; j++)
            {
                bool shared = false;
                for (int k = 0; k < 3; k++) if (Same(b[j], a[k], eps)) shared = true;
                if (shared) sharedCount++; else unique = j;
            }
            if (sharedCount != 2 || unique < 0) return null;
            Point3D na = Normal(a), nb = Normal(b);
            if (na.Length() < 0.5f || nb.Length() < 0.5f || Math.Abs(Point3D.Dot(na, nb)) < 0.9999f) return null;
            // Insert b's other corner into a's shared edge.
            for (int k = 0; k < 3; k++)
            {
                Point3D p = a[k], q = a[(k + 1) % 3];
                bool pShared = false, qShared = false;
                for (int j = 0; j < 3; j++) { if (Same(b[j], p, eps)) pShared = true; if (Same(b[j], q, eps)) qShared = true; }
                if (!pShared || !qShared) continue;
                List<Point3D> quad = new List<Point3D>();
                // q, then a's third corner, then p; b's other corner closes the gap between p and q.
                for (int m = 0; m < 3; m++) quad.Add(a[(k + 1 + m) % 3]);
                quad.Add(b[unique]);
                return IsConvex(quad) ? quad : null;
            }
            return null;
        }

        static bool IsConvex(List<Point3D> pts)
        {
            Point3D n = Normal(pts);
            if (n.Length() < 0.5f) return false;
            for (int i = 0; i < pts.Count; i++)
            {
                Point3D e1 = pts[(i + 1) % pts.Count] - pts[i];
                Point3D e2 = pts[(i + 2) % pts.Count] - pts[(i + 1) % pts.Count];
                if (Point3D.Dot(Point3D.Cross(e1, e2), n) <= 0) return false;
            }
            return true;
        }

        static float MergeTolerance(List<RawShape> shapes)
        {
            float min = float.MaxValue, max = float.MinValue;
            foreach (RawShape s in shapes)
                foreach (Point3D p in s.Pts)
                {
                    min = Math.Min(min, Math.Min(p.X, Math.Min(p.Y, p.Z)));
                    max = Math.Max(max, Math.Max(p.X, Math.Max(p.Y, p.Z)));
                }
            return max > min ? (max - min) * 1e-6f : 1e-6f;
        }

        /// <summary>Joins line segments written one after another (glTF LINES) back into polylines.</summary>
        static List<RawShape> ChainLines(List<RawShape> shapes, float eps)
        {
            List<RawShape> result = new List<RawShape>();
            foreach (RawShape s in shapes)
            {
                RawShape last = result.Count > 0 ? result[result.Count - 1] : null;
                if (!s.Closed && last != null && !last.Closed && last.Color == s.Color && last.Object == s.Object &&
                    s.Pts.Count >= 2 && Same(last.Pts[last.Pts.Count - 1], s.Pts[0], eps))
                {
                    last.Pts.AddRange(s.Pts.GetRange(1, s.Pts.Count - 1));
                    continue;
                }
                result.Add(s.Closed ? s : new RawShape { Pts = new List<Point3D>(s.Pts), Closed = false, Color = s.Color, Object = s.Object });
            }
            return result;
        }

        static ImportResult Finish(List<RawShape> raw, string baseName, int groupId, bool mergePairs)
        {
            ImportResult r = new ImportResult();
            float tol = MergeTolerance(raw);
            if (mergePairs) raw = MergeTrianglePairs(raw, tol);
            raw = ChainLines(raw, tol);
            int faceNo = 0, lineNo = 0;
            foreach (RawShape s in raw)
            {
                if (s.Pts.Count < 2 || (s.Closed && s.Pts.Count < 3)) continue;
                string name = string.IsNullOrEmpty(s.Object) ? baseName : s.Object;
                MyPolygon shape = new MyPolygon();
                foreach (Point3D p in s.Pts) shape.Add(p);
                if (s.Closed) shape.Close();
                shape.Name = name + (s.Closed ? " face " + (++faceNo) : " line " + (++lineNo));
                shape.Color = s.Color.IsEmpty ? (s.Closed ? DefaultColor : Color.Black) : s.Color;
                shape.GroupId = groupId;
                r.Shapes.Add(shape);
                if (s.Closed) r.Faces++; else r.Lines++;
            }
            if (r.Shapes.Count == 0) r.Warnings.Add("The file has no faces or lines that can be shown.");
            return r;
        }

        #endregion

        #region Public entry points

        public static ImportResult Import(string path, int groupId)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            string baseName = Path.GetFileNameWithoutExtension(path);
            switch (ext)
            {
                case ".obj": return ImportObj(path, baseName, groupId);
                case ".stl": return ImportStl(File.ReadAllBytes(path), baseName, groupId);
                case ".ply": return ImportPly(File.ReadAllBytes(path), baseName, groupId);
                case ".gltf":
                case ".glb": return ImportGltf(path, baseName, groupId);
                default: throw new NotSupportedException("Unknown 3D file type '" + ext + "'. Use OBJ, STL, PLY, glTF or GLB.");
            }
        }

        /// <summary>Writes the visible shapes. colorOf gives each shape's colour by its index.</summary>
        public static void Export(string path, IList<MyPolygon> shapes, Func<int, Color> colorOf)
        {
            List<ExportShape> list = new List<ExportShape>();
            for (int i = 0; i < shapes.Count; i++)
            {
                MyPolygon s = shapes[i];
                if (s.Hidden) continue;
                List<Point3D> pts = Sketch.Vertices(s);
                if (pts.Count < 2) continue;
                list.Add(new ExportShape { Pts = pts, Closed = s.IsClosedFace && pts.Count >= 3, Color = colorOf(i), Name = s.Name });
            }
            string ext = Path.GetExtension(path).ToLowerInvariant();
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            switch (ext)
            {
                case ".obj": ExportObj(path, list); break;
                case ".stl": File.WriteAllBytes(path, StlBytes(list)); break;
                case ".ply": File.WriteAllText(path, PlyText(list), new UTF8Encoding(false)); break;
                case ".gltf": File.WriteAllText(path, GltfJson(list, null), new UTF8Encoding(false)); break;
                case ".glb": File.WriteAllBytes(path, GlbBytes(list)); break;
                default: throw new NotSupportedException("Unknown 3D file type '" + ext + "'.");
            }
        }

        class ExportShape
        {
            public List<Point3D> Pts;
            public bool Closed;
            public Color Color;
            public string Name;
        }

        #endregion

        #region OBJ

        static ImportResult ImportObj(string path, string baseName, int groupId)
        {
            List<Point3D> verts = new List<Point3D>();
            List<RawShape> raw = new List<RawShape>();
            Dictionary<string, Color> materials = new Dictionary<string, Color>();
            Color current = Color.Empty;
            string obj = null;
            List<string> warnings = new List<string>();
            foreach (string rawLine in File.ReadLines(path))
            {
                string line = rawLine.Trim();
                int hash = line.IndexOf('#');
                if (hash >= 0) line = line.Substring(0, hash).Trim();
                if (line.Length == 0) continue;
                string[] t = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                switch (t[0])
                {
                    case "v":
                        if (t.Length >= 4) verts.Add(FromYUp(D(t[1]), D(t[2]), D(t[3])));
                        break;
                    case "f":
                    case "l":
                        {
                            List<Point3D> pts = new List<Point3D>();
                            for (int k = 1; k < t.Length; k++)
                            {
                                string first = t[k].Split('/')[0];
                                int index = int.Parse(first, Inv);
                                index = index < 0 ? verts.Count + index : index - 1;
                                if (index < 0 || index >= verts.Count) throw new FormatException("Face refers to vertex " + first + ", which doesn't exist.");
                                pts.Add(verts[index].Clone());
                            }
                            raw.Add(new RawShape { Pts = pts, Closed = t[0] == "f", Color = current, Object = obj });
                        }
                        break;
                    case "o":
                    case "g":
                        if (t.Length > 1) obj = line.Substring(1).Trim();
                        break;
                    case "usemtl":
                        {
                            Color c;
                            current = t.Length > 1 && materials.TryGetValue(line.Substring(6).Trim(), out c) ? c : Color.Empty;
                        }
                        break;
                    case "mtllib":
                        {
                            string mtl = Path.Combine(Path.GetDirectoryName(path) ?? "", line.Substring(6).Trim());
                            if (File.Exists(mtl)) ReadMtl(mtl, materials);
                            else warnings.Add("Colours file " + Path.GetFileName(mtl) + " was not found, so default colours are used.");
                        }
                        break;
                }
            }
            ImportResult r = Finish(raw, baseName, groupId, true);
            r.Warnings.InsertRange(0, warnings);
            return r;
        }

        static void ReadMtl(string path, Dictionary<string, Color> materials)
        {
            string name = null;
            foreach (string rawLine in File.ReadLines(path))
            {
                string line = rawLine.Trim();
                if (line.StartsWith("newmtl ")) name = line.Substring(7).Trim();
                else if (line.StartsWith("Kd ") && name != null)
                {
                    string[] t = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (t.Length >= 4) materials[name] = Rgb(D(t[1]), D(t[2]), D(t[3]));
                }
            }
        }

        static void ExportObj(string path, List<ExportShape> shapes)
        {
            string mtlName = Path.GetFileNameWithoutExtension(path) + ".mtl";
            Dictionary<int, string> mats = new Dictionary<int, string>();
            StringBuilder obj = new StringBuilder();
            StringBuilder mtl = new StringBuilder("# ThreeDApp2 colours\n");
            obj.Append("# Exported from ThreeDApp2\nmtllib ").Append(mtlName).Append('\n');
            int next = 1;
            foreach (ExportShape s in shapes)
            {
                int argb = s.Color.ToArgb();
                string mat;
                if (!mats.TryGetValue(argb, out mat))
                {
                    mat = "color_" + s.Color.R.ToString("X2") + s.Color.G.ToString("X2") + s.Color.B.ToString("X2");
                    mats[argb] = mat;
                    mtl.AppendFormat(Inv, "newmtl {0}\nKd {1:0.####} {2:0.####} {3:0.####}\n\n", mat, s.Color.R / 255.0, s.Color.G / 255.0, s.Color.B / 255.0);
                }
                obj.Append("o ").Append(Safe(s.Name)).Append('\n').Append("usemtl ").Append(mat).Append('\n');
                foreach (Point3D p in s.Pts)
                {
                    double[] c = ToYUp(p);
                    obj.AppendFormat(Inv, "v {0} {1} {2}\n", N(c[0]), N(c[1]), N(c[2]));
                }
                obj.Append(s.Closed ? "f" : "l");
                for (int k = 0; k < s.Pts.Count; k++) obj.Append(' ').Append(next + k);
                obj.Append('\n');
                next += s.Pts.Count;
            }
            File.WriteAllText(path, obj.ToString(), new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(path) ?? "", mtlName), mtl.ToString(), new UTF8Encoding(false));
        }

        static string Safe(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? "shape" : name.Trim().Replace(' ', '_');
        }

        #endregion

        #region STL

        static ImportResult ImportStl(byte[] data, string baseName, int groupId)
        {
            List<RawShape> raw = new List<RawShape>();
            bool binary = data.Length >= 84 && 84 + 50L * BitConverter.ToUInt32(data, 80) == data.Length;
            if (!binary && !Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 5)).Equals("solid", StringComparison.OrdinalIgnoreCase))
                binary = data.Length >= 84;   // Not text; read as binary and let the size check below complain.
            if (binary)
            {
                uint count = BitConverter.ToUInt32(data, 80);
                if (84 + 50L * count > data.Length) throw new FormatException("The STL file is cut short.");
                for (int t = 0; t < count; t++)
                {
                    int o = 84 + t * 50 + 12;   // Skip the stored normal
                    List<Point3D> pts = new List<Point3D>();
                    for (int k = 0; k < 3; k++, o += 12)
                        pts.Add(FromZUp(BitConverter.ToSingle(data, o), BitConverter.ToSingle(data, o + 4), BitConverter.ToSingle(data, o + 8)));
                    raw.Add(new RawShape { Pts = pts, Closed = true, Color = Color.Empty });
                }
            }
            else
            {
                List<Point3D> pts = new List<Point3D>();
                foreach (string rawLine in Encoding.ASCII.GetString(data).Split('\n'))
                {
                    string line = rawLine.Trim();
                    if (line.StartsWith("vertex", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] t = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        pts.Add(FromZUp(D(t[1]), D(t[2]), D(t[3])));
                    }
                    else if (line.StartsWith("endloop", StringComparison.OrdinalIgnoreCase))
                    {
                        if (pts.Count >= 3) raw.Add(new RawShape { Pts = pts, Closed = true, Color = Color.Empty });
                        pts = new List<Point3D>();
                    }
                }
            }
            return Finish(raw, baseName, groupId, true);
        }

        static byte[] StlBytes(List<ExportShape> shapes)
        {
            List<double[]> tris = new List<double[]>();
            foreach (ExportShape s in shapes)
            {
                if (!s.Closed) continue;
                foreach (int[] t in Triangulate(s.Pts))
                {
                    double[] a = ToZUp(s.Pts[t[0]]), b = ToZUp(s.Pts[t[1]]), c = ToZUp(s.Pts[t[2]]);
                    tris.Add(new[] { a[0], a[1], a[2], b[0], b[1], b[2], c[0], c[1], c[2] });
                }
            }
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter w = new BinaryWriter(ms))
            {
                byte[] header = new byte[80];
                Encoding.ASCII.GetBytes("Exported from ThreeDApp2 (Z up)").CopyTo(header, 0);
                w.Write(header);
                w.Write((uint)tris.Count);
                foreach (double[] t in tris)
                {
                    // Facet normal from the corner order (right-hand rule in the file's Z-up space).
                    double ux = t[3] - t[0], uy = t[4] - t[1], uz = t[5] - t[2], vx = t[6] - t[0], vy = t[7] - t[1], vz = t[8] - t[2];
                    double nx = uy * vz - uz * vy, ny = uz * vx - ux * vz, nz = ux * vy - uy * vx;
                    double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    if (len > 0) { nx /= len; ny /= len; nz /= len; }
                    w.Write((float)nx); w.Write((float)ny); w.Write((float)nz);
                    foreach (double v in t) w.Write((float)v);
                    w.Write((ushort)0);
                }
                w.Flush();
                return ms.ToArray();
            }
        }

        #endregion

        #region PLY

        static ImportResult ImportPly(byte[] data, string baseName, int groupId)
        {
            // Header
            int pos = 0;
            List<string> header = new List<string>();
            while (true)
            {
                int end = Array.IndexOf(data, (byte)'\n', pos);
                if (end < 0) throw new FormatException("The PLY header is incomplete.");
                string line = Encoding.ASCII.GetString(data, pos, end - pos).Trim();
                pos = end + 1;
                header.Add(line);
                if (line == "end_header") break;
            }
            if (header[0] != "ply") throw new FormatException("Not a PLY file.");
            string format = "ascii";
            List<PlyElement> elements = new List<PlyElement>();
            foreach (string line in header)
            {
                string[] t = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (t.Length == 0) continue;
                if (t[0] == "format") format = t[1];
                else if (t[0] == "element") elements.Add(new PlyElement { Name = t[1], Count = int.Parse(t[2], Inv) });
                else if (t[0] == "property" && elements.Count > 0)
                {
                    PlyProperty p = t[1] == "list"
                        ? new PlyProperty { IsList = true, CountType = t[2], Type = t[3], Name = t[4] }
                        : new PlyProperty { Type = t[1], Name = t[2] };
                    elements[elements.Count - 1].Props.Add(p);
                }
            }

            PlyReader reader = new PlyReader(data, pos, format);
            List<Point3D> verts = new List<Point3D>();
            List<Color> vertColors = new List<Color>();
            List<RawShape> raw = new List<RawShape>();
            foreach (PlyElement el in elements)
            {
                for (int i = 0; i < el.Count; i++)
                {
                    double x = 0, y = 0, z = 0; int r = -1, g = -1, b = -1;
                    List<int> indices = null;
                    foreach (PlyProperty p in el.Props)
                    {
                        if (p.IsList)
                        {
                            int n = (int)reader.Read(p.CountType);
                            List<int> list = new List<int>(n);
                            for (int k = 0; k < n; k++) list.Add((int)reader.Read(p.Type));
                            if (p.Name == "vertex_indices" || p.Name == "vertex_index") indices = list;
                            continue;
                        }
                        double v = reader.Read(p.Type);
                        switch (p.Name)
                        {
                            case "x": x = v; break;
                            case "y": y = v; break;
                            case "z": z = v; break;
                            case "red": case "diffuse_red": r = ColorByte(v, p.Type); break;
                            case "green": case "diffuse_green": g = ColorByte(v, p.Type); break;
                            case "blue": case "diffuse_blue": b = ColorByte(v, p.Type); break;
                        }
                    }
                    Color c = r >= 0 && g >= 0 && b >= 0 ? Color.FromArgb(r, g, b) : Color.Empty;
                    if (el.Name == "vertex") { verts.Add(FromYUp(x, y, z)); vertColors.Add(c); }
                    else if (el.Name == "face" && indices != null && indices.Count >= 3)
                    {
                        List<Point3D> pts = new List<Point3D>();
                        foreach (int k in indices)
                        {
                            if (k < 0 || k >= verts.Count) throw new FormatException("A PLY face refers to a missing vertex.");
                            pts.Add(verts[k].Clone());
                        }
                        if (c.IsEmpty && !vertColors[indices[0]].IsEmpty) c = vertColors[indices[0]];
                        raw.Add(new RawShape { Pts = pts, Closed = true, Color = c });
                    }
                }
            }
            return Finish(raw, baseName, groupId, true);
        }

        static int ColorByte(double v, string type)
        {
            if (type == "float" || type == "float32" || type == "double" || type == "float64") v *= 255;
            return Math.Max(0, Math.Min(255, (int)Math.Round(v)));
        }

        class PlyElement { public string Name; public int Count; public List<PlyProperty> Props = new List<PlyProperty>(); }
        class PlyProperty { public string Name, Type, CountType; public bool IsList; }

        class PlyReader
        {
            readonly byte[] mData;
            int mPos;
            readonly bool mAscii, mBigEndian;
            string[] mTokens;
            int mToken;

            public PlyReader(byte[] data, int pos, string format)
            {
                mData = data;
                mPos = pos;
                mAscii = format == "ascii";
                mBigEndian = format == "binary_big_endian";
                if (mAscii)
                    mTokens = Encoding.ASCII.GetString(data, pos, data.Length - pos)
                        .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }

            public double Read(string type)
            {
                if (mAscii)
                {
                    if (mToken >= mTokens.Length) throw new FormatException("The PLY file is cut short.");
                    return double.Parse(mTokens[mToken++], NumberStyles.Float, Inv);
                }
                int size;
                switch (type)
                {
                    case "char": case "int8": case "uchar": case "uint8": size = 1; break;
                    case "short": case "int16": case "ushort": case "uint16": size = 2; break;
                    case "int": case "int32": case "uint": case "uint32": case "float": case "float32": size = 4; break;
                    case "double": case "float64": size = 8; break;
                    default: throw new FormatException("Unknown PLY type " + type);
                }
                if (mPos + size > mData.Length) throw new FormatException("The PLY file is cut short.");
                byte[] b = new byte[size];
                Array.Copy(mData, mPos, b, 0, size);
                mPos += size;
                if (mBigEndian == BitConverter.IsLittleEndian) Array.Reverse(b);
                switch (type)
                {
                    case "char": case "int8": return (sbyte)b[0];
                    case "uchar": case "uint8": return b[0];
                    case "short": case "int16": return BitConverter.ToInt16(b, 0);
                    case "ushort": case "uint16": return BitConverter.ToUInt16(b, 0);
                    case "int": case "int32": return BitConverter.ToInt32(b, 0);
                    case "uint": case "uint32": return BitConverter.ToUInt32(b, 0);
                    case "float": case "float32": return BitConverter.ToSingle(b, 0);
                    default: return BitConverter.ToDouble(b, 0);
                }
            }
        }

        static string PlyText(List<ExportShape> shapes)
        {
            int vertexCount = 0, faceCount = 0;
            foreach (ExportShape s in shapes) if (s.Closed) { vertexCount += s.Pts.Count; faceCount++; }
            StringBuilder sb = new StringBuilder();
            sb.Append("ply\nformat ascii 1.0\ncomment Exported from ThreeDApp2\n");
            sb.Append("element vertex ").Append(vertexCount).Append("\nproperty float x\nproperty float y\nproperty float z\n");
            sb.Append("element face ").Append(faceCount).Append("\nproperty list uchar int vertex_indices\n");
            sb.Append("property uchar red\nproperty uchar green\nproperty uchar blue\nend_header\n");
            foreach (ExportShape s in shapes)
            {
                if (!s.Closed) continue;
                foreach (Point3D p in s.Pts)
                {
                    double[] c = ToYUp(p);
                    sb.Append(N(c[0])).Append(' ').Append(N(c[1])).Append(' ').Append(N(c[2])).Append('\n');
                }
            }
            int next = 0;
            foreach (ExportShape s in shapes)
            {
                if (!s.Closed) continue;
                sb.Append(s.Pts.Count);
                for (int k = 0; k < s.Pts.Count; k++) sb.Append(' ').Append(next + k);
                sb.Append(' ').Append(s.Color.R).Append(' ').Append(s.Color.G).Append(' ').Append(s.Color.B).Append('\n');
                next += s.Pts.Count;
            }
            return sb.ToString();
        }

        #endregion

        #region glTF

        static ImportResult ImportGltf(string path, string baseName, int groupId)
        {
            byte[] file = File.ReadAllBytes(path);
            string json;
            byte[] glbBin = null;
            if (file.Length >= 12 && BitConverter.ToUInt32(file, 0) == 0x46546C67)   // "glTF"
            {
                int pos = 12;
                json = null;
                while (pos + 8 <= file.Length)
                {
                    int len = BitConverter.ToInt32(file, pos);
                    uint type = BitConverter.ToUInt32(file, pos + 4);
                    if (type == 0x4E4F534A) json = Encoding.UTF8.GetString(file, pos + 8, len);      // JSON
                    else if (type == 0x004E4942) { glbBin = new byte[len]; Array.Copy(file, pos + 8, glbBin, 0, len); }   // BIN
                    pos += 8 + len;
                }
                if (json == null) throw new FormatException("The GLB file has no JSON chunk.");
            }
            else json = Encoding.UTF8.GetString(file).TrimStart('﻿');

            object root = MiniJson.Parse(json);
            List<object> required = MiniJson.Arr(root, "extensionsRequired");
            if (required != null && required.Count > 0)
                throw new NotSupportedException("This glTF needs " + string.Join(", ", required) + ", which isn't supported. Re-export it without compression.");

            // Buffers
            List<byte[]> buffers = new List<byte[]>();
            List<object> bufferList = MiniJson.Arr(root, "buffers") ?? new List<object>();
            for (int i = 0; i < bufferList.Count; i++)
            {
                string uri = MiniJson.Str(bufferList[i], "uri");
                if (uri == null) buffers.Add(glbBin ?? new byte[0]);
                else if (uri.StartsWith("data:"))
                {
                    int comma = uri.IndexOf(',');
                    buffers.Add(Convert.FromBase64String(uri.Substring(comma + 1)));
                }
                else buffers.Add(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(path) ?? "", Uri.UnescapeDataString(uri))));
            }

            List<object> meshes = MiniJson.Arr(root, "meshes") ?? new List<object>();
            List<object> nodes = MiniJson.Arr(root, "nodes") ?? new List<object>();
            List<object> materials = MiniJson.Arr(root, "materials") ?? new List<object>();
            List<RawShape> raw = new List<RawShape>();

            Action<int, double[], int> visit = null;
            visit = (nodeIndex, parent, depth) =>
            {
                if (depth > 64 || nodeIndex < 0 || nodeIndex >= nodes.Count) return;
                object node = nodes[nodeIndex];
                double[] world = Mul(parent, NodeMatrix(node));
                if (MiniJson.Has(node, "mesh"))
                {
                    int m = (int)MiniJson.Number(node, "mesh", -1);
                    if (m >= 0 && m < meshes.Count)
                        ReadMesh(root, buffers, meshes[m], materials, world, MiniJson.Str(node, "name") ?? MiniJson.Str(meshes[m], "name"), raw);
                }
                List<object> children = MiniJson.Arr(node, "children");
                if (children != null) foreach (object c in children) visit((int)(double)c, world, depth + 1);
            };

            List<object> scenes = MiniJson.Arr(root, "scenes");
            double[] identity = { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            if (scenes != null && scenes.Count > 0)
            {
                int sceneIndex = (int)MiniJson.Number(root, "scene", 0);
                List<object> roots = MiniJson.Arr(scenes[Math.Max(0, Math.Min(scenes.Count - 1, sceneIndex))], "nodes") ?? new List<object>();
                foreach (object n in roots) visit((int)(double)n, identity, 0);
            }
            else
                for (int m = 0; m < meshes.Count; m++)
                    ReadMesh(root, buffers, meshes[m], materials, identity, MiniJson.Str(meshes[m], "name"), raw);

            return Finish(raw, baseName, groupId, true);
        }

        static void ReadMesh(object root, List<byte[]> buffers, object mesh, List<object> materials, double[] world, string name, List<RawShape> raw)
        {
            List<object> prims = MiniJson.Arr(mesh, "primitives") ?? new List<object>();
            foreach (object prim in prims)
            {
                Dictionary<string, object> attrs = MiniJson.Obj(prim, "attributes");
                if (attrs == null || !attrs.ContainsKey("POSITION")) continue;
                float[] pos = ReadAccessor(root, buffers, (int)(double)attrs["POSITION"]);
                int count = pos.Length / 3;
                int[] idx;
                if (MiniJson.Has(prim, "indices"))
                {
                    float[] f = ReadAccessor(root, buffers, (int)MiniJson.Number(prim, "indices", 0));
                    idx = new int[f.Length];
                    for (int i = 0; i < f.Length; i++) idx[i] = (int)f[i];
                }
                else
                {
                    idx = new int[count];
                    for (int i = 0; i < count; i++) idx[i] = i;
                }
                Point3D[] p = new Point3D[count];
                for (int i = 0; i < count; i++)
                {
                    double x = pos[3 * i], y = pos[3 * i + 1], z = pos[3 * i + 2];
                    p[i] = FromYUp(world[0] * x + world[4] * y + world[8] * z + world[12],
                                   world[1] * x + world[5] * y + world[9] * z + world[13],
                                   world[2] * x + world[6] * y + world[10] * z + world[14]);
                }
                Color color = Color.Empty;
                int mat = (int)MiniJson.Number(prim, "material", -1);
                if (mat >= 0 && mat < materials.Count)
                {
                    List<object> f = MiniJson.Arr(MiniJson.Obj(materials[mat], "pbrMetallicRoughness"), "baseColorFactor");
                    if (f != null && f.Count >= 3) color = Rgb(LinearToSrgb((double)f[0]), LinearToSrgb((double)f[1]), LinearToSrgb((double)f[2]));
                }
                int mode = (int)MiniJson.Number(prim, "mode", 4);
                Func<int, Point3D> at = k => { if (k < 0 || k >= count) throw new FormatException("glTF index out of range."); return p[k].Clone(); };
                switch (mode)
                {
                    case 4:   // Triangles
                        for (int i = 0; i + 2 < idx.Length; i += 3)
                            raw.Add(new RawShape { Pts = new List<Point3D> { at(idx[i]), at(idx[i + 1]), at(idx[i + 2]) }, Closed = true, Color = color, Object = name });
                        break;
                    case 5:   // Triangle strip
                        for (int i = 0; i + 2 < idx.Length; i++)
                        {
                            bool even = i % 2 == 0;
                            raw.Add(new RawShape { Pts = new List<Point3D> { at(idx[even ? i : i + 1]), at(idx[even ? i + 1 : i]), at(idx[i + 2]) }, Closed = true, Color = color, Object = name });
                        }
                        break;
                    case 6:   // Triangle fan
                        for (int i = 1; i + 1 < idx.Length; i++)
                            raw.Add(new RawShape { Pts = new List<Point3D> { at(idx[0]), at(idx[i]), at(idx[i + 1]) }, Closed = true, Color = color, Object = name });
                        break;
                    case 1:   // Lines
                        for (int i = 0; i + 1 < idx.Length; i += 2)
                            raw.Add(new RawShape { Pts = new List<Point3D> { at(idx[i]), at(idx[i + 1]) }, Closed = false, Color = color, Object = name });
                        break;
                    case 2:   // Line loop
                    case 3:   // Line strip
                        {
                            List<Point3D> pts = new List<Point3D>();
                            foreach (int k in idx) pts.Add(at(k));
                            if (mode == 2 && pts.Count > 1) pts.Add(pts[0].Clone());
                            raw.Add(new RawShape { Pts = pts, Closed = false, Color = color, Object = name });
                        }
                        break;
                }
            }
        }

        static float[] ReadAccessor(object root, List<byte[]> buffers, int index)
        {
            List<object> accessors = MiniJson.Arr(root, "accessors");
            if (accessors == null || index < 0 || index >= accessors.Count) throw new FormatException("glTF accessor " + index + " is missing.");
            object acc = accessors[index];
            if (MiniJson.Has(acc, "sparse")) throw new NotSupportedException("Sparse glTF accessors aren't supported.");
            int count = (int)MiniJson.Number(acc, "count", 0);
            int comp = (int)MiniJson.Number(acc, "componentType", 5126);
            string type = MiniJson.Str(acc, "type") ?? "SCALAR";
            bool normalized = MiniJson.Has(acc, "normalized") && (bool)((Dictionary<string, object>)acc)["normalized"];
            int n = type == "SCALAR" ? 1 : type == "VEC2" ? 2 : type == "VEC3" ? 3 : type == "VEC4" ? 4 : type == "MAT4" ? 16 : type == "MAT3" ? 9 : 4;
            int size = comp == 5126 || comp == 5125 ? 4 : comp == 5123 || comp == 5122 ? 2 : 1;
            float[] result = new float[count * n];
            if (!MiniJson.Has(acc, "bufferView")) return result;   // All zeros, per the spec
            object view = MiniJson.Arr(root, "bufferViews")[(int)MiniJson.Number(acc, "bufferView", 0)];
            byte[] buf = buffers[(int)MiniJson.Number(view, "buffer", 0)];
            int start = (int)MiniJson.Number(view, "byteOffset", 0) + (int)MiniJson.Number(acc, "byteOffset", 0);
            int stride = (int)MiniJson.Number(view, "byteStride", 0);
            if (stride == 0) stride = n * size;
            for (int i = 0; i < count; i++)
                for (int c = 0; c < n; c++)
                {
                    int o = start + i * stride + c * size;
                    if (o + size > buf.Length) throw new FormatException("glTF data runs past the end of its buffer.");
                    double v;
                    switch (comp)
                    {
                        case 5126: v = BitConverter.ToSingle(buf, o); break;
                        case 5125: v = BitConverter.ToUInt32(buf, o); break;
                        case 5123: v = BitConverter.ToUInt16(buf, o); if (normalized) v /= 65535.0; break;
                        case 5122: v = BitConverter.ToInt16(buf, o); if (normalized) v = Math.Max(v / 32767.0, -1); break;
                        case 5121: v = buf[o]; if (normalized) v /= 255.0; break;
                        case 5120: v = (sbyte)buf[o]; if (normalized) v = Math.Max(v / 127.0, -1); break;
                        default: throw new FormatException("Unknown glTF component type " + comp);
                    }
                    result[i * n + c] = (float)v;
                }
            return result;
        }

        /// <summary>A node's local transform as a column-major 4x4 matrix.</summary>
        static double[] NodeMatrix(object node)
        {
            List<object> m = MiniJson.Arr(node, "matrix");
            if (m != null && m.Count == 16)
            {
                double[] r = new double[16];
                for (int i = 0; i < 16; i++) r[i] = (double)m[i];
                return r;
            }
            List<object> t = MiniJson.Arr(node, "translation"), q = MiniJson.Arr(node, "rotation"), s = MiniJson.Arr(node, "scale");
            double tx = t != null ? (double)t[0] : 0, ty = t != null ? (double)t[1] : 0, tz = t != null ? (double)t[2] : 0;
            double qx = q != null ? (double)q[0] : 0, qy = q != null ? (double)q[1] : 0, qz = q != null ? (double)q[2] : 0, qw = q != null ? (double)q[3] : 1;
            double sx = s != null ? (double)s[0] : 1, sy = s != null ? (double)s[1] : 1, sz = s != null ? (double)s[2] : 1;
            return new[]
            {
                (1 - 2 * (qy * qy + qz * qz)) * sx, (2 * (qx * qy + qz * qw)) * sx, (2 * (qx * qz - qy * qw)) * sx, 0,
                (2 * (qx * qy - qz * qw)) * sy, (1 - 2 * (qx * qx + qz * qz)) * sy, (2 * (qy * qz + qx * qw)) * sy, 0,
                (2 * (qx * qz + qy * qw)) * sz, (2 * (qy * qz - qx * qw)) * sz, (1 - 2 * (qx * qx + qy * qy)) * sz, 0,
                tx, ty, tz, 1,
            };
        }

        static double[] Mul(double[] a, double[] b)   // Column-major a * b
        {
            double[] r = new double[16];
            for (int c = 0; c < 4; c++)
                for (int row = 0; row < 4; row++)
                {
                    double sum = 0;
                    for (int k = 0; k < 4; k++) sum += a[k * 4 + row] * b[c * 4 + k];
                    r[c * 4 + row] = sum;
                }
            return r;
        }

        static double SrgbToLinear(double c) { return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4); }
        static double LinearToSrgb(double c) { c = Math.Max(0, Math.Min(1, c)); return c <= 0.0031308 ? c * 12.92 : 1.055 * Math.Pow(c, 1 / 2.4) - 0.055; }

        /// <summary>
        /// Builds the glTF JSON. Faces become flat-shaded triangles (one primitive per colour), open
        /// lines become a LINES primitive. If glbBuffer is null the binary data is embedded as base64.
        /// </summary>
        static string GltfJson(List<ExportShape> shapes, MemoryStream glbBuffer)
        {
            MemoryStream bin = new MemoryStream();
            BinaryWriter w = new BinaryWriter(bin);
            List<string> accessors = new List<string>(), views = new List<string>(), prims = new List<string>(), mats = new List<string>();
            Dictionary<int, int> matIndex = new Dictionary<int, int>();
            Func<Color, int> material = c =>
            {
                int key = c.ToArgb(), m;
                if (matIndex.TryGetValue(key, out m)) return m;
                m = mats.Count;
                matIndex[key] = m;
                mats.Add(string.Format(Inv, "{{\"name\":\"color_{0:X2}{1:X2}{2:X2}\",\"doubleSided\":true,\"pbrMetallicRoughness\":{{\"baseColorFactor\":[{3},{4},{5},1],\"metallicFactor\":0,\"roughnessFactor\":0.9}}}}",
                    c.R, c.G, c.B, MiniJson.Num(SrgbToLinear(c.R / 255.0)), MiniJson.Num(SrgbToLinear(c.G / 255.0)), MiniJson.Num(SrgbToLinear(c.B / 255.0))));
                return m;
            };

            // Collect triangles per colour, and line segments per colour.
            Dictionary<int, List<double[]>> triByColor = new Dictionary<int, List<double[]>>();
            Dictionary<int, List<double[]>> lineByColor = new Dictionary<int, List<double[]>>();
            Dictionary<int, Color> colors = new Dictionary<int, Color>();
            foreach (ExportShape s in shapes)
            {
                int key = s.Color.ToArgb();
                colors[key] = s.Color;
                if (s.Closed)
                {
                    List<double[]> list;
                    if (!triByColor.TryGetValue(key, out list)) triByColor[key] = list = new List<double[]>();
                    foreach (int[] t in Triangulate(s.Pts))
                        list.Add(new[] { ToYUp(s.Pts[t[0]]), ToYUp(s.Pts[t[1]]), ToYUp(s.Pts[t[2]]) }.SelectMany());
                }
                else
                {
                    List<double[]> list;
                    if (!lineByColor.TryGetValue(key, out list)) lineByColor[key] = list = new List<double[]>();
                    for (int k = 0; k + 1 < s.Pts.Count; k++)
                        list.Add(new[] { ToYUp(s.Pts[k]), ToYUp(s.Pts[k + 1]) }.SelectMany());
                }
            }

            Func<List<double[]>, int, bool, string> addPrim = (items, mode, withNormals) =>
            {
                int vertsPer = mode == 4 ? 3 : 2;
                int vcount = items.Count * vertsPer;
                double[] min = { double.MaxValue, double.MaxValue, double.MaxValue }, max = { double.MinValue, double.MinValue, double.MinValue };
                Align(w);
                long posOffset = bin.Position;
                foreach (double[] it in items)
                    for (int v = 0; v < vertsPer; v++)
                        for (int c = 0; c < 3; c++)
                        {
                            float f = (float)it[v * 3 + c];
                            w.Write(f);
                            min[c] = Math.Min(min[c], f);
                            max[c] = Math.Max(max[c], f);
                        }
                views.Add(string.Format(Inv, "{{\"buffer\":0,\"byteOffset\":{0},\"byteLength\":{1},\"target\":34962}}", posOffset, bin.Position - posOffset));
                int posAcc = accessors.Count;
                accessors.Add(string.Format(Inv, "{{\"bufferView\":{0},\"componentType\":5126,\"count\":{1},\"type\":\"VEC3\",\"min\":[{2},{3},{4}],\"max\":[{5},{6},{7}]}}",
                    views.Count - 1, vcount, MiniJson.Num(min[0]), MiniJson.Num(min[1]), MiniJson.Num(min[2]), MiniJson.Num(max[0]), MiniJson.Num(max[1]), MiniJson.Num(max[2])));
                string normalAttr = "";
                if (withNormals)
                {
                    long nOffset = bin.Position;
                    foreach (double[] it in items)
                    {
                        double ux = it[3] - it[0], uy = it[4] - it[1], uz = it[5] - it[2], vx = it[6] - it[0], vy = it[7] - it[1], vz = it[8] - it[2];
                        double nx = uy * vz - uz * vy, ny = uz * vx - ux * vz, nz = ux * vy - uy * vx;
                        double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                        if (len > 0) { nx /= len; ny /= len; nz /= len; } else { nx = 0; ny = 1; nz = 0; }
                        for (int v = 0; v < 3; v++) { w.Write((float)nx); w.Write((float)ny); w.Write((float)nz); }
                    }
                    views.Add(string.Format(Inv, "{{\"buffer\":0,\"byteOffset\":{0},\"byteLength\":{1},\"target\":34962}}", nOffset, bin.Position - nOffset));
                    accessors.Add(string.Format(Inv, "{{\"bufferView\":{0},\"componentType\":5126,\"count\":{1},\"type\":\"VEC3\"}}", views.Count - 1, vcount));
                    normalAttr = ",\"NORMAL\":" + (accessors.Count - 1);
                }
                return "{\"attributes\":{\"POSITION\":" + posAcc + normalAttr + "}";
            };

            foreach (var pair in triByColor)
                prims.Add(addPrim(pair.Value, 4, true) + ",\"mode\":4,\"material\":" + material(colors[pair.Key]) + "}");
            foreach (var pair in lineByColor)
                prims.Add(addPrim(pair.Value, 1, false) + ",\"mode\":1,\"material\":" + material(colors[pair.Key]) + "}");
            Align(w);
            w.Flush();

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"asset\":{\"version\":\"2.0\",\"generator\":\"ThreeDApp2\"},\"scene\":0,\"scenes\":[{\"nodes\":[0]}],");
            sb.Append("\"nodes\":[{\"name\":\"ThreeDApp2 model\"").Append(prims.Count > 0 ? ",\"mesh\":0" : "").Append("}],");
            if (prims.Count > 0) sb.Append("\"meshes\":[{\"name\":\"model\",\"primitives\":[").Append(string.Join(",", prims)).Append("]}],");
            if (mats.Count > 0) sb.Append("\"materials\":[").Append(string.Join(",", mats)).Append("],");
            if (accessors.Count > 0)
            {
                sb.Append("\"accessors\":[").Append(string.Join(",", accessors)).Append("],");
                sb.Append("\"bufferViews\":[").Append(string.Join(",", views)).Append("],");
            }
            byte[] data = bin.ToArray();
            if (glbBuffer != null)
            {
                glbBuffer.Write(data, 0, data.Length);
                sb.Append("\"buffers\":[{\"byteLength\":").Append(data.Length).Append("}]}");
            }
            else
                sb.Append("\"buffers\":[{\"byteLength\":").Append(data.Length).Append(",\"uri\":\"data:application/octet-stream;base64,")
                  .Append(Convert.ToBase64String(data)).Append("\"}]}");
            return sb.ToString();
        }

        static void Align(BinaryWriter w)
        {
            while (w.BaseStream.Position % 4 != 0) w.Write((byte)0);
        }

        static byte[] GlbBytes(List<ExportShape> shapes)
        {
            MemoryStream binChunk = new MemoryStream();
            string json = GltfJson(shapes, binChunk);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            int jsonPad = (4 - jsonBytes.Length % 4) % 4;
            byte[] bin = binChunk.ToArray();
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter w = new BinaryWriter(ms))
            {
                int total = 12 + 8 + jsonBytes.Length + jsonPad + (bin.Length > 0 ? 8 + bin.Length : 0);
                w.Write(0x46546C67u); w.Write(2u); w.Write((uint)total);
                w.Write((uint)(jsonBytes.Length + jsonPad)); w.Write(0x4E4F534Au);
                w.Write(jsonBytes);
                for (int i = 0; i < jsonPad; i++) w.Write((byte)' ');
                if (bin.Length > 0) { w.Write((uint)bin.Length); w.Write(0x004E4942u); w.Write(bin); }
                w.Flush();
                return ms.ToArray();
            }
        }

        #endregion

        #region Small helpers

        static double D(string s) { return double.Parse(s, NumberStyles.Float, Inv); }

        static string N(double v) { return v.ToString("0.######", Inv); }

        static Color Rgb(double r, double g, double b)
        {
            Func<double, int> to = v => Math.Max(0, Math.Min(255, (int)Math.Round(v * 255)));
            return Color.FromArgb(to(r), to(g), to(b));
        }

        static double[] SelectMany(this double[][] parts)
        {
            int n = 0;
            foreach (double[] p in parts) n += p.Length;
            double[] r = new double[n];
            int o = 0;
            foreach (double[] p in parts) { p.CopyTo(r, o); o += p.Length; }
            return r;
        }

        #endregion
    }
}
