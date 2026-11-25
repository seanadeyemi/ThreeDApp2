using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace ThreeDApp2
{
    public enum Tool { Select, Marquee, Line, Rectangle, Circle, PushPull, Move, Rotate, Scale, Pan, Orbit }

    public enum WorkPlaneKind { Front, Ground, Side }

    /// <summary>
    /// The drawing tools: Select (pick, drag vertices, insert vertices), Line, Rectangle,
    /// Circle, Push/Pull and Move/Copy. Every finished operation is added to the scene straight
    /// away, pushed onto the undo stack and autosaved.
    /// </summary>
    public partial class Form1
    {
        const int CircleSegments = 24;
        const float HandleRadius = 8;

        Tool mTool = Tool.Select;
        // Default: the ground the house layers stand on, so you can draw a footprint and push/pull it up.
        WorkPlaneKind mWorkPlane = WorkPlaneKind.Ground;
        float mWorkPlaneOffset = 0;

        PointF mCursor;            // Mouse position relative to the viewport centre
        bool mCursorInside;
        SnapResult mSnap = new SnapResult();
        int mHoverIndex = -1;
        int mSelected = -1;
        string mTyped = "";        // Measurement box contents

        // Plane of the operation in progress
        Plane3D mOpPlane;
        bool mOpPlaneIsFace;

        // Line tool
        readonly List<Point3D> mDraft = new List<Point3D>();
        int mContinuing = -1;      // Open polyline being extended, or -1

        // Rectangle / circle tools
        Point3D mStart;            // First corner or centre
        Point mDownMouse;

        // Push/pull tool
        int mPushFace = -1;
        PointF mPushStartCursor;
        PointF mPushScreenDir;     // Unit screen direction of the face normal
        float mPushPixelsPerUnit;
        float mPushDistance;

        // Move tool
        List<int> mMoveIndices;
        Point3D mMoveBase;
        Point3D mMoveDelta = new Point3D(0, 0, 0);
        bool mMoveCopy;

        // Select tool: vertex dragging
        int mDragShape = -1, mDragVertex = -1;
        Plane3D mDragPlane;
        bool mDragChanged;
        bool mDragClosed;
        bool mSelectOrbit;

        bool OperationActive
        {
            get { return mDraft.Count > 0 || mStart != null || mPushFace >= 0 || mMoveIndices != null || mDragShape >= 0 || mXformDragging || mMarqueeActive; }
        }

        private Plane3D WorkPlane()
        {
            switch (mWorkPlane)
            {
                case WorkPlaneKind.Ground: return new Plane3D(new Point3D(0, mWorkPlaneOffset, 0), new Point3D(0, 1, 0));
                case WorkPlaneKind.Side: return new Plane3D(new Point3D(mWorkPlaneOffset, 0, 0), new Point3D(1, 0, 0));
                default: return new Plane3D(new Point3D(0, 0, mWorkPlaneOffset), new Point3D(0, 0, 1));
            }
        }

        private void SetTool(Tool tool)
        {
            CancelOperation();
            mTool = tool;
            selectBtn.Checked = tool == Tool.Select;
            marqueeBtn.Checked = tool == Tool.Marquee;
            lineBtn.Checked = tool == Tool.Line;
            rectBtn.Checked = tool == Tool.Rectangle;
            circleBtn.Checked = tool == Tool.Circle;
            pushPullBtn.Checked = tool == Tool.PushPull;
            moveBtn.Checked = tool == Tool.Move;
            rotateBtn.Checked = tool == Tool.Rotate;
            scaleBtn.Checked = tool == Tool.Scale;
            panBtn.Checked = tool == Tool.Pan;
            orbitBtn.Checked = tool == Tool.Orbit;
            SetToolCursor();
            UpdateHover();
            UpdateStatus();
            viewport.Invalidate();
        }

        private void SetToolCursor()
        {
            switch (mTool)
            {
                case Tool.Select: viewport.Cursor = Cursors.Default; break;
                case Tool.Move: viewport.Cursor = Cursors.SizeAll; break;
                case Tool.PushPull: viewport.Cursor = Cursors.SizeNS; break;
                case Tool.Pan: viewport.Cursor = mPanDragging ? mGrabHandCursor : mOpenHandCursor; break;
                case Tool.Orbit: viewport.Cursor = mOrbitCursor; break;
                default: viewport.Cursor = Cursors.Cross; break;
            }
        }

        bool mPanDragging;
        bool mOrbitDragging;

        private void CancelOperation()
        {
            CancelTransformDrag();
            mMarqueeActive = false;
            if (mContinuing >= 0 && mContinuing < mScene.Layers.Count)
            {
                mScene.Layers[mContinuing].Hidden = false;
                mScene.Touch();
            }
            if (mDragShape >= 0 && !mDragChanged) mUndo.DiscardLast();
            else if (mDragShape >= 0) Committed();
            mDraft.Clear();
            mContinuing = -1;
            mStart = null;
            mPushFace = -1;
            mMoveIndices = null;
            mDragShape = mDragVertex = -1;
            mOpPlane = null;
            mTyped = "";
            UpdateStatus();
        }

        private void StopSpin()
        {
            mSpinTimer.Enabled = false;
        }

        #region Hover and snapping

        /// <summary>Plane a new operation would start on: the face under the cursor, or the work plane.</summary>
        private void HoverPlane(List<ShapeScreenPoints> frame, out Plane3D plane, out bool isFace)
        {
            int face = mScene.Pick(mCursor, frame, 6, true);
            if (face >= 0)
            {
                Plane3D p = Plane3D.FromPoints(Sketch.Vertices(mScene.Layers[face]));
                if (p != null) { plane = p; isFace = true; return; }
            }
            plane = WorkPlane();
            isFace = false;
        }

        /// <summary>Plane for an operation that starts at a snapped point.</summary>
        private void PlaneForStart(SnapResult start, Plane3D hoverPlane, bool hoverIsFace, out Plane3D plane, out bool isFace)
        {
            if (hoverIsFace && hoverPlane.Contains(start.Point, 0.01f)) { plane = hoverPlane; isFace = true; return; }
            isFace = false;
            // A point picked from existing geometry may be off the work plane: draw parallel to it through the point.
            plane = start.Kind == SnapKind.Plane || start.Kind == SnapKind.Grid ? WorkPlane() : WorkPlane().Through(start.Point);
        }

        /// <summary>Recomputes the shape under the cursor and the snapped 3D point for the current tool.</summary>
        private void UpdateHover()
        {
            if (!mCursorInside || mScene == null) { mSnap = new SnapResult(); return; }
            List<ShapeScreenPoints> frame = mScene.BuildFrame();
            Plane3D hoverPlane; bool hoverIsFace;
            mHoverIndex = -1;
            if (mXformDragging) return;   // The drag itself works out the snap

            switch (mTool)
            {
                case Tool.Select:
                    if (mDragShape >= 0)
                    {
                        int skip = mDragShape;
                        mSnap = mInference.Snap(mScene, mCursor, mDragPlane, null, null, i => i == skip, false, false);
                    }
                    else
                    {
                        mHoverIndex = mScene.Pick(mCursor, frame);
                        mSnap = new SnapResult();
                    }
                    break;

                case Tool.Line:
                    if (mDraft.Count == 0)
                    {
                        HoverPlane(frame, out hoverPlane, out hoverIsFace);
                        mSnap = mInference.Snap(mScene, mCursor, hoverPlane, null, null, null, hoverIsFace, false);
                    }
                    else
                    {
                        Point3D last = mDraft[mDraft.Count - 1];
                        int skip = mContinuing;
                        mSnap = mInference.Snap(mScene, mCursor, mOpPlane.Through(last), last, mDraft, i => i == skip, mOpPlaneIsFace, true);
                    }
                    break;

                case Tool.Rectangle:
                case Tool.Circle:
                    if (mStart == null)
                    {
                        HoverPlane(frame, out hoverPlane, out hoverIsFace);
                        mSnap = mInference.Snap(mScene, mCursor, hoverPlane, null, null, null, hoverIsFace, false);
                    }
                    else
                    {
                        mSnap = mInference.Snap(mScene, mCursor, mOpPlane, mStart, null, null, mOpPlaneIsFace, false);
                        if (mSnap.HasPoint) mSnap.Point = mOpPlane.ProjectPoint(mSnap.Point);
                    }
                    break;

                case Tool.Pan:
                case Tool.Orbit:
                    mSnap = new SnapResult();
                    break;

                case Tool.Marquee:
                    mSnap = new SnapResult();
                    if (!mMarqueeActive) mHoverIndex = mScene.Pick(mCursor, frame);
                    break;

                case Tool.Rotate:
                case Tool.Scale:
                    mSnap = new SnapResult();
                    if (!UpdateTransformHover()) mHoverIndex = mScene.Pick(mCursor, frame);
                    break;

                case Tool.PushPull:
                    if (mPushFace < 0) mHoverIndex = mScene.Pick(mCursor, frame, 6, true);
                    mSnap = new SnapResult();
                    break;

                case Tool.Move:
                    if (mMoveIndices == null && UpdateTransformHover())
                        mSnap = new SnapResult();   // Over a move arrow: nothing else lights up
                    else if (mMoveIndices == null)
                    {
                        mHoverIndex = mScene.Pick(mCursor, frame);
                        HoverPlane(frame, out hoverPlane, out hoverIsFace);
                        mSnap = mInference.Snap(mScene, mCursor, hoverPlane, null, null, null, hoverIsFace, false);
                    }
                    else
                    {
                        List<int> moving = mMoveIndices;
                        mSnap = mInference.Snap(mScene, mCursor, WorkPlane().Through(mMoveBase), mMoveBase, null,
                                                i => moving.Contains(i), false, true);
                        mMoveDelta = mSnap.HasPoint ? mSnap.Point - mMoveBase : new Point3D(0, 0, 0);
                    }
                    break;
            }
        }

        #endregion

        #region Mouse

        private void ToolMouseDown(MouseEventArgs e)
        {
            mDownMouse = e.Location;
            UpdateHover();
            switch (mTool)
            {
                case Tool.Select: SelectMouseDown(); break;
                case Tool.Marquee: MarqueeMouseDown(); break;
                case Tool.Line: if (mSnap.HasPoint) AddLinePoint(mSnap); break;
                case Tool.Rectangle:
                case Tool.Circle: ShapeToolClick(); break;
                case Tool.PushPull: PushPullClick(); break;
                case Tool.Move: if (mMoveIndices != null || !TransformMouseDown()) MoveClick(); break;
                case Tool.Rotate:
                case Tool.Scale: if (!TransformMouseDown()) TransformSelectClick(); break;
                case Tool.Pan:
                    mPanDragging = true;
                    StopSpin();
                    SetToolCursor();   // Closed hand while dragging
                    break;
                case Tool.Orbit:
                    mOrbitDragging = true;
                    StopSpin();
                    break;
            }
            UpdateHover();
            UpdateStatus();
            viewport.Invalidate();
        }

        private void ToolMouseMove(MouseEventArgs e, int dx, int dy)
        {
            if (mXformDragging)
            {
                TransformMouseMove();
                return;
            }
            if (mTool == Tool.Pan && mPanDragging && e.Button == MouseButtons.Left)
            {
                Pan(dx, dy);
                return;
            }
            if (mTool == Tool.Orbit && mOrbitDragging && e.Button == MouseButtons.Left)
            {
                Orbit(dx, dy);
                return;
            }
            if (mSelectOrbit && e.Button == MouseButtons.Left)
            {
                Orbit(dx, dy);
                return;
            }
            if (mTool == Tool.Select && mDragShape >= 0)
            {
                UpdateHover();
                if (mSnap.HasPoint) MoveDraggedVertex(mSnap.Point);
            }
            if (mTool == Tool.PushPull && mPushFace >= 0)
            {
                float along = (mCursor.X - mPushStartCursor.X) * mPushScreenDir.X + (mCursor.Y - mPushStartCursor.Y) * mPushScreenDir.Y;
                mPushDistance = along / mPushPixelsPerUnit;
                if (mInference.GridEnabled) mPushDistance = (float)Math.Round(mPushDistance / mInference.GridSpacing) * mInference.GridSpacing;
            }
        }

        private void ToolMouseUp(MouseEventArgs e)
        {
            mSelectOrbit = false;
            if (mXformDragging)
            {
                TransformMouseUp();
                return;
            }
            if (mMarqueeActive)
            {
                MarqueeMouseUp();
                return;
            }
            if (mOrbitDragging)
            {
                mOrbitDragging = false;
                return;
            }
            if (mPanDragging)
            {
                mPanDragging = false;
                SetToolCursor();
                return;
            }
            if (mTool == Tool.Select && mDragShape >= 0)
            {
                bool changed = mDragChanged;
                mDragShape = mDragVertex = -1;
                if (changed) Committed(); else mUndo.DiscardLast();
                return;
            }
            // Click-and-drag also works for rectangles and circles.
            bool dragged = Math.Abs(e.X - mDownMouse.X) + Math.Abs(e.Y - mDownMouse.Y) > 6;
            if ((mTool == Tool.Rectangle || mTool == Tool.Circle) && mStart != null && dragged)
                ShapeToolClick();
        }

        private void ToolDoubleClick(MouseEventArgs e)
        {
            if (mTool == Tool.Line && mDraft.Count >= 2)
            {
                FinishLine(false);
                return;
            }
            if (mTool == Tool.Select && mSelected >= 0)
                InsertVertexOnSelectedEdge();
        }

        #endregion

        #region Select tool

        private void SelectMouseDown()
        {
            // Grab a vertex handle of the selected shape?
            if (mSelected >= 0 && mSelected < mScene.Layers.Count && !ExtrasValid)
            {
                MyPolygon sel = mScene.Layers[mSelected];
                int count = Scene.VertexCount(sel);
                for (int j = 0; j < count; j++)
                {
                    PointF p;
                    if (!mScene.TryProjectModel(sel.Absolute(j), out p)) continue;
                    if (Math.Abs(p.X - mCursor.X) <= HandleRadius && Math.Abs(p.Y - mCursor.Y) <= HandleRadius)
                    {
                        PushUndo();
                        mDragShape = mSelected;
                        mDragVertex = j;
                        mDragChanged = false;
                        mDragClosed = sel.IsClosedFace;
                        Plane3D face = sel.IsClosedFace ? Plane3D.FromPoints(Sketch.Vertices(sel)) : null;
                        mDragPlane = face != null ? face.Through(sel.Absolute(j)) : WorkPlane().Through(sel.Absolute(j));
                        StopSpin();
                        return;
                    }
                }
            }

            int hit = mScene.Pick(mCursor, mScene.BuildFrame());
            ClickSelect(hit);
            mSelectOrbit = hit < 0;   // Dragging on empty space orbits the view
        }

        private void MoveDraggedVertex(Point3D p)
        {
            MyPolygon shape = mScene.Layers[mDragShape];
            if (Sketch.Same(shape[mDragVertex], p)) return;
            shape[mDragVertex] = p.Clone();
            // Keep the closing vertex on top of the first one so the face stays closed.
            if (mDragVertex == 0 && mDragClosed)
                shape[shape.GetSize() - 1] = p.Clone();
            mDragChanged = true;
            mScene.Touch();
        }

        private void InsertVertexOnSelectedEdge()
        {
            int sel = mSelected;
            SnapResult s = mInference.Snap(mScene, mCursor, WorkPlane(), null, null, i => i != sel, false, false);
            if (s.Kind != SnapKind.OnEdge || s.LayerIndex != sel) return;
            PushUndo();
            mScene.Layers[sel].InsertAt(s.VertexIndex + 1, s.Point.Clone());
            Committed();
        }

        private void DeleteSelected()
        {
            if (mSelected < 0 || OperationActive) return;
            PushUndo();
            List<int> members = SelectionIndices();
            for (int k = members.Count - 1; k >= 0; k--)
                mScene.Layers.RemoveAt(members[k]);
            mSelected = -1;
            Committed();
        }

        #endregion

        #region Line tool

        private void AddLinePoint(SnapResult snap)
        {
            Point3D p = snap.Point.Clone();

            if (mDraft.Count == 0)
            {
                StopSpin();
                List<ShapeScreenPoints> frame = mScene.BuildFrame();
                Plane3D hoverPlane; bool hoverIsFace;
                HoverPlane(frame, out hoverPlane, out hoverIsFace);
                PlaneForStart(snap, hoverPlane, hoverIsFace, out mOpPlane, out mOpPlaneIsFace);

                // Starting on the end of an open polyline continues it.
                bool atStart;
                int idx = Sketch.FindOpenEndpoint(mScene.Layers, p, -1, out atStart);
                if (idx >= 0)
                {
                    mContinuing = idx;
                    mDraft.AddRange(Sketch.OrientedPoints(mScene.Layers[idx], atStart, true));
                    mScene.Layers[idx].Hidden = true;
                    mScene.Touch();
                }
                else
                {
                    mDraft.Add(p);
                }
                return;
            }

            if (Sketch.Same(p, mDraft[mDraft.Count - 1])) return;   // Double click / same point

            // Clicking the first point closes the outline and fills it.
            if (Sketch.Same(p, mDraft[0]))
            {
                if (mDraft.Count >= 3) FinishLine(true);
                return;
            }

            // Ending on the end of another open polyline joins the two.
            bool otherAtStart;
            int other = Sketch.FindOpenEndpoint(mScene.Layers, p, mContinuing, out otherAtStart);
            if (other >= 0)
            {
                MyPolygon otherShape = mScene.Layers[other];
                List<Point3D> tail = Sketch.OrientedPoints(otherShape, otherAtStart, false);
                bool closes = Sketch.Join(mDraft, tail);
                if (closes) mDraft.RemoveAt(mDraft.Count - 1);
                FinishLine(closes, otherShape);
                return;
            }

            mDraft.Add(p);
        }

        private void FinishLine(bool closed, MyPolygon joined = null)
        {
            if (mDraft.Count < 2 || (closed && mDraft.Count < 3))
            {
                CancelOperation();
                return;
            }

            PushUndo();
            string name;
            Color color;
            int group = 0;
            MyPolygon continued = mContinuing >= 0 ? mScene.Layers[mContinuing] : null;
            if (continued != null)
            {
                name = continued.Name;
                color = continued.Color;
                group = continued.GroupId;
                if (closed && name.StartsWith("Line ")) name = NextName("Polygon");
            }
            else
            {
                name = NextName(closed ? "Polygon" : "Line");
                color = NextColor();
            }

            MyPolygon shape = Sketch.MakeShape(mDraft, closed, name, color, group);
            if (continued != null) mScene.Layers[mContinuing] = shape;
            else mScene.Layers.Add(shape);
            if (joined != null) mScene.Layers.Remove(joined);
            mSelected = mScene.Layers.IndexOf(shape);   // After the removal, which can shift indices

            mContinuing = -1;   // Already replaced, so CancelOperation must not un-hide anything
            mDraft.Clear();
            CancelOperation();
            Committed();
        }

        private void RemoveLastPoint()
        {
            if (mTool != Tool.Line || mDraft.Count == 0) return;
            mDraft.RemoveAt(mDraft.Count - 1);
            if (mDraft.Count == 0) CancelOperation();
            else viewport.Invalidate();
        }

        #endregion

        #region Rectangle and circle tools

        private void ShapeToolClick()
        {
            if (!mSnap.HasPoint) return;
            if (mStart == null)
            {
                StopSpin();
                List<ShapeScreenPoints> frame = mScene.BuildFrame();
                Plane3D hoverPlane; bool hoverIsFace;
                HoverPlane(frame, out hoverPlane, out hoverIsFace);
                PlaneForStart(mSnap, hoverPlane, hoverIsFace, out mOpPlane, out mOpPlaneIsFace);
                mStart = mOpPlane.ProjectPoint(mSnap.Point);
                return;
            }
            List<Point3D> pts = ShapePreview(mSnap.Point);
            if (pts == null) return;
            CommitShape(pts, mTool == Tool.Rectangle ? "Rectangle" : "Circle");
        }

        private List<Point3D> ShapePreview(Point3D end)
        {
            if (mStart == null || end == null) return null;
            if (mTool == Tool.Rectangle) return Sketch.Rectangle(mOpPlane, mStart, end);
            float r = Point3D.Distance(mStart, mOpPlane.ProjectPoint(end));
            return Sketch.Circle(mOpPlane, mStart, r, CircleSegments, end);
        }

        private void CommitShape(List<Point3D> pts, string prefix)
        {
            PushUndo();
            MyPolygon shape = Sketch.MakeShape(pts, true, NextName(prefix), NextColor(), 0);
            mScene.Layers.Add(shape);
            mSelected = mScene.Layers.Count - 1;
            CancelOperation();
            Committed();
        }

        #endregion

        #region Push/pull tool

        private void PushPullClick()
        {
            if (mPushFace >= 0)
            {
                CommitPushPull(mPushDistance);
                return;
            }
            int face = mHoverIndex;
            if (face < 0 || !mScene.Layers[face].IsClosedFace) return;
            List<Point3D> verts = Sketch.Vertices(mScene.Layers[face]);
            Plane3D plane = Plane3D.FromPoints(verts);
            if (plane == null) return;

            StopSpin();
            mPushFace = face;
            mPushStartCursor = mCursor;
            mPushDistance = 0;
            // How far, and in which screen direction, one unit along the normal moves on screen.
            PointF a, b;
            Point3D c = Sketch.Centroid(verts);
            if (mScene.TryProjectModel(c, out a) && mScene.TryProjectModel(c + plane.Normal, out b))
            {
                float dx = b.X - a.X, dy = b.Y - a.Y;
                float len = (float)Math.Sqrt(dx * dx + dy * dy);
                if (len > 2)
                {
                    mPushScreenDir = new PointF(dx / len, dy / len);
                    mPushPixelsPerUnit = len;
                    return;
                }
            }
            // Face seen head-on: use vertical mouse movement instead.
            mPushScreenDir = new PointF(0, -1);
            mPushPixelsPerUnit = Math.Max(2f, mScene.FocalLength / mScene.Distance);
        }

        private List<MyPolygon> PushPullPreview(float distance)
        {
            if (mPushFace < 0) return new List<MyPolygon>();
            return Sketch.Extrude(mScene.Layers[mPushFace], distance, EffectiveColor(mPushFace), 0);
        }

        private void CommitPushPull(float distance)
        {
            if (mPushFace < 0 || Math.Abs(distance) < Sketch.Eps) { CancelOperation(); return; }
            PushUndo();
            MyPolygon face = mScene.Layers[mPushFace];
            int group = face.GroupId != 0 ? face.GroupId : mScene.NextGroupId();
            Color color = EffectiveColor(mPushFace);
            face.GroupId = group;
            face.Color = color;
            mScene.Layers.AddRange(Sketch.Extrude(face, distance, color, group));
            mSelected = mPushFace;
            CancelOperation();
            Committed();
        }

        #endregion

        #region Move tool

        private void MoveClick()
        {
            if (mMoveIndices == null)
            {
                if (mHoverIndex < 0) return;
                StopSpin();
                mMoveIndices = mScene.GroupMembers(mHoverIndex);
                mMoveBase = mSnap.HasPoint ? mSnap.Point.Clone() : Sketch.Centroid(Sketch.Vertices(mScene.Layers[mHoverIndex]));
                mMoveDelta = new Point3D(0, 0, 0);
                mMoveCopy = ModifierKeys.HasFlag(Keys.Control);
                return;
            }
            CommitMove(mMoveDelta);
        }

        private void CommitMove(Point3D delta)
        {
            if (mMoveIndices == null || delta.Length() < Sketch.Eps) { CancelOperation(); return; }
            PushUndo();
            bool copy = mMoveCopy || ModifierKeys.HasFlag(Keys.Control);
            int firstIndex = mMoveIndices[0];
            if (copy)
            {
                int newGroup = mScene.Layers[firstIndex].GroupId != 0 ? mScene.NextGroupId() : 0;
                int firstCopy = mScene.Layers.Count;
                foreach (int i in mMoveIndices)
                {
                    MyPolygon c = mScene.Layers[i].Clone();
                    c.Color = EffectiveColor(i);
                    c.GroupId = newGroup;
                    c.Name = c.Name + " copy";
                    c.PtChange(delta);
                    mScene.Layers.Add(c);
                }
                mSelected = firstCopy;
            }
            else
            {
                foreach (int i in mMoveIndices) mScene.Layers[i].PtChange(delta);
                mSelected = firstIndex;
            }
            CancelOperation();
            Committed();
        }

        #endregion

        #region Measurement box (typed values) and finishing

        private void FinishOperation()
        {
            if (mTool == Tool.Line && mDraft.Count >= 2) FinishLine(false);
        }

        /// <summary>
        /// Applies a typed value, like SketchUp's Measurements box: a length for lines and moves,
        /// "width,height" for rectangles, a radius for circles and a distance for push/pull.
        /// </summary>
        private void ApplyTypedValue()
        {
            string text = mTyped;
            mTyped = "";
            float[] values = ParseValues(text);
            if (values == null || values.Length == 0) { UpdateStatus(); return; }
            float v = values[0];

            switch (mTool)
            {
                case Tool.Line:
                    if (mDraft.Count > 0 && mSnap.HasPoint)
                    {
                        Point3D last = mDraft[mDraft.Count - 1];
                        Point3D dir = (mSnap.Point - last).Normalized();
                        if (dir.Length() > 0.5f)
                            AddLinePoint(new SnapResult { Point = last + dir * v, Kind = SnapKind.Plane });
                    }
                    break;
                case Tool.Rectangle:
                    if (mStart != null)
                    {
                        float w = v, h = values.Length > 1 ? values[1] : v;
                        if (mSnap.HasPoint)
                        {
                            Point3D d = mSnap.Point - mStart;
                            if (Point3D.Dot(d, mOpPlane.U) < 0) w = -w;
                            if (Point3D.Dot(d, mOpPlane.V) < 0) h = -h;
                        }
                        if (Math.Abs(w) > Sketch.Eps && Math.Abs(h) > Sketch.Eps)
                            CommitShape(Sketch.RectangleFromSize(mOpPlane, mStart, w, h), "Rectangle");
                    }
                    break;
                case Tool.Circle:
                    if (mStart != null && v > Sketch.Eps)
                        CommitShape(Sketch.Circle(mOpPlane, mStart, v, CircleSegments, mSnap.HasPoint ? mSnap.Point : null), "Circle");
                    break;
                case Tool.PushPull:
                    if (mPushFace >= 0)
                        CommitPushPull(mPushDistance < 0 ? -v : v);   // Keep the direction the mouse is pulling
                    break;
                case Tool.Move:
                    if (mMoveIndices != null)
                    {
                        if (values.Length >= 3) CommitMove(new Point3D(values[0], values[1], values[2]));
                        else if (mMoveDelta.Length() > Sketch.Eps) CommitMove(mMoveDelta.Normalized() * v);
                    }
                    else ApplyTypedTransform(values);
                    break;
                case Tool.Rotate:
                case Tool.Scale:
                    ApplyTypedTransform(values);
                    break;
            }
            UpdateStatus();
            viewport.Invalidate();
        }

        private static float[] ParseValues(string text)
        {
            string[] parts = text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<float> values = new List<float>();
            foreach (string part in parts)
            {
                float v;
                if (!float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) return null;
                values.Add(v);
            }
            return values.ToArray();
        }

        #endregion

        #region Status bar

        private void UpdateStatus()
        {
            string hint, measure = "";
            switch (mTool)
            {
                case Tool.Line:
                    if (mDraft.Count == 0)
                        hint = "Line: click to start. Start on the end of an existing line to continue it.";
                    else
                    {
                        hint = "Line: click the next point. Click the first point to close and fill, Enter or double-click to finish, Esc to cancel.";
                        if (mSnap.HasPoint) measure = "Length " + Point3D.Distance(mDraft[mDraft.Count - 1], mSnap.Point).ToString("0.##");
                    }
                    break;
                case Tool.Rectangle:
                    hint = mStart == null ? "Rectangle: click the first corner (on a face to draw on it)." : "Rectangle: click the opposite corner, or type width,height and press Enter.";
                    if (mStart != null && mSnap.HasPoint)
                    {
                        Point3D d = mSnap.Point - mStart;
                        measure = string.Format("Size {0:0.##} x {1:0.##}", Math.Abs(Point3D.Dot(d, mOpPlane.U)), Math.Abs(Point3D.Dot(d, mOpPlane.V)));
                    }
                    break;
                case Tool.Circle:
                    hint = mStart == null ? "Circle: click the centre (on a face to draw on it)." : "Circle: click to set the radius, or type it and press Enter.";
                    if (mStart != null && mSnap.HasPoint) measure = "Radius " + Point3D.Distance(mStart, mSnap.Point).ToString("0.##");
                    break;
                case Tool.PushPull:
                    hint = mPushFace < 0 ? "Push/Pull: click a filled face." : "Push/Pull: move the mouse and click, or type a distance and press Enter.";
                    if (mPushFace >= 0) measure = "Distance " + mPushDistance.ToString("0.##");
                    break;
                case Tool.Move:
                    if (mMoveIndices == null) hint = TransformHint(out measure);
                    else
                    {
                        hint = "Move: click to put it down, or type a distance (or x,y,z) and press Enter." + (mMoveCopy ? " (copying)" : "");
                        measure = "Distance " + mMoveDelta.Length().ToString("0.##");
                    }
                    break;
                case Tool.Rotate:
                case Tool.Scale:
                    hint = TransformHint(out measure);
                    break;
                case Tool.Marquee:
                    hint = mMarqueeActive
                        ? (MarqueeSelect.IsCrossing(mMarqueeStart, mCursor) ? "Box Select (crossing): selects everything the box touches." : "Box Select (window): selects shapes entirely inside the box.")
                        : "Box Select: drag left to right for shapes fully inside, right to left for anything touched. Shift adds, Ctrl removes.";
                    break;
                case Tool.Pan:
                    hint = "Pan: drag to slide the view. Esc or S returns to Select.";
                    break;
                case Tool.Orbit:
                    hint = "Orbit: drag to turn and tilt the view. Esc or S returns to Select.";
                    break;
                default:
                    hint = ExtrasValid
                        ? SelectionCount + " shapes selected: M / Q / E to move, rotate or scale them together, Del to delete. Shift-click adds or removes."
                        : mSelected >= 0
                        ? "Select: drag an orange handle to move a vertex, double-click an edge to add one, Del to delete. Shift-click adds more."
                        : "Select: click a shape (Shift-click for more, B to box select). Drag empty space or right-drag to orbit.";
                    break;
            }
            if (mSnap.HasPoint && mSnap.Label.Length > 0) hint = mSnap.Label + "  |  " + hint;
            toolStatus.Text = hint;
            measureStatus.Text = mTyped.Length > 0 ? measure + "   Typed: " + mTyped + "_" : measure;
        }

        #endregion

        #region Preview painting

        private void DrawToolPreview(Graphics g)
        {
            using (Pen preview = new Pen(Color.FromArgb(40, 40, 40), 1.5f))
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(70, 255, 140, 0)))
            {
                switch (mTool)
                {
                    case Tool.Line:
                        if (mDraft.Count > 0)
                        {
                            DrawPolyline3D(g, preview, mDraft, false);
                            foreach (Point3D p in mDraft) DrawVertexDot(g, p);
                            if (mSnap.HasPoint)
                            {
                                bool closing = mDraft.Count >= 3 && Sketch.Same(mSnap.Point, mDraft[0]);
                                if (closing) FillPolygon3D(g, fill, mDraft);
                                using (Pen rubber = new Pen(mSnap.Kind == SnapKind.Axis ? AxisColors[mSnap.AxisIndex] : Color.Black, 2f))
                                {
                                    rubber.DashStyle = DashStyle.Dash;
                                    DrawSegment(g, rubber, mDraft[mDraft.Count - 1], mSnap.Point);
                                }
                            }
                        }
                        break;
                    case Tool.Rectangle:
                    case Tool.Circle:
                        if (mStart != null && mSnap.HasPoint)
                        {
                            List<Point3D> pts = ShapePreview(mSnap.Point);
                            if (pts != null)
                            {
                                FillPolygon3D(g, fill, pts);
                                DrawPolyline3D(g, preview, pts, true);
                            }
                            if (mTool == Tool.Circle)
                                using (Pen radius = new Pen(Color.Gray, 1f) { DashStyle = DashStyle.Dot })
                                    DrawSegment(g, radius, mStart, mSnap.Point);
                        }
                        break;
                    case Tool.PushPull:
                        if (mPushFace >= 0)
                            foreach (MyPolygon p in PushPullPreview(mPushDistance))
                            {
                                List<Point3D> v = Sketch.Vertices(p);
                                FillPolygon3D(g, fill, v);
                                DrawPolyline3D(g, preview, v, true);
                            }
                        else if (mHoverIndex >= 0)
                        {
                            List<Point3D> v = Sketch.Vertices(mScene.Layers[mHoverIndex]);
                            using (HatchBrush hatch = new HatchBrush(HatchStyle.Percent25, Color.FromArgb(150, 30, 144, 255), Color.Transparent))
                                FillPolygon3D(g, hatch, v);
                        }
                        break;
                    case Tool.Move:
                        if (mMoveIndices != null)
                        {
                            using (Pen moved = new Pen(Color.FromArgb(255, 140, 0), 2f))
                                foreach (int i in mMoveIndices)
                                {
                                    List<Point3D> v = Sketch.Vertices(mScene.Layers[i]);
                                    for (int k = 0; k < v.Count; k++) v[k] = v[k] + mMoveDelta;
                                    DrawPolyline3D(g, moved, v, mScene.Layers[i].IsClosedFace);
                                }
                            if (mSnap.HasPoint)
                                using (Pen rubber = new Pen(mSnap.Kind == SnapKind.Axis ? AxisColors[mSnap.AxisIndex] : Color.Black, 1.5f) { DashStyle = DashStyle.Dash })
                                    DrawSegment(g, rubber, mMoveBase, mSnap.Point);
                        }
                        break;
                }
            }
            DrawTransformHandles(g);
            DrawMarquee(g);
            DrawSnapMarker(g);
        }

        private void DrawVertexDot(Graphics g, Point3D p)
        {
            PointF s;
            if (!mScene.TryProjectModel(p, out s)) return;
            g.FillEllipse(Brushes.Black, s.X - 2.5f, s.Y - 2.5f, 5, 5);
        }

        private void DrawSnapMarker(Graphics g)
        {
            if (!mCursorInside || !mSnap.HasPoint || mTool == Tool.Select && mDragShape < 0 || mTool == Tool.PushPull) return;
            PointF s;
            if (!mScene.TryProjectModel(mSnap.Point, out s)) return;
            Color c;
            switch (mSnap.Kind)
            {
                case SnapKind.Endpoint:
                    c = Color.FromArgb(0, 170, 0);
                    using (Pen pen = new Pen(c, 2f)) g.DrawEllipse(pen, s.X - 6, s.Y - 6, 12, 12);
                    break;
                case SnapKind.Midpoint:
                    c = Color.FromArgb(0, 170, 200);
                    using (Pen pen = new Pen(c, 2f)) g.DrawEllipse(pen, s.X - 6, s.Y - 6, 12, 12);
                    break;
                case SnapKind.OnEdge:
                    c = Color.FromArgb(220, 30, 30);
                    using (Pen pen = new Pen(c, 2f)) g.DrawRectangle(pen, s.X - 5, s.Y - 5, 10, 10);
                    break;
                case SnapKind.OnFace:
                    c = Color.FromArgb(40, 80, 220);
                    using (Pen pen = new Pen(c, 2f))
                        g.DrawPolygon(pen, new[] { new PointF(s.X, s.Y - 6), new PointF(s.X + 6, s.Y), new PointF(s.X, s.Y + 6), new PointF(s.X - 6, s.Y) });
                    break;
                case SnapKind.Axis:
                    c = AxisColors[mSnap.AxisIndex];
                    using (SolidBrush b = new SolidBrush(c)) g.FillEllipse(b, s.X - 4, s.Y - 4, 8, 8);
                    break;
                default:
                    c = Color.DimGray;
                    using (Pen pen = new Pen(c, 1.5f))
                    {
                        g.DrawLine(pen, s.X - 5, s.Y, s.X + 5, s.Y);
                        g.DrawLine(pen, s.X, s.Y - 5, s.X, s.Y + 5);
                    }
                    break;
            }
            string label = mSnap.Label;
            if (label.Length > 0)
            {
                using (Font font = new Font("Segoe UI", 8.5f))
                using (SolidBrush back = new SolidBrush(Color.FromArgb(230, 255, 255, 230)))
                using (SolidBrush text = new SolidBrush(c))
                {
                    SizeF size = g.MeasureString(label, font);
                    RectangleF box = new RectangleF(s.X + 12, s.Y + 10, size.Width + 4, size.Height + 2);
                    g.FillRectangle(back, box);
                    g.DrawRectangle(Pens.Gray, box.X, box.Y, box.Width, box.Height);
                    g.DrawString(label, font, text, box.X + 2, box.Y + 1);
                }
            }
        }

        #endregion
    }
}
