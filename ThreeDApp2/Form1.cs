using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ThreeDApp2
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string filename);

        Point3D mCamera;
        Point3D mScreen;
        MyList<MyPolygon> mShape = new MyList<MyPolygon>();

        ShapeScreenPoints[] screenPoints = new ShapeScreenPoints[3];
        PointInfo pointInfo = new PointInfo();
        PointInfo pointInfoEnd = new PointInfo();
        Color DefaultColor = Color.Gray;
        Point[] line = new Point[2];
        Point[] rectPts = new Point[2];
        Cursor CurrentCursor = null;
        string CurrentCursorIcon = string.Empty;

        int ptCount = -1;

        bool MouseIsDown = false;
        bool inBetween = false;


        Point infiniteX = Point.Empty;
        Point infinite2DEndY = Point.Empty;
        Point infiniteZ = Point.Empty;

        Point infiniteStartX = Point.Empty;
        Point infinite2DStartY = Point.Empty;
        Point infiniteStartZ = Point.Empty;



        Point3D infinite3DEndY = null;
        Point3D infinite3DX = null;
        Point3D infinite3DZ = null;

        Point3D infinite3DStartX = null;
        Point3D infinite3DStartY = null;
        Point3D infinite3DStartZ = null;


        MyPolygon NewPolygon = null;
        PolygonHistory polygonHistory = null;

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
        PolygonData polyData = PolygonData.Empty;

        Point3D xAxisLine = new Point3D(150, 0, 20);
        Point3D yAxisLine = new Point3D(0, -150, 20);
        Point3D zAxisLine = new Point3D(0, 0, 60);

        Point3D zeroPt = new Point3D(0, 0, 20);

        Point xAxisLine2D = Point.Empty;
        Point yAxisLine2D = Point.Empty;
        Point zAxisLine2D = Point.Empty;
        Point zeroPt2D = Point.Empty;

        Point point02D = Point.Empty;

        int s = 10;
        private bool FirstShapeAdded;

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
            AdjustOffset();


            InitializeScene();

        }

        private void AdjustOffset()
        {
            offsetWidth = ClientRectangle.Width / 2;
            offsetHeight = ClientRectangle.Height / 2;
        }

        private void InitializeScene()
        {


            shapeOrigin.X = 0;//ClientRectangle.Width / 2;
            shapeOrigin.Y = 10;
            shapeOrigin.Z = 30;


            mCamera = new Point3D(0, 0, 0);  // Our viewpoint
            mScreen = new Point3D(0, 0, 30); // Where we project the image to
                                             // mScreen = new Point3D(0, 0, shapeOrigin.Z); // Where we project the image to



            //point0 = new Point3D(-30, 0, 30); // Where we project the image to
            //Point cpt = new Point();
            ////RotatePoint(ref cpt, ref mCamera, Axis.X, 90);

            //mCamera = new Point3D(-30, 0, 30);  // Our viewpoint
            //mScreen = new Point3D(-10, 0, 30); // Where we project the image to




            //mShape[0] = new MyPolygon();
            //mShape[1] = new MyPolygon();
            //mShape[2] = new MyPolygon();



            // Define a shape
            //mShape[0].Add(new Point3D(0, 10, 20));
            //mShape[0].Add(new Point3D(0, 0, 20));
            //mShape[0].Add(new Point3D(10, 0, 20));
            //mShape[0].Add(new Point3D(10, 10, 20));
            //mShape[0].Add(new Point3D(5, 15, 20));
            MyPolygon shape1 = new MyPolygon();

            shape1.Add(new Point3D(0, 10, 20));
            shape1.Add(new Point3D(0, 0, 20));
            shape1.Add(new Point3D(10, 0, 20));
            shape1.Add(new Point3D(10, 10, 20));
            shape1.Add(new Point3D(5, 15, 20));
            shape1.Close();

            shape1.Center = new Point3D(5, 8, 20);

            Set2DPoints(shape1, ref shape1.ScreenPoints.PtList);
            mShape.Add(shape1);



            infinite3DStartY = new Point3D(0, 0, 20);
            infinite2DStartY = Fetch2DPoint(infinite3DStartY);


            infinite3DEndY = new Point3D(0, 30, 20);
            infinite2DEndY = Fetch2DPoint(infinite3DEndY);

            MyPolygon shape2 = new MyPolygon();
            shape2.Add(new Point3D(0, 10, 30));
            shape2.Add(new Point3D(0, 0, 30));
            shape2.Add(new Point3D(10, 0, 30));
            shape2.Add(new Point3D(10, 10, 30));
            shape2.Add(new Point3D(5, 15, 30));
            shape2.Close();
            shape2.Center = new Point3D(5, 8, 30);

            //Set2DPoints(mShape[1], ref screenPoints[1].PtList);
            Set2DPoints(shape2, ref shape2.ScreenPoints.PtList);
            mShape.Add(shape2);


            MyPolygon shape3 = new MyPolygon();
            shape3.Add(new Point3D(0, 10, 50));
            shape3.Add(new Point3D(0, 0, 50));
            shape3.Add(new Point3D(10, 0, 50));
            shape3.Add(new Point3D(10, 10, 50));
            shape3.Add(new Point3D(5, 15, 50));
            shape3.Close();
            shape3.Center = new Point3D(5, 8, 50);

            //Set2DPoints(mShape[2], ref screenPoints[2].PtList);
            Set2DPoints(shape3, ref shape3.ScreenPoints.PtList);
            mShape.Add(shape3);


            // ... but at different positions
            //mShape[1].PtChange(0, 0, 30);
            //mShape[2].PtChange(0, 0, 50);

            //mShape[1].mPosition.z = 30;
            //mShape[2].mPosition.z = 50;

            pointInfo.p3d = new Point3D(0, 0, 20);
            pointInfo.foundPoint = Fetch2DPoint(pointInfo.p3d);

            pointInfoEnd.p3d = new Point3D(0, 10, 20);
            pointInfoEnd.foundPoint = Fetch2DPoint(pointInfoEnd.p3d);

            xAxisLine2D = Fetch2DPoint(xAxisLine);
            yAxisLine2D = Fetch2DPoint(yAxisLine);
            zAxisLine2D = Fetch2DPoint(zAxisLine);

            zeroPt2D = Fetch2DPoint(zeroPt);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            //   ResetCursor();
            // ResetThisCursor();
            //SetIcon(Icons.Select);


            MouseIsDown = false;

            if (NewPolygon == null) return;

            if (pointInfo != null)
            {
                if (pointInfo.IsOnPoint)
                {

                    //if (!NewPolygon.Contains(pointInfo.p3d))
                    //{
                    NewPolygon.Add(pointInfo.p3d);
                    NewPolygon.ScreenPoints.PtList.Add(pointInfo.foundPoint);

                    var polyg = new MyPolygon();

                    ////////////////////////////////////////
                    int foundpolyIndex = FindPolygonIndexFromPoint(pointInfo.foundPoint);
                    if (foundpolyIndex != -1)
                    {
                        polygonHistory.Polygon2Index = foundpolyIndex;//pointInfo.foundPointPolygonIndex;
                        polyg = mShape[foundpolyIndex];//mShape[pointInfo.foundPointPolygonIndex];
                    }
                    else
                    {
                        polygonHistory.Polygon2Index = pointInfo.foundPointPolygonIndex;
                        polyg = mShape[pointInfo.foundPointPolygonIndex];
                    }
                    //polygonHistory.Polygon2Index = foundpolyIndex;//pointInfo.foundPointPolygonIndex;
                    //var polyg = mShape[foundpolyIndex];//mShape[pointInfo.foundPointPolygonIndex];
                    Point3D ptBefore = null;
                    Point ptBefore2D = Point.Empty;

                    int previousIndex = pointInfo.foundPointIndex - 1;
                    //TODO: still have to handle when we end up on the same point (
                    if (previousIndex >= 0)
                    {
                        ptBefore = polyg[previousIndex];
                        ptBefore2D = polyg.ScreenPoints.PtList[previousIndex];
                    }
                    else
                    {



                        //if (polyg.GetSize() > 2)
                        //{
                        //    ptBefore = polyg[polyg.GetSize() - 1];
                        //    ptBefore2D = polyg.screenPoints.PtList[polyg.GetSize() - 1];
                        //}
                    }
                    //Point3D ptAfter = null;
                    //Point ptAfter2D = Point.Empty;

                    //if ((pointInfo.foundPointIndex + 1) < polyg.GetSize())
                    //{
                    //    ptAfter = polyg[pointInfo.foundPointIndex + 1];
                    //    ptAfter2D = polyg.screenPoints.PtList[pointInfo.foundPointIndex + 1];
                    //}
                    //else
                    //{
                    //    if (polyg.GetSize() > 2)
                    //    {
                    //        ptAfter = polyg[0];
                    //        ptAfter2D = polyg.screenPoints.PtList[0];
                    //    }
                    //}


                    polygonHistory.poly2BeforePoint = ptBefore;
                    polygonHistory.poly2BeforePoint2D = ptBefore2D;

                    //polygonHistory.poly2AfterPoint = ptAfter;
                    //polygonHistory.poly2AfterPoint2D = ptAfter2D;

                    ////////////////////////////////////////////////////////////////

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

                    //polyData.PtInPoly
                    var poly = mShape[pointInfo.foundPointPolygonIndex];
                    var lastIndex = poly.GetSize() - 1;
                    // if (ptCount == 0 || ptCount == lastIndex)

                    //is this polygon (the one with found point) still open?
                    if (!mShape[pointInfo.foundPointPolygonIndex].Closed)
                    {
                        //Point3D p1 = new Point3D(0, 0, 0);
                        //Point3D p2 = new Point3D(0, 0, 0);

                        //lets close it
                        if (pointInfo.foundPointIndex == 0)//did we end on the first index
                        {

                            mShape[pointInfo.foundPointPolygonIndex].Add(NewPolygon[1]);
                            mShape[pointInfo.foundPointPolygonIndex].ScreenPoints.PtList.Add(NewPolygon.ScreenPoints.PtList[1]);

                            //p1 = new Point3D(NewPolygon[1]);
                            //p2 = new Point3D(NewPolygon[0]);

                        }
                        else if (pointInfo.foundPointIndex == lastIndex) //did we end up on the last index
                        {
                            mShape[pointInfo.foundPointPolygonIndex].Add(NewPolygon[0]);
                            mShape[pointInfo.foundPointPolygonIndex].ScreenPoints.PtList.Add(NewPolygon.ScreenPoints.PtList[0]);

                            //p1 = new Point3D(NewPolygon[0]);
                            //p2 = new Point3D(NewPolygon[1]);
                        }




                        //find the ending point in other polygons, if you find it, add the line (2 points) to mShape array as a newpolygon with the points after if they exist
                        //cos we need to prepare for the next polygon that needs closing

                        bool foundBoth = false;
                        bool foundOne = false;
                        bool foundTwo = false;
                        int foundShapeOneIndex = -1;
                        int foundShapeTwoIndex = -1;
                        int foundShapeBothPtsIndex = -1;
                        //int foundShapePointIndex = -1;

                        //  bool foundShapePointIndex1 = false;
                        //   bool foundShapePointIndex2 = false;
                        int _foundShapePointIndex1 = -1;
                        int _foundShapePointIndex2 = -1;


                        int _foundShapeBothPointIndex1 = -1;
                        int _foundShapeBothPointIndex2 = -1;



                        for (int k = 0; k < mShape.Count; k++)
                        {
                            // found
                            //foundShapePointIndex = mShape[k].screenPoints.PtList.FindIndex(i => { return i == pointInfo.foundPoint; });
                            var foundShapePointIndex1 = mShape[k].ScreenPoints.PtList.FindIndex(i => { return i == NewPolygon.ScreenPoints.PtList[0]; });// mShape[k].Contains(NewPolygon[0]);//.FindIndex(i => { return i == pointInfo.foundPoint; });
                            var foundShapePointIndex2 = mShape[k].ScreenPoints.PtList.FindIndex(i => { return i == NewPolygon.ScreenPoints.PtList[1]; }); //mShape[k].Contains(NewPolygon[1]);//.FindIndex(i => { return i == pointInfo.foundPoint; });

                            ////did we find it
                            //if (foundShapePointIndex != -1 )//&& (mShape[k].Closed == false))
                            //{

                            //    //store the polygon index where it was found
                            //    foundShapeIndex = k;
                            //    break;
                            //}

                            if (foundShapePointIndex1 != -1 && foundShapePointIndex2 == -1)
                            {
                                foundOne = true;
                                foundShapeOneIndex = k;
                                _foundShapePointIndex1 = foundShapePointIndex2;
                            }
                            if (foundShapePointIndex1 == -1 && foundShapePointIndex2 != -1)
                            {
                                foundTwo = true;
                                foundShapeTwoIndex = k;
                                _foundShapePointIndex2 = foundShapePointIndex2;
                            }

                            // if (foundShapePointIndex1 && foundShapePointIndex2 && mShape[k].Closed)
                            if (foundShapePointIndex1 != -1 && foundShapePointIndex2 != -1 && mShape[k].Closed)
                            {
                                foundBoth = true;
                                _foundShapeBothPointIndex1 = foundShapePointIndex1;
                                _foundShapeBothPointIndex2 = foundShapePointIndex2;
                                foundShapeBothPtsIndex = k;
                            }
                        }



                        //if (foundShapePointIndex != -1 && foundShapeIndex != -1)
                        if ((foundOne || foundTwo) && foundBoth)
                        {


                            //var myPoly = Array.Find<MyPolygon>(mShape, i =>
                            //{
                            //    return i.Contains(pointInfo.p3d);
                            //});

                            var myPoly = mShape[foundShapeTwoIndex];

                            var polyIndex = myPoly[pointInfo.foundPointIndex];

                            // if (pointInfo.foundPointIndex == 0)
                            if (_foundShapePointIndex2 == 0)
                            {
                                int sz = myPoly.GetSize();
                                //int lastindex = sz - 1;
                                int prev = sz - 2;

                                ptBefore = myPoly[prev];
                                ptBefore2D = myPoly.ScreenPoints.PtList[prev];

                            }
                            else
                            {
                                int prev = _foundShapePointIndex2 - 1;//pointInfo.foundPointIndex - 1;

                                ptBefore = myPoly[prev];
                                ptBefore2D = myPoly.ScreenPoints.PtList[prev];
                            }
                            polygonHistory.poly2BeforePoint = ptBefore;
                            polygonHistory.poly2BeforePoint2D = ptBefore2D;

                            if (polygonHistory.poly1BeforePoint2D != polygonHistory.poly2BeforePoint2D)
                            {
                                //add existing lines (points) to NewPolygon, one at the start, one at the end
                                if (polygonHistory.poly1BeforePoint != null)
                                {
                                    //  NewPolygon.Add(polygonHistory.poly1BeforePoint);
                                    NewPolygon.InsertAt(0, polygonHistory.poly1BeforePoint);
                                    // NewPolygon.screenPoints.PtList.Add(polygonHistory.poly1BeforePoint2D);
                                    NewPolygon.ScreenPoints.PtList.Insert(0, polygonHistory.poly1BeforePoint2D);//.Add(polygonHistory.poly1BeforePoint2D);
                                }

                                if (polygonHistory.poly2BeforePoint != null)
                                {
                                    NewPolygon.Add(polygonHistory.poly2BeforePoint);
                                    NewPolygon.ScreenPoints.PtList.Add(polygonHistory.poly2BeforePoint2D);
                                }

                                if (foundBoth)
                                {
                                    //if((polygonHistory.poly1BeforePoint2D == mShape[foundShapeBothPtsIndex].screenPoints.PtList[_foundShapeBothPointIndex1] &&
                                    //    polygonHistory.poly2BeforePoint2D  == mShape[foundShapeBothPtsIndex].screenPoints.PtList[_foundShapeBothPointIndex2])||
                                    //    (polygonHistory.poly1BeforePoint2D == mShape[foundShapeBothPtsIndex].screenPoints.PtList[_foundShapeBothPointIndex2] &&
                                    //    polygonHistory.poly2BeforePoint2D == mShape[foundShapeBothPtsIndex].screenPoints.PtList[_foundShapeBothPointIndex1]))

                                    //var hasLastPoly = Array.Exists<MyPolygon>(mShape, i =>
                                    //{
                                    //    return i.ScreenPoints.PtList.Contains(polygonHistory.poly1BeforePoint2D) &&
                                    //    i.ScreenPoints.PtList.Contains(polygonHistory.poly2BeforePoint2D) && i.Closed;
                                    //});

                                    var hasLastPoly = mShape.Exists(i =>
                                    {
                                        return i.ScreenPoints.PtList.Contains(polygonHistory.poly1BeforePoint2D) &&
                                        i.ScreenPoints.PtList.Contains(polygonHistory.poly2BeforePoint2D) && i.Closed;
                                    });

                                    if (hasLastPoly)
                                    {
                                        NewPolygon.Close();
                                        NewPolygon.ScreenPoints.PtList.Add(NewPolygon.ScreenPoints.PtList[0]);
                                    }
                                }

                                //resize the array and add the NewPolygon
                                //Array.Resize(ref mShape, mShape.Length + 1);
                                //mShape[mShape.GetUpperBound(0)] = NewPolygon;

                                mShape.Add(NewPolygon);

                                polygonHistory = null;
                            }





                        }


                    }//its already closed so we must be adding a new polygon(a line, then nearby points) to the mShape array                     
                    else
                    {

                        //bool found = false;
                        //int foundShapeIndex = -1;
                        //int foundShapePointIndex = -1;

                        //look for ending point (foundPoint) in the existing polygons in mShape array
                        //for (int k = 0; k < mShape.Length; k++)
                        //{
                        //    // found
                        //    foundShapePointIndex = mShape[k].screenPoints.PtList.FindIndex(i => { return i == pointInfo.foundPoint; });

                        //    //did we find it
                        //    if (foundShapePointIndex != -1 && (mShape[k].Closed() == false))
                        //    {
                        //        //store the polygon index where it was found
                        //        foundShapeIndex = k;
                        //        break;
                        //    }
                        //}

                        //if (foundShapePointIndex != -1 && foundShapeIndex != -1)
                        //{
                        //    //if (pointInfo.foundPointIndex == 0)//did we end on the first index
                        //    //{

                        //    //    mShape[foundShapeIndex].Add(NewPolygon[1]);
                        //    //    mShape[foundShapeIndex].screenPoints.PtList.Add(NewPolygon.screenPoints.PtList[1]);
                        //    //}
                        //    //else if (pointInfo.foundPointIndex == lastIndex) //did we end up on the last index
                        //    //{
                        //    //    mShape[foundShapeIndex].Add(NewPolygon[0]);
                        //    //    mShape[foundShapeIndex].screenPoints.PtList.Add(NewPolygon.screenPoints.PtList[0]);
                        //    //}


                        //    mShape[foundShapeIndex].InsertAt(foundShapePointIndex, NewPolygon[0]);
                        //    mShape[foundShapeIndex].screenPoints.PtList.Insert(foundShapePointIndex, NewPolygon.screenPoints.PtList[0]);

                        //    //mShape[foundShapeIndex].Add( NewPolygon[0]);
                        //    // mShape[foundShapeIndex].screenPoints.PtList.Add( NewPolygon.screenPoints.PtList[0]);


                        //}


                        //lets make sure we are not starting and ending on the same point
                        if (polygonHistory.poly1BeforePoint2D != polygonHistory.poly2BeforePoint2D)
                        {
                            //add existing lines (points) to NewPolygon, one at the start, one at the end  
                            if (polygonHistory.poly1BeforePoint != null)
                            {

                                NewPolygon.InsertAt(0, polygonHistory.poly1BeforePoint);
                                NewPolygon.ScreenPoints.PtList.Insert(0, polygonHistory.poly1BeforePoint2D);
                            }

                            if (polygonHistory.poly2BeforePoint != null)
                            {
                                NewPolygon.Add(polygonHistory.poly2BeforePoint);
                                NewPolygon.ScreenPoints.PtList.Add(polygonHistory.poly2BeforePoint2D);
                            }


                            //if(polygonHistory.poly1AfterPoint != null)
                            //{
                            //    NewPolygon.Add(polygonHistory.poly1AfterPoint);
                            //    NewPolygon.screenPoints.PtList.Add(polygonHistory.poly1AfterPoint2D);
                            //}




                            //if (polygonHistory.poly2AfterPoint != null)
                            //{
                            //    NewPolygon.Add(polygonHistory.poly2AfterPoint);
                            //    NewPolygon.screenPoints.PtList.Add(polygonHistory.poly2AfterPoint2D);
                            //}


                            //resize the array and add the NewPolygon
                            //Array.Resize(ref mShape, mShape.Length + 1);
                            //mShape[mShape.GetUpperBound(0)] = NewPolygon;
                            mShape.Add(NewPolygon);

                            polygonHistory = null;

                        }

                    }



                    NewPolygon = null;
                    line = new Point[2];
                    rectPts = new Point[2];
                    FirstShapeAdded = false;


                    AddPolygonsToComboBox();
                }
            }

            //Segments.Add(NewSegment);
            //NewSegment = null;
            //picCanvas.Refresh();
            Invalidate();
        }
        private void SetIcon(string IconName)
        {

            if (Cursor != null)
            {
                var loc = ConfigurationManager.AppSettings["cursorLoc"];
                string file = $@"{loc}\{IconName}";
                if (Cursor.Current != null)
                {
                    Cursor mycursor = new Cursor(Cursor.Current.Handle);
                    IntPtr colorcursorhandle = LoadCursorFromFile(file);
                    mycursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, mycursor, new object[] { colorcursorhandle });
                    this.Cursor = mycursor;
                    CurrentCursorIcon = IconName;
                }
                //else
                //{
                //    Cursor cs = new Cursor(file);
                //    this.Cursor = cs;
                //}

            }

        }
        private void SaveCurrentCursor(String IconName)
        {
            // Cursor mycursor = new Cursor(Cursor.Current.Handle);
            CurrentCursor = new Cursor(Cursor.Current.Handle);
            SetIcon(IconName);

        }

        private void ResetThisCursor()
        {
            if (CurrentCursor != null)
            {
                //  mycursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, mycursor, new object[] { colorcursorhandle });
                //Cursor cursor = new Cursor(CurrentCursor.Handle);
                ////cursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, cursor, new object[] { colorcursorhandle });

                //this.Cursor = cursor;
                CurrentCursor = new Cursor(CurrentCursor.Handle);
                IntPtr colorcursorhandle = LoadCursorFromFile($@"C:\Users\OFFICE10\Documents\Visual Studio 2017\Projects\ThreeDApp2\ThreeDApp2\cursors\{CurrentCursorIcon}");
                CurrentCursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, CurrentCursor, new object[] { colorcursorhandle });
                this.Cursor = CurrentCursor;
            }
        }

        private bool isSelectIcon()
        {
            return CurrentCursorIcon == Icons.Select ? true : false;
        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (polyData.PtInPoly)
                {
                    if (isSelectIcon())
                    {
                        polyData.IsSelected = true;
                        polyData.SelectedPolyIndex = polyData.PolyIndex;
                    }
                    else
                    {
                        polyData.IsSelected = false;
                        polyData.SelectedPolyIndex = -1;
                    }
                }
                else
                {
                    polyData.IsSelected = false;
                    polyData.SelectedPolyIndex = -1;
                }


                if (panBtn.Checked)
                {
                    MouseIsDown = true;
                }
                else if (rectBtn.Checked)
                {
                    MouseIsDown = true;
                    rectPts[0] = TransformPoint(e.Location);
                }
                else if (drawBtn.Checked)
                {
                    if (pointInfo != null)
                    {
                        if (pointInfo.IsOnPoint)
                        {
                            NewPolygon = new MyPolygon();
                            NewPolygon.Add(pointInfo.p3d);
                            NewPolygon.ScreenPoints.PtList.Add(pointInfo.foundPoint);

                            polygonHistory = new PolygonHistory();
                            polygonHistory.Polygon1Index = pointInfo.foundPointPolygonIndex;
                            var poly = mShape[pointInfo.foundPointPolygonIndex];
                            Point3D ptBefore = null;
                            Point pt2Dbefore = Point.Empty;
                            int previousIndex = pointInfo.foundPointIndex - 1;
                            //TODO: still have to handle when we end up on the same point (
                            if (previousIndex >= 0)
                            {
                                ptBefore = poly[previousIndex];
                                pt2Dbefore = poly.ScreenPoints.PtList[previousIndex];
                            }
                            else
                            {

                                //if (poly.GetSize() > 2)
                                {
                                    ptBefore = poly[poly.GetSize() - 2];
                                    pt2Dbefore = poly.ScreenPoints.PtList[poly.GetSize() - 2];
                                }
                            }
                            //Point3D ptAfter = null;
                            //Point pt2DAfter = Point.Empty;
                            //if((pointInfo.foundPointIndex + 1) < poly.GetSize())
                            //{
                            //    ptAfter = poly[pointInfo.foundPointIndex + 1];
                            //    pt2DAfter = poly.screenPoints.PtList[pointInfo.foundPointIndex + 1];
                            //}
                            //else
                            //{
                            //    //if(poly.GetSize() > 2)
                            //    //{
                            //    //    ptAfter = poly[0];
                            //    //    pt2DAfter = poly.screenPoints.PtList[0];
                            //    //}
                            //}


                            polygonHistory.poly1BeforePoint = ptBefore;
                            polygonHistory.poly1BeforePoint2D = pt2Dbefore;
                            //polygonHistory.poly1AfterPoint = ptAfter;
                            //polygonHistory.poly1AfterPoint2D = pt2DAfter;






                            Vector vDiff = Point3D.Diff(pointInfo.p3d, infinite3DStartY);

                            var oldPt1Start = new Point3D(infinite3DEndY.X, infinite3DEndY.Y, infinite3DEndY.Z);

                            var newPtend = oldPt1Start.Translate(vDiff);

                            infinite3DEndY.X = newPtend.X;
                            infinite3DEndY.Y = newPtend.Y;
                            infinite3DEndY.Z = newPtend.Z;


                            infinite3DStartY.X = pointInfo.p3d.X;
                            infinite3DStartY.Y = pointInfo.p3d.Y;
                            infinite3DStartY.Z = pointInfo.p3d.Z;

                            infinite2DStartY = Fetch2DPoint(infinite3DStartY);
                            infinite2DEndY = Fetch2DPoint(infinite3DEndY);




                        }


                    }
                }



            }
            Invalidate();

        }

        private int FindPolygonIndexFromPoint(Point searchPt)
        {

            int ind = -1;

            for (int k = 0; k < mShape.Count; k++)
            {
                if (mShape[k].ScreenPoints.PtList.Contains(searchPt))
                {
                    ind = k;
                }
            }
            return ind;

            //var pol = mShape.FindIndex(p =>
            //{

            //    int ind = p.ScreenPoints.PtList.FindIndex(pt =>
            //    {
            //        if (PointInRange(searchPt, pt))
            //        {
            //            return true;
            //        }
            //        else
            //        {
            //            return false;
            //        }

            //    });

            //    if (ind != -1)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }

            //});

            //return pol;
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

        private Point TransformPoint(Point point)
        {
            point.X -= offsetWidth;
            point.Y -= offsetHeight;

            return point;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {


            //lastMousePos = Point.Empty;
            if (e.Button == MouseButtons.Left)
            {
                if (panBtn.Checked)
                {
                    if (MouseIsDown)
                    {
                        float delta = 0.1f;
                        var x = e.X - lastMousePos.X;
                        var y = e.Y - lastMousePos.Y;
                        // float delta = 0.1f;



                        if (x > 0 && x > y)
                        {
                            mCamera.X -= delta;
                            Update2dPoints();
                            Invalidate();
                        }
                        if (x < 0 && x < y)
                        {
                            mCamera.X += delta;
                            Update2dPoints();
                            Invalidate();
                        }

                        if (y > 0 && y > x)
                        {
                            mCamera.Y -= delta;
                            Update2dPoints();
                            Invalidate();
                        }

                        if (y < 0 && y < x)
                        {
                            mCamera.Y += delta;
                            Update2dPoints();
                            Invalidate();
                        }

                        lastMousePos = new Point(e.X, e.Y);

                    }
                }
                else if (rectBtn.Checked)
                {
                    if (MouseIsDown)
                    {
                        if (mShape.Count > 0)
                        {
                            Point currentpoint = TransformPoint(e.Location);
                            rectPts[1] = TransformPoint(e.Location);

                            MyPolygon rect = CreatePolygonFromPoints(rectPts);
                            if (FirstShapeAdded == false)
                            {
                                mShape.Add(rect);
                                FirstShapeAdded = true;
                            }
                            else
                            {
                                int lastindex = mShape.Count - 1;
                                //MyPolygon currentRect = mShape[lastindex];
                                MyPolygon updatedRect = UpdatePolygonFromPoint(rectPts);

                                //if (currentpoint.X > rectPts[0].X)
                                {
                                    mShape[lastindex] = updatedRect;
                                }
                            }


                        }

                    }

                }
            }
            if (e.Button == MouseButtons.Middle)
            {

                // SaveCurrentCursor(Icons.Orbit);
                //SetIcon(Icons.Orbit);

                var x = e.X - lastMousePos.X;
                var y = e.Y - lastMousePos.Y;
                //textVal = String.Format("{0} :: {1}", e.X, e.Y);
                textVal = String.Format("{0} :: {1}", x, y);

                //if (lastMousePos != Point.Empty)
                {

                    if (x > 0 && x > y)
                    {
                        if (orbitBtn.Checked)
                        {
                            RotateAll(Axis.Y, -1);
                        }
                        else
                        {
                            RotateShapes(Axis.Y, -1);
                        }

                        Invalidate();
                    }
                    if (x < 0 && x < y)
                    {
                        if (orbitBtn.Checked)
                        {
                            RotateAll(Axis.Y, 1);
                        }
                        else
                        {
                            RotateShapes(Axis.Y, 1);
                        }
                        Invalidate();
                    }
                    if (y > 0 && y > x)
                    {
                        if (orbitBtn.Checked)
                        {
                            RotateAll(Axis.X, -1);
                        }
                        else
                        {
                            RotateShapes(Axis.X, -1);
                        }
                        Invalidate();
                    }
                    if (y < 0 && y < x)
                    {
                        if (orbitBtn.Checked)
                        {
                            RotateAll(Axis.X, 1);
                        }
                        else
                        {
                            RotateShapes(Axis.X, 1);
                        }
                        Invalidate();
                    }

                }

                lastMousePos = new Point(e.X, e.Y);

            }

            Point cursorPt = e.Location;
            Point foundPt = Point.Empty;


            List<MyPolygon> candidates = new List<MyPolygon>();

            for (int j = 0; j < mShape.Count; j++)
            {
                var poly = mShape[j];

                Polygon2D p2d = new Polygon2D();

                Point[] pts = poly.ScreenPoints.PtList.ToArray();

                for (int g = 0; g < pts.Length; g++)
                {
                    pts[g].X += offsetWidth;
                    pts[g].Y += offsetHeight;
                }

                CheckIfPointIsOnLineInPolygon(e.Location, ref poly);
                if (poly.IsOnLine)
                {
                    poly.linePolygonIndex = j;
                }



                polyData.PtInPoly = p2d.IsPointInPolygon(pts, e.Location);//p2d.PointIn(e.Location);

                if (polyData.PtInPoly)
                {
                    poly.parentPointInPolyIndex = j;

                    candidates.Add(poly);

                }

            }

            //get the one closest to the screen by sorting with their centroids
            if (candidates.Count > 0)
            {
                candidates.Sort();
                var g = candidates[0];
                if (g.parentPointInPolyIndex != -1)
                {
                    polyData.PtInPoly = true;
                    polyData.PolyIndex = g.parentPointInPolyIndex;
                }

            }



            //while going thru each polygon
            for (int k = 0; k < mShape.Count; k++)
            {

                //search thru its 2D points to find the cursors point
                int index = mShape[k].ScreenPoints.PtList.FindIndex(p =>
                {
                    if (PointInRange(cursorPt, p))
                    {

                        foundPt = p;
                        //pointInfo = new PointInfo();
                        pointInfo.IsOnPoint = true;
                        pointInfo.foundPoint = foundPt;
                        //infiniteY.X = foundPt.X;
                        //infiniteY.Y = foundPt.Y;
                        return true;
                    }
                    else
                    {
                        foundPt = Point.Empty;
                        //pointInfo = new PointInfo();
                        pointInfo.IsOnPoint = false;
                        //pointInfo.foundPoint = Point.Empty;
                        //pointInfo.p3d = null;
                        //infiniteY = Point.Empty;
                        return false;
                    }

                }

               );
                if (foundPt != Point.Empty)
                {
                    pointInfo.p3d = mShape[k].Absolute(index);
                    pointInfo.foundPointIndex = index;
                    ptCount = mShape[k].PointSeparation(index, mShape[k].GetSize() - 1);
                    pointInfo.foundPointPolygonIndex = k;

                    break;
                }
            }


            //scan each polygon's 2d list looking for this point
            //var pol = mShape.FindIndex(p =>
            //{

            //    int ind = p.ScreenPoints.PtList.FindIndex(pt =>
            //    {
            //        if (PointInRange(cursorPt, pt))
            //        {
            //            foundPt = pt;
            //            pointInfo.IsOnPoint = true;
            //            pointInfo.foundPoint = foundPt;
            //            return true;
            //        }
            //        else
            //        {
            //            foundPt = Point.Empty;
            //            pointInfo.IsOnPoint = false;
            //            return false;
            //        }

            //    });

            //    if (foundPt != Point.Empty)
            //    {
            //        pointInfo.p3d = p.Absolute(ind);
            //        pointInfo.foundPointIndex = ind;
            //        ptCount = p.PointSeparation(ind, p.GetSize() - 1);
            //        // pointInfo.foundPointPolygonIndex = ;

            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }

            //});

            //pointInfo.foundPointPolygonIndex = pol;



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
                if (NewPolygon.ScreenPoints.PtList.Count == 1)
                {
                    //set line array as from existing point to cursor point

                    line[0] = NewPolygon.ScreenPoints.PtList[0];
                    line[1] = new Point(e.Location.X - offsetWidth, e.Location.Y - offsetHeight); //offsetWidth and offsetHeight are translations from top left corner of screen

                }
            }



            Invalidate();
        }

        private void CheckIfPointIsOnLineInPolygon(Point location, ref MyPolygon poly)
        {
            for (int m = 0; m < poly.ScreenPoints.PtList.Count; m++)
            {
                int lastIndex = poly.ScreenPoints.PtList.Count - 2;
                var start = m < lastIndex ? m : lastIndex;
                var end = m < lastIndex ? m + 1 : 0;
                var pt1 = poly.ScreenPoints.PtList[start];
                var pt2 = poly.ScreenPoints.PtList[end];

                var curPt = TransformPoint(location);

                var isOnLine = poly.ScreenPoints.Between(pt1, pt2, curPt);
                if (isOnLine)
                {
                    poly.IsOnLine = true;
                    poly.HiliteIndex1 = start;
                    poly.HiliteIndex2 = end;
                }
                else
                {
                    poly.IsOnLine = false;
                    poly.HiliteIndex1 = -1;
                    poly.HiliteIndex2 = -1;
                }

            }
        }

        private MyPolygon UpdatePolygonFromPoint(Point[] RectPts)
        {
            Point topRight = RectPts[1];
            Point bottomLeft = RectPts[0];

            Point topLeft = new Point(bottomLeft.X, topRight.Y);
            Point bottomRight = new Point(topRight.X, bottomLeft.Y);



            var p3dtopRight = Fetch3DPoint(topRight);
            var p3dbottomLeft = Fetch3DPoint(bottomLeft);
            var p3dtopLeft = Fetch3DPoint(topLeft);
            var p3dbottomRight = Fetch3DPoint(bottomRight);


            MyPolygon rectPoly = new MyPolygon();
            rectPoly.Add(p3dtopLeft);
            rectPoly.Add(p3dtopRight);
            rectPoly.Add(p3dbottomRight);
            rectPoly.Add(p3dbottomLeft);
            rectPoly.Close();

            Set2DPoints(rectPoly, ref rectPoly.ScreenPoints.PtList);
            return rectPoly;



        }

        private MyPolygon CreatePolygonFromPoints(Point[] RectPts)
        {
            Point topRight = RectPts[1];
            Point bottomLeft = RectPts[0];

            Point topLeft = new Point(bottomLeft.X, topRight.Y);
            Point bottomRight = new Point(topRight.X, bottomLeft.Y);



            var p3dtopRight = Fetch3DPoint(topRight);
            var p3dbottomLeft = Fetch3DPoint(bottomLeft);
            var p3dtopLeft = Fetch3DPoint(topLeft);
            var p3dbottomRight = Fetch3DPoint(bottomRight);


            MyPolygon rectPoly = new MyPolygon();
            rectPoly.Add(p3dtopLeft);
            rectPoly.Add(p3dtopRight);
            rectPoly.Add(p3dbottomRight);
            rectPoly.Add(p3dbottomLeft);
            rectPoly.Close();

            Set2DPoints(rectPoly, ref rectPoly.ScreenPoints.PtList);
            return rectPoly;

        }

        //checks if point is closeby to pointChecked creating a snap effect
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
            for (int i = 0; i < mShape.Count; i++)
            {
                Set2DPoints(mShape[i], ref mShape[i].ScreenPoints.PtList);
            }

            Set2DPoint(infinite3DStartY, ref infinite2DStartY);
            Set2DPoint(infinite3DEndY, ref infinite2DEndY);

            Set2DPoint(xAxisLine, ref xAxisLine2D);
            Set2DPoint(yAxisLine, ref yAxisLine2D);
            Set2DPoint(zAxisLine, ref zAxisLine2D);
            Set2DPoint(zeroPt, ref zeroPt2D);

        }
        private void RotateCamera(Axis axis, double degrees)
        {
            float depth = 0;
            switch (axis)
            {
                case Axis.X:
                    mCamera = Point3D.Translate(mCamera, shapeOrigin, zeroPt);
                    mCamera = Point3D.RotateX(mCamera, degrees);
                    mCamera = Point3D.Translate(mCamera, zeroPt, shapeOrigin);
                    // depth = mCamera.Z;
                    break;
                case Axis.Y:
                    mCamera = Point3D.Translate(mCamera, shapeOrigin, zeroPt);
                    mCamera = Point3D.RotateY(mCamera, degrees);
                    mCamera = Point3D.Translate(mCamera, zeroPt, shapeOrigin);
                    //rotPt3d = Point3D.RotateY(pt3d, degrees);
                    //  depth = mCamera.Z;
                    break;
                case Axis.Z:
                    //rotPt3d = Point3D.RotateZ(pt3d, degrees);
                    mCamera = Point3D.Translate(mCamera, shapeOrigin, zeroPt);
                    mCamera = Point3D.RotateZ(mCamera, degrees);
                    mCamera = Point3D.Translate(mCamera, zeroPt, shapeOrigin);
                    // depth = mCamera.Z;
                    break;
            }
        }
        private void RotateAll(Axis axis, double degrees)
        {
            for (int i = 0; i < mShape.Count; i++)
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
                mShape[i].ScreenPoints.PtList = ptList;
                mShape[i].Center.Z = depth;

                //screenPoints[i].PtList = ptList;
                //mShape[i] = po;
            }




            if (infinite2DEndY != Point.Empty)
            {
                if (infinite3DEndY != null)
                {
                    switch (axis)
                    {
                        case Axis.X:
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, shapeOrigin, point0);
                            infinite3DStartY = Point3D.RotateX(infinite3DStartY, degrees);
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, point0, shapeOrigin);

                            infinite3DEndY = Point3D.Translate(infinite3DEndY, shapeOrigin, point0);
                            infinite3DEndY = Point3D.RotateX(infinite3DEndY, degrees);
                            infinite3DEndY = Point3D.Translate(infinite3DEndY, point0, shapeOrigin);

                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, shapeOrigin, point0);
                            pointInfo.p3d = Point3D.RotateX(pointInfo.p3d, degrees);
                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, point0, shapeOrigin);

                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, shapeOrigin, point0);
                            pointInfoEnd.p3d = Point3D.RotateX(pointInfoEnd.p3d, degrees);
                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, point0, shapeOrigin);

                            ///////////////////////////////////////////////////////////////////////////////////////////

                            xAxisLine = Point3D.Translate(xAxisLine, shapeOrigin, point0);
                            xAxisLine = Point3D.RotateX(xAxisLine, degrees);
                            xAxisLine = Point3D.Translate(xAxisLine, point0, shapeOrigin);

                            yAxisLine = Point3D.Translate(yAxisLine, shapeOrigin, point0);
                            yAxisLine = Point3D.RotateX(yAxisLine, degrees);
                            yAxisLine = Point3D.Translate(yAxisLine, point0, shapeOrigin);

                            zAxisLine = Point3D.Translate(zAxisLine, shapeOrigin, point0);
                            zAxisLine = Point3D.RotateX(zAxisLine, degrees);
                            zAxisLine = Point3D.Translate(zAxisLine, point0, shapeOrigin);

                            zeroPt = Point3D.Translate(zeroPt, shapeOrigin, point0);
                            zeroPt = Point3D.RotateX(zeroPt, degrees);
                            zeroPt = Point3D.Translate(zeroPt, point0, shapeOrigin);

                            break;
                        case Axis.Y:
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, shapeOrigin, point0);
                            infinite3DStartY = Point3D.RotateY(infinite3DStartY, degrees);
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, point0, shapeOrigin);

                            infinite3DEndY = Point3D.Translate(infinite3DEndY, shapeOrigin, point0);
                            infinite3DEndY = Point3D.RotateY(infinite3DEndY, degrees);
                            infinite3DEndY = Point3D.Translate(infinite3DEndY, point0, shapeOrigin);

                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, shapeOrigin, point0);
                            pointInfo.p3d = Point3D.RotateY(pointInfo.p3d, degrees);
                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, point0, shapeOrigin);

                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, shapeOrigin, point0);
                            pointInfoEnd.p3d = Point3D.RotateY(pointInfoEnd.p3d, degrees);
                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, point0, shapeOrigin);


                            //////////////////////////////////////////////////////

                            xAxisLine = Point3D.Translate(xAxisLine, shapeOrigin, point0);
                            xAxisLine = Point3D.RotateY(xAxisLine, degrees);
                            xAxisLine = Point3D.Translate(xAxisLine, point0, shapeOrigin);

                            yAxisLine = Point3D.Translate(yAxisLine, shapeOrigin, point0);
                            yAxisLine = Point3D.RotateY(yAxisLine, degrees);
                            yAxisLine = Point3D.Translate(yAxisLine, point0, shapeOrigin);

                            zAxisLine = Point3D.Translate(zAxisLine, shapeOrigin, point0);
                            zAxisLine = Point3D.RotateY(zAxisLine, degrees);
                            zAxisLine = Point3D.Translate(zAxisLine, point0, shapeOrigin);

                            zeroPt = Point3D.Translate(zeroPt, shapeOrigin, point0);
                            zeroPt = Point3D.RotateY(zeroPt, degrees);
                            zeroPt = Point3D.Translate(zeroPt, point0, shapeOrigin);


                            break;
                        case Axis.Z:
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, shapeOrigin, point0);
                            infinite3DStartY = Point3D.RotateZ(infinite3DStartY, degrees);
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, point0, shapeOrigin);

                            infinite3DEndY = Point3D.Translate(infinite3DEndY, shapeOrigin, point0);
                            infinite3DEndY = Point3D.RotateZ(infinite3DEndY, degrees);
                            infinite3DEndY = Point3D.Translate(infinite3DEndY, point0, shapeOrigin);

                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, shapeOrigin, point0);
                            pointInfo.p3d = Point3D.RotateZ(pointInfo.p3d, degrees);
                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, point0, shapeOrigin);

                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, shapeOrigin, point0);
                            pointInfoEnd.p3d = Point3D.RotateZ(pointInfoEnd.p3d, degrees);
                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, point0, shapeOrigin);



                            xAxisLine = Point3D.Translate(xAxisLine, shapeOrigin, point0);
                            xAxisLine = Point3D.RotateZ(xAxisLine, degrees);
                            xAxisLine = Point3D.Translate(xAxisLine, point0, shapeOrigin);

                            yAxisLine = Point3D.Translate(yAxisLine, shapeOrigin, point0);
                            yAxisLine = Point3D.RotateZ(yAxisLine, degrees);
                            yAxisLine = Point3D.Translate(yAxisLine, point0, shapeOrigin);

                            zAxisLine = Point3D.Translate(zAxisLine, shapeOrigin, point0);
                            zAxisLine = Point3D.RotateZ(zAxisLine, degrees);
                            zAxisLine = Point3D.Translate(zAxisLine, point0, shapeOrigin);

                            zeroPt = Point3D.Translate(zeroPt, shapeOrigin, point0);
                            zeroPt = Point3D.RotateZ(zeroPt, degrees);
                            zeroPt = Point3D.Translate(zeroPt, point0, shapeOrigin);
                            break;
                    }
                    infinite2DEndY = Fetch2DPoint(infinite3DEndY);
                    infinite2DStartY = Fetch2DPoint(infinite3DStartY);
                    pointInfo.foundPoint = Fetch2DPoint(pointInfo.p3d);
                    xAxisLine2D = Fetch2DPoint(xAxisLine);
                    yAxisLine2D = Fetch2DPoint(yAxisLine);
                    zAxisLine2D = Fetch2DPoint(zAxisLine);
                    zeroPt2D = Fetch2DPoint(zeroPt);
                }



            }




        }





        private void RotateShapes(Axis axis, double degrees)
        {
            for (int i = 0; i < mShape.Count; i++)
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
                mShape[i].ScreenPoints.PtList = ptList;
                mShape[i].Center.Z = depth;

                //screenPoints[i].PtList = ptList;
                //mShape[i] = po;
            }


            if (infinite2DEndY != Point.Empty)
            {
                if (infinite3DEndY != null)
                {
                    switch (axis)
                    {
                        case Axis.X:
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, shapeOrigin, point0);
                            infinite3DStartY = Point3D.RotateX(infinite3DStartY, degrees);
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, point0, shapeOrigin);

                            infinite3DEndY = Point3D.Translate(infinite3DEndY, shapeOrigin, point0);
                            infinite3DEndY = Point3D.RotateX(infinite3DEndY, degrees);
                            infinite3DEndY = Point3D.Translate(infinite3DEndY, point0, shapeOrigin);

                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, shapeOrigin, point0);
                            pointInfo.p3d = Point3D.RotateX(pointInfo.p3d, degrees);
                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, point0, shapeOrigin);

                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, shapeOrigin, point0);
                            pointInfoEnd.p3d = Point3D.RotateX(pointInfoEnd.p3d, degrees);
                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, point0, shapeOrigin);


                            break;
                        case Axis.Y:
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, shapeOrigin, point0);
                            infinite3DStartY = Point3D.RotateY(infinite3DStartY, degrees);
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, point0, shapeOrigin);

                            infinite3DEndY = Point3D.Translate(infinite3DEndY, shapeOrigin, point0);
                            infinite3DEndY = Point3D.RotateY(infinite3DEndY, degrees);
                            infinite3DEndY = Point3D.Translate(infinite3DEndY, point0, shapeOrigin);

                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, shapeOrigin, point0);
                            pointInfo.p3d = Point3D.RotateY(pointInfo.p3d, degrees);
                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, point0, shapeOrigin);

                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, shapeOrigin, point0);
                            pointInfoEnd.p3d = Point3D.RotateY(pointInfoEnd.p3d, degrees);
                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, point0, shapeOrigin);
                            break;
                        case Axis.Z:
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, shapeOrigin, point0);
                            infinite3DStartY = Point3D.RotateZ(infinite3DStartY, degrees);
                            infinite3DStartY = Point3D.Translate(infinite3DStartY, point0, shapeOrigin);

                            infinite3DEndY = Point3D.Translate(infinite3DEndY, shapeOrigin, point0);
                            infinite3DEndY = Point3D.RotateZ(infinite3DEndY, degrees);
                            infinite3DEndY = Point3D.Translate(infinite3DEndY, point0, shapeOrigin);

                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, shapeOrigin, point0);
                            pointInfo.p3d = Point3D.RotateZ(pointInfo.p3d, degrees);
                            pointInfo.p3d = Point3D.Translate(pointInfo.p3d, point0, shapeOrigin);

                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, shapeOrigin, point0);
                            pointInfoEnd.p3d = Point3D.RotateZ(pointInfoEnd.p3d, degrees);
                            pointInfoEnd.p3d = Point3D.Translate(pointInfoEnd.p3d, point0, shapeOrigin);
                            break;
                    }
                    infinite2DEndY = Fetch2DPoint(infinite3DEndY);
                    infinite2DStartY = Fetch2DPoint(infinite3DStartY);
                    pointInfo.foundPoint = Fetch2DPoint(pointInfo.p3d);
                }



            }

            //Point3D[] pts = new Point3D[3];
            //pts[0] = mShape[0][0];//.Absolute(0);
            //pts[1] = mShape[0][1];//.Absolute(1);
            //pts[2] = mShape[0][2];//.Absolute(2);

            //normalSample = calcNormal(pts);


        }

        private void RotatePoint(ref Point Pt2d, ref Point3D point3D, Axis axis, double degrees)
        {
            switch (axis)
            {
                case Axis.X:

                    point3D = Point3D.Translate(point3D, shapeOrigin, point0);
                    point3D = Point3D.RotateX(point3D, degrees);
                    point3D = Point3D.Translate(point3D, point0, shapeOrigin);
                    break;
                case Axis.Y:

                    point3D = Point3D.Translate(point3D, shapeOrigin, point0);
                    point3D = Point3D.RotateY(point3D, degrees);
                    point3D = Point3D.Translate(point3D, point0, shapeOrigin);
                    break;
                case Axis.Z:

                    point3D = Point3D.Translate(point3D, shapeOrigin, point0);
                    point3D = Point3D.RotateZ(point3D, degrees);
                    point3D = Point3D.Translate(point3D, point0, shapeOrigin);
                    break;
            }
            Pt2d = Fetch2DPoint(point3D);

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
                        infinite3DEndY = Point3D.MoveObject(infinite3DEndY, new Point3D(pointInfoEnd.p3d.X, pointInfoEnd.p3d.Y, pointInfoEnd.p3d.Z));
                        infinite2DEndY = Fetch2DPoint(infinite3DEndY);
                        Invalidate();
                    }
                    else
                    {
                        RotateShapes(Axis.Z, 10);
                        infinite3DEndY = Point3D.MoveObject(infinite3DEndY, new Point3D(pointInfoEnd.p3d.X, pointInfoEnd.p3d.Y, pointInfoEnd.p3d.Z));
                        infinite2DEndY = Fetch2DPoint(infinite3DEndY);
                        Invalidate();
                    }
                    break;

                case Keys.S:
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        RotateCamera(Axis.X, -1);
                        Update2dPoints();
                        Invalidate();
                    }
                    else
                    {
                        RotateCamera(Axis.X, 1);
                        Update2dPoints();
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
            g.DrawString($"Camera: {mCamera.X} {mCamera.Y} {mCamera.Z}", SystemFonts.DefaultFont, Brushes.Black, 5f, 25f);
            g.DrawString($"Cur Pt: {currentPoint.X}, {currentPoint.Y}", SystemFonts.DefaultFont, Brushes.Black, 5f, 45f);
            //g.DrawString($"mouse pos: {textVal}", SystemFonts.DefaultFont, Brushes.Black, 5f, 45f);
            // g.DrawString($"normal: X: {normalSample.X}, Y: {normalSample.Y}, Z: {normalSample.Z}", SystemFonts.DefaultFont, Brushes.Black, 5f, 65f);
            g.DrawString($"Screen: X: {mScreen.X}, Y: {mScreen.Y}, Z: {mScreen.Z}", SystemFonts.DefaultFont, Brushes.Black, 5f, 65f);
            int currentpoly = -1;



            if (infinite3DEndY != null)
            {
                g.DrawString($"infinite: X: {infinite3DEndY.X}, Y: {infinite3DEndY.Y}, Z: {infinite3DEndY.Z}", SystemFonts.DefaultFont, Brushes.Black, 5f, 85f);

            }



            g.TranslateTransform(ClientRectangle.Width / 2, ClientRectangle.Height / 2);
            Point startPt = new Point();
            g.SmoothingMode = SmoothingMode.AntiAlias;


            g.DrawLine(Pens.Red, zeroPt2D, xAxisLine2D);
            g.DrawLine(Pens.Blue, zeroPt2D, yAxisLine2D);
            g.DrawLine(Pens.Green, zeroPt2D, zAxisLine2D);




            //Array.Sort(mShape);
            //Array.Reverse(mShape);

            mShape.Sort();
            mShape.Reverse();





            // Draw each of our shapes
            for (int i = 0; i < mShape.Count; i++)
            {
                Color shadedColor = FinalColor(mShape[i], new Point3D(5, 50, 10), g);
                //g.FillPolygon(Brushes.DarkGray, mShape[i].screenPoints.PtList.ToArray());
                var brush = new SolidBrush(shadedColor);
                if (mShape[i].Closed)
                {
                    // g.FillPolygon(brush, mShape[i].screenPoints.PtList.ToArray());
                    g.FillPolygon(Brushes.DarkGray, mShape[i].ScreenPoints.PtList.ToArray());
                    brush.Dispose();
                }

                bool inPoly = false;

                if (polyData != PolygonData.Empty)
                {
                    if (polyData.PtInPoly)
                    {
                        if (polyData.PolyIndex == i)
                        {
                            inPoly = true;
                            var br = new HatchBrush(HatchStyle.DottedDiamond, Color.Black, Color.Gray);

                            g.FillPolygon(br, mShape[i].ScreenPoints.PtList.ToArray());

                            br.Dispose();

                            currentpoly = i + 1;

                        }
                    }
                    if (i == polyData.SelectedPolyIndex && polyData.SelectedPolyIndex != -1)
                    {
                        if (polyData.IsSelected)
                        {
                            var pn = new Pen(Color.Purple, 4f);

                            g.DrawPolygon(pn, mShape[i].ScreenPoints.PtList.ToArray());

                            pn.Dispose();
                        }
                    }
                }

                if (polygonListCombobox.SelectedIndex != -1)
                {
                    //var poly = polygonListCombobox.SelectedItem as MyPolygon;
                    //if (poly != null)
                    if (polygonListCombobox.SelectedIndex == i)
                    {
                        //if(mShape[i] == poly)                       

                        if (polygonListCombobox.SelectedIndex == i)
                        {
                            var br = new HatchBrush(HatchStyle.DashedHorizontal, Color.Blue, Color.Gray);
                            g.FillPolygon(br, mShape[i].ScreenPoints.PtList.ToArray());
                            br.Dispose();
                        }
                    }
                }



                // Go thru each point
                for (int j = 0; j < mShape[i].GetSize(); j++)
                {

                    pt = mShape[i].ScreenPoints.PtList[j];


                    if (j == 0)
                    {
                        startPt = pt;
                    }
                    else
                    {
                        //if (inPoly)
                        //{
                        //    //var pn = new Pen(Color.Blue, 5f);                            

                        //    {
                        //        g.DrawLine(Pens.Blue, startPt, pt);  // Draw a line to the others
                        //    }

                        //    //pn.Dispose();
                        //}
                        //else
                        {
                            g.DrawLine(Pens.Black, startPt, pt);  // Draw a line to the others
                        }

                        if (mShape[i].IsOnLine)
                        {
                            g.DrawString($"On Line: {currentpoly}", SystemFonts.DefaultFont, Brushes.Black, 5f, ClientRectangle.Bottom - 25f);

                            if (mShape[i].HiliteIndex1 != -1 && mShape[i].HiliteIndex2 != -1)
                            {
                                int startIndex = mShape[i].HiliteIndex1;
                                int endIndex = mShape[i].HiliteIndex2;
                                Point Pt1 = mShape[i].ScreenPoints.PtList[startIndex];
                                Point Pt2 = mShape[i].ScreenPoints.PtList[endIndex];
                                if ((startPt == Pt1) && (pt == Pt2))
                                {
                                    g.DrawLine(Pens.Red, startPt, pt);  // Draw a line to the others
                                }
                            }

                        }



                        startPt = pt;



                    }

                }
            }

            if (currentPoint != Point.Empty)
            {
                g.FillEllipse(Brushes.Red, new RectangleF(currentPoint.X - 3, currentPoint.Y - 3, 6, 6));
            }

            if (drawBtn.Checked && !rectBtn.Checked)
            {
                if (NewPolygon != null)
                {
                    //if(NewPolygon.screenPoints.PtList.Count > 1)
                    if (line.Length > 1)
                    {
                        g.DrawLine(Pens.Black, line[0], line[1]);
                    }
                }
            }

            if (rectBtn.Checked)
            {
                DrawRectangle(g, line[0], line[1]);
            }


            if (infinite2DEndY != Point.Empty)
            {
                //if (inBetween)
                {
                    //if(infinite3DEndY.Z < 0)
                    //{
                    //    infinite2DStartY.Y = -1 * infinite2DStartY.Y;
                    //}

                    // if (PointInfo.Between(infinite2DStartY, infinite2DEndY, line[1]))
                    {
                        ////////////////////   g.DrawLine(new Pen(Color.Red, 3f), infinite2DStartY, infinite2DEndY);
                    }

                }

            }




            //g.DrawLine(Pens.Red, zeroPt2D, xAxisLine2D);

            //g.DrawLine(Pens.Blue, zeroPt2D, yAxisLine2D);
            //g.DrawLine(Pens.Green, zeroPt2D, zAxisLine2D);








            g.TranslateTransform(-(ClientRectangle.Width / 2), -(ClientRectangle.Height / 2));
            // g.DrawString($"Is in between: {inBetween}", SystemFonts.DefaultFont, Brushes.Black, 5f, 105f);
            g.DrawString($"Is in between: {inBetween}", SystemFonts.DefaultFont, Brushes.Black, 5f, 105f);
            g.DrawString($"Is in polygon: {polyData.PtInPoly}", SystemFonts.DefaultFont, Brushes.Black, 5f, 125f);
            g.DrawString($"point count: {ptCount}", SystemFonts.DefaultFont, Brushes.Black, 5f, 145f);
            g.DrawString($"Polygons: {mShape.Count}", SystemFonts.DefaultFont, Brushes.Black, 5f, 165f);
            if (currentpoly != -1)
            {
                g.DrawString($"On Poly: {currentpoly}", SystemFonts.DefaultFont, Brushes.Black, 5f, 185f);
            }


            float toppt = 185f;
            for (int n = 0; n < mShape.Count; n++)
            {
                toppt += 20;
                var status = mShape[n].Closed ? "Closed" : "Open";
                if (currentpoly == (n + 1))
                {
                    g.DrawString($"Polygon {n + 1}, {status}, points:{mShape[n].ScreenPoints.PtList.Count} ", SystemFonts.MessageBoxFont, Brushes.Red, 5f, toppt);
                    StringBuilder pts = new StringBuilder();
                    StringBuilder ptindexes = new StringBuilder();
                    var ptlist = mShape[n].ScreenPoints.PtList;
                    int j = 0;
                    ptlist.ForEach(i => { pts.AppendFormat("{0}, {1}     ", i.X, i.Y); ptindexes.AppendFormat("{0}              ", j); j++; });
                    g.DrawString($"{pts.ToString()}", SystemFonts.DefaultFont, Brushes.DarkBlue, 300f, 45f);
                    g.DrawString($"{ptindexes.ToString()}", SystemFonts.DefaultFont, Brushes.DarkBlue, 300f, 65f);

                }
                else
                {
                    g.DrawString($"Polygon {n + 1}, {status}, points:{mShape[n].ScreenPoints.PtList.Count} ", SystemFonts.DefaultFont, Brushes.Black, 5f, toppt);

                }
            }




        }

        private void DrawRectangle(Graphics g, Point point1, Point point2)
        {
            //throw new NotImplementedException();
        }

        private double getY(float x, double slope, double intercept) { return (slope * x) + intercept; }
        private double getX(float y, double slope, double intercept) { return (y - intercept) / slope; }

        private void UpdateOrtho()
        {
            for (int i = 0; i < mShape.Count; i++)
            {
                SwitchToOrtho(mShape[i], ref mShape[i].ScreenPoints.PtList);
            }

        }


        private void SwitchToOrtho(MyPolygon polygon, ref List<Point> ptList)
        {
            ptList.Clear();
            for (int j = 0; j < polygon.GetSize(); j++)
            {
                Point pt = new Point();
                var view = polygon.Absolute(j);
                pt.X = (Int32)((s * (view.X - mScreen.X)) + (view.X - mCamera.X));
                pt.Y = (Int32)((s * (view.Y - mScreen.Y)) + (view.Y - mCamera.Z));

                ptList.Add(pt);
            }
        }

        private void Set2DPoints(MyPolygon polygon, ref List<Point> ptList)
        {
            if (!settingsBtn.Checked)
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
            else
            {
                SwitchToOrtho(polygon, ref ptList);
            }

        }

        private void Set2DPoint(Point3D point3D, ref Point point)
        {
            Point pt = new Point();
            if (!settingsBtn.Checked)
            {
                var obj = point3D;
                pt.X = (Int32)(s * (obj.X - mCamera.X) * mScreen.Z / (obj.Z - mCamera.Z));
                pt.Y = (Int32)(s * (obj.Y - mCamera.Y) * mScreen.Z / (obj.Z - mCamera.Z));
            }
            else
            {
                pt = Set2DPointOrtho(point3D);
            }

            point = pt;
        }

        private Point Set2DPointOrtho(Point3D point3D)
        {
            Point pt = new Point();
            var view = point3D;
            pt.X = (Int32)((s * (view.X - mScreen.X)) + (view.X - mCamera.X));
            pt.Y = (Int32)((s * (view.Y - mScreen.Y)) + (view.Y - mCamera.Z));

            return pt;
        }

        private Point Fetch2DPointOrtho(Point3D view)
        {
            Point pt = new Point();

            pt.X = (Int32)((s * (view.X - mScreen.X)) + (view.X - mCamera.X));
            pt.Y = (Int32)((s * (view.Y - mScreen.Y)) + (view.Y - mCamera.Z));

            return pt;
        }

        private Point Fetch2DPoint(Point3D pt3d)
        {
            Point pt = new Point();
            if (!settingsBtn.Checked)
            {
                pt.X = (Int32)(s * (pt3d.X - mCamera.X) * mScreen.Z / (pt3d.Z - mCamera.Z));
                pt.Y = (Int32)(s * (pt3d.Y - mCamera.Y) * mScreen.Z / (pt3d.Z - mCamera.Z));
            }
            else
            {
                Fetch2DPointOrtho(pt3d);
            }




            return pt;
        }

        private Point3D Fetch3DPoint(Point pt)
        {
            Point3D pt3d = new Point3D(0, 0, 0);

            pt3d.Z = 19;//((s * mScreen.Z / pt.X) * (pt3d.X - mCamera.X)) + mCamera.Z;


            pt3d.X = (((((pt3d.Z - mCamera.Z) * pt.X) / mScreen.Z) / s) + mCamera.X);
            pt3d.Y = (((((pt3d.Z - mCamera.Z) * pt.Y) / mScreen.Z) / s) + mCamera.Y);


            return pt3d;
        }


        private float mix(float value1, float value2, float factor)
        {
            return (value1 * factor) + (value2 * (1 - factor));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            //BuildGraph();

            SetIcon(Icons.Select);

            pointInfo.p3d = new Point3D(0, 0, 20);
            pointInfo.foundPoint = Fetch2DPoint(pointInfo.p3d);

            AddPolygonsToComboBox();

            //  SetInitialRotation();
            //RotateAll(Axis.X, -30);
            //RotateAll(Axis.Y, 30);
        }

        private void SetInitialRotation()
        {
            RotateAll(Axis.Z, 45);
            RotateAll(Axis.X, 60);
        }

        public void AddPolygonsToComboBox()
        {
            //polygonListCombobox.Sorted = false;

            List<MyPolygon> myPolygons = new List<MyPolygon>();
            myPolygons.AddRange(mShape);

            //myPolygons.Sort();
            //myPolygons.Reverse();

            polygonListCombobox.Items.Clear();

            polygonListCombobox.Items.AddRange(myPolygons.ToArray());
            // Array.ForEach<Polygon>(msh, i => { polygonListCombobox.Items.Add(i); });

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
                shade *= 2;
                if (shade < 0)
                {
                    shade = 0;
                }
                if (shade > 255)
                {
                    shade = 255;
                }

                //gr.DrawString(shadeVal.ToString() + $" {lightPoint.X}, {lightPoint.Y}, {lightPoint.Z}", SystemFonts.DefaultFont, Brushes.Black, new PointF(5f, 55f));
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

            //CursorConverter cc = new CursorConverter();
            //object cur = cc.ConvertFrom(global::ThreeDApp2.Properties.Resources.Zoom_In);
            //Cursor = (System.Windows.Forms.Cursor)cur;

            var loc = ConfigurationManager.AppSettings["cursorLoc"];//ConfigurationSettings.AppSettings["cursorLoc"];

            Cursor mycursor = new Cursor(Cursor.Current.Handle);
            IntPtr colorcursorhandle = LoadCursorFromFile($@"{loc}\ZoomIn.cur");
            mycursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, mycursor, new object[] { colorcursorhandle });
            this.Cursor = mycursor;

            int delta = 1;
            mCamera.Z += delta;
            Update2dPoints();
            Invalidate();
        }

        private void zoomOutBtn_Click(object sender, EventArgs e)
        {
            //CursorConverter cc = new CursorConverter();
            //object cur = cc.ConvertFrom(global::ThreeDApp2.Properties.Resources.Zoom_Out);
            //Cursor = (System.Windows.Forms.Cursor)cur;

            Cursor mycursor = new Cursor(Cursor.Current.Handle);
            var loc = ConfigurationManager.AppSettings["cursorLoc"];
            //dinosau2.ani is in windows folder：
            string path = $@"{loc}\ZoomOut.cur";
            IntPtr colorcursorhandle = LoadCursorFromFile(path);
            mycursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, mycursor, new object[] { colorcursorhandle });
            this.Cursor = mycursor;

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
            for (int k = 0; k < mShape.Count; k++)
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

            for (int k = 0; k < mShape.Count; k++)
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

        private void leftViewBtn_Click(object sender, EventArgs e)
        {
            RotateAll(Axis.Y, 10);
            // Update2dPoints();
            Invalidate();
        }

        private void topViewBtn_Click(object sender, EventArgs e)
        {
            RotateCamera(Axis.X, 10);
            Update2dPoints();
            Invalidate();
        }

        private void rightViewBtn_Click(object sender, EventArgs e)
        {
            RotateCamera(Axis.Z, 10);
            Update2dPoints();
            Invalidate();
        }

        private void bottomViewBtn_Click(object sender, EventArgs e)
        {
            RotateCamera(Axis.X, -10);
            Update2dPoints();
            Invalidate();
        }

        private void drawBtn_DisplayStyleChanged(object sender, EventArgs e)
        {

        }

        private void drawBtn_Click(object sender, EventArgs e)
        {
            //CursorConverter cc = new CursorConverter();
            //object cur = cc.ConvertFrom(global::ThreeDApp2.Properties.Resources.flash_8_pencil);
            //Cursor = (System.Windows.Forms.Cursor)cur;



            //Cursor mycursor = new Cursor(Cursor.Current.Handle);
            //IntPtr colorcursorhandle = LoadCursorFromFile(@"C:\Users\OFFICE10\Documents\Visual Studio 2017\Projects\ThreeDApp2\ThreeDApp2\cursors\flash 8 pencil.cur");
            //mycursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, mycursor, new object[] { colorcursorhandle });
            //this.Cursor = mycursor;
            // SetIcon(Icons.Pencil);

            string icon;
            icon = drawBtn.Checked ? Icons.Pencil : Icons.Select;
            SetIcon(icon);
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {

        }

        private void moveBtn_Click(object sender, EventArgs e)
        {
            //CursorConverter cc = new CursorConverter();
            //object cur = cc.ConvertFrom(global::ThreeDApp2.Properties.Resources.BurninLitt14_Cursor_1_Drag_Or_Move);
            //Cursor = (System.Windows.Forms.Cursor)cur;

            //Cursor mycursor = new Cursor(Cursor.Current.Handle);
            ////dinosau2.ani is in windows folder：
            //IntPtr colorcursorhandle = LoadCursorFromFile(@"C:\Users\OFFICE10\Documents\Visual Studio 2017\Projects\ThreeDApp2\ThreeDApp2\cursors\BurninLitt14 Cursor 1 Drag Or Move.cur");
            //mycursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, mycursor, new object[] { colorcursorhandle });
            //this.Cursor = mycursor;

            string icon;
            icon = moveBtn.Checked ? Icons.Move : Icons.Select;
            SetIcon(icon);
        }

        private void panBtn_Click(object sender, EventArgs e)
        {
            //CursorConverter cc = new CursorConverter();
            // object cur = cc.ConvertFrom(global::ThreeDApp2.Properties.Resources.);
            //Cursor = (System.Windows.Forms.Cursor)cur;

            if (panBtn.Checked)
            {
                SetIcon(Icons.Pan);
            }
            else
            {
                SetIcon(Icons.Select);
            }

        }

        private void selectBtn_Click(object sender, EventArgs e)
        {
            SetIcon(Icons.Select);
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void polygonListCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //  foreach()

            Invalidate();

        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            //Array.Clear(mShape, 0, mShape.Length);
            mShape.Clear();

            //Array.Resize(ref mShape, 3);

            mShape = new MyList<MyPolygon>();
            InitializeScene();
            //  SetInitialRotation();
        }

        private void undoBtn_Click(object sender, EventArgs e)
        {
            mShape.Undo();
            Invalidate();
        }

        private void redoBtn_Click(object sender, EventArgs e)
        {
            mShape.Redo();
            Invalidate();
        }

        private void rectBtn_Click(object sender, EventArgs e)
        {

        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            AdjustOffset();
        }

        private void settingsBtn_Click(object sender, EventArgs e)
        {
            //UpdateOrtho();
            Invalidate();
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            DeleteSelectedPoly();
        }

        private void DeleteSelectedPoly()
        {
            if (polyData.SelectedPolyIndex != -1)
            {
                mShape.RemoveAt(polyData.SelectedPolyIndex);
                polyData.SelectedPolyIndex = -1;
                polyData.IsSelected = false;
                Invalidate();
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void Form1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedPoly();
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
        public Point3D p3d = new Point3D(0, 0, 20);
        public Point3D p3dBefore = null;
        public Point3D p3dAfter = null;
        public bool IsOnPoint = false;
        public Point foundPoint = Point.Empty;
        public Point pointBefore = Point.Empty;
        public Point pointAfter = Point.Empty;
        public int foundPointIndex = -1;
        public const float EPSILON = 0.001f;
        public int foundPointPolygonIndex = -1;


        public static bool Between(Point a, Point b, Point c)
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

        public static bool Colinear(Point a, Point b, Point c)
        {
            // return (ABS(TriArea2(a, b, c)) < EPSILON);	// inefficient
            float CrossZ = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
            return (Math.Abs(CrossZ) < EPSILON);
        }
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

    public class PolygonData
    {
        public bool PtInPoly = false;
        public int PolyIndex = -1;

        public static PolygonData Empty
        {
            get { return new PolygonData(); }
        }

        public bool IsSelected { get; internal set; }
        public int SelectedPolyIndex { get; internal set; }
    }
    public class PolygonHistory
    {
        public int Polygon1Index = -1;
        public int Polygon2Index = -1;

        public Point3D poly1BeforePoint = null;
        public Point poly1BeforePoint2D = Point.Empty;


        public Point3D poly1AfterPoint = null;
        public Point poly1AfterPoint2D = Point.Empty;

        public Point3D poly2BeforePoint = null;
        public Point poly2BeforePoint2D = Point.Empty;

        public Point3D poly2AfterPoint = null;
        public Point poly2AfterPoint2D = Point.Empty;

    }
    public class Icons
    {
        public static string ZoomIn
        {
            get { return "ZoomIn.cur"; }
        }

        public static string ZoomOut
        {
            get { return "ZoomOut.cur"; }
        }

        public static string Pan
        {
            get { return "Mario hand.ani"; }
        }

        public static string Pencil
        {
            // get { return "pencil (34).cur"; }
            get { return "pencil(26).cur"; }
        }
        public static string Move
        {
            get { return "BurninLitt14 Cursor 1 Drag Or Move.cur"; }
        }
        public static string Orbit
        {
            get { return "Earth-orbit.ani"; }
        }
        public static string Select
        {
            get { return "Aero Arrow Select.cur"; }
        }

    }
}

