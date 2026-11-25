using System;
using System.Collections.Generic;
using System.Drawing;

namespace ThreeDApp2
{
    /// <summary>
    /// Geometry helpers for the drawing tools: building rectangles, circles and push/pull
    /// extrusions, and joining / continuing polylines. No UI code, so it can be tested on its own.
    /// </summary>
    public static class Sketch
    {
        public const float Eps = 1e-3f;

        public static bool Same(Point3D a, Point3D b)
        {
            return a != null && b != null && Point3D.Distance(a, b) <= Eps;
        }

        /// <summary>The distinct vertices of a shape (closing vertex left out), as copies.</summary>
        public static List<Point3D> Vertices(MyPolygon shape)
        {
            List<Point3D> pts = new List<Point3D>();
            int count = Scene.VertexCount(shape);
            for (int j = 0; j < count; j++) pts.Add(shape.Absolute(j).Clone());
            return pts;
        }

        public static Point3D Centroid(IList<Point3D> pts)
        {
            double x = 0, y = 0, z = 0;
            foreach (Point3D p in pts) { x += p.X; y += p.Y; z += p.Z; }
            int n = Math.Max(1, pts.Count);
            return new Point3D(x / n, y / n, z / n);
        }

        /// <summary>Builds a shape from points. A closed shape gets a closing vertex and is drawn filled.</summary>
        public static MyPolygon MakeShape(IList<Point3D> pts, bool closed, string name, Color color, int group)
        {
            MyPolygon shape = new MyPolygon();
            foreach (Point3D p in pts) shape.Add(p.Clone());
            // Drop a repeated last point so Close() adds exactly one closing vertex.
            if (shape.GetSize() > 1 && Same(shape[0], shape[shape.GetSize() - 1])) shape.RemoveAt(shape.GetSize() - 1);
            if (closed && shape.GetSize() >= 3) shape.Close();
            shape.Name = name;
            shape.Color = color;
            shape.GroupId = group;
            return shape;
        }

        /// <summary>Axis-aligned (in the plane's U/V axes) rectangle with opposite corners p0 and p1.</summary>
        public static List<Point3D> Rectangle(Plane3D plane, Point3D p0, Point3D p1)
        {
            p0 = plane.ProjectPoint(p0);
            p1 = plane.ProjectPoint(p1);
            Point3D d = p1 - p0;
            float a = Point3D.Dot(d, plane.U);
            float b = Point3D.Dot(d, plane.V);
            if (Math.Abs(a) < Eps || Math.Abs(b) < Eps) return null;
            return RectangleFromSize(plane, p0, a, b);
        }

        public static List<Point3D> RectangleFromSize(Plane3D plane, Point3D p0, float a, float b)
        {
            p0 = plane.ProjectPoint(p0);
            return new List<Point3D>
            {
                p0.Clone(),
                p0 + plane.U * a,
                p0 + plane.U * a + plane.V * b,
                p0 + plane.V * b,
            };
        }

        /// <summary>Regular polygon approximating a circle, starting in the direction of <paramref name="towards"/>.</summary>
        public static List<Point3D> Circle(Plane3D plane, Point3D center, float radius, int segments, Point3D towards = null)
        {
            if (radius < Eps || segments < 3) return null;
            center = plane.ProjectPoint(center);
            Point3D u = plane.U, v = plane.V;
            if (towards != null)
            {
                Point3D dir = plane.ProjectPoint(towards) - center;
                if (dir.Length() > Eps)
                {
                    u = dir.Normalized();
                    v = Point3D.Cross(plane.Normal, u).Normalized();
                }
            }
            List<Point3D> pts = new List<Point3D>();
            for (int i = 0; i < segments; i++)
            {
                double ang = 2 * Math.PI * i / segments;
                pts.Add(center + u * (float)(radius * Math.Cos(ang)) + v * (float)(radius * Math.Sin(ang)));
            }
            return pts;
        }

        /// <summary>
        /// Push/pull: extrudes a closed face along its normal by <paramref name="distance"/>.
        /// Returns the new side faces followed by the top cap (the original face stays as the bottom).
        /// </summary>
        public static List<MyPolygon> Extrude(MyPolygon face, float distance, Color color, int group)
        {
            List<MyPolygon> result = new List<MyPolygon>();
            List<Point3D> bottom = Vertices(face);
            Plane3D plane = Plane3D.FromPoints(bottom);
            if (plane == null || Math.Abs(distance) < Eps) return result;
            Point3D offset = plane.Normal * distance;
            List<Point3D> top = new List<Point3D>();
            foreach (Point3D p in bottom) top.Add(p + offset);

            for (int i = 0; i < bottom.Count; i++)
            {
                int j = (i + 1) % bottom.Count;
                result.Add(MakeShape(new[] { bottom[i], bottom[j], top[j], top[i] }, true, face.Name + " side " + (i + 1), color, group));
            }
            result.Add(MakeShape(top, true, face.Name + " top", color, group));
            return result;
        }

        /// <summary>
        /// Finds an open polyline (not a filled face) that has <paramref name="p"/> as its first or last
        /// vertex. Returns its index, or -1.
        /// </summary>
        public static int FindOpenEndpoint(List<MyPolygon> layers, Point3D p, int excludeIndex, out bool atStart)
        {
            atStart = false;
            for (int i = 0; i < layers.Count; i++)
            {
                if (i == excludeIndex || layers[i].Hidden) continue;
                MyPolygon shape = layers[i];
                if (shape.IsClosedFace || shape.GetSize() < 2) continue;
                if (Same(shape.Absolute(0), p)) { atStart = true; return i; }
                if (Same(shape.Absolute(shape.GetSize() - 1), p)) { atStart = false; return i; }
            }
            return -1;
        }

        /// <summary>
        /// Points of an open polyline ordered so that the endpoint we connect to comes last
        /// (for continuing it) or first (for joining onto it).
        /// </summary>
        public static List<Point3D> OrientedPoints(MyPolygon shape, bool endpointIsStart, bool endpointLast)
        {
            List<Point3D> pts = Vertices(shape);
            bool reverse = endpointLast ? endpointIsStart : !endpointIsStart;
            if (reverse) pts.Reverse();
            return pts;
        }

        /// <summary>
        /// Appends <paramref name="tail"/> to <paramref name="draft"/>, skipping the shared joining point.
        /// Returns true if the result closes on itself (last point == first point, 3+ distinct points).
        /// </summary>
        public static bool Join(List<Point3D> draft, List<Point3D> tail)
        {
            int start = (draft.Count > 0 && tail.Count > 0 && Same(draft[draft.Count - 1], tail[0])) ? 1 : 0;
            for (int i = start; i < tail.Count; i++) draft.Add(tail[i].Clone());
            return ClosesOnItself(draft);
        }

        public static bool ClosesOnItself(List<Point3D> pts)
        {
            return pts.Count >= 4 && Same(pts[0], pts[pts.Count - 1]);
        }
    }
}
