using System;
using System.Collections.Generic;
using System.Drawing;

namespace ThreeDApp2
{
    public enum TransformMode { Move, Rotate, Scale }

    /// <summary>Which part of the on-screen transform handles is under the mouse or being dragged.</summary>
    public enum HandlePart
    {
        None,
        Axis,      // Move / scale along one axis (arrow or box on a stick)
        Plane,     // Move in the plane facing Axis (small square between two arrows)
        Center,    // Move freely (with snapping) or scale evenly
        Ring,      // Rotate about Axis
    }

    public struct HandleHit
    {
        public HandlePart Part;
        public int Axis;     // 0 = X (red), 1 = Y (blue, vertical), 2 = Z (green); for Plane, the axis it faces

        public HandleHit(HandlePart part, int axis) { Part = part; Axis = axis; }
        public static readonly HandleHit None = new HandleHit(HandlePart.None, -1);
        public bool IsNone { get { return Part == HandlePart.None; } }
        public bool Same(HandleHit o) { return Part == o.Part && (Part == HandlePart.Center || Part == HandlePart.None || Axis == o.Axis); }
    }

    /// <summary>A move, rotation or scale about a centre, as produced by dragging or typing.</summary>
    public class TransformValue
    {
        public TransformMode Mode;
        public Point3D Center = new Point3D(0, 0, 0);
        public Point3D Offset = new Point3D(0, 0, 0);       // Move
        public Point3D Axis = new Point3D(0, 1, 0);         // Rotate: unit axis
        public double AngleDegrees;                         // Rotate
        public Point3D Factors = new Point3D(1, 1, 1);      // Scale, per axis

        public static TransformValue Move(Point3D offset) { return new TransformValue { Mode = TransformMode.Move, Offset = offset.Clone() }; }
        public static TransformValue Rotate(Point3D center, Point3D axis, double degrees)
        {
            return new TransformValue { Mode = TransformMode.Rotate, Center = center.Clone(), Axis = axis.Normalized(), AngleDegrees = degrees };
        }
        public static TransformValue Scale(Point3D center, Point3D factors)
        {
            return new TransformValue { Mode = TransformMode.Scale, Center = center.Clone(), Factors = factors.Clone() };
        }

        public bool IsIdentity
        {
            get
            {
                switch (Mode)
                {
                    case TransformMode.Move: return Offset.Length() < 1e-6f;
                    case TransformMode.Rotate: return Math.Abs(AngleDegrees) < 1e-6;
                    default: return Math.Abs(Factors.X - 1) < 1e-6f && Math.Abs(Factors.Y - 1) < 1e-6f && Math.Abs(Factors.Z - 1) < 1e-6f;
                }
            }
        }

        /// <summary>Where the model point p ends up.</summary>
        public Point3D Apply(Point3D p)
        {
            switch (Mode)
            {
                case TransformMode.Move:
                    return p + Offset;
                case TransformMode.Rotate:
                    {
                        // Rodrigues' rotation formula about Axis through Center.
                        double a = AngleDegrees * Math.PI / 180, c = Math.Cos(a), s = Math.Sin(a);
                        double vx = p.X - Center.X, vy = p.Y - Center.Y, vz = p.Z - Center.Z;
                        double kx = Axis.X, ky = Axis.Y, kz = Axis.Z;
                        double dot = kx * vx + ky * vy + kz * vz;
                        double cx = ky * vz - kz * vy, cy = kz * vx - kx * vz, cz = kx * vy - ky * vx;
                        return new Point3D(Center.X + vx * c + cx * s + kx * dot * (1 - c),
                                           Center.Y + vy * c + cy * s + ky * dot * (1 - c),
                                           Center.Z + vz * c + cz * s + kz * dot * (1 - c));
                    }
                default:
                    return new Point3D(Center.X + (p.X - Center.X) * Factors.X,
                                       Center.Y + (p.Y - Center.Y) * Factors.Y,
                                       Center.Z + (p.Z - Center.Z) * Factors.Z);
            }
        }

