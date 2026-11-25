using System.Collections.Generic;

namespace ThreeDApp2
{
    /// <summary>Snapshot based undo/redo of the scene's shape list.</summary>
    public class UndoStack
    {
        readonly List<List<MyPolygon>> mUndo = new List<List<MyPolygon>>();
        readonly List<List<MyPolygon>> mRedo = new List<List<MyPolygon>>();
        public int Limit = 100;

        public bool CanUndo { get { return mUndo.Count > 0; } }
        public bool CanRedo { get { return mRedo.Count > 0; } }

        public static List<MyPolygon> Snapshot(List<MyPolygon> layers)
        {
            List<MyPolygon> copy = new List<MyPolygon>(layers.Count);
            foreach (MyPolygon p in layers)
            {
                MyPolygon c = p.Clone();
                copy.Add(c);
            }
            return copy;
        }

        /// <summary>Call before changing the shapes.</summary>
        public void Push(List<MyPolygon> current)
        {
            mUndo.Add(Snapshot(current));
            if (mUndo.Count > Limit) mUndo.RemoveAt(0);
            mRedo.Clear();
        }

        /// <summary>Drops the newest snapshot (used when an operation turned out to change nothing).</summary>
        public void DiscardLast()
        {
            if (mUndo.Count > 0) mUndo.RemoveAt(mUndo.Count - 1);
        }

        public List<MyPolygon> Undo(List<MyPolygon> current)
        {
            if (!CanUndo) return current;
            mRedo.Add(Snapshot(current));
            List<MyPolygon> s = mUndo[mUndo.Count - 1];
            mUndo.RemoveAt(mUndo.Count - 1);
            return s;
        }

        public List<MyPolygon> Redo(List<MyPolygon> current)
        {
            if (!CanRedo) return current;
            mUndo.Add(Snapshot(current));
            List<MyPolygon> s = mRedo[mRedo.Count - 1];
            mRedo.RemoveAt(mRedo.Count - 1);
            return s;
        }

        public void Clear()
        {
            mUndo.Clear();
            mRedo.Clear();
        }
    }
}
