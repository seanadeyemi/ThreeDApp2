using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDApp2
{
    public class ShapeScreenPoints
    {
        public List<Point> PtList = new List<Point>();
        float EPSILON = 0.001f;

        public bool Between(Point a, Point b, Point c)
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

        public bool Colinear(Point a, Point b, Point c)
        {
            // return (ABS(TriArea2(a, b, c)) < EPSILON);	// inefficient
            float CrossZ = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
            return (Math.Abs(CrossZ) < EPSILON);
        }
    }
}
