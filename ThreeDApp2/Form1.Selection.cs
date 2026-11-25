using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ThreeDApp2
{
    /// <summary>
    /// Selecting several shapes at once: the Box Select tool (drag a rectangle, SketchUp-style
    /// window / crossing) and Shift-click. mSelected stays the main shape; the others are kept
    /// beside it and dropped whenever the shape list changes underneath them.
    /// </summary>
    public partial class Form1
    {
        // Extra selected shapes, valid only for the main shape, shape list and count they were made for.
        List<int> mExtraSelected = new List<int>();
        int mExtraFor = -2;
        object mExtraLayers;
        int mExtraCount = -1;

        // Box Select tool
        bool mMarqueeActive;
        PointF mMarqueeStart;

        bool ExtrasValid
        {
            get { return mExtraSelected.Count > 0 && mExtraFor == mSelected && ReferenceEquals(mExtraLayers, mScene.Layers) && mExtraCount == mScene.Layers.Count; }
        }

        /// <summary>Every selected shape (whole groups), sorted.</summary>
        private List<int> SelectionIndices()
        {
            List<int> result = mSelected >= 0 ? mScene.GroupMembers(mSelected) : new List<int>();
            if (mSelected < 0 || !ExtrasValid) return result;
            HashSet<int> seen = new HashSet<int>(result);
            foreach (int i in mExtraSelected)
                if (i < mScene.Layers.Count)
                    foreach (int m in mScene.GroupMembers(i))
                        if (seen.Add(m)) result.Add(m);
            result.Sort();
            return result;
        }

        private int SelectionCount { get { return SelectionIndices().Count; } }

        /// <summary>Changes for the selection, part of the cached scene picture's key.</summary>
        private string SelectionKey()
        {
            if (mSelected < 0) return "-";
            if (!ExtrasValid) return mSelected.ToString();
            return mSelected + "+" + string.Join(".", mExtraSelected);
        }

        /// <summary>Selects exactly these shapes (the first becomes the main one).</summary>
        private void SetSelection(IList<int> indices)
        {
            mExtraSelected = new List<int>();
            mSelected = indices.Count > 0 ? indices[0] : -1;
            for (int k = 1; k < indices.Count; k++)
                if (indices[k] != mSelected && !mExtraSelected.Contains(indices[k])) mExtraSelected.Add(indices[k]);
            mExtraFor = mSelected;
            mExtraLayers = mScene.Layers;
            mExtraCount = mScene.Layers.Count;
            SyncSelectionUi();
        }

        /// <summary>
        /// Combines a click or box result with the selection: plain replaces, Shift adds
        /// (a Shift-click on a selected shape removes it), Ctrl removes.
        /// </summary>
        private void ApplySelection(List<int> hits, bool isClick)
        {
            bool shift = ModifierKeys.HasFlag(Keys.Shift), ctrl = ModifierKeys.HasFlag(Keys.Control);
            if (!shift && !ctrl) { SetSelection(hits); return; }
            List<int> current = SelectionIndices();
            HashSet<int> set = new HashSet<int>(current);
            foreach (int h in hits)
            {
                List<int> group = mScene.GroupMembers(h);
                bool selected = set.Contains(h);
                bool remove = ctrl || (shift && isClick && selected);
                foreach (int m in group)
                {
                    if (remove) set.Remove(m);
                    else set.Add(m);
                }
            }
            List<int> result = new List<int>();
            foreach (int i in current) if (set.Contains(i)) result.Add(i);   // Keep the main shape first if it stays
            foreach (int i in set) if (!result.Contains(i)) result.Add(i);
            SetSelection(result);
        }

        /// <summary>A plain click in the Select / Rotate / Scale tools; Shift adds or removes.</summary>
        private void ClickSelect(int hit)
        {
            if (hit < 0)
            {
                if (!ModifierKeys.HasFlag(Keys.Shift) && !ModifierKeys.HasFlag(Keys.Control)) SetSelection(new int[0]);
                return;
            }
            ApplySelection(new List<int> { hit }, true);
        }

        #region Box Select tool

        private void MarqueeMouseDown()
        {
            StopSpin();
            mMarqueeActive = true;
            mMarqueeStart = mCursor;
        }

        private void MarqueeMouseUp()
        {
            if (!mMarqueeActive) return;
            mMarqueeActive = false;
            List<ShapeScreenPoints> frame = mScene.BuildFrame();
            if (Math.Abs(mCursor.X - mMarqueeStart.X) + Math.Abs(mCursor.Y - mMarqueeStart.Y) < 4)
            {
                ClickSelect(mScene.Pick(mCursor, frame));
                return;
            }
            RectangleF box = MarqueeSelect.Box(mMarqueeStart, mCursor);
            List<int> hits = MarqueeSelect.Pick(frame, box, MarqueeSelect.IsCrossing(mMarqueeStart, mCursor));
            ApplySelection(hits, false);
            int n = SelectionCount;
            saveStatus.Text = n == 0 ? "Nothing selected" : n == 1 ? "1 shape selected" : n + " shapes selected";
        }

        private void DrawMarquee(Graphics g)
        {
            if (!mMarqueeActive) return;
            RectangleF box = MarqueeSelect.Box(mMarqueeStart, mCursor);
            bool crossing = MarqueeSelect.IsCrossing(mMarqueeStart, mCursor);
            // Window: solid blue. Crossing: dashed green (like SketchUp).
            Color c = crossing ? Color.FromArgb(30, 150, 60) : Color.FromArgb(30, 110, 220);
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(40, c)))
            using (Pen pen = new Pen(c, 1.5f))
            {
                if (crossing) pen.DashStyle = DashStyle.Dash;
                g.FillRectangle(fill, box);
                g.DrawRectangle(pen, box.X, box.Y, box.Width, box.Height);
            }
        }

        #endregion
    }
}
