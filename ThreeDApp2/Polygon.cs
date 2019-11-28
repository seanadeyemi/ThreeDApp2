using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ThreeDApp2
{
    public class Polygon 
	{
        //int m_n;
        protected List<Point3D> m_point = new List<Point3D> { };
        float EPSILON = 0.001f;
		public Point3D Center = new Point3D(0,0,0);
		public ShapeScreenPoints screenPoints = new ShapeScreenPoints();

		public Polygon()
        {

        }
        public Point3D this[int index]
        {
            get { return this.m_point[index]; }
            set { this.m_point[index] = value; }

            //ASSERT(index >= 0 && index<m_n);
            // return m_point[index];
        }

        public bool Add(Point3D P)
        {
            //int s = GetSize() + 1;
            // var size = SetSize(s);

            // if (size == false)
            // {
            //     return false;
            // }

            // int m = GetSize() - 1;

            //m_point[m] = P;
            m_point.Add(P);
            return true;
        }
        protected Point3D Point(int index)
        {
         //   Debug.Assert(index > 0 && index < m_point.Count);
            
            return m_point[index];
        }


        public int GetSize()
        {
            return m_point.Count;
          //  return m_n;
        }

        //public bool SetSize(int nSize)
        //{
        //    // Case 1: Set polygon as empty
        //    if (nSize == 0)
        //    {

        //        m_point = null;
        //        m_n = 0;
        //        return true;
        //    }

        //    // Case 2: Allocate new memory for polygon
        //   // if (m_point == null)
        //    //{
        //    //if()
        //    //    m_point = new Point3D[nSize];//(C3Point*)malloc(nSize * sizeof(C3Point));


        //        if (m_point == null)
        //        {
        //            m_n = 0;
        //            return false;/*Error(NULL, "Low on memory: \nUnable to allocate memory for %d points.", nSize);*/
        //        }
        //        else
        //        {
        //            m_n = nSize;
        //            return true;
        //        }
        //    //}

        //    // Case 3: Resize existing polygon

        //    // Try and resize memory. If it doesn't work - leave polygon
        //    // how it was to begin with.
        //    Point3D[] points = new Point3D[nSize];/*(C3Point*)realloc(m_point, nSize * sizeof(C3Point));*/
        //    if (points == null)
        //        return false;  //return Error(NULL, "Low on memory:\nUnable to resize polygon to %d points.", nSize);

        //    m_point = points;
        //    m_n = nSize;
        //    return true;
        //}

        public void RemoveAll()
        {
            m_point.Clear();
            //SetSize(0);
        }

        public bool Close()
        {
            if (!Closed())
            {
                // if (!SetSize(GetSize() + 1)) return false;
                //  m_point[GetSize() - 1] = m_point[0];
                m_point.Add(m_point[0]);
            }
            return true;
        }
        public bool Closed()
        {
            if (ABS(m_point[GetSize() - 1].X - m_point[0].X) > EPSILON) return false;
            if (ABS(m_point[GetSize() - 1].Y - m_point[0].Y) > EPSILON) return false;
            if (ABS(m_point[GetSize() - 1].Z - m_point[0].Z) > EPSILON) return false;
            return true;
        }

        public float ABS(float x)
        {
            return ((x) < D2Real(0.0) ? -(x) : (x));
        }
        public float D2Real(double x)
        {
            return (float)x;
        }
		//public int CompareTo(Polygon otherFace)
		//{
		//	return (int)(this.Center.Z - otherFace.Center.Z); //In order of which is closest to the screen
		//}
	}


}
