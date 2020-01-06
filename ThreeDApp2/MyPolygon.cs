using System;
using System.Collections.Generic;

namespace ThreeDApp2
{
    public class MyPolygon : Polygon, IComparable<MyPolygon>, IEquatable<MyPolygon>
    {
        public Point3D mPosition;
        const float EPSILON = 0.001f;

        public int parentPointInPolyIndex { get; internal set; } = -1;
        public bool IsOnLine { get; internal set; } = false;
        public int HiliteIndex1 { get; internal set; } = -1;
        public int HiliteIndex2 { get; internal set; } = -1;
        public int linePolygonIndex { get; internal set; } = -1;

        public MyPolygon()
        {
            mPosition = new Point3D(0, 0, 0);
        }


        public void PtChange(Point3D pt)
        {
            //return m_point[index];
            m_point.ForEach(x =>
            {
                x.X += pt.X;
                x.Y += pt.Y;
                x.Z += pt.Z;

            });

        }

        public void PtChange(float x, float y, float z)
        {
            //return m_point[index];
            m_point.ForEach(p =>
            {
                p.X += x;
                p.Y += y;
                p.Z += z;

            });

        }

        public Point3D Absolute(int j)
        {
            try
            {

                //var name = new StackFrame(1).GetMethod().Name;
                //Console.WriteLine(name);
                //var ln = new StackFrame(1).GetFileLineNumber();
                //Console.WriteLine(ln);

                return mPosition + Point(j);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        //public bool Closed()
        //{
        //    if (ABS(m_point[GetSize() - 1].X - m_point[0].X) > EPSILON) return false;
        //    if (ABS(m_point[GetSize() - 1].Y - m_point[0].Y) > EPSILON) return false;
        //    if (ABS(m_point[GetSize() - 1].Z - m_point[0].Z) > EPSILON) return false;
        //    return true;
        //}



        public int CompareTo(MyPolygon otherFace)
        {
            //  return (int)(this.Center.Z - otherFace.Center.Z); //In order of which is closest to the screen
            //return Math.Ceiling(this.Centroid().Z - otherFace.Centroid().Z); //In order of which is closest to the screen

            return Math.Sign(this.Centroid().Z - otherFace.Centroid().Z);


        }

        public bool Equals(MyPolygon other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;

            }

            if(this.GetSize() != other.GetSize())
            {
                return false;
            }

            List<bool> vals = new List<bool>();

            for (int j = 0; j < other.GetSize(); j++)
            {
                if (this[j].X == other[j].X && this[j].Y == other[j].Y && this[j].Z == other[j].Z)
                {
                    vals.Add(true);
                }
                else
                {
                    vals.Add(false);
                }
            }

            if (vals.TrueForAll(i => { return i == true; }) && vals.Count == other.GetSize())
            {
                return true;

            }
            return false;

        }
        public static bool operator ==(MyPolygon poly1, MyPolygon poly2)
        {
            if (ReferenceEquals(poly1, poly2))
            {
                return true;
            }

            if (ReferenceEquals(poly1, null))
            {
                return false;
            }
            if (ReferenceEquals(poly2, null))
            {
                return false;
            }

            if(poly1.GetSize() != poly2.GetSize())
            {
                return false;
            }

            int ptsFound = 0;
            for (int i = 0; i < poly1.GetSize(); i++)
            {
                if ((poly1[i].X == poly2[i].X) && (poly1[i].Y == poly2[i].Y) && (poly1[i].Z == poly2[i].Z))
                {
                    ptsFound++;
                }
            }

            if (ptsFound == poly1.GetSize() && ptsFound == poly2.GetSize())
            {
                return true;
            }
            return false;

        }

        public static bool operator !=(MyPolygon poly1, MyPolygon poly2)
        {
            return !(poly1 == poly2);
        }
    }
}