        /// <summary>Short description for the status bar, e.g. "Angle 45°".</summary>
        public string Describe()
        {
            switch (Mode)
            {
                case TransformMode.Move:
                    return string.Format("Distance {0:0.##}  ({1:0.##}, {2:0.##}, {3:0.##})", Offset.Length(), Offset.X, Offset.Y, Offset.Z);
                case TransformMode.Rotate:
                    return string.Format("Angle {0:0.#}°", AngleDegrees);
                default:
                    if (Math.Abs(Factors.X - Factors.Y) < 1e-6f && Math.Abs(Factors.Y - Factors.Z) < 1e-6f)
                        return string.Format("Scale {0:0.###}", Factors.X);
                    return string.Format("Scale {0:0.###}, {1:0.###}, {2:0.###}", Factors.X, Factors.Y, Factors.Z);
            }
        }
    }

    /// <summary>
    /// Move / rotate / scale handles like other 3D apps: three coloured arrows, rings or boxes on sticks
    /// around the selection's centre. They keep a constant size on screen. Hit testing and the drag
    /// maths live here so they can be tested without a window.
    /// </summary>
    public class TransformHandles
    {
        public TransformMode Mode = TransformMode.Move;
        public Point3D Center = new Point3D(0, 0, 0);
        public float PixelSize = 90;           // Length of an arrow on screen
        public float PickRadius = 7;

        // Drag snapping
        public float MoveStep = 1;             // Model units (0 = no snapping)
        public double AngleStep = 15;          // Degrees
        public float ScaleStep = 0.1f;         // Factor increments

        public static readonly Point3D[] Axes = { new Point3D(1, 0, 0), new Point3D(0, 1, 0), new Point3D(0, 0, 1) };

        /// <summary>Model length that shows as <see cref="PixelSize"/> pixels at the centre, or 0 if the centre is behind the camera.</summary>
        public float ModelSize(Scene scene)
        {
            if (scene.Orthographic) return PixelSize / Math.Max(1e-6f, scene.OrthoScale);
            float depth = scene.ToWorld(Center).Z - scene.Camera.Z;
            if (depth < scene.NearPlane) return 0;
            return PixelSize * depth / scene.FocalLength;
        }

        /// <summary>Screen end point of axis k's handle (and whether the axis is long enough on screen to use).</summary>
        public bool AxisEnd(Scene scene, int k, out PointF center, out PointF end)
        {
            center = end = PointF.Empty;
            float size = ModelSize(scene);
            if (size <= 0) return false;
            if (!scene.TryProjectModel(Center, out center)) return false;
            if (!scene.TryProjectModel(Center + Axes[k] * size, out end)) return false;
            return Dist(center, end) > PixelSize * 0.2f;   // Pointing at the viewer: too short to drag
        }

        /// <summary>Screen corners of the square that moves in the plane facing axis k.</summary>
        public PointF[] PlaneSquare(Scene scene, int k)
        {
            float size = ModelSize(scene);
            if (size <= 0) return null;
            Point3D u = Axes[(k + 1) % 3] * size, v = Axes[(k + 2) % 3] * size;
            Point3D[] corners = { Center + u * 0.25f + v * 0.25f, Center + u * 0.45f + v * 0.25f, Center + u * 0.45f + v * 0.45f, Center + u * 0.25f + v * 0.45f };
            PointF[] s = new PointF[4];
            for (int i = 0; i < 4; i++) if (!scene.TryProjectModel(corners[i], out s[i])) return null;
            // Seen edge-on the square is a sliver; dragging it would fling the shape about.
            float area = Math.Abs((s[1].X - s[0].X) * (s[3].Y - s[0].Y) - (s[3].X - s[0].X) * (s[1].Y - s[0].Y));
            float full = (0.2f * PixelSize) * (0.2f * PixelSize);
            return area > full * 0.15f ? s : null;
        }

