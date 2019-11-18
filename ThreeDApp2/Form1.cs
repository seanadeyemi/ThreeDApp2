using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ThreeDApp2
{
    public partial class Form1 : Form
    {
        Point3D mCamera;
        Point3D mScreen;
        MyPolygon[] mShape = new MyPolygon[3];
        ShapeScreenPoints[] screenPoints = new ShapeScreenPoints[3];
        Point3D point0 = new Point3D(0, 0, 0); //Used for reference
        Point3D shapeOrigin = new Point3D(0, 0, 0);

        int s = 10;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;

            shapeOrigin.X = 0;//ClientRectangle.Width / 2;
            shapeOrigin.Y = 10;
            shapeOrigin.Z = 30;


            mCamera = new Point3D(0, 0, 0);  // Our viewpoint
            //mScreen = new Point3D(0, 0, 30); // Where we project the image to
            mScreen = new Point3D(0, 0, shapeOrigin.Z); // Where we project the image to
            mShape[0] = new MyPolygon();
            mShape[1] = new MyPolygon();
            mShape[2] = new MyPolygon();


            screenPoints[0] = new ShapeScreenPoints();
            screenPoints[1] = new ShapeScreenPoints();
            screenPoints[2] = new ShapeScreenPoints();

            //mShape[0].SetSize(1);
            //mShape[1].SetSize(1);
            //mShape[2].SetSize(1);

            //mShape[0].RemoveAll();
            //mShape[1].RemoveAll();
            //mShape[2].RemoveAll();


            // Define a shape
            mShape[0].Add(new Point3D(0, 10, 20));
            mShape[0].Add(new Point3D(0, 0, 20));
            mShape[0].Add(new Point3D(10, 0, 20));
            mShape[0].Add(new Point3D(10, 10, 20));
            mShape[0].Add(new Point3D(5, 15, 20));
            mShape[0].Close();

            Set2DPoints(mShape[0], ref screenPoints[0].PtList);

            // Shapes 1 and 2 are exactly the same as Shape 0 ...
            //mShape[1] = mShape[0];
            //mShape[2] = mShape[0];

            mShape[1].Add(new Point3D(0, 10, 30));
            mShape[1].Add(new Point3D(0, 0, 30));
            mShape[1].Add(new Point3D(10, 0, 30));
            mShape[1].Add(new Point3D(10, 10, 30));
            mShape[1].Add(new Point3D(5, 15, 30));
            mShape[1].Close();

            Set2DPoints(mShape[1], ref screenPoints[1].PtList);


            mShape[2].Add(new Point3D(0, 10, 50));
            mShape[2].Add(new Point3D(0, 0, 50));
            mShape[2].Add(new Point3D(10, 0, 50));
            mShape[2].Add(new Point3D(10, 10, 50));
            mShape[2].Add(new Point3D(5, 15, 50));
            mShape[2].Close();

            Set2DPoints(mShape[2], ref screenPoints[2].PtList);


            // ... but at different positions
            //mShape[1].PtChange(0, 0, 30);
            //mShape[2].PtChange(0, 0, 50);

            //mShape[1].mPosition.z = 30;
            //mShape[2].mPosition.z = 50;

        }
        private void Update2dPoints()
        {
            Set2DPoints(mShape[0], ref screenPoints[0].PtList);
            Set2DPoints(mShape[1], ref screenPoints[1].PtList);
            Set2DPoints(mShape[2], ref screenPoints[2].PtList);
        }

        private void RotateShapes(Axis axis, double degrees)
        {
            for (int i = 0; i < 3; i++)
            {
                // Go thru each point
                MyPolygon po = new MyPolygon();
                List<Point> ptList = new List<Point>();
                for (int j = 0; j < mShape[i].GetSize(); j++)
                {
                    Point3D pt3d = mShape[i].Absolute(j);

                    Point3D rotPt3d = new Point3D(0, 0, 0);
                    switch (axis)
                    {
                        case Axis.X:
                            pt3d = Point3D.Translate(pt3d, shapeOrigin, point0);
                            pt3d = Point3D.RotateX(pt3d, degrees);
                            rotPt3d = Point3D.Translate(pt3d, point0, shapeOrigin);
                            break;
                        case Axis.Y:
                            pt3d = Point3D.Translate(pt3d, shapeOrigin, point0);
                            pt3d = Point3D.RotateY(pt3d, degrees);
                            rotPt3d = Point3D.Translate(pt3d, point0, shapeOrigin);
                            //rotPt3d = Point3D.RotateY(pt3d, degrees);
                            break;                           
                        case Axis.Z:
                            //rotPt3d = Point3D.RotateZ(pt3d, degrees);
                            pt3d = Point3D.Translate(pt3d, shapeOrigin, point0);
                            pt3d = Point3D.RotateZ(pt3d, degrees);
                            rotPt3d = Point3D.Translate(pt3d, point0, shapeOrigin);
                            break;
                    }


                    po.Add(rotPt3d);
                    //Update2dPoints();
                }

                Set2DPoints(po, ref ptList);
                screenPoints[i].PtList = ptList;
                mShape[i] = po;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int delta = 1;

            switch (e.KeyCode)
            {
                case Keys.Right:
                    mCamera.X += delta;
                    Update2dPoints();
                    Invalidate();
                    break;
                case Keys.Left:
                    mCamera.X -= delta;
                    Update2dPoints();
                    Invalidate();
                    break;
                case Keys.Down:
                    mCamera.Y -= delta;
                    Update2dPoints();
                    Invalidate();
                    break;
                case Keys.Up:
                    mCamera.Y += delta;
                    Update2dPoints();
                    Invalidate();
                    break;
                case Keys.Add:
                    mCamera.Z += delta;
                    Update2dPoints();
                    Invalidate();
                    break;

                case Keys.Subtract:
                    mCamera.Z -= delta;
                    Update2dPoints();
                    Invalidate();
                    break;

                case Keys.X:
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        RotateShapes(Axis.X, -10);
                        Invalidate();
                    }
                    else
                    {
                        RotateShapes(Axis.X, 10);
                        Invalidate();
                    }
                    break;
                case Keys.Y:
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        RotateShapes(Axis.Y, -10);
                        Invalidate();
                    }
                    else
                    {
                        RotateShapes(Axis.Y, 10);
                        Invalidate();
                    }
                    break;
                case Keys.Z:
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        RotateShapes(Axis.Z, -10);
                        Invalidate();
                    }
                    else
                    {
                        RotateShapes(Axis.Z, 10);
                        Invalidate();
                    }
                    break;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //throw new NotImplementedException();
            //int s = 10;  // Just for scaling
            Point pt = new Point();
            Graphics g = e.Graphics;
            g.TranslateTransform(ClientRectangle.Width / 2, ClientRectangle.Height / 2);
            Point startPt = new Point();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Matrix m;

            // Draw each of our shapes
            for (int i = 0; i < 3; i++)
            {
                // Go thru each point
                for (int j = 0; j < mShape[i].GetSize(); j++)
                {
                    var obj = mShape[i].Absolute(j);//[j];

                    //pt.X = (Int32)(s * (obj.X - mCamera.X) * mScreen.Z / (obj.Z - mCamera.Z));
                    //pt.Y = (Int32)(s * (obj.Y - mCamera.Y) * mScreen.Z / (obj.Z - mCamera.Z));

                    pt = screenPoints[i].PtList[j];

                    if (j == 0)
                    {
                        startPt = pt;

                    }
                    else
                    {
                        g.DrawLine(Pens.Black, startPt, pt);  // Draw a line to the others
                        startPt = pt;
                    }

                }
            }
        }

        private void Set2DPoints(MyPolygon polygon, ref List<Point> ptList)
        {
            ptList.Clear();
            for (int j = 0; j < polygon.GetSize(); j++)
            {
                Point pt = new Point();
                var obj = polygon.Absolute(j);//[j];
                pt.X = (Int32)(s * (obj.X - mCamera.X) * mScreen.Z / (obj.Z - mCamera.Z));
                pt.Y = (Int32)(s * (obj.Y - mCamera.Y) * mScreen.Z / (obj.Z - mCamera.Z));

                ptList.Add(pt);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
    }

    public enum Axis
    {
        X, Y, Z
    }
}

