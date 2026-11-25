using System;
using System.Collections.Generic;
using System.Drawing;

namespace ThreeDApp2
{
    public enum SnapKind
    {
        None,      // Cursor does not hit the drawing plane
        Plane,     // Free point on the drawing plane
        Grid,      // Point snapped to the grid
        OnFace,    // Point on a face under the cursor
        Axis,      // Point on a line through the anchor parallel to a model axis
        OnEdge,    // Point on an existing edge
        Midpoint,  // Middle of an existing edge
        Endpoint,  // An existing vertex
    }

    public class SnapResult
    {
        public Point3D Point;
        public SnapKind Kind = SnapKind.None;
        public int AxisIndex = -1;       // 0 = X (red), 1 = Y (blue, vertical), 2 = Z (green) when Kind == Axis
        public int LayerIndex = -1;      // Shape the snap came from, for Endpoint / Midpoint / OnEdge
        public int VertexIndex = -1;     // Vertex for Endpoint, first vertex of the edge for Midpoint / OnEdge

        public bool HasPoint { get { return Point != null; } }

        public string Label
        {
            get
            {
                switch (Kind)
                {
                    case SnapKind.Endpoint: return "Endpoint";
                    case SnapKind.Midpoint: return "Midpoint";
                    case SnapKind.OnEdge: return "On Edge";
                    case SnapKind.OnFace: return "On Face";
                    case SnapKind.Grid: return "Grid";
                    // SketchUp colours: red = X, blue = vertical (Y here), green = depth (Z here).
                    case SnapKind.Axis: return AxisIndex == 0 ? "On Red Axis (X)" : AxisIndex == 1 ? "On Blue Axis (Y, vertical)" : "On Green Axis (Z)";
                    default: return "";
                }
            }
        }
    }

    /// <summary>
    /// SketchUp-style inference engine: turns a mouse position into a 3D point, snapping to
    /// existing vertices, edge midpoints, edges, axis directions from the previous point, and the grid.
    /// </summary>
    public class Inference
    {
        public float EndpointRadius = 10;
        public float MidpointRadius = 8;
        public float EdgeRadius = 6;
        public float AxisRadius = 10;
        public bool GridEnabled = true;
        public float GridSpacing = 1;

        static readonly Point3D[] Axes = { new Point3D(1, 0, 0), new Point3D(0, 1, 0), new Point3D(0, 0, 1) };

        /// <param name="scene">Scene to snap against.</param>
        /// <param name="cursor">Mouse position relative to the centre of the viewport.</param>
        /// <param name="plane">Drawing plane for free points.</param>
        /// <param name="anchor">Previous point (for axis inference), or null.</param>
        /// <param name="draft">Points of the shape being drawn (they can be snapped to as well), or null.</param>
        /// <param name="skipLayer">Return true for shapes that must not be snapped to, or null.</param>
        /// <param name="planeIsFace">The plane came from a face under the cursor (labels free points "On Face").</param>
        /// <param name="offPlaneAxes">Allow axis inference that leaves the plane (e.g. drawing straight up).</param>
        public SnapResult Snap(Scene scene, PointF cursor, Plane3D plane, Point3D anchor, IList<Point3D> draft,
                               Func<int, bool> skipLayer, bool planeIsFace, bool offPlaneAxes)
        {
            Point3D rayO, rayD;
            scene.GetModelRay(cursor, out rayO, out rayD);

            SnapResult best = SnapToVertices(scene, cursor, draft, skipLayer);
            if (best != null) return best;
            best = SnapToEdges(scene, cursor, rayO, rayD, skipLayer);
            if (best != null) return best;

            if (anchor != null)
            {
                best = SnapToAxes(scene, cursor, rayO, rayD, plane, anchor, offPlaneAxes);
                if (best != null) return best;
            }

            SnapResult r = new SnapResult();
            Point3D hit;
            if (plane == null || !plane.IntersectRay(rayO, rayD, out hit)) return r;
            r.Point = hit;
            r.Kind = planeIsFace ? SnapKind.OnFace : SnapKind.Plane;

            if (GridEnabled && !planeIsFace && GridSpacing > 0)
            {
                int axis = plane.AlignedAxis();
                if (axis >= 0)
                {
                    float x = hit.X, y = hit.Y, z = hit.Z;
                    if (axis != 0) x = Round(x);
                    if (axis != 1) y = Round(y);
                    if (axis != 2) z = Round(z);
                    r.Point = new Point3D(x, y, z);
                    r.Kind = SnapKind.Grid;
                }
            }
            return r;
        }

        float Round(float v)
        {
            return (float)(Math.Round(v / GridSpacing) * GridSpacing);
        }

