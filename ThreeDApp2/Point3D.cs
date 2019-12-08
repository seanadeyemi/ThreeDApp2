using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDApp2
{
    public class Point3D
    {
        public float X, Y, Z;
        public const float EPSILON = 0.001f;	
        public Point3D(double v1, double v2, double v3)
        {
            X = D2Real(v1);
            Y = D2Real(v2);
            Z = D2Real(v3);
        }

        public static Point3D operator + (Point3D A, Point3D P)
        {
            return new Point3D(A.X + P.X, A.Y + P.Y, A.Z + P.Z);
        }

      

        public static Point3D operator - (Point3D A, Point3D P)
        {
            return new Point3D(A.X - P.X, A.Y - P.Y, A.Z - P.Z);
        }

        public static float operator * (Point3D P, Point3D M)
        {
            return M.X * P.X + M.Y * P.Y + M.Z * P.Z;

        } // dot product
        public static Point3D operator * (float f, Point3D P)
        {
            return new Point3D(P.X * f, P.Y * f, P.Z * f);
        } // scalar product

        public float D2Real(double x)
        {
            return (float)x;
        }

        public void Scale(float factor)
        {
            X *= factor;
            Y *= factor;
            Z *= factor;
        }
        float Length()
        {
            return D2Real(Math.Sqrt(X * X + Y * Y + Z * Z));
        }



        public static Point3D RotateX(Point3D point3D, double degrees)
        {
            //Here we use Euler's matrix formula for rotating a 3D point x degrees around the x-axis
            //[ a  b  c ] [ x ]   [ x*a + y*b + z*c ]
            //[ d  e  f ] [ y ] = [ x*d + y*e + z*f ]
            //[ g  h  i ] [ z ]   [ x*g + y*h + z*i ]
            //[ 1    0        0   ]
            //[ 0   cos(x)  sin(x)]
            //[ 0   -sin(x) cos(x)]
            double cDegrees = (Math.PI * degrees) / 180.0f; //Convert degrees to radian for .Net Cos/Sin functions
            double cosDegrees = Math.Cos(cDegrees);
            double sinDegrees = Math.Sin(cDegrees);
            double y = (point3D.Y * cosDegrees) + (point3D.Z * sinDegrees);
            double z = (point3D.Y * -sinDegrees) + (point3D.Z * cosDegrees);
            return new Point3D(point3D.X, y, z);
        }
        public static Point3D RotateY(Point3D point3D, double degrees)
        {
            //Y-axis
            //[ cos(x)   0    sin(x)]
            //[   0      1      0   ]
            //[-sin(x)   0    cos(x)]
            double cDegrees = (Math.PI * degrees) / 180.0; //Radians
            double cosDegrees = Math.Cos(cDegrees);
            double sinDegrees = Math.Sin(cDegrees);
            double x = (point3D.X * cosDegrees) + (point3D.Z * sinDegrees);
            double z = (point3D.X * -sinDegrees) + (point3D.Z * cosDegrees);
            return new Point3D(x, point3D.Y, z);
        }
        public static Point3D RotateZ(Point3D point3D, double degrees)
        {
            //Z-axis
            //[ cos(x)  sin(x) 0]
            //[ -sin(x) cos(x) 0]
            //[    0     0     1]
            double cDegrees = (Math.PI * degrees) / 180.0; //Radians
            double cosDegrees = Math.Cos(cDegrees);
            double sinDegrees = Math.Sin(cDegrees);
            double x = (point3D.X * cosDegrees) + (point3D.Y * sinDegrees);
            double y = (point3D.X * -sinDegrees) + (point3D.Y * cosDegrees);
            return new Point3D(x, y, point3D.Z);
        }
        public static Point3D Translate(Point3D points3D, Point3D oldOrigin, Point3D newOrigin)
        {
            //Moves a 3D point based on a moved reference point
            Point3D difference = new Point3D(newOrigin.X - oldOrigin.X, newOrigin.Y - oldOrigin.Y, newOrigin.Z - oldOrigin.Z);
            points3D.X += difference.X;
            points3D.Y += difference.Y;
            points3D.Z += difference.Z;
            return points3D;
        }

        public static Point3D MoveObject(Point3D fromPoint, Point3D position2)
        {
            
            float x = fromPoint.X + (position2.X - fromPoint.X);
            float y = fromPoint.Y + (position2.Y - fromPoint.Y);
            float z = fromPoint.Z + (position2.Z - fromPoint.Z);
            return new Point3D(x,y,z);
        }

        public Point3D Translate(Vector v)
        {
            Point3D pt3d = this;
            return pt3d + v;
        }

       public static bool Between(Point3D a, Point3D b, Point3D c)
        // Returns TRUE iff (a,b,c) are collinear and point c lies on the closed segement ab.
        {
            var col = Colinear(a, b, c);
            if (!col)
                return false;

            /* If ab not vertical, check betweenness on x; else on y. */
            if (a.X != b.X)
                return ((a.X <= c.X) && (c.X <= b.X)) ||
                        ((a.X >= c.X) && (c.X >= b.X));
            else
                return ((a.Y <= c.Y) && (c.Y <= b.Y)) ||
                        ((a.Y >= c.Y) && (c.Y >= b.Y));
        }

        public static bool Colinear(Point3D a, Point3D b, Point3D c)
        {
            // return (ABS(TriArea2(a, b, c)) < EPSILON);	// inefficient
            float CrossZ = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
            return (Math.Abs(CrossZ) < EPSILON);
        }

     

        //These are to make the above functions workable with arrays of 3D points
        public static Point3D[] RotateX(Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateX(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] RotateY(Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateY(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] RotateZ(Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateZ(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] Translate(Point3D[] points3D, Point3D oldOrigin, Point3D newOrigin)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = Translate(points3D[i], oldOrigin, newOrigin);
            }
            return points3D;
        }

       public float Length2()
        {
            return (X * X + Y * Y + Z * Z);
        }

        public static float distance (Point3D pt1, Point3D pt2)
        {
            return (float)Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2) + Math.Pow(pt1.Z - pt2.Z, 2));
                
        }

        public static Vector Diff(Point3D farPt, Point3D nearPt)
        {
            Vector vector = new Vector(0, 0, 0);
            vector.X = farPt.X - nearPt.X;
            vector.Y = farPt.Y - nearPt.Y;
            vector.Z = farPt.Z - nearPt.Z;

            return vector;

        }
    }
}

