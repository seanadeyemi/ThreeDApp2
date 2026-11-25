namespace ThreeDApp2
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.selectBtn = new System.Windows.Forms.ToolStripButton();
            this.lineBtn = new System.Windows.Forms.ToolStripButton();
            this.rectBtn = new System.Windows.Forms.ToolStripButton();
            this.circleBtn = new System.Windows.Forms.ToolStripButton();
            this.pushPullBtn = new System.Windows.Forms.ToolStripButton();
            this.moveBtn = new System.Windows.Forms.ToolStripButton();
            this.panBtn = new System.Windows.Forms.ToolStripButton();
            this.orbitBtn = new System.Windows.Forms.ToolStripButton();
            this.toolSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.importBtn = new System.Windows.Forms.ToolStripButton();
            this.exportBtn = new System.Windows.Forms.ToolStripButton();
            this.toolSepFile = new System.Windows.Forms.ToolStripSeparator();
            this.undoBtn = new System.Windows.Forms.ToolStripButton();
            this.redoBtn = new System.Windows.Forms.ToolStripButton();
            this.toolSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.frontViewBtn = new System.Windows.Forms.ToolStripButton();
            this.topViewBtn = new System.Windows.Forms.ToolStripButton();
            this.leftViewBtn = new System.Windows.Forms.ToolStripButton();
            this.rightViewBtn = new System.Windows.Forms.ToolStripButton();
            this.isoViewBtn = new System.Windows.Forms.ToolStripButton();
            this.toolSep3 = new System.Windows.Forms.ToolStripSeparator();
            this.planeLabel = new System.Windows.Forms.ToolStripLabel();
            this.planeCombo = new System.Windows.Forms.ToolStripComboBox();
            this.offsetLabel = new System.Windows.Forms.ToolStripLabel();
            this.offsetBox = new System.Windows.Forms.ToolStripTextBox();
            this.gridBtn = new System.Windows.Forms.ToolStripButton();
            this.axesBtn = new System.Windows.Forms.ToolStripButton();
            this.toolSep4 = new System.Windows.Forms.ToolStripSeparator();
            this.shapesLabel = new System.Windows.Forms.ToolStripLabel();
            this.shapesCombo = new System.Windows.Forms.ToolStripComboBox();
            this.colorBtn = new System.Windows.Forms.ToolStripButton();
            this.deleteBtn = new System.Windows.Forms.ToolStripButton();
            this.toolSep5 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsBtn = new System.Windows.Forms.ToolStripDropDownButton();
            this.centerOnLaunchItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groundSkyItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gizmoItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.measureStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.saveStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.viewport = new ThreeDApp2.ViewportPanel();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // toolStrip1
            //
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectBtn,
            this.lineBtn,
            this.rectBtn,
            this.circleBtn,
            this.pushPullBtn,
            this.moveBtn,
            this.panBtn,
            this.orbitBtn,
            this.toolSep1,
            this.importBtn,
            this.exportBtn,
            this.toolSepFile,
            this.undoBtn,
            this.redoBtn,
            this.toolSep2,
            this.frontViewBtn,
            this.topViewBtn,
            this.leftViewBtn,
            this.rightViewBtn,
            this.isoViewBtn,
            this.toolSep3,
            this.planeLabel,
            this.planeCombo,
            this.offsetLabel,
            this.offsetBox,
            this.gridBtn,
            this.axesBtn,
            this.toolSep4,
            this.shapesLabel,
            this.shapesCombo,
            this.colorBtn,
            this.deleteBtn,
            this.toolSep5,
            this.optionsBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.toolStrip1.Size = new System.Drawing.Size(1100, 35);
            this.toolStrip1.TabIndex = 0;
            //
            // selectBtn
            //
            this.selectBtn.Name = "selectBtn";
            this.selectBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.selectBtn.Text = "Select";
            this.selectBtn.Click += new System.EventHandler(this.selectBtn_Click);
            this.selectBtn.ToolTipText = "Select (S): pick shapes, drag vertices, double-click an edge to add one";
            //
            // lineBtn
            //
            this.lineBtn.Name = "lineBtn";
            this.lineBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.lineBtn.Text = "Line";
            this.lineBtn.Click += new System.EventHandler(this.lineBtn_Click);
            this.lineBtn.ToolTipText = "Line (L): click points; click the first point to close and fill";
            //
            // rectBtn
            //
            this.rectBtn.Name = "rectBtn";
            this.rectBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rectBtn.Text = "Rectangle";
            this.rectBtn.Click += new System.EventHandler(this.rectBtn_Click);
            this.rectBtn.ToolTipText = "Rectangle (R)";
            //
            // circleBtn
            //
            this.circleBtn.Name = "circleBtn";
            this.circleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.circleBtn.Text = "Circle";
            this.circleBtn.Click += new System.EventHandler(this.circleBtn_Click);
            this.circleBtn.ToolTipText = "Circle (C)";
            //
            // pushPullBtn
            //
            this.pushPullBtn.Name = "pushPullBtn";
            this.pushPullBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.pushPullBtn.Text = "Push/Pull";
            this.pushPullBtn.Click += new System.EventHandler(this.pushPullBtn_Click);
            this.pushPullBtn.ToolTipText = "Push/Pull (P): pull a filled face into a solid";
            //
            // moveBtn
            //
            this.moveBtn.Name = "moveBtn";
            this.moveBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.moveBtn.Text = "Move";
            this.moveBtn.Click += new System.EventHandler(this.moveBtn_Click);
            this.moveBtn.ToolTipText = "Move (M): hold Ctrl when you click to copy";
            //
            // panBtn
            //
            this.panBtn.Name = "panBtn";
            this.panBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.panBtn.Text = "Pan";
            this.panBtn.Click += new System.EventHandler(this.panBtn_Click);
            this.panBtn.ToolTipText = "Pan (H): drag to slide the view";
            //
            // importBtn
            //
            this.importBtn.Name = "importBtn";
            this.importBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.importBtn.Text = "Import";
            this.importBtn.Click += new System.EventHandler(this.importBtn_Click);
            this.importBtn.ToolTipText = "Import a 3D model: OBJ, STL, PLY, glTF/GLB (Ctrl+I)";
            //
            // exportBtn
            //
            this.exportBtn.Name = "exportBtn";
            this.exportBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.exportBtn.Text = "Export";
            this.exportBtn.Click += new System.EventHandler(this.exportBtn_Click);
            this.exportBtn.ToolTipText = "Export the model as OBJ, STL, PLY, glTF or GLB (Ctrl+E)";
            //
            // orbitBtn
            //
            this.orbitBtn.Name = "orbitBtn";
            this.orbitBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.orbitBtn.Text = "Orbit";
            this.orbitBtn.Click += new System.EventHandler(this.orbitBtn_Click);
            this.orbitBtn.ToolTipText = "Orbit (O): drag to turn and tilt the view";
            //
            // undoBtn
            //
            this.undoBtn.Name = "undoBtn";
            this.undoBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.undoBtn.Text = "Undo";
            this.undoBtn.Click += new System.EventHandler(this.undoBtn_Click);
            this.undoBtn.ToolTipText = "Undo (Ctrl+Z)";
            //
            // redoBtn
            //
            this.redoBtn.Name = "redoBtn";
            this.redoBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.redoBtn.Text = "Redo";
            this.redoBtn.Click += new System.EventHandler(this.redoBtn_Click);
            this.redoBtn.ToolTipText = "Redo (Ctrl+Y)";
            //
            // frontViewBtn
            //
            this.frontViewBtn.Name = "frontViewBtn";
            this.frontViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.frontViewBtn.Text = "Front";
            this.frontViewBtn.Click += new System.EventHandler(this.frontViewBtn_Click);
            this.frontViewBtn.ToolTipText = "Front view (Ctrl+1)";
            //
            // topViewBtn
            //
            this.topViewBtn.Name = "topViewBtn";
            this.topViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.topViewBtn.Text = "Top";
            this.topViewBtn.Click += new System.EventHandler(this.topViewBtn_Click);
            this.topViewBtn.ToolTipText = "Top view (Ctrl+2)";
            //
            // leftViewBtn
            //
            this.leftViewBtn.Name = "leftViewBtn";
            this.leftViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.leftViewBtn.Text = "Left";
            this.leftViewBtn.Click += new System.EventHandler(this.leftViewBtn_Click);
            this.leftViewBtn.ToolTipText = "Left view (Ctrl+3)";
            //
            // rightViewBtn
            //
            this.rightViewBtn.Name = "rightViewBtn";
            this.rightViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rightViewBtn.Text = "Right";
            this.rightViewBtn.Click += new System.EventHandler(this.rightViewBtn_Click);
            this.rightViewBtn.ToolTipText = "Right view (Ctrl+4)";
            //
            // isoViewBtn
            //
            this.isoViewBtn.Name = "isoViewBtn";
            this.isoViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.isoViewBtn.Text = "Iso";
            this.isoViewBtn.Click += new System.EventHandler(this.isoViewBtn_Click);
            this.isoViewBtn.ToolTipText = "Start / iso view (Ctrl+5, Home)";
            //
            // planeLabel
            //
            this.planeLabel.Name = "planeLabel";
            this.planeLabel.Text = "Draw on:";
            //
            // planeCombo
            //
            this.planeCombo.Name = "planeCombo";
            this.planeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.planeCombo.Size = new System.Drawing.Size(110, 31);
            this.planeCombo.SelectedIndexChanged += new System.EventHandler(this.planeCombo_SelectedIndexChanged);
            this.planeCombo.ToolTipText = "Plane that new shapes are drawn on when the cursor is not over a face";
            //
            // offsetLabel
            //
            this.offsetLabel.Name = "offsetLabel";
            this.offsetLabel.Text = "at";
            //
            // offsetBox
            //
            this.offsetBox.Name = "offsetBox";
            this.offsetBox.Size = new System.Drawing.Size(45, 31);
            this.offsetBox.TextChanged += new System.EventHandler(this.offsetBox_TextChanged);
            this.offsetBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.offsetBox_KeyDown);
            this.offsetBox.ToolTipText = "Position of the drawing plane along its normal";
            //
            // gridBtn
            //
            this.gridBtn.Name = "gridBtn";
            this.gridBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.gridBtn.Text = "Grid";
            this.gridBtn.Click += new System.EventHandler(this.gridBtn_Click);
            this.gridBtn.ToolTipText = "Show the grid and snap to it (G)";
            //
            // axesBtn
            //
            this.axesBtn.Name = "axesBtn";
            this.axesBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.axesBtn.Text = "Axes";
            this.axesBtn.Click += new System.EventHandler(this.axesBtn_Click);
            this.axesBtn.ToolTipText = "Show the X/Y/Z axes (A)";
            //
            // shapesLabel
            //
            this.shapesLabel.Name = "shapesLabel";
            this.shapesLabel.Text = "Shapes:";
            //
            // shapesCombo
            //
            this.shapesCombo.Name = "shapesCombo";
            this.shapesCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.shapesCombo.Size = new System.Drawing.Size(150, 31);
            this.shapesCombo.SelectedIndexChanged += new System.EventHandler(this.shapesCombo_SelectedIndexChanged);
            this.shapesCombo.ToolTipText = "All shapes in the scene. Pick one to select it";
            //
            // colorBtn
            //
            this.colorBtn.Name = "colorBtn";
            this.colorBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.colorBtn.Text = "Color...";
            this.colorBtn.Click += new System.EventHandler(this.colorBtn_Click);
            this.colorBtn.ToolTipText = "Change the colour of the selected shape";
            //
            // deleteBtn
            //
            this.deleteBtn.Name = "deleteBtn";
            this.deleteBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteBtn.Text = "Delete";
            this.deleteBtn.Click += new System.EventHandler(this.deleteBtn_Click);
            this.deleteBtn.ToolTipText = "Delete the selected shape (Del)";
            //
            // optionsBtn
            //
            this.optionsBtn.Name = "optionsBtn";
            this.optionsBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            this.optionsBtn.Text = "Options";
            this.optionsBtn.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.centerOnLaunchItem,
            this.groundSkyItem,
            this.gizmoItem});
            this.optionsBtn.ToolTipText = "Options";
            //
            // centerOnLaunchItem
            //
            this.centerOnLaunchItem.CheckOnClick = true;
            this.centerOnLaunchItem.Name = "centerOnLaunchItem";
            this.centerOnLaunchItem.Text = "Centre the view on launch (vertical axis upright in the middle)";
            this.centerOnLaunchItem.Click += new System.EventHandler(this.centerOnLaunchItem_Click);
            //
            // groundSkyItem
            //
            this.groundSkyItem.CheckOnClick = true;
            this.groundSkyItem.Name = "groundSkyItem";
            this.groundSkyItem.Text = "Show ground, sky and horizon";
            this.groundSkyItem.Click += new System.EventHandler(this.groundSkyItem_Click);
            //
            // gizmoItem
            //
            this.gizmoItem.CheckOnClick = true;
            this.gizmoItem.Name = "gizmoItem";
            this.gizmoItem.Text = "Show navigation gizmo (top-right corner)";
            this.gizmoItem.Click += new System.EventHandler(this.gizmoItem_Click);
            //
            // statusStrip1
            //
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStatus,
            this.measureStatus,
            this.saveStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 678);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1100, 22);
            this.statusStrip1.TabIndex = 2;
            //
            // toolStatus
            //
            this.toolStatus.Name = "toolStatus";
            this.toolStatus.Spring = true;
            this.toolStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // measureStatus
            //
            this.measureStatus.AutoSize = false;
            this.measureStatus.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.measureStatus.Name = "measureStatus";
            this.measureStatus.Size = new System.Drawing.Size(260, 17);
            this.measureStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // saveStatus
            //
            this.saveStatus.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.saveStatus.Name = "saveStatus";
            this.saveStatus.Size = new System.Drawing.Size(0, 17);
            //
            // viewport
            //
            this.viewport.BackColor = System.Drawing.Color.FromArgb(245, 245, 248);
            this.viewport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewport.Location = new System.Drawing.Point(0, 35);
            this.viewport.Name = "viewport";
            this.viewport.Size = new System.Drawing.Size(1100, 643);
            this.viewport.TabIndex = 1;
            //
            // Form1
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.viewport);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.MinimumSize = new System.Drawing.Size(640, 400);
            this.Name = "Form1";
            this.Text = "ThreeDApp2 - 3D Layer Viewer";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton selectBtn;
        private System.Windows.Forms.ToolStripButton lineBtn;
        private System.Windows.Forms.ToolStripButton rectBtn;
        private System.Windows.Forms.ToolStripButton circleBtn;
        private System.Windows.Forms.ToolStripButton pushPullBtn;
        private System.Windows.Forms.ToolStripButton moveBtn;
        private System.Windows.Forms.ToolStripButton panBtn;
        private System.Windows.Forms.ToolStripButton orbitBtn;
        private System.Windows.Forms.ToolStripButton importBtn;
        private System.Windows.Forms.ToolStripButton exportBtn;
        private System.Windows.Forms.ToolStripSeparator toolSepFile;
        private System.Windows.Forms.ToolStripSeparator toolSep1;
        private System.Windows.Forms.ToolStripButton undoBtn;
        private System.Windows.Forms.ToolStripButton redoBtn;
        private System.Windows.Forms.ToolStripSeparator toolSep2;
        private System.Windows.Forms.ToolStripButton frontViewBtn;
        private System.Windows.Forms.ToolStripButton topViewBtn;
        private System.Windows.Forms.ToolStripButton leftViewBtn;
        private System.Windows.Forms.ToolStripButton rightViewBtn;
        private System.Windows.Forms.ToolStripButton isoViewBtn;
        private System.Windows.Forms.ToolStripSeparator toolSep3;
        private System.Windows.Forms.ToolStripLabel planeLabel;
        private System.Windows.Forms.ToolStripComboBox planeCombo;
        private System.Windows.Forms.ToolStripLabel offsetLabel;
        private System.Windows.Forms.ToolStripTextBox offsetBox;
        private System.Windows.Forms.ToolStripButton gridBtn;
        private System.Windows.Forms.ToolStripButton axesBtn;
        private System.Windows.Forms.ToolStripSeparator toolSep4;
        private System.Windows.Forms.ToolStripLabel shapesLabel;
        private System.Windows.Forms.ToolStripComboBox shapesCombo;
        private System.Windows.Forms.ToolStripButton colorBtn;
        private System.Windows.Forms.ToolStripButton deleteBtn;
        private System.Windows.Forms.ToolStripSeparator toolSep5;
        private System.Windows.Forms.ToolStripDropDownButton optionsBtn;
        private System.Windows.Forms.ToolStripMenuItem gizmoItem;
        private System.Windows.Forms.ToolStripMenuItem centerOnLaunchItem;
        private System.Windows.Forms.ToolStripMenuItem groundSkyItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStatus;
        private System.Windows.Forms.ToolStripStatusLabel measureStatus;
        private System.Windows.Forms.ToolStripStatusLabel saveStatus;
        private ThreeDApp2.ViewportPanel viewport;
    }
}