        SnapResult SnapToVertices(Scene scene, PointF cursor, IList<Point3D> draft, Func<int, bool> skipLayer)
        {
            SnapResult best = null;
            float bestDist = EndpointRadius;
            for (int i = 0; i < scene.Layers.Count; i++)
            {
                MyPolygon shape = scene.Layers[i];
                if (shape.Hidden || (skipLayer != null && skipLayer(i))) continue;
                int count = Scene.VertexCount(shape);
                for (int j = 0; j < count; j++)
                {
                    float d = ScreenDistance(scene, shape.Absolute(j), cursor);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = new SnapResult { Point = shape.Absolute(j).Clone(), Kind = SnapKind.Endpoint, LayerIndex = i, VertexIndex = j };
                    }
                }
            }
            if (draft != null)
            {
                for (int j = 0; j < draft.Count; j++)
                {
                    float d = ScreenDistance(scene, draft[j], cursor);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = new SnapResult { Point = draft[j].Clone(), Kind = SnapKind.Endpoint, VertexIndex = j };
                    }
                }
            }
            if (best != null) return best;

            // Midpoints come second so a vertex always wins when both are close.
            bestDist = MidpointRadius;
            for (int i = 0; i < scene.Layers.Count; i++)
            {
                MyPolygon shape = scene.Layers[i];
                if (shape.Hidden || (skipLayer != null && skipLayer(i))) continue;
                for (int j = 0; j + 1 < shape.GetSize(); j++)
                {
                    Point3D mid = (shape.Absolute(j) + shape.Absolute(j + 1)) * 0.5f;
                    float d = ScreenDistance(scene, mid, cursor);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = new SnapResult { Point = mid, Kind = SnapKind.Midpoint, LayerIndex = i, VertexIndex = j };
                    }
                }
            }
            return best;
        }

        SnapResult SnapToEdges(Scene scene, PointF cursor, Point3D rayO, Point3D rayD, Func<int, bool> skipLayer)
        {
            SnapResult best = null;
            float bestDist = EdgeRadius;
            for (int i = 0; i < scene.Layers.Count; i++)
            {
                MyPolygon shape = scene.Layers[i];
                if (shape.Hidden || (skipLayer != null && skipLayer(i))) continue;
                for (int j = 0; j + 1 < shape.GetSize(); j++)
                {
                    Point3D a = shape.Absolute(j), b = shape.Absolute(j + 1);
                    Point3D onEdge = ClosestOnSegmentToRay(rayO, rayD, a, b);
                    if (onEdge == null) continue;
                    float d = ScreenDistance(scene, onEdge, cursor);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = new SnapResult { Point = onEdge, Kind = SnapKind.OnEdge, LayerIndex = i, VertexIndex = j };
                    }
                }
            }
            return best;
        }

        SnapResult SnapToAxes(Scene scene, PointF cursor, Point3D rayO, Point3D rayD, Plane3D plane, Point3D anchor, bool offPlaneAxes)
        {
            SnapResult best = null;
            float bestDist = AxisRadius;
            for (int k = 0; k < 3; k++)
            {
                Point3D axis = Axes[k];
                bool inPlane = plane == null || Math.Abs(Point3D.Dot(axis, plane.Normal)) < 1e-3f;
                if (!inPlane && !offPlaneAxes) continue;
                float t;
                if (!ClosestOnLineToRay(rayO, rayD, anchor, axis, out t)) continue;
                if (Math.Abs(t) < Sketch.Eps) continue;
                Point3D p = anchor + axis * t;
                if (GridEnabled && GridSpacing > 0)
                    p = anchor + axis * Round(t);
                float d = ScreenDistance(scene, anchor + axis * t, cursor);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = new SnapResult { Point = p, Kind = SnapKind.Axis, AxisIndex = k };
                }
            }
            return best;
        }

        static float ScreenDistance(Scene scene, Point3D model, PointF cursor)
        {
            PointF s;
            if (!scene.TryProjectModel(model, out s)) return float.MaxValue;
            float dx = s.X - cursor.X, dy = s.Y - cursor.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Parameter t of the point on line a + t*e closest to the ray o + s*d (s &gt; 0).</summary>
        public static bool ClosestOnLineToRay(Point3D o, Point3D d, Point3D a, Point3D e, out float t)
        {
            t = 0;
            Point3D w0 = o - a;
            float b = Point3D.Dot(d, e), dd = Point3D.Dot(d, d), ee = Point3D.Dot(e, e);
            float denom = dd * ee - b * b;
            if (Math.Abs(denom) < 1e-9f) return false;
            float dw = Point3D.Dot(d, w0), ew = Point3D.Dot(e, w0);
            float s = (b * ew - ee * dw) / denom;
            if (s <= 0) return false;
            t = (dd * ew - b * dw) / denom;
            return true;
        }

        public static Point3D ClosestOnSegmentToRay(Point3D o, Point3D d, Point3D a, Point3D b)
        {
            Point3D e = b - a;
            float t;
            if (!ClosestOnLineToRay(o, d, a, e, out t)) return null;
            t = Math.Max(0, Math.Min(1, t));
            return a + e * t;
        }
    }
}
