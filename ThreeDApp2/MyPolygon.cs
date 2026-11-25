using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDApp2
{
    public class MyPolygon : Polygon
    {
        public Point3D mPosition;

        /// <summary>Display name, e.g. "Rectangle 4".</summary>
        public string Name = "";

        /// <summary>Fill / line colour. Color.Empty means "use the layer palette".</summary>
        public Color Color = Color.Empty;

        /// <summary>Faces made by one push/pull share a group id so they select and move together. 0 = no group.</summary>
        public int GroupId;

        /// <summary>Not drawn (used while a polyline is being continued). Not saved.</summary>
        public bool Hidden;

        public MyPolygon()
        {
            mPosition = new Point3D(0, 0, 0);
        }

        /// <summary>Deep copy: new vertex objects, same name, colour and group.</summary>
        public MyPolygon Clone()
        {
            MyPolygon copy = new MyPolygon();
            copy.mPosition = mPosition.Clone();
            foreach (Point3D p in m_point)
                copy.Add(p.Clone());
            copy.Name = Name;
            copy.Color = Color;
            copy.GroupId = GroupId;
            return copy;
        }

        /// <summary>True when the last vertex repeats the first, i.e. the outline is a filled face.</summary>
        public bool IsClosedFace
        {
            get { return GetSize() >= 4 && Closed(); }
        }

        public void PtChange(Point3D pt)
        {
            //return m_point[index];
            m_point.ForEach(x => {
                x.X += pt.X;
                x.Y += pt.Y;
                x.Z += pt.Z;

            });

        }

        public void PtChange(float x, float y, float z)
        {
            //return m_point[index];
            m_point.ForEach(p => {
                p.X += x;
                p.Y += y;
                p.Z += z;

            });

        }

        public Point3D Absolute(int j)
        {
            return mPosition + Point(j);
        }
    }
}
