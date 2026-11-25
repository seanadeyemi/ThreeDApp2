using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ThreeDApp2
{
    /// <summary>
    /// Holds the layers, camera and orientation, and turns them into 2D screen polygons
    /// ordered back to front (painter's algorithm). Contains no WinForms code so the maths
    /// can be checked on its own.
    /// </summary>
    public class Scene
    {
        public List<MyPolygon> Layers = new List<MyPolygon>();
        public Point3D Camera = new Point3D(0, 0, 0);   // Our viewpoint
        public float ScreenZ = 30;                       // Distance to the projection plane
        public float Scale = 10;                         // Pixels per world unit on the projection plane
        public float NearPlane = 0.5f;                   // Layers closer than this to the camera are not drawn
        public Point3D Pivot = new Point3D(0, 0, 0);    // Point the layers rotate around
        public Matrix3D Orientation = Matrix3D.Identity();

        // Light comes from over the viewer's shoulder.
        static readonly double[] LightDir = Normalize(new double[] { -0.4, 0.5, -1.0 });

        public void Rotate(Axis axis, double degrees)
        {
            Orientation = Matrix3D.Rotation(axis, degrees) * Orientation;
        }

        public void ResetOrientation()
        {
            Orientation = Matrix3D.Identity();
        }

        #region Turntable orbit camera (SketchUp style)

        // The camera always looks straight at Pivot (the orbit target) from Distance away.
        // Yaw turns around the model's vertical (Y) axis, Pitch tilts up/down and is clamped,
        // and there is never any roll, so the horizon stays level however much you orbit.
        public double Yaw;            // Degrees
        public double Pitch;          // Degrees, -90 (from below) .. 90 (from above)
        public float Distance = 40;   // Camera to target

        public const double MaxPitch = 90;

        public void SetOrbit(double yaw, double pitch)
        {
            Yaw = ((yaw % 360) + 540) % 360 - 180;   // Keep in -180..180
            Pitch = Math.Max(-MaxPitch, Math.Min(MaxPitch, pitch));
            ApplyOrbit();
        }

        /// <summary>Rebuilds Orientation and Camera from Yaw, Pitch, Distance and Pivot.</summary>
        public void ApplyOrbit()
        {
            Orientation = Matrix3D.Rotation(Axis.X, Pitch) * Matrix3D.Rotation(Axis.Y, Yaw);
            Distance = Math.Max(0.5f, Distance);
            Camera = new Point3D(Pivot.X, Pivot.Y, Pivot.Z - Distance);
        }

        /// <summary>Works out Yaw/Pitch/Distance from Orientation and Camera (e.g. after loading a file).</summary>
        public void DeriveOrbit()
        {
            double m00 = Orientation[0, 0], m02 = Orientation[0, 2];
            double m10 = Orientation[1, 0], m11 = Orientation[1, 1], m12 = Orientation[1, 2];
            double yawRad = Math.Atan2(m02, m00);
            double sy = Math.Sin(yawRad), cy = Math.Cos(yawRad);
            double sp = -m10 * sy + m12 * cy;
            Yaw = yawRad * 180 / Math.PI;
            Pitch = Math.Atan2(sp, m11) * 180 / Math.PI;
            Distance = Math.Max(0.5f, Pivot.Z - Camera.Z);
            SetOrbit(Yaw, Pitch);
        }

        /// <summary>Focal length for a vertical field of view on a viewport of the given pixel height.</summary>
        public void SetFieldOfView(double fovDegrees, float viewportHeight)
        {
            double half = Math.Max(1, Math.Min(170, fovDegrees)) * Math.PI / 360;
            float focal = (float)(Math.Max(1f, viewportHeight) / 2 / Math.Tan(half));
            Scale = focal / ScreenZ;
        }

        public float FocalLength { get { return Scale * ScreenZ; } }

        /// <summary>Slides the orbit target so the scene follows the mouse by (dx, dy) pixels.</summary>
        public void PanPixels(float dx, float dy)
        {
            float worldPerPixel = Distance / FocalLength;
            Point3D move = Orientation.Transpose().Transform(new Point3D(-dx * worldPerPixel, dy * worldPerPixel, 0));
            Pivot = Pivot + move;
            ApplyOrbit();
        }

        /// <summary>
        /// Moves the camera towards (factor &lt; 1) or away from (factor &gt; 1) the target, drifting the
        /// target towards the point under the cursor so the zoom heads where you point (like SketchUp).
        /// </summary>
        public void Dolly(float factor, PointF cursor)
        {
            float newDistance = Math.Max(0.5f, Distance * factor);
            // Point under the cursor at the target's depth.
            Point3D o, d;
            GetModelRay(cursor, out o, out d);
            Plane3D focus = new Plane3D(Pivot, Orientation.Transpose().Transform(new Point3D(0, 0, 1)));
            Point3D hit;
            if (focus.IntersectRay(o, d, out hit))
                Pivot = Pivot + (hit - Pivot) * (1 - newDistance / Distance);
            Distance = newDistance;
            ApplyOrbit();
        }

        /// <summary>
        /// Centres the target on everything in the scene (and the origin, if asked) and backs off
        /// until it all fits.
        /// </summary>
        public void ZoomExtents(double fovDegrees, float aspect = 1, bool includeOrigin = false)
        {
            List<Point3D> pts = new List<Point3D>();
            if (includeOrigin) pts.Add(new Point3D(0, 0, 0));
            foreach (MyPolygon layer in Layers)
                for (int j = 0; j < layer.GetSize(); j++) pts.Add(layer.Absolute(j));
            if (pts.Count <= (includeOrigin ? 1 : 0)) { Pivot = new Point3D(0, 0, 0); Distance = 40; ApplyOrbit(); return; }

            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
            foreach (Point3D p in pts)
            {
                minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
                minZ = Math.Min(minZ, p.Z); maxZ = Math.Max(maxZ, p.Z);
            }
            Pivot = new Point3D((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
            float radius = 0;
            foreach (Point3D p in pts) radius = Math.Max(radius, Point3D.Distance(p, Pivot));
            radius = Math.Max(radius, 1);
            // Fit the bounding sphere in the narrower of the two view angles, with a little margin.
            double half = fovDegrees * Math.PI / 360;
            half = Math.Min(half, Math.Atan(Math.Tan(half) * Math.Max(0.1f, aspect)));
            Distance = (float)(radius / Math.Sin(half) * 1.05);
            ApplyOrbit();
        }

        #endregion

        /// <summary>Centre of all layer vertices (closing vertices ignored).</summary>
        public Point3D Centroid()
        {
            double x = 0, y = 0, z = 0;
            int n = 0;
            foreach (MyPolygon layer in Layers)
            {
                int count = VertexCount(layer);
                for (int j = 0; j < count; j++)
                {
                    Point3D p = layer.Absolute(j);
                    x += p.X; y += p.Y; z += p.Z; n++;
                }
            }
            return n == 0 ? new Point3D(0, 0, 0) : new Point3D(x / n, y / n, z / n);
        }

        public Point3D ToWorld(Point3D model)
        {
            return Orientation.Transform(model - Pivot) + Pivot;
        }

        /// <summary>Inverse of <see cref="ToWorld"/>.</summary>
        public Point3D ToModel(Point3D world)
        {
            return Orientation.Transpose().Transform(world - Pivot) + Pivot;
        }

        public bool TryProjectModel(Point3D model, out PointF screen)
        {
            return TryProject(ToWorld(model), out screen);
        }

        /// <summary>
        /// The ray (in model space) that starts at the camera and passes through a screen point.
        /// This is the inverse of the projection, used to turn mouse positions into 3D points.
        /// </summary>
        public void GetModelRay(PointF screen, out Point3D origin, out Point3D direction)
        {
            Point3D worldDir = new Point3D(screen.X / (Scale * ScreenZ), -screen.Y / (Scale * ScreenZ), 1);
            origin = ToModel(Camera);
            direction = Orientation.Transpose().Transform(worldDir);
        }

        /// <summary>
        /// Projects a model-space line segment, clipping it against the near plane so lines that
        /// pass behind the camera (axes, grid, rubber-band lines) still draw correctly.
        /// </summary>
        public bool TryProjectSegmentModel(Point3D a, Point3D b, out PointF pa, out PointF pb)
        {
            pa = pb = PointF.Empty;
            Point3D wa = ToWorld(a), wb = ToWorld(b);
            float za = wa.Z - Camera.Z, zb = wb.Z - Camera.Z;
            if (za < NearPlane && zb < NearPlane) return false;
            if (za < NearPlane)
                wa = wa + (wb - wa) * ((NearPlane - za) / (zb - za));
            else if (zb < NearPlane)
                wb = wb + (wa - wb) * ((NearPlane - zb) / (za - zb));
            return TryProject(wa, out pa) && TryProject(wb, out pb);
        }

        /// <summary>Largest group id in use plus one.</summary>
        public int NextGroupId()
        {
            int max = 0;
            foreach (MyPolygon p in Layers) max = Math.Max(max, p.GroupId);
            return max + 1;
        }

        /// <summary>Indices of every shape in the same group as <paramref name="index"/> (just itself if ungrouped).</summary>
        public List<int> GroupMembers(int index)
        {
            List<int> result = new List<int>();
            if (index < 0 || index >= Layers.Count) return result;
            int group = Layers[index].GroupId;
            if (group == 0) { result.Add(index); return result; }
            for (int i = 0; i < Layers.Count; i++)
                if (Layers[i].GroupId == group) result.Add(i);
            return result;
        }

        /// <summary>
        /// The shape under a screen point: the nearest filled face containing it, or an open
        /// polyline within <paramref name="lineTolerance"/> pixels. Returns -1 if none.
        /// </summary>
        public int Pick(PointF screen, List<ShapeScreenPoints> frame, float lineTolerance = 6, bool facesOnly = false)
        {
            // frame is ordered farthest first, so walk it backwards to find the nearest hit.
            for (int i = frame.Count - 1; i >= 0; i--)
            {
                ShapeScreenPoints f = frame[i];
                if (f.Closed)
                {
                    if (PointInPolygon(f.PtList, screen)) return f.LayerIndex;
                }
                else if (!facesOnly && DistanceToPolyline(f.PtList, screen, false) <= lineTolerance)
                {
                    return f.LayerIndex;
                }
            }
            return -1;
        }

        public static bool PointInPolygon(IList<PointF> poly, PointF p)
        {
            bool inside = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                PointF a = poly[i], b = poly[j];
                if ((a.Y > p.Y) != (b.Y > p.Y) &&
                    p.X < (b.X - a.X) * (p.Y - a.Y) / (b.Y - a.Y) + a.X)
                    inside = !inside;
            }
            return inside;
        }

        public static float DistanceToSegment(PointF p, PointF a, PointF b)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len2 = dx * dx + dy * dy;
            float t = len2 < 1e-9f ? 0 : ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / len2;
            t = Math.Max(0, Math.Min(1, t));
            float cx = a.X + t * dx - p.X, cy = a.Y + t * dy - p.Y;
            return (float)Math.Sqrt(cx * cx + cy * cy);
        }

        public static float DistanceToPolyline(IList<PointF> pts, PointF p, bool closed)
        {
            float best = float.MaxValue;
            int n = pts.Count;
            int segs = closed ? n : n - 1;
            for (int i = 0; i < segs; i++)
                best = Math.Min(best, DistanceToSegment(p, pts[i], pts[(i + 1) % n]));
            if (n == 1)
                best = (float)Math.Sqrt((p.X - pts[0].X) * (p.X - pts[0].X) + (p.Y - pts[0].Y) * (p.Y - pts[0].Y));
            return best;
        }

        /// <summary>
        /// Perspective projection onto the screen plane. Screen Y grows downwards, so world Y is
        /// flipped to keep "up" pointing up. Returns false for points too close to or behind the camera.
        /// </summary>
        public bool TryProject(Point3D world, out PointF screen)
        {
            float dz = world.Z - Camera.Z;
            if (dz < NearPlane)
            {
                screen = PointF.Empty;
                return false;
            }
            screen = new PointF(
                Scale * (world.X - Camera.X) * ScreenZ / dz,
                -Scale * (world.Y - Camera.Y) * ScreenZ / dz);
            return true;
        }

        /// <summary>
        /// Projects every layer and returns them sorted farthest first, ready to paint in order.
        /// Each entry remembers which layer it came from, so sorting never mixes layers up.
        /// </summary>
        public List<ShapeScreenPoints> BuildFrame()
        {
            List<ShapeScreenPoints> frame = new List<ShapeScreenPoints>();
            for (int i = 0; i < Layers.Count; i++)
            {
                MyPolygon layer = Layers[i];
                if (layer.Hidden) continue;
                int count = VertexCount(layer);
                if (count < 2) continue;

                List<Point3D> world = new List<Point3D>();
                for (int j = 0; j < count; j++)
                    world.Add(ToWorld(layer.Absolute(j)));

                ShapeScreenPoints shape = new ShapeScreenPoints();
                shape.LayerIndex = i;
                shape.Closed = layer.IsClosedFace;
                bool visible = true;
                double depth = 0;
                foreach (Point3D w in world)
                {
                    PointF pt;
                    if (!TryProject(w, out pt)) { visible = false; break; }
                    shape.PtList.Add(pt);
                    depth += w.Z - Camera.Z;
                }
                if (!visible) continue;

                shape.Z = (float)(depth / count);
                shape.Shade = (float)Shade(world);
                frame.Add(shape);
            }
            // Stable sort: equal depths keep layer order instead of flickering between frames.
            return frame.OrderByDescending(f => f.Z).ThenBy(f => f.LayerIndex).ToList();
        }

        /// <summary>Number of distinct vertices, i.e. without the repeated closing vertex.</summary>
        public static int VertexCount(Polygon polygon)
        {
            int n = polygon.GetSize();
            if (n > 1 && polygon.Closed()) n--;
            return n;
        }

        /// <summary>Simple two-sided Lambert shading from the polygon's normal (Newell's method).</summary>
        public static double Shade(List<Point3D> pts)
        {
            double nx = 0, ny = 0, nz = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                Point3D a = pts[i];
                Point3D b = pts[(i + 1) % pts.Count];
                nx += (a.Y - b.Y) * (a.Z + b.Z);
                ny += (a.Z - b.Z) * (a.X + b.X);
                nz += (a.X - b.X) * (a.Y + b.Y);
            }
            double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len < 1e-9) return 1.0;
            double dot = Math.Abs((nx * LightDir[0] + ny * LightDir[1] + nz * LightDir[2]) / len);
            return 0.35 + 0.65 * dot;
        }

        static double[] Normalize(double[] v)
        {
            double len = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            return new double[] { v[0] / len, v[1] / len, v[2] / len };
        }
    }
}