        /// <summary>Screen polyline of the rotation ring about axis k (radius = handle size).</summary>
        public PointF[] Ring(Scene scene, int k, int segments = 64)
        {
            float size = ModelSize(scene);
            if (size <= 0) return null;
            Point3D u = Axes[(k + 1) % 3], v = Axes[(k + 2) % 3];
            PointF[] pts = new PointF[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                double a = 2 * Math.PI * i / segments;
                Point3D p = Center + u * (float)(size * Math.Cos(a)) + v * (float)(size * Math.Sin(a));
                if (!scene.TryProjectModel(p, out pts[i])) return null;
            }
            return pts;
        }

        /// <summary>What part of the handles is under the cursor (screen position relative to the viewport centre).</summary>
        public HandleHit HitTest(Scene scene, PointF cursor)
        {
            PointF c;
            if (ModelSize(scene) <= 0 || !scene.TryProjectModel(Center, out c)) return HandleHit.None;

            if (Mode == TransformMode.Rotate)
            {
                HandleHit best = HandleHit.None;
                float bestD = PickRadius;
                for (int k = 0; k < 3; k++)
                {
                    PointF[] ring = Ring(scene, k);
                    if (ring == null) continue;
                    float d = Scene.DistanceToPolyline(ring, cursor, false);
                    if (d < bestD) { bestD = d; best = new HandleHit(HandlePart.Ring, k); }
                }
                return best;
            }

            // Centre: free move, or even scale.
            float centerRadius = Mode == TransformMode.Scale ? 8 : 9;
            if (Math.Abs(cursor.X - c.X) <= centerRadius && Math.Abs(cursor.Y - c.Y) <= centerRadius)
                return new HandleHit(HandlePart.Center, -1);

            if (Mode == TransformMode.Move)
                for (int k = 0; k < 3; k++)
                {
                    PointF[] sq = PlaneSquare(scene, k);
                    if (sq != null && Scene.PointInPolygon(sq, cursor)) return new HandleHit(HandlePart.Plane, k);
                }

            HandleHit axis = HandleHit.None;
            float axisD = PickRadius + 1;
            for (int k = 0; k < 3; k++)
            {
                PointF a, b;
                if (!AxisEnd(scene, k, out a, out b)) continue;
                // The stick from a little way out to the tip, plus the arrowhead / box at the end.
                PointF from = new PointF(a.X + (b.X - a.X) * 0.15f, a.Y + (b.Y - a.Y) * 0.15f);
                float d = Math.Min(Scene.DistanceToSegment(cursor, from, b), Dist(cursor, b) - 3);
                if (d < axisD) { axisD = d; axis = new HandleHit(HandlePart.Axis, k); }
            }
            return axis;
        }

        #region Dragging

        HandleHit mDrag = HandleHit.None;
        float mStartT;                 // Axis parameter where the drag started
        Point3D mStartHit;             // Plane / ring hit where the drag started
        PointF mStartCursor;
        double mPrevRaw, mAngle;       // Rotation: last raw angle and the unwrapped total
        PointF mScreenDir;             // Rotation seen edge-on: screen direction that turns the ring

        public HandleHit Dragging { get { return mDrag; } }

        /// <summary>Starts dragging the given handle. Returns false if it can't be dragged from this view.</summary>
        public bool BeginDrag(Scene scene, PointF cursor, HandleHit hit)
        {
            mDrag = HandleHit.None;
            mStartCursor = cursor;
            mAngle = mPrevRaw = 0;
            Point3D o, d;
            scene.GetModelRay(cursor, out o, out d);
            switch (hit.Part)
            {
                case HandlePart.Axis:
                    if (!Inference.ClosestOnLineToRay(o, d, Center, Axes[hit.Axis], out mStartT)) return false;
                    if (Mode == TransformMode.Scale && Math.Abs(mStartT) < 1e-4f) return false;
                    break;
                case HandlePart.Plane:
                    if (!new Plane3D(Center, Axes[hit.Axis]).IntersectRay(o, d, out mStartHit)) return false;
                    break;
                case HandlePart.Ring:
                    {
                        PointF c;
                        if (!scene.TryProjectModel(Center, out c)) return false;
                        if (!RingAngle(scene, o, d, hit.Axis, out mPrevRaw))
                        {
                            // Ring seen edge-on: turn it by dragging across the line it makes on screen.
                            PointF a, b;
                            AxisEndLoose(scene, hit.Axis, out a, out b);
                            float ax = b.X - a.X, ay = b.Y - a.Y, len = (float)Math.Sqrt(ax * ax + ay * ay);
                            mScreenDir = len > 1e-3f ? new PointF(-ay / len, ax / len) : new PointF(1, 0);
                            mPrevRaw = double.NaN;
                        }
                    }
                    break;
                case HandlePart.Center:
                    if (Mode == TransformMode.Scale)
                    {
                        PointF c;
                        if (!scene.TryProjectModel(Center, out c) || Dist(c, cursor) < 2) return false;
                    }
                    break;
                default:
                    return false;
            }
            mDrag = hit;
            return true;
        }

