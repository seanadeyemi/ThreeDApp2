using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ThreeDApp2
{
    /// <summary>
    /// The navigation gizmo in the top-right corner of the viewport (like Blender's): drag it to orbit,
    /// click an axis ball to swing the view round to look along that axis.
    /// </summary>
    public partial class Form1
    {
        readonly OrbitGizmo mGizmo = new OrbitGizmo();
        bool mShowGizmo = true;
        bool mGizmoHover;                 // Mouse over the gizmo disc
        int mGizmoHoverId = -1;           // GizmoHandle.Id under the mouse
        bool mGizmoDragging;              // Left button went down on the gizmo
        bool mGizmoMoved;                 // ...and it has been dragged (so it's an orbit, not a click)
        Point mGizmoDownAt;
        Cursor mOrbitCursor = Cursors.SizeAll;

        // Smooth swing to a new view after clicking an axis.
        Timer mViewAnimTimer;
        double mAnimFromYaw, mAnimFromPitch, mAnimToYaw, mAnimToPitch;
        DateTime mAnimStart;
        const double ViewAnimMs = 240;

        private void PlaceGizmo()
        {
            mGizmo.Place(viewport.ClientSize, DeviceDpi / 96f);
        }

        /// <summary>Returns true if the gizmo takes this mouse press.</summary>
        private bool GizmoMouseDown(MouseEventArgs e)
        {
            if (!mShowGizmo || e.Button != MouseButtons.Left || !mGizmo.Contains(e.Location)) return false;
            mGizmoDragging = true;
            mGizmoMoved = false;
            mGizmoDownAt = e.Location;
            StopSpin();
            mViewAnimTimer.Enabled = false;
            return true;
        }

        /// <summary>Returns true if the gizmo handled this mouse move.</summary>
        private bool GizmoMouseMove(MouseEventArgs e, int dx, int dy)
        {
            if (mGizmoDragging)
            {
                if (!mGizmoMoved && (Math.Abs(e.X - mGizmoDownAt.X) > 3 || Math.Abs(e.Y - mGizmoDownAt.Y) > 3))
                    mGizmoMoved = true;
                if (mGizmoMoved) Orbit(dx, dy);
                UpdateGizmoHover(e.Location);
                return true;
            }
            bool wasOver = mGizmoHover;
            UpdateGizmoHover(e.Location);
            if (mGizmoHover != wasOver && !mOrbiting && !mPanning)
            {
                if (mGizmoHover) viewport.Cursor = mGizmoHoverId >= 0 ? Cursors.Hand : mOrbitCursor;
                else SetToolCursor();
            }
            else if (mGizmoHover)
                viewport.Cursor = mGizmoHoverId >= 0 ? Cursors.Hand : mOrbitCursor;
            return mGizmoHover;
        }

        /// <summary>Returns true if the gizmo handled this mouse release.</summary>
        private bool GizmoMouseUp(MouseEventArgs e)
        {
            if (!mGizmoDragging || e.Button != MouseButtons.Left) return false;
            mGizmoDragging = false;
            if (!mGizmoMoved)
            {
                GizmoHandle? hit = mGizmo.HitTest(mScene.Orientation, e.Location);
                if (hit.HasValue)
                {
                    double yaw, pitch;
                    OrbitGizmo.ViewFor(hit.Value, mScene.Yaw, out yaw, out pitch);
                    AnimateViewTo(yaw, pitch);
                }
            }
            UpdateGizmoHover(e.Location);
            if (!mGizmoHover) SetToolCursor();
            return true;
        }

        private void UpdateGizmoHover(Point p)
        {
            mGizmoHover = mShowGizmo && mGizmo.Contains(p);
            GizmoHandle? h = mGizmoHover ? mGizmo.HitTest(mScene.Orientation, p) : null;
            mGizmoHoverId = h.HasValue ? h.Value.Id : -1;
        }

        /// <summary>Swings the view to (yaw, pitch) over a quarter of a second, taking the short way round.</summary>
        private void AnimateViewTo(double yaw, double pitch)
        {
            StopSpin();
            mAnimFromYaw = mScene.Yaw;
            mAnimFromPitch = mScene.Pitch;
            double delta = ((yaw - mAnimFromYaw) % 360 + 540) % 360 - 180;
            mAnimToYaw = mAnimFromYaw + delta;
            mAnimToPitch = pitch;
            mAnimStart = DateTime.Now;
            mViewAnimTimer.Enabled = true;
        }

        private void ViewAnimTick()
        {
            double t = Math.Min(1, (DateTime.Now - mAnimStart).TotalMilliseconds / ViewAnimMs);
            double ease = t * t * (3 - 2 * t);
            mScene.SetOrbit(mAnimFromYaw + (mAnimToYaw - mAnimFromYaw) * ease,
                            mAnimFromPitch + (mAnimToPitch - mAnimFromPitch) * ease);
            if (t >= 1) mViewAnimTimer.Enabled = false;
            UpdateHover();
            viewport.Invalidate();
        }

        private void DrawGizmo(Graphics g)
        {
            if (!mShowGizmo) return;
            PlaceGizmo();
            PointF c = mGizmo.Center;
            float R = mGizmo.GrabRadius;

            // Soft disc behind it, stronger while the mouse is over it (like Blender).
            int discAlpha = mGizmoHover || mGizmoDragging ? 70 : 28;
            using (SolidBrush disc = new SolidBrush(Color.FromArgb(discAlpha, 255, 255, 255)))
                g.FillEllipse(disc, c.X - R, c.Y - R, 2 * R, 2 * R);
            if (mGizmoHover || mGizmoDragging)
                using (Pen ring = new Pen(Color.FromArgb(60, 0, 0, 0), 1f))
                    g.DrawEllipse(ring, c.X - R, c.Y - R, 2 * R, 2 * R);

            float br = mGizmo.BallRadius;
            using (Font font = new Font("Segoe UI", Math.Max(7f, br * 0.95f), FontStyle.Bold, GraphicsUnit.Pixel))
            using (StringFormat centre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                foreach (GizmoHandle h in mGizmo.Layout(mScene.Orientation))
                {
                    Color col = AxisColors[h.Axis];
                    PointF p = new PointF(c.X + h.Offset.X, c.Y + h.Offset.Y);
                    bool hot = h.Id == mGizmoHoverId;
                    // Farther ends are drawn a little paler so the depth reads at a glance.
                    float fade = 0.75f + 0.25f * (1 - (h.Depth + 1) / 2);
                    if (h.Positive)
                    {
                        using (Pen stem = new Pen(Color.FromArgb((int)(255 * fade), col), Math.Max(2f, br * 0.28f)))
                        {
                            stem.StartCap = stem.EndCap = LineCap.Round;
                            g.DrawLine(stem, c, p);
                        }
                        using (SolidBrush ball = new SolidBrush(hot ? Lighten(col, 0.35f) : col))
                            g.FillEllipse(ball, p.X - br, p.Y - br, 2 * br, 2 * br);
                        g.DrawString(OrbitGizmo.Label(h.Axis), font, hot ? Brushes.White : Brushes.Black, p, centre);
                    }
                    else
                    {
                        using (SolidBrush ball = new SolidBrush(Color.FromArgb(hot ? 230 : 110, col)))
                            g.FillEllipse(ball, p.X - br, p.Y - br, 2 * br, 2 * br);
                        using (Pen rim = new Pen(col, 1.4f))
                            g.DrawEllipse(rim, p.X - br, p.Y - br, 2 * br, 2 * br);
                        if (hot)
                            g.DrawString("-" + OrbitGizmo.Label(h.Axis), font, Brushes.White, p, centre);
                    }
                }
            }
        }

        private static Color Lighten(Color c, float amount)
        {
            return Color.FromArgb(c.A, c.R + (int)((255 - c.R) * amount), c.G + (int)((255 - c.G) * amount), c.B + (int)((255 - c.B) * amount));
        }
    }
}
