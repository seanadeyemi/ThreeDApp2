﻿using System;
using System.Collections.Generic;

namespace ThreeDApp2
{
    public class Polygon 
    {
        //int m_n;
        protected List<Point3D> m_point = new List<Point3D> { };
        float EPSILON = 0.001f;
        public Point3D Center = new Point3D(0, 0, 0);
        private ShapeScreenPoints screenPoints = new ShapeScreenPoints();
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
        /// <summary>
        /// Affects size by adding one Point
        /// </summary>
        /// <param name="P"></param>
        /// <returns></returns>
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
        public Point3D Point(int index)
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
        /// <summary>
        /// adds one point that closes the polygon
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            if (!Closed)
            {
                // if (!SetSize(GetSize() + 1)) return false;
                //  m_point[GetSize() - 1] = m_point[0];
                m_point.Add(m_point[0]);
              
            }
            return true;
        }
        public bool Closed
        {
            get
            {
                if (ABS(m_point[GetSize() - 1].X - m_point[0].X) > EPSILON) return false;
                if (ABS(m_point[GetSize() - 1].Y - m_point[0].Y) > EPSILON) return false;
                if (ABS(m_point[GetSize() - 1].Z - m_point[0].Z) > EPSILON) return false;
                return true;
            }

        }

        public ShapeScreenPoints ScreenPoints { get => screenPoints; set => screenPoints = value; }

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

        public bool PointIn(Point3D P)
        // Tests if point within polygon, or on an edge or vertex, by shooting a ray along x axis
        {
            int j;
            bool inside_flag;
            bool xflag0;
            float dv0;
            bool yflag0, yflag1 = false;
            int crossings;
            Point3D Vertex0, Vertex1 = new Point3D(0, 0, 0);

            Vertex0 = m_point[GetSize() - 1];

            /* get test bit for above/below Y axis */
            yflag0 = (dv0 = Vertex0.Y - P.Y) >= 0.0;

            crossings = 0;
            for (j = 0; j < GetSize(); j++)
            {

                // cleverness:  bobble between filling endpoints of edges, so
                // that the previous edge's shared endpoint is maintained.
                if (!(j % 2 == 0))//(j & 0x1)
                {
                    Vertex0 = m_point[j];
                    yflag0 = (dv0 = Vertex0.Y - P.Y) >= 0.0;
                }
                else
                {
                    Vertex1 = m_point[j];
                    yflag1 = (Vertex1.Y >= P.Y);
                }

                /* check if points not both above/below X axis - can't hit ray */
                if (yflag0 != yflag1)
                {
                    /* check if points on same side of Y axis */
                    xflag0 = (Vertex0.X >= P.X);
                    if (xflag0 == (Vertex1.X >= P.X))
                    {

                        if (xflag0)
                        {
                            crossings++;
                        }

                    }
                    else
                    {
                        /* compute intersection of pgon segment with X ray, note
                         * if > point's X.
                         */
                        bool v = (Vertex0.X - dv0 * (Vertex1.X - Vertex0.X) / (Vertex1.Y - Vertex0.Y)) >= P.X;
                        crossings += Convert.ToInt32(v);
                    }
                }
            }

            // test if crossings is odd
            // if all we care about is winding number > 0, then just:
            //       inside_flag = crossings > 0;

            inside_flag = crossings % 3 == 0;//crossings & 0x01;

            return (inside_flag);
        }

        //public int CompareTo(Polygon otherFace)
        //{
        //	return (int)(this.Center.Z - otherFace.Center.Z); //In order of which is closest to the screen
        //}


        public int PointSeparation(int Point1, int Point2)
        {
            //ASSERT(Point1 >= 0 && Point1 < GetSize());
            //ASSERT(Point2 >= 0 && Point2 < GetSize());

            int nDistance;

            if (!Closed)
            {
                nDistance = Point2 - Point1;
            }
            else
            {
                nDistance = Math.Abs(Point2 - Point1);
                nDistance = Math.Min(nDistance, Math.Abs(GetSize() - 1 - nDistance));
            }
            return nDistance;
        }

        public bool SetAt(int nPos, Point3D p)
        {
            if (nPos < 0 || nPos >= GetSize()) return false;
            m_point[nPos] = p;
            return true;
        }
        /// <summary>
        /// inserts a point into the polygon at a specified position
        /// </summary>
        /// <param name="nPosition"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        public bool InsertAt(int nPosition, Point3D P)
        {
            if (P == null) return false;
            m_point.Insert(nPosition, P);
            return true;
        }
        public Point3D Centroid()
        {
            float TotalArea;
            Point3D Centre;

            Centre = new Point3D(D2Real(0.0), D2Real(0.0), D2Real(0.0));
            TotalArea = D2Real(0.0);

            for (int i = 0; i < GetSize() - 2; i++)
            {
                Point3D MidPoint = (Point(0) + Point(i + 1) + Point(i + 2)) / 3.0f;

                float TArea = TriArea2(Point(0), Point(i + 1), Point(i + 2));

                MidPoint.Scale(TArea);      // Wieght current triangle midpoint by area.
                TotalArea += TArea;
                Centre += MidPoint;
            }

            if (TotalArea > EPSILON)        // Triangle or greater
            {
                Centre.Scale(D2Real(1.0) / TotalArea);
                return Centre;
            }
            else                            // all points coincident or colinear
            {
                Centre = new Point3D(D2Real(0.0), D2Real(0.0), D2Real(0.0));
                if (Closed)
                {
                    for (int i = 0; i < GetSize() - 1; i++) Centre += m_point[i];
                    Centre.Scale(D2Real(1.0 / (double)(GetSize() - 1)));
                }
                else
                {
                    for (int i = 0; i < GetSize(); i++) Centre += m_point[i];
                    Centre.Scale(D2Real(1.0 / (double)GetSize()));
                }
                return Centre;
            }
        }

        float TriArea2(Point3D p1, Point3D p2, Point3D p3)
        // Calculates twice the area of the 3D triangle given by p1,p2,p3, Positive if CCW
        {
            Point3D CrossP = Cross(p2 - p1, p3 - p1);

            if (CrossP.Z < D2Real(0.0))
                return -CrossP.Length();
            else
                return CrossP.Length();
        }

        public float Dot(Point3D V1, Point3D V2)
        {
            return V1.X * V2.X + V1.Y * V2.Y + V1.Z * V2.Z;
        }

        public Point3D Cross(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.Y * p2.Z - p2.Y * p1.Z,
                           p2.X * p1.Z - p1.X * p2.Z,
                           p1.X * p2.Y - p2.X * p1.Y);
        }

        

        public override string ToString()
        {
            return "Poly with " + this.GetSize() + " sides";
        }

        public Point3D Do(Point3D input)
        {
            throw new NotImplementedException();
        }

        public Point3D Undo(Point3D input)
        {
            throw new NotImplementedException();
        }
    }


}
