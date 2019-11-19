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
		Point currentPoint = Point.Empty;
		string textVal;
		Point lastMousePos = Point.Empty;


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

			shapeOrigin.X = 0;//ClientRectangle.Width / 2;
			shapeOrigin.Y = 10;
			shapeOrigin.Z = 30;


			mCamera = new Point3D(0, 0, 0);  // Our viewpoint
			mScreen = new Point3D(0, 0, 30); // Where we project the image to
											 // mScreen = new Point3D(0, 0, shapeOrigin.Z); // Where we project the image to
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

			mShape[0].Center = new Point3D(5, 8, 20);


			//     Set2DPoints(mShape[0], ref screenPoints[0].PtList);
			Set2DPoints(mShape[0], ref mShape[0].screenPoints.PtList);



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
						RotateShapes(Axis.X, 1);
						Invalidate();
					}
					if (x < 0 && x < y)
					{
						RotateShapes(Axis.X, -1);
						Invalidate();
					}

					if (y > 0 && y > x)
					{
						RotateShapes(Axis.Y, 1);
						Invalidate();
					}

					if (y < 0 && y < x)
					{
						RotateShapes(Axis.Y, -1);
						Invalidate();
					}


				}

				lastMousePos = new Point(e.X, e.Y);



			}

			Point cursorPt = e.Location;



			Point foundPt = mShape[0].screenPoints.PtList.Find(p =>

		   {
			   if (((cursorPt.X >= (p.X - 10)) && (cursorPt.X <= (p.X + 10))) && ((cursorPt.Y >= (p.Y - 10)) && (cursorPt.Y <= (p.Y + 10))))
			   {
				   //textVal = "Seen!";
				   return true;
			   }
			   else
			   {
				   //textVal = string.Empty;
				   return false;
			   }

		   });

			if (foundPt != null)
			{
				currentPoint = foundPt;
			}
			else
			{
				currentPoint = Point.Empty;
			}
			Invalidate();
		}

		//private void Update2dPoints()
		//{
		//    Set2DPoints(mShape[0], ref screenPoints[0].PtList);
		//    Set2DPoints(mShape[1], ref screenPoints[1].PtList);
		//    Set2DPoints(mShape[2], ref screenPoints[2].PtList);
		//}
		private void Update2dPoints()
		{
			Set2DPoints(mShape[0], ref mShape[0].screenPoints.PtList);
			Set2DPoints(mShape[1], ref mShape[1].screenPoints.PtList);
			Set2DPoints(mShape[2], ref mShape[2].screenPoints.PtList);
		}
		private void RotateShapes(Axis axis, double degrees)
		{
			for (int i = 0; i < 3; i++)
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
			g.TranslateTransform(ClientRectangle.Width / 2, ClientRectangle.Height / 2);
			Point startPt = new Point();
			g.SmoothingMode = SmoothingMode.AntiAlias;



			//Matrix m;
			Array.Sort(mShape);
			Array.Reverse(mShape);


			// Draw each of our shapes
			for (int i = 0; i < mShape.Length; i++)
			{


				g.FillPolygon(Brushes.DarkGray, mShape[i].screenPoints.PtList.ToArray());
				//g.FillPolygon(Brushes.DarkGray, screenPoints[i].PtList.ToArray());


				// Go thru each point
				for (int j = 0; j < mShape[i].GetSize(); j++)
				{
					var obj = mShape[i].Absolute(j);//[j];

					//pt.X = (Int32)(s * (obj.X - mCamera.X) * mScreen.Z / (obj.Z - mCamera.Z));
					//pt.Y = (Int32)(s * (obj.Y - mCamera.Y) * mScreen.Z / (obj.Z - mCamera.Z));

					//  pt = screenPoints[i].PtList[j];
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
				g.FillEllipse(Brushes.Red, new RectangleF(currentPoint.X - 3, currentPoint.Y - 3, 3, 3));
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