        public void EndDrag() { mDrag = HandleHit.None; }

        /// <summary>
        /// The transform for the cursor's current position during a drag. Steps snap the result
        /// (pass fine = true, e.g. while Shift is held, for 1° and 0.01 steps and no move snapping).
        /// Moving by the centre handle is done with the snapping engine instead (see Form1.Transform).
        /// </summary>
        public TransformValue DragTo(Scene scene, PointF cursor, bool fine)
        {
            Point3D o, d;
            scene.GetModelRay(cursor, out o, out d);
            switch (mDrag.Part)
            {
                case HandlePart.Axis:
                    {
                        float t;
                        if (!Inference.ClosestOnLineToRay(o, d, Center, Axes[mDrag.Axis], out t)) t = mStartT;
                        if (Mode == TransformMode.Move)
                        {
                            float delta = SnapStep(t - mStartT, fine ? 0 : MoveStep);
                            return TransformValue.Move(Axes[mDrag.Axis] * delta);
                        }
                        float f = SnapFactor(t / mStartT, fine);
                        Point3D factors = new Point3D(1, 1, 1);
                        if (mDrag.Axis == 0) factors.X = f; else if (mDrag.Axis == 1) factors.Y = f; else factors.Z = f;
                        return TransformValue.Scale(Center, factors);
                    }
                case HandlePart.Plane:
                    {
                        Point3D hit;
                        if (!new Plane3D(Center, Axes[mDrag.Axis]).IntersectRay(o, d, out hit)) hit = mStartHit;
                        Point3D off = hit - mStartHit;
                        float step = fine ? 0 : MoveStep;
                        off = new Point3D(SnapStep(off.X, step), SnapStep(off.Y, step), SnapStep(off.Z, step));
                        if (mDrag.Axis == 0) off.X = 0; else if (mDrag.Axis == 1) off.Y = 0; else off.Z = 0;
                        return TransformValue.Move(off);
                    }
                case HandlePart.Ring:
                    {
                        double raw;
                        if (!double.IsNaN(mPrevRaw) && RingAngle(scene, o, d, mDrag.Axis, out raw))
                        {
                            double step = raw - mPrevRaw;
                            while (step > Math.PI) step -= 2 * Math.PI;
                            while (step < -Math.PI) step += 2 * Math.PI;
                            mAngle += step;
                            mPrevRaw = raw;
                        }
                        else if (double.IsNaN(mPrevRaw))
                        {
                            float along = (cursor.X - mStartCursor.X) * mScreenDir.X + (cursor.Y - mStartCursor.Y) * mScreenDir.Y;
                            mAngle = along / PixelSize * 1.5;   // About 90° for a drag of one arrow length
                        }
                        double deg = mAngle * 180 / Math.PI;
                        double inc = fine ? 1 : AngleStep;
                        if (inc > 0) deg = Math.Round(deg / inc) * inc;
                        return TransformValue.Rotate(Center, Axes[mDrag.Axis], deg);
                    }
                case HandlePart.Center:
                    if (Mode == TransformMode.Scale)
                    {
                        PointF c;
                        scene.TryProjectModel(Center, out c);
                        float f = SnapFactor(Dist(c, cursor) / Math.Max(1e-3f, Dist(c, mStartCursor)), fine);
                        return TransformValue.Scale(Center, new Point3D(f, f, f));
                    }
                    break;
            }
            return null;
        }

