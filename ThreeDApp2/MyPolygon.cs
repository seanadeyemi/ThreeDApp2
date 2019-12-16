using System;

namespace ThreeDApp2
{
    public class MyPolygon : Polygon, IComparable<MyPolygon>
    {
        public Point3D mPosition;
        const float EPSILON = 0.001f;

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
            return (int)(this.Centroid().Z - otherFace.Centroid().Z); //In order of which is closest to the screen
        }
    }
}
