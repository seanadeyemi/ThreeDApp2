using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace ThreeDApp2
{
    /// <summary>
    /// Saves and loads a scene (shapes, camera and view) as a small human-readable text file.
    ///
    ///   threedapp2-scene 1
    ///   camera x y z
    ///   view scale screenZ
    ///   pivot x y z
    ///   orientation m00 m01 m02 m10 m11 m12 m20 m21 m22
    ///   shape closed(0|1) argb(hex|-) group name...
    ///   v x y z
    ///   end
    /// </summary>
    public static class SceneFile
    {
        const string Header = "threedapp2-scene 1";
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string Serialize(Scene scene)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Header);
            sb.AppendLine("camera " + P(scene.Camera));
            sb.AppendLine("view " + F(scene.Scale) + " " + F(scene.ScreenZ));
            sb.AppendLine("pivot " + P(scene.Pivot));
            sb.Append("orientation");
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    sb.Append(" " + scene.Orientation[i, j].ToString("R", Inv));
            sb.AppendLine();
            foreach (MyPolygon shape in scene.Layers)
            {
                bool closed = shape.IsClosedFace;
                string color = shape.Color.IsEmpty ? "-" : shape.Color.ToArgb().ToString("X8", Inv);
                string name = (shape.Name ?? "").Replace('\r', ' ').Replace('\n', ' ');
                sb.AppendLine("shape " + (closed ? "1" : "0") + " " + color + " " + shape.GroupId.ToString(Inv) + " " + name);
                int count = Scene.VertexCount(shape);
                for (int j = 0; j < count; j++)
                {
                    Point3D v = shape.Absolute(j);
                    sb.Append("v ").Append(F(v.X)).Append(' ').Append(F(v.Y)).Append(' ').Append(F(v.Z)).Append('\n');
                }
                sb.AppendLine("end");
            }
            return sb.ToString();
        }

        public static Scene Deserialize(string text)
        {
            Scene scene = new Scene();
            string[] lines = text.Replace("\r", "").Split('\n');
            if (lines.Length == 0 || lines[0].Trim() != Header)
                throw new FormatException("Not a ThreeDApp2 scene file.");

            MyPolygon current = null;
            bool currentClosed = false;
            for (int n = 1; n < lines.Length; n++)
            {
                string line = lines[n].Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                string[] t = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                switch (t[0])
                {
                    case "camera": scene.Camera = ReadPoint(t, 1); break;
                    case "view":
                        scene.Scale = ReadFloat(t[1]);
                        scene.ScreenZ = ReadFloat(t[2]);
                        break;
                    case "pivot": scene.Pivot = ReadPoint(t, 1); break;
                    case "orientation":
                        Matrix3D m = new Matrix3D();
                        for (int i = 0; i < 9; i++)
                            m[i / 3, i % 3] = double.Parse(t[1 + i], Inv);
                        scene.Orientation = m;
                        break;
                    case "shape":
                        current = new MyPolygon();
                        currentClosed = t[1] == "1";
                        current.Color = t[2] == "-" ? Color.Empty : Color.FromArgb(int.Parse(t[2], NumberStyles.HexNumber, Inv));
                        current.GroupId = int.Parse(t[3], Inv);
                        current.Name = t.Length > 4 ? string.Join(" ", t, 4, t.Length - 4) : "";
                        break;
                    case "v":
                        if (current == null) throw new FormatException("Vertex outside a shape on line " + (n + 1));
                        current.Add(ReadPoint(t, 1));
                        break;
                    case "end":
                        if (current == null) throw new FormatException("Unexpected 'end' on line " + (n + 1));
                        if (currentClosed && current.GetSize() >= 3) current.Close();
                        if (current.GetSize() >= 2) scene.Layers.Add(current);
                        current = null;
                        break;
                    default:
                        // Unknown lines are ignored so newer files still open.
                        break;
                }
            }
            return scene;
        }

        public static void Save(Scene scene, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            // Write to a temp file first so a crash mid-save never leaves a half-written file.
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, Serialize(scene), Encoding.UTF8);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        public static Scene Load(string path)
        {
            return Deserialize(File.ReadAllText(path, Encoding.UTF8));
        }

        static string F(float v) { return v.ToString("R", Inv); }
        static string P(Point3D p) { return F(p.X) + " " + F(p.Y) + " " + F(p.Z); }
        static float ReadFloat(string s) { return float.Parse(s, Inv); }
        static Point3D ReadPoint(string[] t, int i)
        {
            return new Point3D(ReadFloat(t[i]), ReadFloat(t[i + 1]), ReadFloat(t[i + 2]));
        }
    }
}
