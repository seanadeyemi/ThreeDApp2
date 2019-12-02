﻿using System;
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
        PointInfo pointInfo = new PointInfo();
        Color DefaultColor = Color.Gray;
        Point[] line = new Point[2];
        bool MouseIsDown = false;


        Point infiniteX = Point.Empty;
        Point infiniteY = Point.Empty;
        Point infiniteZ = Point.Empty;

        Point3D infinite3DY = null;
        Point3D infinite3DX = null;
        Point3D infinite3DZ = null;


        MyPolygon NewPolygon = null;
        Point3D point0 = new Point3D(0, 0, 0); //Used for reference
        Point3D shapeOrigin = new Point3D(0, 0, 0);
        Point currentPoint = Point.Empty;
        string textVal;
        Point lastMousePos = Point.Empty;
        int offsetWidth, offsetHeight;

        Point3D normalSample = new Point3D(0, 0, 0);
        Dictionary<int, Point3D> pointRef = new Dictionary<int, Point3D>();
        Dictionary<int, int> shapeRef = new Dictionary<int, int>();
        Point3D defaultY3D = new Point3D(0, 300, 20);

        int s = 10;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseWheel += Form1_MouseWheel;
            this.MouseDown += Form1_MouseDown;
            this.MouseUp += Form1_MouseUp;
            offsetWidth = ClientRectangle.Width / 2;
            offsetHeight = ClientRectangle.Height / 2;


            shapeOrigin.X = 0;//ClientRectangle.Width / 2;
            shapeOrigin.Y = 10;
            shapeOrigin.Z = 30;


            mCamera = new Point3D(0, 0, 0);  // Our viewpoint
            mScreen = new Point3D(0, 0, 30); // Where we project the image to
                                             // mScreen = new Point3D(0, 0, shapeOrigin.Z); // Where we project the image to
            mShape[0] = new MyPolygon();
            mShape[1] = new MyPolygon();
            mShape[2] = new MyPolygon();



            //screenPoints[0] = new ShapeScreenPoints();
            //screenPoints[1] = new ShapeScreenPoints();
            //screenPoints[2] = new ShapeScreenPoints();

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

            mShape[0].Center = new Point3D(5, 8, 20);


            //     Set2DPoints(mShape[0], ref screenPoints[0].PtList);
            Set2DPoints(mShape[0], ref mShape[0].screenPoints.PtList);



            infinite3DY = defaultY3D;
            infiniteY = Fetch2DPoint(infinite3DY);



            // Shapes 1 and 2 are exactly the same as Shape 0 ...
            //mShape[1] = mShape[0];
            //mShape[2] = mShape[0];

            mShape[1].Add(new Point3D(0, 10, 30));
            mShape[1].Add(new Point3D(0, 0, 30));
            mShape[1].Add(new Point3D(10, 0, 30));
            mShape[1].Add(new Point3D(10, 10, 30));
            mShape[1].Add(new Point3D(5, 15, 30));
            mShape[1].Close();
            mShape[1].Center = new Point3D(5, 8, 30);

            //Set2DPoints(mShape[1], ref screenPoints[1].PtList);
            Set2DPoints(mShape[1], ref mShape[1].screenPoints.PtList);




            mShape[2].Add(new Point3D(0, 10, 50));
            mShape[2].Add(new Point3D(0, 0, 50));
            mShape[2].Add(new Point3D(10, 0, 50));
            mShape[2].Add(new Point3D(10, 10, 50));
            mShape[2].Add(new Point3D(5, 15, 50));
            mShape[2].Close();
            mShape[2].Center = new Point3D(5, 8, 50);

            //Set2DPoints(mShape[2], ref screenPoints[2].PtList);
            Set2DPoints(mShape[2], ref mShape[2].screenPoints.PtList);



            // ... but at different positions
            //mShape[1].PtChange(0, 0, 30);
            //mShape[2].PtChange(0, 0, 50);

            //mShape[1].mPosition.z = 30;
            //mShape[2].mPosition.z = 50;

        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseIsDown = false;

            if (NewPolygon == null) return;

            if (pointInfo != null)
            {
                if (pointInfo.IsOnPoint)
                {

                    //if (!NewPolygon.Contains(pointInfo.p3d))
                    //{
                    NewPolygon.Add(pointInfo.p3d);
                    NewPolygon.screenPoints.PtList.Add(pointInfo.foundPoint);
                    //}
                    //if (!NewPolygon.Contains(pointInfo.p3dBefore))
                    //{
                    //    NewPolygon.Add(pointInfo.p3dBefore);
                    //}

                    //if (!NewPolygon.Contains(pointInfo.p3dAfter))
                    //{
                    //    NewPolygon.Add(pointInfo.p3dAfter);
                    //}



                    //NewPolygon.screenPoints.PtList.Add(pointInfo.pointBefore);


                    //NewPolygon.screenPoints.PtList.Add(pointInfo.pointAfter);


                    //NewPolygon.screenPoints.PtBeforeList.Add(pointInfo.pointBefore);
                    //NewPolygon.screenPoints.PtAfterList.Add(pointInfo.pointAfter);

                    Array.Resize(ref mShape, mShape.Length + 1);
                    mShape[mShape.GetUpperBound(0)] = NewPolygon;

                    NewPolygon = null;
                    line = new Point[2];
                    infiniteY = Point.Empty;
                    infinite3DY = null;
                    // var pt = e.Location;//pointInfo.foundPoint;
                    //var pt = Fetch2DPoint(pointInfo.p3d);
                    //   NewPolygon.screenPoints.PtList = new List<Point>();
                    //  NewPolygon.screenPoints.PtList.Add(pt);

                    //line[1] = pointInfo.foundPoint;//pt;

                    //BuildGraph();
                }
            }

            //Segments.Add(NewSegment);
            //NewSegment = null;
            //picCanvas.Refresh();
            Invalidate();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (panBtn.Checked)
                {
                    MouseIsDown = true;
                }
                if (drawBtn.Checked)
                {
                    if (pointInfo != null)
                    {
                        if (pointInfo.IsOnPoint)
                        {
                            NewPolygon = new MyPolygon();
                            NewPolygon.Add(pointInfo.p3d);
                            NewPolygon.screenPoints.PtList.Add(pointInfo.foundPoint);

                            infinite3DY = pointInfo.p3d;
                            infinite3DY.Y += 300;
                            infiniteY = Fetch2DPoint(infinite3DY);

                            //NewPolygon.Add(pointInfo.p3dBefore);
                            //NewPolygon.Add(pointInfo.p3dAfter);




                            //NewPolygon.screenPoints.PtList.Add(pointInfo.pointBefore);
                            //NewPolygon.screenPoints.PtList.Add(pointInfo.pointAfter);
                            //NewPolygon.screenPoints.PtBeforeList.Add(pointInfo.pointBefore);
                            //NewPolygon.screenPoints.PtAfterList.Add(pointInfo.pointAfter);

                            //var pt = pointInfo.foundPoint;//Fetch2DPoint(pointInfo.p3d);
                            ////NewPolygon.screenPoints.PtList = new List<Point>();
                            ////NewPolygon.screenPoints.PtList.Add(pt);
                            // line[0] = pt;
                        }


                    }
                }



            }
            Invalidate();

        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            // The amount by which we adjust scale per wheel click.
            const float scale_per_delta = 0.1f / 120;

            //// Update the drawing based upon the mouse wheel scrolling.
            //ImageScale += e.Delta * scale_per_delta;
            //if (ImageScale < 0) ImageScale = 0;

            //// Size the image.
            //picImage.Size = new Size(
            //	(int)(ImageWidth * ImageScale),
            //	(int)(ImageHeight * ImageScale));
            mCamera.Z += e.Delta * scale_per_delta * 10;
            Update2dPoints();
            Invalidate();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //lastMousePos = Point.Empty;
            if (e.Button == MouseButtons.Left)
            {
                if (MouseIsDown)
                {
                    int delta = 1;
                    var x = e.X - lastMousePos.X;
                    var y = e.Y - lastMousePos.Y;

                    if (x > 0 && x > y)
                    {
                        mCamera.Y -= delta;
                        Update2dPoints();
                        Invalidate();
                    }
                    if (x < 0 && x < y)
                    {
                        mCamera.Y += delta;
                        Update2dPoints();
                        Invalidate();
                    }

                    if (y > 0 && y > x)
                    {
                        mCamera.X -= delta;
                        Update2dPoints();
                        Invalidate();
                    }

                    if (y < 0 && y < x)
                    {
                        mCamera.X += delta;
                        Update2dPoints();
                        Invalidate();
                    }

                }
            }
            if (e.Button == MouseButtons.Middle)
            {

                var x = e.X - lastMousePos.X;
                var y = e.Y - lastMousePos.Y;
                //textVal = String.Format("{0} :: {1}", e.X, e.Y);
                textVal = String.Format("{0} :: {1}", x, y);

                //if (lastMousePos != Point.Empty)
                {


                    if (x > 0 && x > y)
                    {
                        RotateShapes(Axis.Y, -1);
                        Invalidate();
                    }
                    if (x < 0 && x < y)
                    {
                        RotateShapes(Axis.Y, 1);
                        Invalidate();
                    }

                    if (y > 0 && y > x)
                    {
                        RotateShapes(Axis.X, -1);
                        Invalidate();
                    }

                    if (y < 0 && y < x)
                    {
                        RotateShapes(Axis.X, 1);
                        Invalidate();
                    }


                }

                lastMousePos = new Point(e.X, e.Y);



            }

            Point cursorPt = e.Location;
            Point foundPt = Point.Empty;

            //while going thru each polygon
            for (int k = 0; k < mShape.Length; k++)
            {

                //search thru its 2D points to find the cursors point
                int index = mShape[k].screenPoints.PtList.FindIndex(p =>
                {
                    if (PointInRange(cursorPt, p))
                    {

                        foundPt = p;
                        pointInfo = new PointInfo();
                        pointInfo.IsOnPoint = true;
                        pointInfo.foundPoint = foundPt;
                        //infiniteY.X = foundPt.X;
                        //infiniteY.Y = foundPt.Y;
                        return true;
                    }
                    else
                    {
                        foundPt = Point.Empty;
                        pointInfo = new PointInfo();
                        pointInfo.IsOnPoint = false;
                        pointInfo.foundPoint = Point.Empty;
                        pointInfo.p3d = null;
                        infiniteY = Point.Empty;
                        return false;
                    }

                }
               );
                if (foundPt != Point.Empty)
                {
                    pointInfo.p3d = mShape[k].Absolute(index);
                    pointInfo.foundPointIndex = index;
                    pointInfo.p3dBefore = index - 1 >= 0 ? mShape[k].Absolute(index - 1) : mShape[k].Absolute(0);
                    pointInfo.p3dAfter = index + 1 < mShape[k].GetSize() ? mShape[k].Absolute(index + 1) : mShape[k].Absolute(mShape[k].GetSize() - 1);
                    pointInfo.pointBefore = (index - 1 >= 0) ? mShape[k].screenPoints.PtList[index - 1] : mShape[k].screenPoints.PtList[0];
                    pointInfo.pointAfter = (index + 1 < mShape[k].screenPoints.PtList.Count) ? mShape[k].screenPoints.PtList[index + 1] : mShape[k].screenPoints.PtList[mShape[k].screenPoints.PtList.Count - 1];

                    //infinite3DY = defaultY3D;

                    //infinite3DY.X = pointInfo.p3d.X;
                    //infinite3DY.Z = pointInfo.p3d.Z;
                    //infinite3DY.Y = pointInfo.p3d.Y + 300;

                   

                    break;
                }
            }



            if (foundPt != null)
            {
                currentPoint = foundPt;
            }
            else
            {
                currentPoint = Point.Empty;
            }

            if (NewPolygon != null)
            {


                if (NewPolygon.screenPoints.PtList.Count == 1)
                {
                    //set line array as from existing point to cursor point

                    line[0] = NewPolygon.screenPoints.PtList[0];
                    line[1] = new Point(e.Location.X - offsetWidth, e.Location.Y - offsetHeight); //offsetWidth and offsetHeight are translations from top left corner of screen



                }
            }



            Invalidate();
        }

        public bool PointInRange(Point point, Point pointChecked)
        {
            return (((point.X >= ((pointChecked.X + offsetWidth) - 10)) &&
                (point.X <= ((pointChecked.X + offsetWidth) + 10))) &&
                ((point.Y >= ((pointChecked.Y + offsetHeight) - 10)) &&
                (point.Y <= ((pointChecked.Y + offsetHeight) + 10))));
        }

        //private void Update2dPoints()
        //{
        //    Set2DPoints(mShape[0], ref screenPoints[0].PtList);
        //    Set2DPoints(mShape[1], ref screenPoints[1].PtList);
        //    Set2DPoints(mShape[2], ref screenPoints[2].PtList);
        //}
        private void Update2dPoints()
        {
            for (int i = 0; i < mShape.Length; i++)
            {
                Set2DPoints(mShape[i], ref mShape[i].screenPoints.PtList);
            }


        }
        private void RotateShapes(Axis axis, double degrees)
        {
            for (int i = 0; i < mShape.Length; i++)
            {
                // Go thru each point
                MyPolygon po = new MyPolygon();
                List<Point> ptList = new List<Point>();
                float depth = 0;

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
                            depth = rotPt3d.Z;
                            break;
                        case Axis.Y:
                            pt3d = Point3D.Translate(pt3d, shapeOrigin, point0);
                            pt3d = Point3D.RotateY(pt3d, degrees);
                            rotPt3d = Point3D.Translate(pt3d, point0, shapeOrigin);
                            //rotPt3d = Point3D.RotateY(pt3d, degrees);
                            depth = rotPt3d.Z;
                            break;
                        case Axis.Z:
                            //rotPt3d = Point3D.RotateZ(pt3d, degrees);
                            pt3d = Point3D.Translate(pt3d, shapeOrigin, point0);
                            pt3d = Point3D.RotateZ(pt3d, degrees);
                            rotPt3d = Point3D.Translate(pt3d, point0, shapeOrigin);
                            depth = rotPt3d.Z;
                            break;
                    }


                    po.Add(rotPt3d);
                    //Update2dPoints();
                }

                Set2DPoints(po, ref ptList);
                mShape[i] = po;
                mShape[i].screenPoints.PtList = ptList;
                mShape[i].Center.Z = depth;

                //screenPoints[i].PtList = ptList;
                //mShape[i] = po;
            }


            if (infiniteY != Point.Empty)
            {
                if (infinite3DY != null)
                {
                    switch (axis)
                    {
                        case Axis.X:
                            infinite3DY = Point3D.Translate(infinite3DY, shapeOrigin, point0);
                            infinite3DY = Point3D.RotateX(infinite3DY, degrees);
                            infinite3DY = Point3D.Translate(infinite3DY, point0, shapeOrigin);
                            break;
                        case Axis.Y:
                            infinite3DY = Point3D.Translate(infinite3DY, shapeOrigin, point0);
                            infinite3DY = Point3D.RotateY(infinite3DY, degrees);
                            infinite3DY = Point3D.Translate(infinite3DY, point0, shapeOrigin);
                            break;
                        case Axis.Z:
                            infinite3DY = Point3D.Translate(infinite3DY, shapeOrigin, point0);
                            infinite3DY = Point3D.RotateZ(infinite3DY, degrees);
                            infinite3DY = Point3D.Translate(infinite3DY, point0, shapeOrigin);
                            break;
                    }
                    infiniteY = Fetch2DPoint(infinite3DY);
                }



            }

            //Point3D[] pts = new Point3D[3];
            //pts[0] = mShape[0][0];//.Absolute(0);
            //pts[1] = mShape[0][1];//.Absolute(1);
            //pts[2] = mShape[0][2];//.Absolute(2);

            //normalSample = calcNormal(pts);


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
            g.DrawString($"Camera: {mCamera.Z}", SystemFonts.DefaultFont, Brushes.Black, 5f, 5f);
            g.DrawString($"Cur Pt: {currentPoint.X}, {currentPoint.Y}", SystemFonts.DefaultFont, Brushes.Black, 5f, 25f);
            g.DrawString($"mouse pos: {textVal}", SystemFonts.DefaultFont, Brushes.Black, 5f, 45f);
            g.DrawString($"normal: X: {normalSample.X}, Y: {normalSample.Y}, Z: {normalSample.Z}", SystemFonts.DefaultFont, Brushes.Black, 5f, 65f);
            g.TranslateTransform(ClientRectangle.Width / 2, ClientRectangle.Height / 2);
            Point startPt = new Point();
            g.SmoothingMode = SmoothingMode.AntiAlias;



            //Matrix m;
            Array.Sort(mShape);
            Array.Reverse(mShape);


            // Draw each of our shapes
            for (int i = 0; i < mShape.Length; i++)
            {
                Color shadedColor = FinalColor(mShape[i], new Point3D(5, 50, 10), g);
                //g.FillPolygon(Brushes.DarkGray, mShape[i].screenPoints.PtList.ToArray());
                var brush = new SolidBrush(shadedColor);
                g.FillPolygon(brush, mShape[i].screenPoints.PtList.ToArray());


                // Go thru each point
                for (int j = 0; j < mShape[i].GetSize(); j++)
                {

                    pt = mShape[i].screenPoints.PtList[j];


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

            if (currentPoint != Point.Empty)
            {
                g.FillEllipse(Brushes.Red, new RectangleF(currentPoint.X - 3, currentPoint.Y - 3, 6, 6));
            }

            if (NewPolygon != null)
            {
                //if(NewPolygon.screenPoints.PtList.Count > 1)
                if (line.Length > 1)
                {
                    g.DrawLine(Pens.Black, line[0], line[1]);
                }

                //if (infiniteY != Point.Empty)
                //{
                //    g.DrawLine(Pens.Red, line[0], infiniteY);
                //}
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

        private Point Fetch2DPoint(Point3D pt3d)
        {
            Point pt = new Point();
            pt.X = (Int32)(s * (pt3d.X - mCamera.X) * mScreen.Z / (pt3d.Z - mCamera.Z));
            pt.Y = (Int32)(s * (pt3d.Y - mCamera.Y) * mScreen.Z / (pt3d.Z - mCamera.Z));
            ///////////////////////////////////////////////////////

            //(pt3d.Z - mCamera.Z)/(pt3d.X - mCamera.X) = s * mScreen.Z/ pt.X;

            return pt;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            //BuildGraph();
        }

        private double dotProduct(double[] vect_A,
                           double[] vect_B)
        {

            double product = 0;

            // Loop for calculate cot product 
            for (int i = 0; i < 3; i++)
                product = product + vect_A[i]
                                 * vect_B[i];
            return product;
        }
        private Color FinalColor(MyPolygon poly, Point3D lightPoint, Graphics gr)
        {
            Color output = Color.Empty;
            if (poly.GetSize() > 2)
            {
                Point3D[] pts = new Point3D[3];
                pts[0] = poly[0];//.Absolute(0);
                pts[1] = poly[1];//.Absolute(1);
                pts[2] = poly[2];//.Absolute(2);

                var unitNormal = calcNormal(pts);


                ReduceToUnit(ref lightPoint);
                var shadeVal = dotProduct(new double[] { unitNormal.X, unitNormal.Y, unitNormal.Z }, new double[] { lightPoint.X, lightPoint.Y, lightPoint.Z });
                shadeVal *= 255;
                var shade = Convert.ToInt32(shadeVal);
                if (shade < 0)
                {
                    shade = 0;
                }

                gr.DrawString(shadeVal.ToString() + $" {lightPoint.X}, {lightPoint.Y}, {lightPoint.Z}", SystemFonts.DefaultFont, Brushes.Black, new PointF(5f, 55f));
                // output = Color.FromArgb(DefaultColor.R + shade, DefaultColor.G + shade, DefaultColor.B + shade);
                output = Color.FromArgb(shade, shade, shade);
            }

            return output;
        }

        private Point3D calcNormal(Point3D[] v)               // Calculates Normal For A Quad Using 3 Points
        {
            Point3D outer = new Point3D(0, 0, 0);
            Point3D v1 = new Point3D(0, 0, 0);
            Point3D v2 = new Point3D(0, 0, 0);                   // Vector 1 (x,y,z) & Vector 2 (x,y,z)


            //const int x = 0;                     // Define X Coord
            //const int y = 1;                     // Define Y Coord
            //const int z = 2;                     // Define Z Coord

            // Finds The Vector Between 2 Points By Subtracting
            // The x,y,z Coordinates From One Point To Another.

            // Calculate The Vector From Point 1 To Point 0
            v1.X = v[0].X - v[1].X;                  // Vector 1.x=Vertex[0].x-Vertex[1].x
            v1.Y = v[0].Y - v[1].Y;                  // Vector 1.y=Vertex[0].y-Vertex[1].y
            v1.Z = v[0].Z - v[1].Z;                  // Vector 1.z=Vertex[0].y-Vertex[1].z
                                                     // Calculate The Vector From Point 2 To Point 1
            v2.X = v[1].X - v[2].X;                  // Vector 2.x=Vertex[0].x-Vertex[1].x
            v2.Y = v[1].Y - v[2].Y;                  // Vector 2.y=Vertex[0].y-Vertex[1].y
            v2.Z = v[1].Z - v[2].Z;                  // Vector 2.z=Vertex[0].z-Vertex[1].z
                                                     // Compute The Cross Product To Give Us A Surface Normal
            outer.X = v1.Y * v2.Z - v1.Z * v2.Y;             // Cross Product For Y - Z
            outer.Y = v1.Z * v2.X - v1.X * v2.Z;             // Cross Product For X - Z
            outer.Z = v1.X * v2.Y - v1.Y * v2.X;             // Cross Product For X - Y

            ReduceToUnit(ref outer);                      // Normalize The Vectors
            return outer;
        }

        void ReduceToUnit(ref Point3D vector)                  // Reduces A Normal Vector (3 Coordinates)
        {                                   // To A Unit Normal Vector With A Length Of One.
            float length;                           // Holds Unit Length
                                                    // Calculates The Length Of The Vector
            length = (float)Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));

            if (length == 0.0f)                      // Prevents Divide By 0 Error By Providing
                length = 1.0f;                      // An Acceptable Value For Vectors To Close To 0.

            vector.X /= length;                        // Dividing Each Element By
            vector.Y /= length;                        // The Length Results In A
            vector.Z /= length;                        // Unit Normal Vector.
        }

        private void zoomInBtn_Click(object sender, EventArgs e)
        {
            int delta = 1;
            mCamera.Z += delta;
            Update2dPoints();
            Invalidate();
        }

        private void zoomOutBtn_Click(object sender, EventArgs e)
        {
            int delta = 1;
            mCamera.Z -= delta;
            Update2dPoints();
            Invalidate();
        }

        void BuildGraph2()
        {
            List<int> verts = new List<int>();
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>();

            int id = 0;
            for (int k = 0; k < mShape.Length; k++)
            {

                var poly = mShape[k];
                var total = k > 0 ? mShape[k - 1].GetSize() : 0;
                int? firstId = null;


                for (int j = 0; j < poly.GetSize(); j++)
                {
                    if (j == 0)
                    {
                        firstId = id;
                    }
                    verts.Add(id);
                    pointRef.Add(id, poly[j]);
                    shapeRef.Add(k, j);

                    var totalId = id + total;
                    //if(j+1 < poly.GetSize())
                    if (j < (poly.GetSize() - 1))
                    {
                        edges.Add(Tuple.Create(id, totalId + 1));
                    }
                    else
                    {
                        edges.Add(Tuple.Create(id, firstId.Value));
                    }
                    //edges.Add(Tuple.Create(id, ));
                    //var edges = new[] { Tuple.Create(1, 2) };
                    id++;
                }
            }

        }

        void BuildGraph()
        {
            //read vertices or nodes
            var Nodes = new Dictionary<int, PathSNode>();
            int m = 0;
            int g = 0;
            int max = 0;

            foreach (var poly in mShape)
            {
                max += poly.GetSize();
            }

            for (int k = 0; k < mShape.Length; k++)
            {

                var poly = mShape[k];
                for (int j = 0; j < poly.GetSize(); j++)
                {
                    var vertex = poly[j];
                    PathSNode new_node = new PathSNode();
                    new_node.Id = m;
                    new_node.Location = vertex;

                    Nodes.Add(new_node.Id, new_node);
                    m++;
                }

                //read edges or links
                var Links = new Dictionary<string, PathSLink>();
                for (int n = 0; n < poly.GetSize(); n++)
                {
                    PathSLink new_link = new PathSLink();
                    int num = g + 1 < max ? 1 : 0;
                    if (n + 1 < poly.GetSize())
                    {
                        new_link.Node1 = Nodes[g];
                        new_link.Node2 = Nodes[g + num];

                        Links.Add(g + "-" + (g + num), new_link);

                        new_link.Node1.Links.Add(g + num, new_link);
                        new_link.Node2.Links.Add(g, new_link);

                        g++;
                    }


                }
            }


        }

        public Func<T, IEnumerable<T>> ShortestPathFunction<T>(Graph<T> graph, T start)
        {
            var previous = new Dictionary<T, T>();

            var queue = new Queue<T>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var vertex = queue.Dequeue();
                foreach (var neighbor in graph.AdjacencyList[vertex])
                {
                    if (previous.ContainsKey(neighbor))
                        continue;

                    previous[neighbor] = vertex;
                    queue.Enqueue(neighbor);
                }
            }

            Func<T, IEnumerable<T>> shortestPath = v =>
            {
                var path = new List<T> { };

                var current = v;
                while (!current.Equals(start))
                {
                    path.Add(current);
                    current = previous[current];
                };

                path.Add(start);
                path.Reverse();

                return path;
            };

            return shortestPath;
        }

    }

    public enum Axis
    {
        X, Y, Z
    }

    public class PointInfo
    {
        public Point3D p3d = null;
        public Point3D p3dBefore = null;
        public Point3D p3dAfter = null;
        public bool IsOnPoint = false;
        public Point foundPoint = Point.Empty;
        public Point pointBefore = Point.Empty;
        public Point pointAfter = Point.Empty;
        public int foundPointIndex = -1;
    }
    public class Graph<T>
    {
        public Graph() { }
        public Graph(IEnumerable<T> vertices, IEnumerable<Tuple<T, T>> edges)
        {
            foreach (var vertex in vertices)
                AddVertex(vertex);

            foreach (var edge in edges)
                AddEdge(edge);
        }

        public Dictionary<T, HashSet<T>> AdjacencyList { get; } = new Dictionary<T, HashSet<T>>();

        public void AddVertex(T vertex)
        {
            AdjacencyList[vertex] = new HashSet<T>();
        }

        public void AddEdge(Tuple<T, T> edge)
        {
            if (AdjacencyList.ContainsKey(edge.Item1) && AdjacencyList.ContainsKey(edge.Item2))
            {
                AdjacencyList[edge.Item1].Add(edge.Item2);
                AdjacencyList[edge.Item2].Add(edge.Item1);
            }
        }
    }
}

