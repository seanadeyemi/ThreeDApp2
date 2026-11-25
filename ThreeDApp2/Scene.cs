using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ThreeDApp2
{
    /// <summary>
    /// Holds the layers, camera and orientation, and turns them into 2D screen polygons
    /// ordered back to front (painter's algorithm). Contains no WinForms code so the maths
    /// can be checked on its own.
    /// </summary>
    public class Scene
    {
        public List<MyPolygon> Layers = new List<MyPolygon>();
        public Point3D Camera = new Point3D(0, 0, 0);   // Our viewpoint
        public float ScreenZ = 30;                       // Distance to the projection plane
        public float Scale = 10;                         // Pixels per world unit on the projection plane
        public float NearPlane = 0.5f;                   // Layers closer than this to the camera are not drawn
        public Point3D Pivot = new Point3D(0, 0, 0);    // Point the layers rotate around
        public Matrix3D Orientation = Matrix3D.Identity();

        // Light comes from over the viewer's shoulder.
        static readonly double[] LightDir = Normalize(new double[] { -0.4, 0.5, -1.0 });

        public void Rotate(Axis axis, double degrees)
        {
            Orientation = Matrix3D.Rotation(axis, degrees) * Orientation;
        }

        public void ResetOrientation()
        {
            Orientation = Matrix3D.Identity();
        }

        /// <summary>Centre of all layer vertices (closing vertices ignored).</summary>
        public Point3D Centroid()
        {
            double x = 0, y = 0, z = 0;
            int n = 0;
            foreach (MyPolygon layer in Layers)
            {
                int count = VertexCount(layer);
                for (int j = 0; j < count; j++)
                {
                    Point3D p = layer.Absolute(j);
                    x += p.X; y += p.Y; z += p.Z; n++;
                }
            }
            return n == 0 ? new Point3D(0, 0, 0) : new Point3D(x / n, y / n, z / n);
        }

        public Point3D ToWorld(Point3D model)
        {
            return Orientation.Transform(model - Pivot) + Pivot;
        }

        /// <summary>
        /// Perspective projection onto the screen plane. Screen Y grows downwards, so world Y is
        /// flipped to keep "up" pointing up. Returns false for points too close to or behind the camera.
        /// </summary>
        public bool TryProject(Point3D world, out PointF screen)
        {
            float dz = world.Z - Camera.Z;
            if (dz < NearPlane)
            {
                screen = PointF.Empty;
                return false;
            }
            screen = new PointF(
                Scale * (world.X - Camera.X) * ScreenZ / dz,
                -Scale * (world.Y - Camera.Y) * ScreenZ / dz);
            return true;
        }

        /// <summary>
        /// Projects every layer and returns them sorted farthest first, ready to paint in order.
        /// Each entry remembers which layer it came from, so sorting never mixes layers up.
        /// </summary>
        public List<ShapeScreenPoints> BuildFrame()
        {
            List<ShapeScreenPoints> frame = new List<ShapeScreenPoints>();
            for (int i = 0; i < Layers.Count; i++)
            {
                MyPolygon layer = Layers[i];
                int count = VertexCount(layer);
                if (count < 2) continue;

                List<Point3D> world = new List<Point3D>();
                for (int j = 0; j < count; j++)
                    world.Add(ToWorld(layer.Absolute(j)));

                ShapeScreenPoints shape = new ShapeScreenPoints();
                shape.LayerIndex = i;
                bool visible = true;
                double depth = 0;
                foreach (Point3D w in world)
                {
                    PointF pt;
                    if (!TryProject(w, out pt)) { visible = false; break; }
                    shape.PtList.Add(pt);
                    depth += w.Z - Camera.Z;
                }
                if (!visible) continue;

                shape.Z = (float)(depth / count);
                shape.Shade = (float)Shade(world);
                frame.Add(shape);
            }
            // Stable sort: equal depths keep layer order instead of flickering between frames.
            return frame.OrderByDescending(f => f.Z).ThenBy(f => f.LayerIndex).ToList();
        }

        /// <summary>Number of distinct vertices, i.e. without the repeated closing vertex.</summary>
        public static int VertexCount(Polygon polygon)
        {
            int n = polygon.GetSize();
            if (n > 1 && polygon.Closed()) n--;
            return n;
        }

        /// <summary>Simple two-sided Lambert shading from the polygon's normal (Newell's method).</summary>
        static double Shade(List<Point3D> pts)
        {
            double nx = 0, ny = 0, nz = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                Point3D a = pts[i];
                Point3D b = pts[(i + 1) % pts.Count];
                nx += (a.Y - b.Y) * (a.Z + b.Z);
                ny += (a.Z - b.Z) * (a.X + b.X);
                nz += (a.X - b.X) * (a.Y + b.Y);
            }
            double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len < 1e-9) return 1.0;
            double dot = Math.Abs((nx * LightDir[0] + ny * LightDir[1] + nz * LightDir[2]) / len);
            return 0.35 + 0.65 * dot;
        }

        static double[] Normalize(double[] v)
        {
            double len = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            return new double[] { v[0] / len, v[1] / len, v[2] / len };
        }
    }
}
