using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace ThreeDApp2
{
    /// <summary>
    /// Main window: owns the scene, the view (camera, orbit, zoom), painting, keyboard shortcuts,
    /// saving and the toolbar. The drawing tools live in Form1.Tools.cs.
    /// </summary>
    public partial class Form1 : Form
    {
        const float LayerSpacing = 10;   // Gap between layers added with N
        const float GridExtent = 30;     // Grid is drawn from -GridExtent to +GridExtent
        const float AxisLength = 500;

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

        static readonly Color[] AxisColors = { Color.FromArgb(220, 40, 40), Color.FromArgb(30, 160, 60), Color.FromArgb(40, 80, 220) };

        Scene mScene;
        readonly Inference mInference = new Inference();
        readonly UndoStack mUndo = new UndoStack();
        bool mWireframe;
        bool mShowHud = true;
        bool mTranslucent;
        bool mShowAxes = true;
        bool mShowGrid = true;
        Timer mSpinTimer;
        int mNextColor = 3;
        string mFilePath;          // File chosen with Save As / Open, if any
        readonly string mAutosavePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ThreeDApp2", "autosave.t3d");

        // View dragging
        bool mOrbiting;
        bool mPanning;
        Point mLastMouse;

        bool mUpdatingUi;

        public Form1()
        {
            InitializeComponent();
            KeyPreview = true;

            viewport.Paint += Viewport_Paint;
            viewport.MouseDown += Viewport_MouseDown;
            viewport.MouseMove += Viewport_MouseMove;
            viewport.MouseUp += Viewport_MouseUp;
            viewport.MouseDoubleClick += Viewport_MouseDoubleClick;
            viewport.MouseWheel += Viewport_MouseWheel;
            viewport.MouseLeave += (s, e) => { mCursorInside = false; viewport.Invalidate(); };
            this.KeyDown += Form1_KeyDown;
            this.FormClosing += (s, e) => Autosave();

            mSpinTimer = new Timer(components);
            mSpinTimer.Interval = 30;
            mSpinTimer.Tick += (s, e) => { mScene.Rotate(Axis.Y, 1.5); viewport.Invalidate(); };

            mUpdatingUi = true;
            planeCombo.Items.AddRange(new object[] { "Front (XY)", "Ground (XZ)", "Side (YZ)" });
            planeCombo.SelectedIndex = (int)mWorkPlane;
            offsetBox.Text = mWorkPlaneOffset.ToString(CultureInfo.CurrentCulture);
            mUpdatingUi = false;

            if (!LoadAutosave())
                ResetScene();
            mUndo.Clear();
            SetTool(Tool.Select);
            RefreshShapesCombo();
            UpdateToggleButtons();
        }

        #region Scene setup and view

        /// <summary>Builds the original three house-shaped layers at Z = 20, 30 and 50.</summary>
        private void ResetScene()
        {
            mScene = new Scene();
            AddHouseLayer(20);
            AddHouseLayer(30);
            AddHouseLayer(50);
            mSelected = -1;
            ResetView();
        }

        /// <summary>Looks straight at the middle of everything in the scene, unrotated.</summary>
        private void ResetView()
        {
            mScene.Pivot = mScene.Layers.Count > 0 ? mScene.Centroid() : new Point3D(0, 0, 30);
            mScene.Camera = new Point3D(mScene.Pivot.X, mScene.Pivot.Y, Math.Min(0, mScene.Pivot.Z - 33.3f));
            mScene.ResetOrientation();
            mScene.ScreenZ = 30;
            mScene.Scale = 10;
            viewport.Invalidate();
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
            shape.Name = "Layer " + (mScene.Layers.Count + 1);
            mScene.Layers.Add(shape);
        }

        private void AddLayerBehind()
        {
            float maxZ = 20 - LayerSpacing;
            foreach (MyPolygon layer in mScene.Layers)
                for (int j = 0; j < layer.GetSize(); j++)
                    maxZ = Math.Max(maxZ, layer.Absolute(j).Z);
            PushUndo();
            AddHouseLayer(maxZ + LayerSpacing);
            Committed();
        }

        private void SetView(Matrix3D orientation)
        {
            mSpinTimer.Enabled = false;
            mScene.Orientation = orientation;
            viewport.Invalidate();
        }

        private static Matrix3D FrontView() { return Matrix3D.Identity(); }
        private static Matrix3D TopView() { return Matrix3D.Rotation(Axis.X, 90); }
        private static Matrix3D LeftView() { return Matrix3D.Rotation(Axis.Y, -90); }
        private static Matrix3D RightView() { return Matrix3D.Rotation(Axis.Y, 90); }
        private static Matrix3D IsoView() { return Matrix3D.Rotation(Axis.X, 30) * Matrix3D.Rotation(Axis.Y, -45); }

        private void Orbit(int dx, int dy)
        {
            if (dx != 0) mScene.Rotate(Axis.Y, -dx * 0.5);
            if (dy != 0) mScene.Rotate(Axis.X, -dy * 0.5);
        }

        /// <summary>Slides the camera sideways so the scene follows the mouse.</summary>
        private void Pan(int dx, int dy)
        {
            float depth = Math.Max(1f, mScene.ToWorld(mScene.Pivot).Z - mScene.Camera.Z);
            float worldPerPixel = depth / (mScene.Scale * mScene.ScreenZ);
            mScene.Camera.X -= dx * worldPerPixel;
            mScene.Camera.Y += dy * worldPerPixel;
        }

        private Color EffectiveColor(int index)
        {
            MyPolygon shape = mScene.Layers[index];
            return shape.Color.IsEmpty ? LayerColors[index % LayerColors.Length] : shape.Color;
        }

        private Color NextColor()
        {
            Color c = LayerColors[mNextColor % LayerColors.Length];
            mNextColor++;
            return c;
        }

        /// <summary>"Rectangle 3" style name, numbered after the highest existing one.</summary>
        private string NextName(string prefix)
        {
            int max = 0;
            foreach (MyPolygon p in mScene.Layers)
            {
                int n;
                if (p.Name != null && p.Name.StartsWith(prefix + " ") &&
                    int.TryParse(p.Name.Substring(prefix.Length + 1), out n))
                    max = Math.Max(max, n);
            }
            return prefix + " " + (max + 1);
        }

        #endregion

        #region Undo, autosave and files

        private void PushUndo()
        {
            mUndo.Push(mScene.Layers);
        }

        /// <summary>Call after every change to the shapes: refreshes the UI and autosaves.</summary>
        private void Committed()
        {
            RefreshShapesCombo();
            Autosave();
            viewport.Invalidate();
        }

        private void Undo()
        {
            CancelOperation();
            if (!mUndo.CanUndo) return;
            mScene.Layers = mUndo.Undo(mScene.Layers);
            mSelected = -1;
            Committed();
        }

        private void Redo()
        {
            CancelOperation();
            if (!mUndo.CanRedo) return;
            mScene.Layers = mUndo.Redo(mScene.Layers);
            mSelected = -1;
            Committed();
        }

        private void Autosave()
        {
            try
            {
                SceneFile.Save(mScene, mAutosavePath);
                if (!string.IsNullOrEmpty(mFilePath))
                    SceneFile.Save(mScene, mFilePath);
                saveStatus.Text = "Autosaved " + DateTime.Now.ToString("t");
            }
            catch (Exception ex)
            {
                saveStatus.Text = "Autosave failed: " + ex.Message;
            }
        }

        private bool LoadAutosave()
        {
            try
            {
                if (!File.Exists(mAutosavePath)) return false;
                Scene loaded = SceneFile.Load(mAutosavePath);
                mScene = loaded;
                saveStatus.Text = "Restored your last session";
                return true;
            }
            catch (Exception ex)
            {
                saveStatus.Text = "Could not restore last session: " + ex.Message;
                return false;
            }
        }

        private void NewScene()
        {
            CancelOperation();
            PushUndo();
            ResetScene();
            mFilePath = null;
            Text = "ThreeDApp2 - 3D Layer Viewer";
            Committed();
        }

        private void SaveAs()
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "ThreeDApp2 scene (*.t3d)|*.t3d|All files (*.*)|*.*";
                dlg.FileName = string.IsNullOrEmpty(mFilePath) ? "scene.t3d" : Path.GetFileName(mFilePath);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                mFilePath = dlg.FileName;
                Text = "ThreeDApp2 - " + Path.GetFileName(mFilePath);
                Autosave();
            }
        }

        private void OpenFile()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "ThreeDApp2 scene (*.t3d)|*.t3d|All files (*.*)|*.*";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    Scene loaded = SceneFile.Load(dlg.FileName);
                    CancelOperation();
                    mScene = loaded;
                    mUndo.Clear();
                    mSelected = -1;
                    mFilePath = dlg.FileName;
                    Text = "ThreeDApp2 - " + Path.GetFileName(mFilePath);
                    Committed();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not open the file:\n" + ex.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion

        #region Toolbar

        private void selectBtn_Click(object sender, EventArgs e) { SetTool(Tool.Select); }
        private void lineBtn_Click(object sender, EventArgs e) { SetTool(Tool.Line); }
        private void rectBtn_Click(object sender, EventArgs e) { SetTool(Tool.Rectangle); }
        private void circleBtn_Click(object sender, EventArgs e) { SetTool(Tool.Circle); }
        private void pushPullBtn_Click(object sender, EventArgs e) { SetTool(Tool.PushPull); }
        private void moveBtn_Click(object sender, EventArgs e) { SetTool(Tool.Move); }
        private void undoBtn_Click(object sender, EventArgs e) { Undo(); }
        private void redoBtn_Click(object sender, EventArgs e) { Redo(); }
        private void frontViewBtn_Click(object sender, EventArgs e) { SetView(FrontView()); }
        private void topViewBtn_Click(object sender, EventArgs e) { SetView(TopView()); }
        private void leftViewBtn_Click(object sender, EventArgs e) { SetView(LeftView()); }
        private void rightViewBtn_Click(object sender, EventArgs e) { SetView(RightView()); }
        private void isoViewBtn_Click(object sender, EventArgs e) { SetView(IsoView()); }
        private void deleteBtn_Click(object sender, EventArgs e) { DeleteSelected(); }

        private void gridBtn_Click(object sender, EventArgs e)
        {
            mShowGrid = !mShowGrid;
            UpdateToggleButtons();
        }

        private void axesBtn_Click(object sender, EventArgs e)
        {
            mShowAxes = !mShowAxes;
            UpdateToggleButtons();
        }

        private void colorBtn_Click(object sender, EventArgs e)
        {
            if (mSelected < 0) return;
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.Color = EffectiveColor(mSelected);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                PushUndo();
                foreach (int i in mScene.GroupMembers(mSelected))
                    mScene.Layers[i].Color = dlg.Color;
                Committed();
            }
        }

        private void planeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mUpdatingUi) return;
            mWorkPlane = (WorkPlaneKind)planeCombo.SelectedIndex;
            viewport.Focus();
            viewport.Invalidate();
        }

        private void offsetBox_TextChanged(object sender, EventArgs e)
        {
            if (mUpdatingUi) return;
            float v;
            if (float.TryParse(offsetBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out v) ||
                float.TryParse(offsetBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
            {
                mWorkPlaneOffset = v;
                viewport.Invalidate();
            }
        }

        private void offsetBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                viewport.Focus();
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void shapesCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mUpdatingUi) return;
            mSelected = shapesCombo.SelectedIndex;
            viewport.Focus();
            viewport.Invalidate();
        }

        private void RefreshShapesCombo()
        {
            mUpdatingUi = true;
            shapesCombo.BeginUpdate();
            shapesCombo.Items.Clear();
            for (int i = 0; i < mScene.Layers.Count; i++)
            {
                string name = mScene.Layers[i].Name;
                shapesCombo.Items.Add(string.IsNullOrEmpty(name) ? "Shape " + (i + 1) : name);
            }
            if (mSelected >= mScene.Layers.Count) mSelected = -1;
            shapesCombo.SelectedIndex = mSelected;
            shapesCombo.EndUpdate();
            mUpdatingUi = false;
            undoBtn.Enabled = mUndo.CanUndo;
            redoBtn.Enabled = mUndo.CanRedo;
            deleteBtn.Enabled = colorBtn.Enabled = mSelected >= 0;
        }

        private void SyncSelectionUi()
        {
            mUpdatingUi = true;
            shapesCombo.SelectedIndex = mSelected < shapesCombo.Items.Count ? mSelected : -1;
            mUpdatingUi = false;
            deleteBtn.Enabled = colorBtn.Enabled = mSelected >= 0;
        }

        private void UpdateToggleButtons()
        {
            gridBtn.Checked = mShowGrid;
            axesBtn.Checked = mShowAxes;
            mInference.GridEnabled = mShowGrid;
            viewport.Invalidate();
        }

        private bool EditingToolbarText()
        {
            return offsetBox.Focused || planeCombo.Focused || shapesCombo.Focused;
        }

        #endregion

        #region Keyboard

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (EditingToolbarText())
            {
                if (e.KeyCode == Keys.Escape) viewport.Focus();
                return;
            }

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Z: Undo(); break;
                    case Keys.Y: Redo(); break;
                    case Keys.S: SaveAs(); break;
                    case Keys.O: OpenFile(); break;
                    case Keys.N: NewScene(); break;
                    case Keys.D1: SetView(FrontView()); break;
                    case Keys.D2: SetView(TopView()); break;
                    case Keys.D3: SetView(LeftView()); break;
                    case Keys.D4: SetView(RightView()); break;
                    case Keys.D5: SetView(IsoView()); break;
                    default: return;
                }
                e.Handled = e.SuppressKeyPress = true;
                return;
            }

            // While a tool is mid-operation, number keys go to the measurement box (like SketchUp).
            char typed;
            if (OperationActive && TryMeasurementChar(e, out typed))
            {
                mTyped += typed;
                UpdateStatus();
                e.Handled = e.SuppressKeyPress = true;
                return;
            }

            float delta = e.Shift ? 0.25f : 1f;
            double angle = e.Shift ? -10 : 10;

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (mTyped.Length > 0) ApplyTypedValue();
                    else FinishOperation();
                    break;
                case Keys.Escape:
                    if (mTyped.Length > 0) mTyped = "";
                    else if (OperationActive) CancelOperation();
                    else { SetTool(Tool.Select); mSelected = -1; SyncSelectionUi(); }
                    break;
                case Keys.Back:
                    if (mTyped.Length > 0) mTyped = mTyped.Substring(0, mTyped.Length - 1);
                    else RemoveLastPoint();
                    break;
                case Keys.Delete: DeleteSelected(); break;

                // Tools
                case Keys.S: SetTool(Tool.Select); break;
                case Keys.L: SetTool(Tool.Line); break;
                case Keys.R: SetTool(Tool.Rectangle); break;
                case Keys.C: SetTool(Tool.Circle); break;
                case Keys.P: SetTool(Tool.PushPull); break;
                case Keys.M: SetTool(Tool.Move); break;

                // Camera
                case Keys.Right: mScene.Camera.X += delta; break;
                case Keys.Left: mScene.Camera.X -= delta; break;
                case Keys.Up: mScene.Camera.Y += delta; break;
                case Keys.Down: mScene.Camera.Y -= delta; break;
                case Keys.Add:
                case Keys.Oemplus:
                    mScene.Camera.Z += delta; break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    mScene.Camera.Z -= delta; break;
                case Keys.PageUp: mScene.Scale = Math.Min(100f, mScene.Scale * 1.1f); break;
                case Keys.PageDown: mScene.Scale = Math.Max(1f, mScene.Scale / 1.1f); break;
                case Keys.Home: mSpinTimer.Enabled = false; ResetView(); break;

                case Keys.X: mScene.Rotate(Axis.X, angle); break;
                case Keys.Y: mScene.Rotate(Axis.Y, angle); break;
                case Keys.Z: mScene.Rotate(Axis.Z, angle); break;

                // Display
                case Keys.Space: mSpinTimer.Enabled = !mSpinTimer.Enabled && !OperationActive; break;
                case Keys.W: mWireframe = !mWireframe; break;
                case Keys.T: mTranslucent = !mTranslucent; break;
                case Keys.G: mShowGrid = !mShowGrid; UpdateToggleButtons(); break;
                case Keys.A: mShowAxes = !mShowAxes; UpdateToggleButtons(); break;
                case Keys.H:
                case Keys.F1:
                    mShowHud = !mShowHud; break;
                case Keys.N: AddLayerBehind(); break;
                default:
                    return;
            }
            e.Handled = e.SuppressKeyPress = true;
            UpdateHover();
            UpdateStatus();
            viewport.Invalidate();
        }

        private static bool TryMeasurementChar(KeyEventArgs e, out char c)
        {
            c = '\0';
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9 && !e.Shift) c = (char)('0' + (e.KeyCode - Keys.D0));
            else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) c = (char)('0' + (e.KeyCode - Keys.NumPad0));
            else if (e.KeyCode == Keys.OemPeriod || e.KeyCode == Keys.Decimal) c = '.';
            else if (e.KeyCode == Keys.Oemcomma) c = ',';
            else if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract) c = '-';
            return c != '\0';
        }

        #endregion

        #region Mouse (view part; tool handling is in Form1.Tools.cs)

        private PointF ToScene(Point p)
        {
            return new PointF(p.X - viewport.ClientSize.Width / 2f, p.Y - viewport.ClientSize.Height / 2f);
        }

        private void Viewport_MouseDown(object sender, MouseEventArgs e)
        {
            mLastMouse = e.Location;
            mCursor = ToScene(e.Location);
            mCursorInside = true;
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                if (ModifierKeys.HasFlag(Keys.Shift)) mPanning = true;
                else mOrbiting = true;
                return;
            }
            if (e.Button == MouseButtons.Left)
                ToolMouseDown(e);
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - mLastMouse.X, dy = e.Y - mLastMouse.Y;
            mLastMouse = e.Location;
            mCursor = ToScene(e.Location);
            mCursorInside = true;

            if (mPanning) { Pan(dx, dy); }
            else if (mOrbiting) { Orbit(dx, dy); }
            else ToolMouseMove(e, dx, dy);

            UpdateHover();
            UpdateStatus();
            viewport.Invalidate();
        }

        private void Viewport_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                mOrbiting = mPanning = false;
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                if (mOrbiting) mOrbiting = false;
                ToolMouseUp(e);
            }
            viewport.Invalidate();
        }

        private void Viewport_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) ToolDoubleClick(e);
            viewport.Invalidate();
        }

        private void Viewport_MouseWheel(object sender, MouseEventArgs e)
        {
            mScene.Camera.Z += Math.Sign(e.Delta) * 2f;
            UpdateHover();
            viewport.Invalidate();
        }

        #endregion

        #region Painting

        private void Viewport_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(viewport.BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsState state = g.Save();
            g.TranslateTransform(viewport.ClientSize.Width / 2f, viewport.ClientSize.Height / 2f);

            if (mShowGrid) DrawGrid(g);
            if (mShowAxes) DrawAxes(g);

            List<ShapeScreenPoints> frame = mScene.BuildFrame();
            List<int> selectedGroup = mSelected >= 0 ? mScene.GroupMembers(mSelected) : new List<int>();

            // Painter's algorithm: frame is ordered farthest first.
            foreach (ShapeScreenPoints shape in frame)
            {
                PointF[] pts = shape.PtList.ToArray();
                Color baseColor = EffectiveColor(shape.LayerIndex);
                bool selected = selectedGroup.Contains(shape.LayerIndex);
                bool hovered = shape.LayerIndex == mHoverIndex && !selected;

                if (shape.Closed)
                {
                    if (!mWireframe && pts.Length >= 3)
                    {
                        int alpha = mTranslucent ? 150 : 255;
                        using (SolidBrush brush = new SolidBrush(Darken(baseColor, shape.Shade, alpha)))
                            g.FillPolygon(brush, pts);
                    }
                    using (Pen pen = new Pen(mWireframe ? baseColor : Darken(baseColor, 0.45f, 255), 1.5f))
                        g.DrawPolygon(pen, pts);
                }
                else
                {
                    using (Pen pen = new Pen(Darken(baseColor, 0.8f, 255), 2.5f))
                        g.DrawLines(pen, pts);
                }

                if (hovered)
                    using (Pen pen = new Pen(Color.FromArgb(160, 30, 144, 255), 3f))
                        DrawOutline(g, pen, pts, shape.Closed);
                if (selected)
                    using (Pen pen = new Pen(Color.FromArgb(255, 140, 0), 3f))
                        DrawOutline(g, pen, pts, shape.Closed);
            }

            // Vertex handles on the selected shape, for dragging.
            if (mSelected >= 0 && mSelected < mScene.Layers.Count && mTool == Tool.Select)
            {
                MyPolygon sel = mScene.Layers[mSelected];
                int count = Scene.VertexCount(sel);
                for (int j = 0; j < count; j++)
                {
                    PointF p;
                    if (!mScene.TryProjectModel(sel.Absolute(j), out p)) continue;
                    g.FillRectangle(Brushes.White, p.X - 3.5f, p.Y - 3.5f, 7, 7);
                    g.DrawRectangle(Pens.DarkOrange, p.X - 3.5f, p.Y - 3.5f, 7, 7);
                }
            }

            DrawToolPreview(g);

            g.Restore(state);

            if (mShowHud) DrawHud(g, frame.Count);
        }

        private static void DrawOutline(Graphics g, Pen pen, PointF[] pts, bool closed)
        {
            if (pts.Length < 2) return;
            if (closed) g.DrawPolygon(pen, pts);
            else g.DrawLines(pen, pts);
        }

        private void DrawSegment(Graphics g, Pen pen, Point3D a, Point3D b)
        {
            PointF pa, pb;
            if (mScene.TryProjectSegmentModel(a, b, out pa, out pb))
                g.DrawLine(pen, pa, pb);
        }

        private void DrawGrid(Graphics g)
        {
            Plane3D plane = WorkPlane();
            float step = mInference.GridSpacing;
            // Centre the grid on the scene (rounded to a major line) so it sits under the shapes.
            float major = step * 5;
            Point3D c = plane.ProjectPoint(mScene.Pivot);
            Point3D center = new Point3D(Math.Round(c.X / major) * major, Math.Round(c.Y / major) * major, Math.Round(c.Z / major) * major);
            center = plane.ProjectPoint(center);
            using (Pen minor = new Pen(Color.FromArgb(28, 0, 0, 0), 1f))
            using (Pen majorPen = new Pen(Color.FromArgb(55, 0, 0, 0), 1f))
            {
                int n = (int)(GridExtent / step);
                for (int i = -n; i <= n; i++)
                {
                    float t = i * step;
                    Pen pen = (i % 5 == 0) ? majorPen : minor;
                    DrawSegment(g, pen, center + plane.U * t + plane.V * -GridExtent, center + plane.U * t + plane.V * GridExtent);
                    DrawSegment(g, pen, center + plane.V * t + plane.U * -GridExtent, center + plane.V * t + plane.U * GridExtent);
                }
            }
        }

        private void DrawAxes(Graphics g)
        {
            Point3D origin = new Point3D(0, 0, 0);
            Point3D[] dirs = { new Point3D(1, 0, 0), new Point3D(0, 1, 0), new Point3D(0, 0, 1) };
            for (int k = 0; k < 3; k++)
            {
                using (Pen solid = new Pen(Color.FromArgb(170, AxisColors[k]), 1.5f))
                using (Pen dashed = new Pen(Color.FromArgb(120, AxisColors[k]), 1f))
                {
                    dashed.DashStyle = DashStyle.Dash;
                    DrawSegment(g, solid, origin, dirs[k] * AxisLength);
                    DrawSegment(g, dashed, origin, dirs[k] * -AxisLength);
                }
            }
        }

        private void DrawPolyline3D(Graphics g, Pen pen, IList<Point3D> pts, bool closed)
        {
            for (int i = 0; i + 1 < pts.Count; i++) DrawSegment(g, pen, pts[i], pts[i + 1]);
            if (closed && pts.Count > 2) DrawSegment(g, pen, pts[pts.Count - 1], pts[0]);
        }

        private void FillPolygon3D(Graphics g, Brush brush, IList<Point3D> pts)
        {
            List<PointF> screen = new List<PointF>();
            foreach (Point3D p in pts)
            {
                PointF s;
                if (!mScene.TryProjectModel(p, out s)) return;
                screen.Add(s);
            }
            if (screen.Count >= 3) g.FillPolygon(brush, screen.ToArray());
        }

        private void DrawHud(Graphics g, int visibleLayers)
        {
            Point3D c = mScene.Camera;
            string[] lines =
            {
                string.Format("Shapes: {0} ({1} visible)   Camera: ({2:0.##}, {3:0.##}, {4:0.##})   Zoom: {5:0.#}   Tool: {6}",
                    mScene.Layers.Count, visibleLayers, c.X, c.Y, c.Z, mScene.Scale, mTool),
                string.Format("Mode: {0}{1}{2}",
                    mWireframe ? "wireframe" : "filled",
                    mTranslucent && !mWireframe ? ", translucent" : "",
                    mSpinTimer.Enabled ? ", spinning" : ""),
                "",
                "Tools: S select  L line  R rectangle  C circle  P push/pull  M move   Esc: cancel",
                "While drawing: type a length + Enter   Enter/double-click: finish line   Backspace: remove point",
                "Right-drag: orbit   Shift+right-drag: pan   Wheel or +/-: forward/back   PgUp/PgDn: zoom",
                "X/Y/Z: rotate (Shift reverses)   Ctrl+1..5: views   Home: reset view   Space: spin",
                "W wireframe  T translucent  G grid  A axes  N add layer  Del delete  Ctrl+Z/Y  Ctrl+S/O/N  H hide",
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

        #endregion
    }
}
