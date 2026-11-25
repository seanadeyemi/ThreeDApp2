using System;
using System.Collections.Generic;
using System.Drawing;

namespace ThreeDApp2
{
    /// <summary>One axis end on the navigation gizmo.</summary>
    public struct GizmoHandle
    {
        public int Axis;          // 0 = X, 1 = Y, 2 = Z
        public bool Positive;     // The +axis end (labelled) or the -axis end
        public PointF Offset;     // Screen offset from the gizmo centre, in pixels (y down)
        public float Depth;       // Larger = farther from the viewer

        public int Id { get { return Axis * 2 + (Positive ? 0 : 1); } }
    }

    /// <summary>
    /// Blender-style navigation gizmo: the three model axes drawn from a small ball in the corner of
    /// the viewport, turning with the view. Dragging it orbits; clicking an axis end snaps the view to
    /// look along that axis. This class only does the geometry, so it can be tested without a window.
    /// </summary>
    public class OrbitGizmo
    {
        public PointF Center;     // In viewport pixels
        public float Radius = 46; // Centre to an axis ball
        public float BallRadius = 9;

        /// <summary>Area that grabs the mouse (the disc behind the gizmo).</summary>
        public float GrabRadius { get { return Radius + BallRadius + 3; } }

        /// <summary>Places the gizmo in the top-right corner of a viewport of the given size.</summary>
        public void Place(Size viewport, float scale)
        {
            Radius = 46 * scale;
            BallRadius = 9 * scale;
            float margin = 14 * scale;
            Center = new PointF(viewport.Width - margin - GrabRadius, margin + GrabRadius);
        }

        /// <summary>The six axis ends for a view orientation, sorted farthest first (draw order).</summary>
        public List<GizmoHandle> Layout(Matrix3D orientation)
        {
            List<GizmoHandle> handles = new List<GizmoHandle>();
            for (int axis = 0; axis < 3; axis++)
            {
                Point3D dir = new Point3D(axis == 0 ? 1 : 0, axis == 1 ? 1 : 0, axis == 2 ? 1 : 0);
                Point3D v = orientation.Transform(dir);
                for (int s = 0; s < 2; s++)
                {
                    float sign = s == 0 ? 1 : -1;
                    handles.Add(new GizmoHandle
                    {
                        Axis = axis,
                        Positive = s == 0,
                        Offset = new PointF(sign * v.X * Radius, -sign * v.Y * Radius),
                        Depth = sign * v.Z,
                    });
                }
            }
            handles.Sort((a, b) => b.Depth.CompareTo(a.Depth));
            return handles;
        }

        public bool Contains(PointF p)
        {
            float dx = p.X - Center.X, dy = p.Y - Center.Y;
            return dx * dx + dy * dy <= GrabRadius * GrabRadius;
        }

        /// <summary>The axis end under the point (nearest the viewer wins), or null.</summary>
        public GizmoHandle? HitTest(Matrix3D orientation, PointF p)
        {
            List<GizmoHandle> handles = Layout(orientation);
            for (int i = handles.Count - 1; i >= 0; i--)   // Nearest first
            {
                GizmoHandle h = handles[i];
                float dx = p.X - (Center.X + h.Offset.X), dy = p.Y - (Center.Y + h.Offset.Y);
                if (dx * dx + dy * dy <= (BallRadius + 2) * (BallRadius + 2)) return h;
            }
            return null;
        }

        /// <summary>
        /// The turn and tilt that look straight down an axis end at the viewer, i.e. that axis then points
        /// out of the screen. For the vertical axis the current turn is kept (rounded to a quarter turn) so
        /// top/bottom views stay lined up with the model.
        /// </summary>
        public static void ViewFor(GizmoHandle h, double currentYaw, out double yaw, out double pitch)
        {
            switch (h.Axis)
            {
                case 0: yaw = h.Positive ? 90 : -90; pitch = 0; break;      // +X: right view, -X: left view
                case 2: yaw = h.Positive ? 180 : 0; pitch = 0; break;       // +Z: back, -Z: front
                default:
                    yaw = Math.Round(currentYaw / 90) * 90;
                    pitch = h.Positive ? 90 : -90;                          // +Y: top, -Y: bottom
                    break;
            }
        }

        public static string Label(int axis) { return axis == 0 ? "X" : axis == 1 ? "Y" : "Z"; }
    }
}