        /// <summary>Angle of the cursor's ray hit around ring k, or false if the ring is seen almost edge-on.</summary>
        bool RingAngle(Scene scene, Point3D o, Point3D d, int k, out double angle)
        {
            angle = 0;
            Point3D n = Axes[k];
            if (Math.Abs(Point3D.Dot(d.Normalized(), n)) < 0.12f) return false;
            Point3D hit;
            if (!new Plane3D(Center, n).IntersectRay(o, d, out hit)) return false;
            Point3D v = hit - Center, u = Axes[(k + 1) % 3], w = Axes[(k + 2) % 3];
            float x = Point3D.Dot(v, u), y = Point3D.Dot(v, w);
            if (x * x + y * y < 1e-10f) return false;
            angle = Math.Atan2(y, x);   // u x w = axis, so this angle turns the same way as Rodrigues' formula
            return true;
        }

        void AxisEndLoose(Scene scene, int k, out PointF a, out PointF b)
        {
            a = b = PointF.Empty;
            float size = Math.Max(1e-3f, ModelSize(scene));
            scene.TryProjectModel(Center, out a);
            scene.TryProjectModel(Center + Axes[k] * size, out b);
        }

        float SnapFactor(float f, bool fine)
        {
            if (float.IsNaN(f) || float.IsInfinity(f)) f = 1;
            float step = fine ? 0.01f : ScaleStep;
            if (step > 0) f = (float)(Math.Round(f / step) * step);
            if (Math.Abs(f) < 0.01f) f = f < 0 ? -0.01f : 0.01f;   // Never squash flat
            return f;
        }

        static float SnapStep(float v, float step)
        {
            return step > 0 ? (float)(Math.Round(v / step) * step) : v;
        }

        #endregion

        static float Dist(PointF a, PointF b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Centre of the bounding box of the given shapes (where the handles sit).</summary>
        public static Point3D BoundsCenter(IList<MyPolygon> layers, IEnumerable<int> indices)
        {
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
            bool any = false;
            foreach (int i in indices)
            {
                MyPolygon s = layers[i];
                for (int j = 0; j < s.GetSize(); j++)
                {
                    Point3D p = s.Absolute(j);
                    minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
                    minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
                    minZ = Math.Min(minZ, p.Z); maxZ = Math.Max(maxZ, p.Z);
                    any = true;
                }
            }
            return any ? new Point3D((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2) : new Point3D(0, 0, 0);
        }

        /// <summary>Copies of the raw points of each shape, so a drag can be re-applied from the start each time.</summary>
        public static List<Point3D[]> Snapshot(IList<MyPolygon> layers, IList<int> indices)
        {
            List<Point3D[]> result = new List<Point3D[]>(indices.Count);
            foreach (int i in indices)
            {
                List<Point3D> raw = layers[i].RawPoints;
                Point3D[] copy = new Point3D[raw.Count];
                for (int j = 0; j < raw.Count; j++) copy[j] = raw[j].Clone();
                result.Add(copy);
            }
            return result;
        }

        /// <summary>Sets every point of the shapes to the transform of its snapshot position.</summary>
        public static void ApplyToShapes(IList<MyPolygon> layers, IList<int> indices, List<Point3D[]> original, TransformValue value)
        {
            for (int n = 0; n < indices.Count; n++)
            {
                MyPolygon s = layers[indices[n]];
                List<Point3D> raw = s.RawPoints;
                Point3D[] orig = original[n];
                Point3D pos = s.mPosition;
                for (int j = 0; j < raw.Count && j < orig.Length; j++)
                {
                    Point3D moved = value == null ? orig[j] + pos : value.Apply(orig[j] + pos);
                    raw[j].X = moved.X - pos.X;
                    raw[j].Y = moved.Y - pos.Y;
                    raw[j].Z = moved.Z - pos.Z;
                }
            }
        }
    }
}
