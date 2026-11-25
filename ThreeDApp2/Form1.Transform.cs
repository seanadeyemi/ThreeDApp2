using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ThreeDApp2
{
    /// <summary>
    /// Move, Rotate and Scale with on-screen handles (arrows, rings and boxes) around the selected
    /// shape, like other 3D apps. Drags snap (grid, 15°, 0.1 steps; hold Shift for fine control),
    /// Ctrl+drag works on a copy, and typing a value afterwards sets it exactly.
    /// </summary>
    public partial class Form1
    {
        readonly TransformHandles mHandles = new TransformHandles();
        HandleHit mHandleHover = HandleHit.None;

        // Handle drag in progress
        bool mXformDragging;
        List<int> mXformIndices;
        List<Point3D[]> mXformOriginal;
        int mXformCopiesFrom = -1;              // Ctrl+drag: copies were appended from this index
        TransformValue mXformValue;
        HashSet<int> mXformSkip;                // Shapes the centre-handle snapping must ignore
        List<int> mXformPrevSelection;
        double mRingStart;                      // Rotate: where on the ring the drag started (for the wedge)

        // The last finished transform, so a typed value can replace it ("drag a ring, type 45, Enter")
        class XformRecord
        {
            public TransformMode Mode;
            public List<int> Indices;
            public List<Point3D[]> Original;
            public Point3D Center;
            public TransformValue Value;
            public HandleHit Handle;
            public long Version;
            public string Selected;
        }
        XformRecord mLastXform;

        // Handles' centre, cached for the selection and geometry it was worked out for
        long mHandlesVersion = -1;
        string mHandlesSelected = "";
        object mHandlesLayers;

        bool IsTransformTool { get { return mTool == Tool.Move || mTool == Tool.Rotate || mTool == Tool.Scale; } }

        /// <summary>True when the handles are on screen: a transform tool, a selection, and no click-to-move in progress.</summary>
        bool HandlesShown
        {
            get { return IsTransformTool && mSelected >= 0 && mSelected < mScene.Layers.Count && mMoveIndices == null; }
        }

        /// <summary>Typing a number with a transform tool and a selection sets the move / angle / scale exactly.</summary>
        bool TransformTakesTyping { get { return HandlesShown && !mXformDragging; } }

        private void UpdateHandles()
        {
            mHandles.Mode = mTool == Tool.Rotate ? TransformMode.Rotate : mTool == Tool.Scale ? TransformMode.Scale : TransformMode.Move;
            mHandles.PixelSize = 90f * DeviceDpi / 96f;
            mHandles.MoveStep = mInference.GridEnabled ? mInference.GridSpacing : 0;
            if (mXformDragging || !HandlesShown) return;
            string selKey = SelectionKey();
            if (mHandlesVersion != mScene.GeometryVersion || mHandlesSelected != selKey || !ReferenceEquals(mHandlesLayers, mScene.Layers))
            {
                mHandles.Center = TransformHandles.BoundsCenter(mScene.Layers, SelectionIndices());
                mHandlesVersion = mScene.GeometryVersion;
                mHandlesSelected = selKey;
                mHandlesLayers = mScene.Layers;
            }
        }

        /// <summary>Hover for the transform tools; returns true if the cursor is over a handle.</summary>
        private bool UpdateTransformHover()
        {
            UpdateHandles();
            mHandleHover = HandlesShown && !mXformDragging ? mHandles.HitTest(mScene, mCursor) : HandleHit.None;
            return !mHandleHover.IsNone;
        }

        /// <summary>Starts dragging a handle if the click is on one.</summary>
        private bool TransformMouseDown()
        {
            if (!HandlesShown) return false;
            UpdateHandles();
            HandleHit hit = mHandles.HitTest(mScene, mCursor);
            if (hit.IsNone || !mHandles.BeginDrag(mScene, mCursor, hit)) return false;

            StopSpin();
            mTyped = "";
            PushUndo();
            mXformIndices = SelectionIndices();
            mXformCopiesFrom = -1;
            mXformPrevSelection = new List<int>(mXformIndices);
            if (hit.Part == HandlePart.Ring) mRingStart = RingStartAngle(hit.Axis);
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                // Work on a copy, like Ctrl with SketchUp's Move.
                int newGroup = mScene.Layers[mXformIndices[0]].GroupId != 0 ? mScene.NextGroupId() : 0;
                mXformCopiesFrom = mScene.Layers.Count;
                List<int> copies = new List<int>();
                foreach (int i in mXformIndices)
                {
                    MyPolygon c = mScene.Layers[i].Clone();
                    c.Color = EffectiveColor(i);
                    c.GroupId = newGroup;
                    c.Name = c.Name + " copy";
                    copies.Add(mScene.Layers.Count);
                    mScene.Layers.Add(c);
                }
                mXformIndices = copies;
                SetSelection(copies);
                mHandlesSelected = SelectionKey();
                mScene.Touch();
            }
            mXformOriginal = TransformHandles.Snapshot(mScene.Layers, mXformIndices);
            mXformSkip = new HashSet<int>(mXformIndices);
            mXformValue = null;
            mXformDragging = true;
            mHandleHover = hit;
            return true;
        }

        private void TransformMouseMove()
        {
            if (!mXformDragging) return;
            bool fine = ModifierKeys.HasFlag(Keys.Shift);
            TransformValue v;
            if (mHandles.Mode == TransformMode.Move && mHandles.Dragging.Part == HandlePart.Center)
            {
                // Free move: snap the centre to points, edges and axis directions like the Move tool does.
                HashSet<int> skip = mXformSkip;
                mSnap = mInference.Snap(mScene, mCursor, WorkPlane().Through(mHandles.Center), mHandles.Center, null, i => skip.Contains(i), false, true);
                v = mSnap.HasPoint ? TransformValue.Move(mSnap.Point - mHandles.Center) : mXformValue;
            }
            else
            {
                mSnap = new SnapResult();
                v = mHandles.DragTo(mScene, mCursor, fine);
            }
            if (v == null) return;
            mXformValue = v;
            TransformHandles.ApplyToShapes(mScene.Layers, mXformIndices, mXformOriginal, v);
            mScene.Touch();
        }

        private void TransformMouseUp()
        {
            if (!mXformDragging) return;
            mXformDragging = false;
            HandleHit handle = mHandles.Dragging;
            mHandles.EndDrag();
            mSnap = new SnapResult();
            bool changed = mXformValue != null && !mXformValue.IsIdentity;
            if (!changed)
            {
                RestoreTransformStart();
                mUndo.DiscardLast();
                mScene.Touch();
                return;
            }
            Committed();
            mLastXform = new XformRecord
            {
                Mode = mHandles.Mode, Indices = mXformIndices, Original = mXformOriginal, Center = mHandles.Center.Clone(),
                Value = mXformValue, Handle = handle, Version = mScene.GeometryVersion, Selected = SelectionKey(),
            };
            UpdateHandles();
        }

        /// <summary>Esc during a handle drag: put everything back as it was.</summary>
        private void CancelTransformDrag()
        {
            if (!mXformDragging) return;
            mXformDragging = false;
            mHandles.EndDrag();
            RestoreTransformStart();
            mUndo.DiscardLast();
            mScene.Touch();
        }

        private void RestoreTransformStart()
        {
            if (mXformCopiesFrom >= 0)
            {
                mScene.Layers.RemoveRange(mXformCopiesFrom, mScene.Layers.Count - mXformCopiesFrom);
                SetSelection(mXformPrevSelection);
                mXformCopiesFrom = -1;
            }
            else if (mXformIndices != null)
                TransformHandles.ApplyToShapes(mScene.Layers, mXformIndices, mXformOriginal, null);
        }

        /// <summary>Rotate / Scale tools: a click away from the handles selects the shape under the cursor.</summary>
        private void TransformSelectClick()
        {
            int hit = mScene.Pick(mCursor, mScene.BuildFrame());
            ClickSelect(hit);
            mSelectOrbit = hit < 0;   // Dragging on empty space orbits the view
        }

        /// <summary>
        /// A typed value with a transform tool: replaces the last drag with the exact value, or applies
        /// a new transform to the selection. Move takes a distance (along the last move) or x,y,z;
        /// Rotate takes degrees (about the last ring used, else the vertical axis); Scale takes a factor
        /// or x,y,z factors.
        /// </summary>
        private bool ApplyTypedTransform(float[] values)
        {
            if (!HandlesShown || values == null || values.Length == 0) return false;
            TransformMode mode = mHandles.Mode;
            XformRecord last = mLastXform;
            bool reuse = last != null && last.Mode == mode && last.Version == mScene.GeometryVersion && last.Selected == SelectionKey()
                         && ReferenceEquals(mHandlesLayers, mScene.Layers);

            Point3D center = reuse ? last.Center : mHandles.Center.Clone();
            TransformValue value = null;
            switch (mode)
            {
                case TransformMode.Move:
                    if (values.Length >= 3) value = TransformValue.Move(new Point3D(values[0], values[1], values[2]));
                    else if (reuse && last.Value.Offset.Length() > Sketch.Eps) value = TransformValue.Move(last.Value.Offset.Normalized() * values[0]);
                    break;
                case TransformMode.Rotate:
                    {
                        Point3D axis = reuse ? last.Value.Axis : new Point3D(0, 1, 0);
                        value = TransformValue.Rotate(center, axis, values[0]);
                    }
                    break;
                default:
                    {
                        Point3D f;
                        if (values.Length >= 3) f = new Point3D(values[0], values[1], values[2]);
                        else if (reuse && last.Handle.Part == HandlePart.Axis)
                        {
                            f = new Point3D(1, 1, 1);
                            if (last.Handle.Axis == 0) f.X = values[0]; else if (last.Handle.Axis == 1) f.Y = values[0]; else f.Z = values[0];
                        }
                        else f = new Point3D(values[0], values[0], values[0]);
                        if (Math.Abs(f.X) < 1e-4f || Math.Abs(f.Y) < 1e-4f || Math.Abs(f.Z) < 1e-4f) return false;   // Would squash it flat
                        value = TransformValue.Scale(center, f);
                    }
                    break;
            }
            if (value == null)
            {
                saveStatus.Text = "Type x,y,z to move by, or drag an arrow first and then type a distance";
                return false;
            }

            if (reuse)
            {
                // The undo step saved before the drag still holds the shapes as they were.
                TransformHandles.ApplyToShapes(mScene.Layers, last.Indices, last.Original, value);
            }
            else
            {
                PushUndo();
                List<int> indices = SelectionIndices();
                last = new XformRecord
                {
                    Mode = mode, Indices = indices, Original = TransformHandles.Snapshot(mScene.Layers, indices), Center = center,
                    Handle = mode == TransformMode.Rotate ? new HandleHit(HandlePart.Ring, 1) : new HandleHit(HandlePart.Center, -1),
                    Selected = SelectionKey(),
                };
                TransformHandles.ApplyToShapes(mScene.Layers, indices, last.Original, value);
            }
            Committed();
            last.Value = value;
            last.Version = mScene.GeometryVersion;
            mLastXform = last;
            UpdateHandles();
            return true;
        }

        private string TransformHint(out string measure)
        {
            measure = "";
            string name = mTool == Tool.Rotate ? "Rotate" : mTool == Tool.Scale ? "Scale" : "Move";
            if (mXformDragging)
            {
                if (mXformValue != null) measure = mXformValue.Describe();
                return name + ": release to finish, Esc to cancel. Hold Shift for fine steps.";
            }
            if (mLastXform != null && mLastXform.Version == mScene.GeometryVersion && mLastXform.Mode == mHandles.Mode)
                measure = mLastXform.Value.Describe();
            if (!HandlesShown)
                return mTool == Tool.Move
                    ? "Move: click a shape to pick it up (Ctrl to copy), or select one to get move arrows."
                    : name + ": click a shape to select it.";
            switch (mTool)
            {
                case Tool.Rotate:
                    return "Rotate: drag a ring (15° steps, Shift for 1°), Ctrl+drag to rotate a copy, or type an angle and press Enter.";
                case Tool.Scale:
                    return "Scale: drag a box to stretch along an axis, the centre to scale evenly (Shift for fine), or type a factor (or x,y,z).";
                default:
                    return "Move: drag an arrow, a square or the centre (Ctrl+drag copies), click a shape to pick it up, or type x,y,z.";
            }
        }

        #region Drawing

        static readonly Color HandleHighlight = Color.FromArgb(255, 200, 0);

        private void DrawTransformHandles(Graphics g)
        {
            if (!HandlesShown) return;
            UpdateHandles();
            Point3D anchor = mHandles.Center;
            // While moving, the handles travel with the shape.
            if (mXformDragging && mXformValue != null && mHandles.Mode == TransformMode.Move)
                mHandles.Center = anchor + mXformValue.Offset;
            try
            {
                SmoothingMode oldMode = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                HandleHit active = mXformDragging ? mHandles.Dragging : mHandleHover;
                switch (mHandles.Mode)
                {
                    case TransformMode.Move: DrawMoveHandles(g, active); break;
                    case TransformMode.Rotate: DrawRotateHandles(g, active); break;
                    default: DrawScaleHandles(g, active); break;
                }
                g.SmoothingMode = oldMode;
            }
            finally
            {
                mHandles.Center = anchor;
            }
            if (mXformDragging && mHandles.Mode == TransformMode.Move && mXformValue != null)
                using (Pen rubber = new Pen(Color.FromArgb(60, 60, 60), 1.2f) { DashStyle = DashStyle.Dash })
                    DrawSegment(g, rubber, anchor, anchor + mXformValue.Offset);
            if (mXformDragging && mXformValue != null) DrawValueLabel(g, mXformValue.Describe());
        }

        private Color HandleColor(int axis, HandleHit active, HandlePart part)
        {
            bool hot = active.Part == part && (part == HandlePart.Center || active.Axis == axis);
            if (hot) return HandleHighlight;
            return axis >= 0 ? AxisColors[axis] : Color.FromArgb(240, 240, 240);
        }

        private void DrawMoveHandles(Graphics g, HandleHit active)
        {
            for (int k = 0; k < 3; k++)
            {
                PointF[] sq = mHandles.PlaneSquare(mScene, k);
                if (sq == null) continue;
                Color c = HandleColor(k, active, HandlePart.Plane);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(110, c)))
                using (Pen p = new Pen(c, 1.2f))
                {
                    g.FillPolygon(b, sq);
                    g.DrawPolygon(p, sq);
                }
            }
            for (int k = 0; k < 3; k++)
            {
                PointF a, b;
                if (!mHandles.AxisEnd(mScene, k, out a, out b)) continue;
                Color c = HandleColor(k, active, HandlePart.Axis);
                using (Pen p = new Pen(c, 2.5f))
                using (SolidBrush fill = new SolidBrush(c))
                {
                    float dx = b.X - a.X, dy = b.Y - a.Y, len = (float)Math.Sqrt(dx * dx + dy * dy);
                    float ux = dx / len, uy = dy / len, head = 13, half = 5.5f;
                    PointF baseP = new PointF(b.X - ux * head, b.Y - uy * head);
                    g.DrawLine(p, a.X + ux * 10, a.Y + uy * 10, baseP.X, baseP.Y);
                    g.FillPolygon(fill, new[] { b, new PointF(baseP.X - uy * half, baseP.Y + ux * half), new PointF(baseP.X + uy * half, baseP.Y - ux * half) });
                }
            }
            DrawCenterHandle(g, active, true);
        }

        private void DrawRotateHandles(Graphics g, HandleHit active)
        {
            for (int k = 0; k < 3; k++)
            {
                PointF[] ring = mHandles.Ring(mScene, k);
                if (ring == null) continue;
                Color rc = HandleColor(k, active, HandlePart.Ring);
                using (Pen p = new Pen(rc, active.Part == HandlePart.Ring && active.Axis == k ? 3.5f : 2.2f))
                    g.DrawLines(p, ring);
            }
            // The angle turned so far, as a wedge in the ring's plane.
            if (mXformDragging && mXformValue != null && Math.Abs(mXformValue.AngleDegrees) > 0.01)
            {
                int k = mHandles.Dragging.Axis;
                float size = mHandles.ModelSize(mScene);
                Point3D u = TransformHandles.Axes[(k + 1) % 3], w = TransformHandles.Axes[(k + 2) % 3];
                double start = mRingStart;
                List<PointF> wedge = new List<PointF>();
                PointF c0;
                if (mScene.TryProjectModel(mHandles.Center, out c0)) wedge.Add(c0);
                int steps = Math.Max(2, (int)(Math.Abs(mXformValue.AngleDegrees) / 4));
                for (int i = 0; i <= steps; i++)
                {
                    double a = start + mXformValue.AngleDegrees * Math.PI / 180 * i / steps;
                    PointF s;
                    if (mScene.TryProjectModel(mHandles.Center + u * (float)(size * 0.85 * Math.Cos(a)) + w * (float)(size * 0.85 * Math.Sin(a)), out s)) wedge.Add(s);
                }
                if (wedge.Count >= 3)
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(90, AxisColors[k])))
                        g.FillPolygon(b, wedge.ToArray());
            }
            PointF c;
            if (mScene.TryProjectModel(mHandles.Center, out c))
                g.FillEllipse(Brushes.DimGray, c.X - 2.5f, c.Y - 2.5f, 5, 5);
        }

        /// <summary>Angle (in the ring's own u/w basis) the ring drag started at, for the wedge.</summary>
        private double RingStartAngle(int k)
        {
            Point3D o, d;
            mScene.GetModelRay(mCursor, out o, out d);
            Point3D hit;
            if (!new Plane3D(mHandles.Center, TransformHandles.Axes[k]).IntersectRay(o, d, out hit)) return 0;
            Point3D v = hit - mHandles.Center;
            return Math.Atan2(Point3D.Dot(v, TransformHandles.Axes[(k + 2) % 3]), Point3D.Dot(v, TransformHandles.Axes[(k + 1) % 3]));
        }

        private void DrawScaleHandles(Graphics g, HandleHit active)
        {
            for (int k = 0; k < 3; k++)
            {
                PointF a, b;
                if (!mHandles.AxisEnd(mScene, k, out a, out b)) continue;
                Color c = HandleColor(k, active, HandlePart.Axis);
                using (Pen p = new Pen(c, 2.5f))
                using (SolidBrush fill = new SolidBrush(c))
                {
                    float dx = b.X - a.X, dy = b.Y - a.Y, len = (float)Math.Sqrt(dx * dx + dy * dy);
                    g.DrawLine(p, a.X + dx / len * 10, a.Y + dy / len * 10, b.X, b.Y);
                    g.FillRectangle(fill, b.X - 5.5f, b.Y - 5.5f, 11, 11);
                    g.DrawRectangle(Pens.Black, b.X - 5.5f, b.Y - 5.5f, 11, 11);
                }
            }
            DrawCenterHandle(g, active, false);
        }

        private void DrawCenterHandle(Graphics g, HandleHit active, bool round)
        {
            PointF c;
            if (!mScene.TryProjectModel(mHandles.Center, out c)) return;
            Color col = HandleColor(-1, active, HandlePart.Center);
            using (SolidBrush b = new SolidBrush(col))
            using (Pen p = new Pen(Color.FromArgb(60, 60, 60), 1.5f))
            {
                if (round) { g.FillEllipse(b, c.X - 6, c.Y - 6, 12, 12); g.DrawEllipse(p, c.X - 6, c.Y - 6, 12, 12); }
                else { g.FillRectangle(b, c.X - 6, c.Y - 6, 12, 12); g.DrawRectangle(p, c.X - 6, c.Y - 6, 12, 12); }
            }
        }

        private void DrawValueLabel(Graphics g, string text)
        {
            using (Font font = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (SolidBrush back = new SolidBrush(Color.FromArgb(235, 255, 255, 235)))
            {
                SizeF size = g.MeasureString(text, font);
                RectangleF box = new RectangleF(mCursor.X + 16, mCursor.Y + 14, size.Width + 6, size.Height + 2);
                g.FillRectangle(back, box);
                g.DrawRectangle(Pens.Gray, box.X, box.Y, box.Width, box.Height);
                g.DrawString(text, font, Brushes.Black, box.X + 3, box.Y + 1);
            }
        }

        #endregion
    }
}
