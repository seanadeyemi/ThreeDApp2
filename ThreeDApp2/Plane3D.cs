using System;
using System.Collections.Generic;

namespace ThreeDApp2
{
    /// <summary>
    /// An infinite plane in model space, used as the surface the drawing tools draw on.
    /// U and V are unit vectors lying in the plane (used to build rectangles and circles).
    /// </summary>
    public class Plane3D
    {
        public Point3D Origin;
        public Point3D Normal;
        public Point3D U;
        public Point3D V;

        public Plane3D(Point3D origin, Point3D normal)
        {
            Origin = origin.Clone();
            Normal = normal.Normalized();
            ComputeAxes();
        }

        void ComputeAxes()
        {
            // Pick the model axis least parallel to the normal and flatten it onto the plane.
            Point3D[] axes = { new Point3D(1, 0, 0), new Point3D(0, 1, 0), new Point3D(0, 0, 1) };
            Point3D best = axes[0];
            float bestDot = float.MaxValue;
            foreach (Point3D a in axes)
            {
                float d = Math.Abs(Point3D.Dot(a, Normal));
                if (d < bestDot - 1e-6f) { bestDot = d; best = a; }
            }
            U = (best - Normal * Point3D.Dot(best, Normal)).Normalized();
            V = Point3D.Cross(Normal, U).Normalized();
        }

        /// <summary>A copy of this plane moved so it passes through <paramref name="p"/>.</summary>
        public Plane3D Through(Point3D p)
        {
            return new Plane3D(p, Normal);
        }

        public float SignedDistance(Point3D p)
        {
            return Point3D.Dot(p - Origin, Normal);
        }

        public Point3D ProjectPoint(Point3D p)
        {
            return p - Normal * SignedDistance(p);
        }

        public bool Contains(Point3D p, float eps = 1e-3f)
        {
            return Math.Abs(SignedDistance(p)) <= eps;
        }

        /// <summary>Intersects the ray origin + t*dir (t &gt; 0) with the plane.</summary>
        public bool IntersectRay(Point3D origin, Point3D dir, out Point3D hit)
        {
            hit = null;
            float denom = Point3D.Dot(Normal, dir);
            if (Math.Abs(denom) < 1e-6f) return false;
            float t = Point3D.Dot(Normal, Origin - origin) / denom;
            if (t <= 0) return false;
            hit = origin + dir * t;
            return true;
        }

        /// <summary>Index (0 = X, 1 = Y, 2 = Z) of the axis the normal points along, or -1.</summary>
        public int AlignedAxis()
        {
            if (Math.Abs(Math.Abs(Normal.X) - 1) < 1e-4f) return 0;
            if (Math.Abs(Math.Abs(Normal.Y) - 1) < 1e-4f) return 1;
            if (Math.Abs(Math.Abs(Normal.Z) - 1) < 1e-4f) return 2;
            return -1;
        }

        /// <summary>Best-fit plane of a polygon (Newell's method), or null if the points are collinear.</summary>
        public static Plane3D FromPoints(IList<Point3D> pts)
        {
            if (pts == null || pts.Count < 3) return null;
            double nx = 0, ny = 0, nz = 0, cx = 0, cy = 0, cz = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                Point3D a = pts[i];
                Point3D b = pts[(i + 1) % pts.Count];
                nx += (a.Y - b.Y) * (a.Z + b.Z);
                ny += (a.Z - b.Z) * (a.X + b.X);
                nz += (a.X - b.X) * (a.Y + b.Y);
                cx += a.X; cy += a.Y; cz += a.Z;
            }
            double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len < 1e-6) return null;
            return new Plane3D(new Point3D(cx / pts.Count, cy / pts.Count, cz / pts.Count),
                               new Point3D(nx / len, ny / len, nz / len));
        }
    }
}
