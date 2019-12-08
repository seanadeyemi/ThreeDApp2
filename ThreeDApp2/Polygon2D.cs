using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDApp2
{
    public class Polygon2D
    {
        protected List<Point> m_point = new List<Point> { };

        public bool PointIn(Point P)
        // Tests if point within polygon, or on an edge or vertex, by shooting a ray along x axis
        {
            int j;
            bool inside_flag;
            bool xflag0;
            float dv0;
            bool yflag0, yflag1 = false;
            int crossings;
            Point Vertex0, Vertex1 = Point.Empty;//new Point(0, 0, 0);

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

        public int GetSize()
        {
            return m_point.Count;
            
        }
        public bool Add(Point P)
        {
            m_point.Add(P);
            return true;
        }

        public bool IsPointInPolygon(Point[] polygon, Point point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
    }
}
