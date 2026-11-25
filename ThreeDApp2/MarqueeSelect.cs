using System;
using System.Collections.Generic;
using System.Drawing;

namespace ThreeDApp2
{
    /// <summary>
    /// Box selection like SketchUp: dragging left to right selects shapes that are entirely inside
    /// the box ("window"); dragging right to left also selects shapes the box only touches ("crossing").
    /// Works on the projected shapes of a frame, so it needs no window to test.
    /// </summary>
    public static class MarqueeSelect
    {
        /// <summary>The box between two corners, whichever way it was dragged.</summary>
        public static RectangleF Box(PointF a, PointF b)
        {
            return RectangleF.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        /// <summary>Right-to-left drags select what they touch.</summary>
        public static bool IsCrossing(PointF start, PointF end)
        {
            return end.X < start.X;
        }

        /// <summary>Layer indices of the shapes the box selects.</summary>
        public static List<int> Pick(IList<ShapeScreenPoints> frame, RectangleF box, bool crossing)
        {
            List<int> result = new List<int>();
            foreach (ShapeScreenPoints s in frame)
            {
                if (s.Pts.Length == 0) continue;
                RectangleF b = s.Bounds;
                // Quick rejection on the screen bounds (a single point has an empty box, so test by edges).
                if (b.Right < box.Left || b.Left > box.Right || b.Bottom < box.Top || b.Top > box.Bottom) continue;
                if (crossing ? Touches(s, box) : Inside(s, box)) result.Add(s.LayerIndex);
            }
            return result;
        }

        static bool Contains(RectangleF r, PointF p)
        {
            return p.X >= r.Left && p.X <= r.Right && p.Y >= r.Top && p.Y <= r.Bottom;
        }

        static bool Inside(ShapeScreenPoints s, RectangleF box)
        {
            foreach (PointF p in s.Pts)
                if (float.IsNaN(p.X) || !Contains(box, p)) return false;
            return true;
        }

        static bool Touches(ShapeScreenPoints s, RectangleF box)
        {
            PointF[] pts = s.Pts;
            foreach (PointF p in pts)
                if (!float.IsNaN(p.X) && Contains(box, p)) return true;
            int n = pts.Length;
            int segs = s.Closed ? n : n - 1;
            for (int i = 0; i < segs; i++)
            {
                PointF a = pts[i], b = pts[(i + 1) % n];
                if (float.IsNaN(a.X) || float.IsNaN(b.X)) continue;
                if (SegmentHitsBox(a, b, box)) return true;
            }
            // A box drawn entirely inside a big face.
            return s.Closed && n >= 3 && Scene.PointInPolygon(pts, new PointF(box.X + box.Width / 2, box.Y + box.Height / 2));
        }

        /// <summary>Liang-Barsky: does the segment a-b pass through the box?</summary>
        public static bool SegmentHitsBox(PointF a, PointF b, RectangleF box)
        {
            float t0 = 0, t1 = 1, dx = b.X - a.X, dy = b.Y - a.Y;
            float[] p = { -dx, dx, -dy, dy };
            float[] q = { a.X - box.Left, box.Right - a.X, a.Y - box.Top, box.Bottom - a.Y };
            for (int i = 0; i < 4; i++)
            {
                if (p[i] == 0)
                {
                    if (q[i] < 0) return false;
                    continue;
                }
                float t = q[i] / p[i];
                if (p[i] < 0) { if (t > t1) return false; if (t > t0) t0 = t; }
                else { if (t < t0) return false; if (t < t1) t1 = t; }
            }
            return true;
        }
    }
}
