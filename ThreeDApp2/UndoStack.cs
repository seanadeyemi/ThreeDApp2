using System;
using System.Collections.Generic;
using System.Drawing;

namespace ThreeDApp2
{
    /// <summary>
    /// Snapshot undo/redo. Each snapshot packs every shape's points into one float array, which is
    /// several times faster and smaller than copying thousands of point objects, and the stack drops
    /// its oldest steps when big models would otherwise use too much memory.
    /// </summary>
    public class UndoStack
    {
        class Packed
        {
            public float[] Points;      // x, y, z of every raw point (closing vertex included)
            public float PosX, PosY, PosZ;
            public string Name;
            public Color Color;
            public int GroupId;
            public bool Hidden;
        }

        class Snapshot
        {
            public Packed[] Shapes;
            public long Floats;
        }

        readonly List<Snapshot> mUndo = new List<Snapshot>();
        readonly List<Snapshot> mRedo = new List<Snapshot>();
        public int Limit = 100;

        /// <summary>Most point coordinates kept across all steps (about 120 MB); older steps are dropped beyond it.</summary>
        public long MaxFloats = 30000000;

        public bool CanUndo { get { return mUndo.Count > 0; } }
        public bool CanRedo { get { return mRedo.Count > 0; } }
        public int UndoCount { get { return mUndo.Count; } }

        static Snapshot Pack(List<MyPolygon> layers)
        {
            Snapshot s = new Snapshot { Shapes = new Packed[layers.Count] };
            for (int i = 0; i < layers.Count; i++)
            {
                MyPolygon p = layers[i];
                List<Point3D> pts = p.RawPoints;
                float[] f = new float[pts.Count * 3];
                for (int j = 0, o = 0; j < pts.Count; j++, o += 3) { f[o] = pts[j].X; f[o + 1] = pts[j].Y; f[o + 2] = pts[j].Z; }
                s.Shapes[i] = new Packed
                {
                    Points = f,
                    PosX = p.mPosition.X, PosY = p.mPosition.Y, PosZ = p.mPosition.Z,
                    Name = p.Name, Color = p.Color, GroupId = p.GroupId, Hidden = p.Hidden,
                };
                s.Floats += f.Length + 8;
            }
            return s;
        }

        static List<MyPolygon> Unpack(Snapshot s)
        {
            List<MyPolygon> layers = new List<MyPolygon>(s.Shapes.Length);
            foreach (Packed p in s.Shapes)
            {
                MyPolygon shape = new MyPolygon();
                shape.mPosition = new Point3D(p.PosX, p.PosY, p.PosZ);
                for (int o = 0; o < p.Points.Length; o += 3) shape.Add(new Point3D(p.Points[o], p.Points[o + 1], p.Points[o + 2]));
                shape.Name = p.Name;
                shape.Color = p.Color;
                shape.GroupId = p.GroupId;
                shape.Hidden = p.Hidden;
                layers.Add(shape);
            }
            return layers;
        }

        /// <summary>A quick deep copy of a shape list (used to save in the background).</summary>
        public static List<MyPolygon> Copy(List<MyPolygon> layers)
        {
            return Unpack(Pack(layers));
        }

        void Trim()
        {
            while (mUndo.Count > Limit) mUndo.RemoveAt(0);
            long total = 0;
            foreach (Snapshot s in mUndo) total += s.Floats;
            foreach (Snapshot s in mRedo) total += s.Floats;
            while (total > MaxFloats && mUndo.Count > 1) { total -= mUndo[0].Floats; mUndo.RemoveAt(0); }
        }

        public void Push(List<MyPolygon> current)
        {
            mUndo.Add(Pack(current));
            mRedo.Clear();
            Trim();
        }

        public void DiscardLast()
        {
            if (mUndo.Count > 0) mUndo.RemoveAt(mUndo.Count - 1);
        }

        public List<MyPolygon> Undo(List<MyPolygon> current)
        {
            if (!CanUndo) return current;
            mRedo.Add(Pack(current));
            Snapshot s = mUndo[mUndo.Count - 1];
            mUndo.RemoveAt(mUndo.Count - 1);
            return Unpack(s);
        }

        public List<MyPolygon> Redo(List<MyPolygon> current)
        {
            if (!CanRedo) return current;
            mUndo.Add(Pack(current));
            Snapshot s = mRedo[mRedo.Count - 1];
            mRedo.RemoveAt(mRedo.Count - 1);
            Trim();
            return Unpack(s);
        }

        public void Clear()
        {
            mUndo.Clear();
            mRedo.Clear();
        }
    }
}
