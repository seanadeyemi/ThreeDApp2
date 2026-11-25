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
        const double DefaultFov = 35;          // SketchUp's default field of view
        const double DefaultYaw = -35;         // Three-quarter view from the front-left...
        const double DefaultPitch = 12;        // ...and a little above
        const double OrbitDegreesPerPixel = 0.4;

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

        // SketchUp axis colours: X red, vertical (Y) blue, depth (Z) green.
        static readonly Color[] AxisColors = { Color.FromArgb(220, 40, 40), Color.FromArgb(40, 80, 220), Color.FromArgb(30, 160, 60) };

        static readonly Color SkyTop = Color.FromArgb(150, 192, 235);
        static readonly Color SkyHorizon = Color.FromArgb(222, 236, 250);
        static readonly Color GroundHorizon = Color.FromArgb(206, 201, 184);
        static readonly Color GroundNear = Color.FromArgb(176, 168, 145);

        Scene mScene;
        readonly Inference mInference = new Inference();
        readonly UndoStack mUndo = new UndoStack();
        bool mWireframe;
        bool mShowHud = true;
        bool mTranslucent;
        bool mShowAxes = true;
        bool mShowGrid = true;
        double mFov = DefaultFov;
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

        AppSettings mSettings;
        readonly string mSettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ThreeDApp2", "settings.ini");
        Cursor mOpenHandCursor = Cursors.Hand;
        Cursor mGrabHandCursor = Cursors.Hand;

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
            viewport.Resize += (s, e) => { ApplyFieldOfView(); viewport.Invalidate(); };

            mSpinTimer = new Timer(components);
            mSpinTimer.Interval = 30;
            mSpinTimer.Tick += (s, e) => { mScene.SetOrbit(mScene.Yaw - 1.5, mScene.Pitch); viewport.Invalidate(); };

            mViewAnimTimer = new Timer(components);
            mViewAnimTimer.Interval = 15;
            mViewAnimTimer.Tick += (s, e) => ViewAnimTick();

            mSettings = AppSettings.Load(mSettingsPath);
            mShowGizmo = mSettings.ShowGizmo;
            ApplyToolbarIcons();

            mUpdatingUi = true;
            centerOnLaunchItem.Checked = mSettings.CenterOnLaunch;
            groundSkyItem.Checked = mSettings.GroundAndSky;
            gizmoItem.Checked = mSettings.ShowGizmo;
            planeCombo.Items.AddRange(new object[] { "Front (XY)", "Ground (XZ)", "Side (YZ)" });
            planeCombo.SelectedIndex = (int)mWorkPlane;
            offsetBox.Text = mWorkPlaneOffset.ToString(CultureInfo.CurrentCulture);
            mUpdatingUi = false;

            bool restored = LoadAutosave();
            if (!restored)
                ResetScene();
            if (mSettings.CenterOnLaunch || !restored)
                ResetView();               // Centred start view (the default; can be turned off in Options)
            else
            {
                mScene.DeriveOrbit();      // Carry on from the view saved with the last session
                ApplyFieldOfView();
            }
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

        /// <summary>
        /// SketchUp-style start view: a perspective three-quarter view from a little above, with the
        /// vertical (blue, Y) axis upright in the exact centre, equal space left and right, and the
        /// whole model in frame.
        /// </summary>
        private void ResetView()
        {
            mFov = DefaultFov;
            mScene.FrameCentered(mFov, viewport.ClientSize.Width, viewport.ClientSize.Height, DefaultPitch, DefaultYaw);
            viewport.Invalidate();
        }

        /// <summary>Gives every toolbar button its icon, sized for the screen's DPI.</summary>
        private void ApplyToolbarIcons()
        {
            int size = Math.Max(16, (int)Math.Round(24 * DeviceDpi / 96.0));
            toolStrip1.ImageScalingSize = new Size(size, size);
            var icons = new Dictionary<ToolStripItem, string>
            {
                { selectBtn, "select" }, { lineBtn, "line" }, { rectBtn, "rectangle" }, { circleBtn, "circle" },
                { pushPullBtn, "pushpull" }, { moveBtn, "move" }, { panBtn, "pan" }, { orbitBtn, "orbit" }, { importBtn, "import" }, { exportBtn, "export" },
                { undoBtn, "undo" }, { redoBtn, "redo" },
                { frontViewBtn, "view-front" }, { topViewBtn, "view-top" }, { leftViewBtn, "view-left" },
                { rightViewBtn, "view-right" }, { isoViewBtn, "view-iso" },
                { gridBtn, "grid" }, { axesBtn, "axes" }, { colorBtn, "color" }, { deleteBtn, "delete" },
                { optionsBtn, "options" },
            };
            foreach (var pair in icons)
                pair.Key.Image = ToolIcons.Render(pair.Value, size);

            int cursorSize = Math.Max(32, size + 8);
            mOpenHandCursor = ToolIcons.MakeCursor("pan", cursorSize, Cursors.Hand);
            mGrabHandCursor = ToolIcons.MakeCursor("pan-grab", cursorSize, Cursors.Hand);
            mOrbitCursor = ToolIcons.MakeCursor("orbit", cursorSize, Cursors.SizeAll);
        }

        private void SaveSettings()
        {
            try { mSettings.Save(mSettingsPath); }
            catch (Exception ex) { saveStatus.Text = "Could not save options: " + ex.Message; }
        }

        private float Aspect()
        {
            return viewport.ClientSize.Width / (float)Math.Max(1, viewport.ClientSize.Height);
        }

        /// <summary>Recomputes the projection for the current field of view and window height.</summary>
        private void ApplyFieldOfView()
        {
            if (mScene != null) mScene.SetFieldOfView(mFov, viewport.ClientSize.Height);
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

        // Standard views as (turn, tilt) in degrees.
        private void SetView(double yaw, double pitch)
        {
            mSpinTimer.Enabled = false;
            mScene.SetOrbit(yaw, pitch);
            viewport.Invalidate();
        }

        private void FrontView() { SetView(0, 0); }
        private void TopView() { SetView(0, 90); }
        private void LeftView() { SetView(-90, 0); }
        private void RightView() { SetView(90, 0); }
        private void IsoView() { SetView(DefaultYaw, DefaultPitch); }

        /// <summary>
        /// Turntable orbit: horizontal drag turns the model around its vertical axis, vertical drag
        /// tilts it (clamped at straight above / below). There is no roll, so the view stays level.
        /// </summary>
        private void Orbit(int dx, int dy)
        {
            mScene.SetOrbit(mScene.Yaw - dx * OrbitDegreesPerPixel, mScene.Pitch + dy * OrbitDegreesPerPixel);
        }

        /// <summary>Slides the view so the scene follows the mouse; the orbit centre moves with it.</summary>
        private void Pan(int dx, int dy)
        {
            mScene.PanPixels(dx, dy);
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
                    mScene.DeriveOrbit();
                    ApplyFieldOfView();
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

        /// <summary>Adds a 3D model file to the scene as one group (so it selects and moves as a whole).</summary>
        private void ImportModel()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Import 3D model";
                dlg.Filter = ModelExchange.ImportFilter;
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                ImportResult result;
                Cursor old = Cursor;
                try
                {
                    Cursor = Cursors.WaitCursor;
                    result = ModelExchange.Import(dlg.FileName, mScene.NextGroupId());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not import the model:\n" + ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                finally { Cursor = old; }

                if (result.Shapes.Count > 0)
                {
                    CancelOperation();
                    PushUndo();
                    int first = mScene.Layers.Count;
                    mScene.Layers.AddRange(result.Shapes);
                    mSelected = first;
                    mScene.ZoomExtents(mFov, Aspect(), true);   // Imported models can be any size
                    Committed();
                    SyncSelectionUi();
                }
                string summary = string.Format("Imported {0}: {1} faces, {2} lines", Path.GetFileName(dlg.FileName), result.Faces, result.Lines);
                saveStatus.Text = summary;
                if (result.Warnings.Count > 0)
                    MessageBox.Show(this, summary + "\n\n" + string.Join("\n", result.Warnings), "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else if (result.Faces > 20000)
                    MessageBox.Show(this, summary + "\n\nThis is a large model, so the view may be slow.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>Writes every visible shape to a 3D file; the format comes from the extension chosen.</summary>
        private void ExportModel()
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Export 3D model";
                dlg.Filter = ModelExchange.ExportFilter;
                dlg.FileName = (string.IsNullOrEmpty(mFilePath) ? "model" : Path.GetFileNameWithoutExtension(mFilePath)) + ".obj";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    ModelExchange.Export(dlg.FileName, mScene.Layers, EffectiveColor);
                    saveStatus.Text = "Exported " + Path.GetFileName(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not export the model:\n" + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        private void frontViewBtn_Click(object sender, EventArgs e) { FrontView(); }
        private void topViewBtn_Click(object sender, EventArgs e) { TopView(); }
        private void leftViewBtn_Click(object sender, EventArgs e) { LeftView(); }
        private void rightViewBtn_Click(object sender, EventArgs e) { RightView(); }
        private void isoViewBtn_Click(object sender, EventArgs e) { IsoView(); }
        private void deleteBtn_Click(object sender, EventArgs e) { DeleteSelected(); }
        private void panBtn_Click(object sender, EventArgs e) { SetTool(Tool.Pan); }
        private void orbitBtn_Click(object sender, EventArgs e) { SetTool(Tool.Orbit); }
        private void importBtn_Click(object sender, EventArgs e) { ImportModel(); }
        private void exportBtn_Click(object sender, EventArgs e) { ExportModel(); }

        private void gizmoItem_Click(object sender, EventArgs e)
        {
            if (mUpdatingUi) return;
            mShowGizmo = mSettings.ShowGizmo = gizmoItem.Checked;
            SaveSettings();
            viewport.Invalidate();
        }

        private void centerOnLaunchItem_Click(object sender, EventArgs e)
        {
            mSettings.CenterOnLaunch = centerOnLaunchItem.Checked;
            SaveSettings();
        }

        private void groundSkyItem_Click(object sender, EventArgs e)
        {
            mSettings.GroundAndSky = groundSkyItem.Checked;
            SaveSettings();
            viewport.Invalidate();
        }

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
                    case Keys.I: ImportModel(); break;
                    case Keys.E: ExportModel(); break;
                    case Keys.N: NewScene(); break;
                    case Keys.D1: FrontView(); break;
                    case Keys.D2: TopView(); break;
                    case Keys.D3: LeftView(); break;
                    case Keys.D4: RightView(); break;
                    case Keys.D5: IsoView(); break;
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

            float panStep = e.Shift ? 5f : 25f;
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

                // Camera: arrows pan, +/- move in and out, PgUp/PgDn change the field of view
                case Keys.Right: mScene.PanPixels(-panStep, 0); break;
                case Keys.Left: mScene.PanPixels(panStep, 0); break;
                case Keys.Up: mScene.PanPixels(0, panStep); break;
                case Keys.Down: mScene.PanPixels(0, -panStep); break;
                case Keys.Add:
                case Keys.Oemplus:
                    mScene.Dolly(e.Shift ? 0.97f : 0.85f, PointF.Empty); break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    mScene.Dolly(e.Shift ? 1 / 0.97f : 1 / 0.85f, PointF.Empty); break;
                case Keys.PageUp: mFov = Math.Max(10, mFov / 1.1); ApplyFieldOfView(); break;
                case Keys.PageDown: mFov = Math.Min(120, mFov * 1.1); ApplyFieldOfView(); break;
                case Keys.Home: mSpinTimer.Enabled = false; ResetView(); break;

                case Keys.X: mScene.SetOrbit(mScene.Yaw, mScene.Pitch + angle); break;   // Tilt
                case Keys.Y: mScene.SetOrbit(mScene.Yaw + angle, mScene.Pitch); break;   // Turn
                case Keys.Z: mScene.ZoomExtents(mFov, Aspect()); break;                            // Zoom extents

                // Display
                case Keys.Space: mSpinTimer.Enabled = !mSpinTimer.Enabled && !OperationActive; break;
                case Keys.W: mWireframe = !mWireframe; break;
                case Keys.T: mTranslucent = !mTranslucent; break;
                case Keys.G: mShowGrid = !mShowGrid; UpdateToggleButtons(); break;
                case Keys.A: mShowAxes = !mShowAxes; UpdateToggleButtons(); break;
                case Keys.H: SetTool(Tool.Pan); break;
                case Keys.O: SetTool(Tool.Orbit); break;
                case Keys.F1: mShowHud = !mShowHud; break;
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
            if (GizmoMouseDown(e)) { viewport.Invalidate(); return; }
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                if (ModifierKeys.HasFlag(Keys.Shift)) { mPanning = true; viewport.Cursor = mGrabHandCursor; }
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
            else if ((mGizmoDragging || e.Button == MouseButtons.None) && GizmoMouseMove(e, dx, dy)) { }
            else ToolMouseMove(e, dx, dy);

            UpdateHover();
            UpdateStatus();
            viewport.Invalidate();
        }

        private void Viewport_MouseUp(object sender, MouseEventArgs e)
        {
            if (GizmoMouseUp(e)) { viewport.Invalidate(); return; }
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                if (mPanning) SetToolCursor();
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
            if (mShowGizmo && mGizmo.Contains(e.Location)) return;
            if (e.Button == MouseButtons.Left) ToolDoubleClick(e);
            viewport.Invalidate();
        }

        private void Viewport_MouseWheel(object sender, MouseEventArgs e)
        {
            // Zoom towards the point under the cursor, like SketchUp.
            mScene.Dolly(e.Delta > 0 ? 0.85f : 1 / 0.85f, ToScene(e.Location));
            UpdateHover();
            viewport.Invalidate();
        }

        #endregion

        #region Painting

        private void Viewport_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(viewport.BackColor);
            if (mSettings.GroundAndSky) DrawGroundAndSky(g);
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

            DrawGizmo(g);
            if (mShowHud) DrawHud(g, frame.Count);
        }

        /// <summary>
        /// Sky above the horizon and ground below it. The horizon is where the ground plane (XZ, Y = 0)
        /// vanishes at infinity, so drawn shapes standing on the ground meet it correctly.
        /// </summary>
        private void DrawGroundAndSky(Graphics g)
        {
            int w = viewport.ClientSize.Width, h = viewport.ClientSize.Height;
            if (w <= 0 || h <= 0) return;
            float horizon = h / 2f + mScene.HorizonY();
            bool aboveGround = mScene.CameraHeight() >= 0;
            float sky = Math.Max(0, Math.Min(h, horizon));

            // Sky: fills everything when the camera is below the ground (like SketchUp, the ground is not shown from beneath).
            float skyBottom = aboveGround ? sky : h;
            if (skyBottom > 0)
            {
                RectangleF r = new RectangleF(0, 0, w, skyBottom);
                using (LinearGradientBrush b = new LinearGradientBrush(new PointF(0, Math.Min(0, horizon - h)), new PointF(0, horizon + 1), SkyTop, SkyHorizon))
                    g.FillRectangle(b, r);
            }
            if (aboveGround && sky < h)
            {
                RectangleF r = new RectangleF(0, sky, w, h - sky);
                using (LinearGradientBrush b = new LinearGradientBrush(new PointF(0, horizon - 1), new PointF(0, Math.Max(h, horizon + h)), GroundHorizon, GroundNear))
                    g.FillRectangle(b, r);
                if (horizon >= 0 && horizon <= h)
                    using (Pen line = new Pen(Color.FromArgb(120, 120, 130, 140), 1f))
                        g.DrawLine(line, 0, horizon, w, horizon);
            }
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
            string[] lines =
            {
                string.Format("Shapes: {0} ({1} visible)   Turn {2:0}°  Tilt {3:0}°  Distance {4:0.#}  FOV {5:0}°   Tool: {6}",
                    mScene.Layers.Count, visibleLayers, mScene.Yaw, mScene.Pitch, mScene.Distance, mFov, mTool),
                string.Format("Mode: {0}{1}{2}",
                    mWireframe ? "wireframe" : "filled",
                    mTranslucent && !mWireframe ? ", translucent" : "",
                    mSpinTimer.Enabled ? ", spinning" : ""),
                "",
                "Tools: S select  L line  R rectangle  C circle  P push/pull  M move  H pan  O orbit   Esc: cancel",
                "While drawing: type a length + Enter   Enter/double-click: finish line   Backspace: remove point",
                "Middle/right-drag: orbit   Shift+drag: pan   Wheel: zoom to cursor   Corner gizmo: drag to orbit, click an axis to look along it",
                "X/Y: tilt/turn (Shift reverses)   Z: zoom extents   Ctrl+1..5: views   Home: start view   Space: spin",
                "W wireframe  T translucent  G grid  A axes  N add layer  Del delete  Ctrl+Z/Y  Ctrl+S/O/N  Ctrl+I/E import/export  F1 hide",
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
