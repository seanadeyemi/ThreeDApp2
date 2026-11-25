using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ThreeDApp2
{
    public partial class Form1 : Form
    {
        const float LayerSpacing = 10;   // Gap between layers added with N
        const int MinLayers = 1;
        const int MaxLayers = 12;

        // One colour per layer, cycled if there are more layers than colours.
        static readonly Color[] LayerColors =
        {
            Color.FromArgb(70, 130, 180),  // steel blue
            Color.FromArgb(205, 92, 92),   // indian red
            Color.FromArgb(60, 179, 113),  // sea green
            Color.FromArgb(218, 165, 32),  // goldenrod
            Color.FromArgb(147, 112, 219), // medium purple
            Color.FromArgb(72, 209, 204),  // turquoise
        };

        Scene mScene;
        bool mWireframe;
        bool mShowHud = true;
        bool mTranslucent;
        Timer mSpinTimer;
        bool mDragging;
        Point mLastMouse;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ResizeRedraw = true;   // Re-centre the drawing when the window is resized
            KeyPreview = true;

            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            this.MouseWheel += Form1_MouseWheel;

            mSpinTimer = new Timer(components);
            mSpinTimer.Interval = 30;
            mSpinTimer.Tick += (s, e) => { mScene.Rotate(Axis.Y, 1.5); Invalidate(); };

            ResetScene();
        }

        /// <summary>Builds the original three house-shaped layers at Z = 20, 30 and 50.</summary>
        private void ResetScene()
        {
            mScene = new Scene();
            AddHouseLayer(20);
            AddHouseLayer(30);
            AddHouseLayer(50);

            // Rotate around the middle of the stack and look straight at it.
            mScene.Pivot = mScene.Centroid();
            mScene.Camera = new Point3D(mScene.Pivot.X, mScene.Pivot.Y, 0);
            mScene.ScreenZ = 30;
            mScene.Scale = 10;
            Invalidate();
        }

        private void AddHouseLayer(float z)
        {
            MyPolygon shape = new MyPolygon();
            shape.Add(new Point3D(0, 10, z));
            shape.Add(new Point3D(0, 0, z));
            shape.Add(new Point3D(10, 0, z));
            shape.Add(new Point3D(10, 10, z));
            shape.Add(new Point3D(5, 15, z));
            shape.Close();
            mScene.Layers.Add(shape);
        }

        private void AddLayerBehind()
        {
            if (mScene.Layers.Count >= MaxLayers) return;
            float maxZ = float.MinValue;
            foreach (MyPolygon layer in mScene.Layers)
                for (int j = 0; j < layer.GetSize(); j++)
                    maxZ = Math.Max(maxZ, layer.Absolute(j).Z);
            AddHouseLayer(maxZ + LayerSpacing);
        }

        private void RemoveLastLayer()
        {
            if (mScene.Layers.Count > MinLayers)
                mScene.Layers.RemoveAt(mScene.Layers.Count - 1);
        }

        private void RotateShapes(Axis axis, double degrees)
        {
            mScene.Rotate(axis, degrees);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Shift gives finer steps for both moving and rotating.
            float delta = e.Shift ? 0.25f : 1f;
            double angle = (e.Shift ? 2 : 10) * (e.Control ? -1 : 1);

            switch (e.KeyCode)
            {
                case Keys.Right: mScene.Camera.X += delta; break;
                case Keys.Left: mScene.Camera.X -= delta; break;
                case Keys.Up: mScene.Camera.Y += delta; break;
                case Keys.Down: mScene.Camera.Y -= delta; break;

                // Move the camera forwards / backwards (numpad or main keyboard)
                case Keys.Add:
                case Keys.Oemplus:
                    mScene.Camera.Z += delta; break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    mScene.Camera.Z -= delta; break;

                // Zoom the projection without moving the camera
                case Keys.PageUp: mScene.Scale = Math.Min(100f, mScene.Scale * 1.1f); break;
                case Keys.PageDown: mScene.Scale = Math.Max(1f, mScene.Scale / 1.1f); break;

                case Keys.X: RotateShapes(Axis.X, angle); break;
                case Keys.Y: RotateShapes(Axis.Y, angle); break;
                case Keys.Z: RotateShapes(Axis.Z, angle); break;

                case Keys.Space: mSpinTimer.Enabled = !mSpinTimer.Enabled; break;
                case Keys.W: mWireframe = !mWireframe; break;
                case Keys.T: mTranslucent = !mTranslucent; break;
                case Keys.H:
                case Keys.F1:
                    mShowHud = !mShowHud; break;
                case Keys.N: AddLayerBehind(); break;
                case Keys.Delete:
                case Keys.Back:
                    RemoveLastLayer(); break;
                case Keys.R:
                case Keys.Home:
                    mSpinTimer.Enabled = false;
                    ResetScene();
                    break;
                default:
                    return;
            }
            e.Handled = true;
            Invalidate();
        }

        // Arrow keys are normally swallowed for focus navigation; make sure they reach KeyDown.
        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    return false;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mDragging = true;
                mLastMouse = e.Location;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mDragging) return;
            int dx = e.X - mLastMouse.X;
            int dy = e.Y - mLastMouse.Y;
            mLastMouse = e.Location;
            if (dx != 0) mScene.Rotate(Axis.Y, -dx * 0.5);
            if (dy != 0) mScene.Rotate(Axis.X, -dy * 0.5);
            Invalidate();
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) mDragging = false;
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            mScene.Camera.Z += Math.Sign(e.Delta) * 2f;
            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(245, 245, 248));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsState state = g.Save();
            g.TranslateTransform(ClientSize.Width / 2f, ClientSize.Height / 2f);

            List<ShapeScreenPoints> frame = mScene.BuildFrame();

            // Painter's algorithm: frame is ordered farthest first.
            foreach (ShapeScreenPoints shape in frame)
            {
                PointF[] pts = shape.PtList.ToArray();
                Color baseColor = LayerColors[shape.LayerIndex % LayerColors.Length];

                if (!mWireframe && pts.Length >= 3)
                {
                    int alpha = mTranslucent ? 150 : 255;
                    using (SolidBrush brush = new SolidBrush(Darken(baseColor, shape.Shade, alpha)))
                        g.FillPolygon(brush, pts);
                }
                using (Pen pen = new Pen(mWireframe ? baseColor : Darken(baseColor, 0.45f, 255), 1.5f))
                    g.DrawPolygon(pen, pts);
            }

            g.Restore(state);

            if (mShowHud) DrawHud(g, frame.Count);
        }

        private void DrawHud(Graphics g, int visibleLayers)
        {
            Point3D c = mScene.Camera;
            string[] lines =
            {
                string.Format("Layers: {0} ({1} visible)   Camera: ({2:0.##}, {3:0.##}, {4:0.##})   Zoom: {5:0.#}",
                    mScene.Layers.Count, visibleLayers, c.X, c.Y, c.Z, mScene.Scale),
                string.Format("Mode: {0}{1}{2}",
                    mWireframe ? "wireframe" : "filled",
                    mTranslucent && !mWireframe ? ", translucent" : "",
                    mSpinTimer.Enabled ? ", spinning" : ""),
                "",
                "Arrows: move camera   +/-  or wheel: forward/back   PgUp/PgDn: zoom",
                "X/Y/Z: rotate (Ctrl reverses, Shift = fine)   Drag mouse: rotate",
                "Space: spin   W: wireframe   T: translucent   N/Del: add/remove layer",
                "R: reset view   H: hide this help",
            };

            using (Font font = new Font("Consolas", 9f))
            using (SolidBrush back = new SolidBrush(Color.FromArgb(190, 255, 255, 255)))
            {
                float lineHeight = font.GetHeight(g);
                float width = 0;
                foreach (string line in lines)
                    width = Math.Max(width, g.MeasureString(line, font).Width);
                RectangleF box = new RectangleF(8, 8, width + 12, lineHeight * lines.Length + 10);
                g.FillRectangle(back, box);
                g.DrawRectangle(Pens.Silver, box.X, box.Y, box.Width, box.Height);
                for (int i = 0; i < lines.Length; i++)
                    g.DrawString(lines[i], font, Brushes.Black, box.X + 6, box.Y + 5 + i * lineHeight);
            }
        }

        private static Color Darken(Color c, float factor, int alpha)
        {
            factor = Math.Max(0f, Math.Min(1f, factor));
            return Color.FromArgb(alpha, (int)(c.R * factor), (int)(c.G * factor), (int)(c.B * factor));
        }
    }
}
