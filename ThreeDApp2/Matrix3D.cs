using System;

namespace ThreeDApp2
{
    /// <summary>
    /// A 3x3 rotation matrix. The scene keeps one of these as its overall orientation so that
    /// rotations are applied to the untouched model points every frame, instead of rotating the
    /// already-rotated points again (which slowly distorts the shapes through float rounding).
    /// The rotation conventions match <see cref="Point3D.RotateX(Point3D, double)"/> and friends.
    /// </summary>
    public class Matrix3D
    {
        readonly double[,] m = new double[3, 3];

        public double this[int row, int col]
        {
            get { return m[row, col]; }
            set { m[row, col] = value; }
        }

        public static Matrix3D Identity()
        {
            Matrix3D r = new Matrix3D();
            r[0, 0] = r[1, 1] = r[2, 2] = 1;
            return r;
        }

        public static Matrix3D Rotation(Axis axis, double degrees)
        {
            double rad = (Math.PI * degrees) / 180.0;
            double c = Math.Cos(rad);
            double s = Math.Sin(rad);
            Matrix3D r = Identity();
            switch (axis)
            {
                case Axis.X:
                    r[1, 1] = c; r[1, 2] = s;
                    r[2, 1] = -s; r[2, 2] = c;
                    break;
                case Axis.Y:
                    r[0, 0] = c; r[0, 2] = s;
                    r[2, 0] = -s; r[2, 2] = c;
                    break;
                case Axis.Z:
                    r[0, 0] = c; r[0, 1] = s;
                    r[1, 0] = -s; r[1, 1] = c;
                    break;
            }
            return r;
        }

        public static Matrix3D operator *(Matrix3D a, Matrix3D b)
        {
            Matrix3D r = new Matrix3D();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < 3; k++)
                        sum += a[i, k] * b[k, j];
                    r[i, j] = sum;
                }
            return r;
        }

        public Point3D Transform(Point3D p)
        {
            return new Point3D(
                m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2] * p.Z,
                m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2] * p.Z,
                m[2, 0] * p.X + m[2, 1] * p.Y + m[2, 2] * p.Z);
        }
    }
}
