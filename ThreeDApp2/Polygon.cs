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
        const float DEG2RAD = 0.0175f;           // converts degrees to radians
        const float RAD2DEG = 57.2957795f;		 // ...and back again.
        /*const float EPSILON = 0.001f;*/	 // used for REAL comparisons and stuff
        const float PI = 3.1415926535898f;	 // pi
        const float HALF_PI = PI / 2.0f;		 // pi/2
        const float TWO_PI = PI * 2.0f;			 // pi*2
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

        public bool Contains(Point3D p)
        {
            return m_point.Contains(p);
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

        public bool Between(Point3D a, Point3D b, Point3D c)
        // Returns TRUE iff (a,b,c) are collinear and point c lies on the closed segement ab.
        {
            if (!Colinear(a, b, c))
                return false;

            /* If ab not vertical, check betweenness on x; else on y. */
            if (a.X != b.X)
                return ((a.X <= c.X) && (c.X <= b.X)) ||
                        ((a.X >= c.X) && (c.X >= b.X));
            else
                return ((a.Y <= c.Y) && (c.Y <= b.X)) ||
                        ((a.Y >= c.Y) && (c.Y >= b.Y));
        }

        public bool Colinear(Point3D a, Point3D b, Point3D c)
        {
            // return (ABS(TriArea2(a, b, c)) < EPSILON);	// inefficient
            float CrossZ = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
            return (ABS(CrossZ) < EPSILON);
        }

        public float ABS(float x)
        {
            return ((x) < D2Real(0.0) ? -(x) : (x));
        }
        public float D2Real(double x)
        {
            return (float)x;
        }

        public float Angle(Point3D p1, Point3D p2, Point3D p3)
        // returns the angle (in degrees) between the 2 vectors formed from p2->p1, p2->p3.
        {
            double cosA;
            Point3D v, u;

            v = p2 - p1;
            u = p3 - p2;

            cosA = u * v / D2Real(Math.Sqrt((double)(u.Length2() * v.Length2())));
            return D2Real(Math.Max(0.0, Math.Acos(cosA) * RAD2DEG));
        }

        //public int CompareTo(Polygon otherFace)
        //{
        //	return (int)(this.Center.Z - otherFace.Center.Z); //In order of which is closest to the screen
        //}
    }


}
