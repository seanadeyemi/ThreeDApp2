using System;
using System.Diagnostics;

namespace ThreeDApp2
{
    public class MyPolygon : Polygon, IComparable<MyPolygon>
    {
        public Point3D mPosition;
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

        public int CompareTo(MyPolygon otherFace)
        {
            return (int)(this.Center.Z - otherFace.Center.Z); //In order of which is closest to the screen
        }
    }
}
