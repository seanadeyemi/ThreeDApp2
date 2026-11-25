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
            this.toolSep1 = new System.Windows.Forms.ToolStripSeparator();
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
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectBtn,
            this.lineBtn,
            this.rectBtn,
            this.circleBtn,
            this.pushPullBtn,
            this.moveBtn,
            this.toolSep1,
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
            this.deleteBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1100, 25);
            this.toolStrip1.TabIndex = 0;
            //
            // selectBtn
            this.selectBtn.Name = "selectBtn";
            this.selectBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.selectBtn.Text = "Select";
            this.selectBtn.Click += new System.EventHandler(this.selectBtn_Click);
            this.selectBtn.ToolTipText = "Select, move vertices, orbit (S / Esc)";
            // lineBtn
            this.lineBtn.Name = "lineBtn";
            this.lineBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lineBtn.Text = "Line";
            this.lineBtn.Click += new System.EventHandler(this.lineBtn_Click);
            this.lineBtn.ToolTipText = "Draw lines and polygons. Click the first point to close and fill (L)";
            // rectBtn
            this.rectBtn.Name = "rectBtn";
            this.rectBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.rectBtn.Text = "Rectangle";
            this.rectBtn.Click += new System.EventHandler(this.rectBtn_Click);
            this.rectBtn.ToolTipText = "Draw a filled rectangle (R)";
            // circleBtn
            this.circleBtn.Name = "circleBtn";
            this.circleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.circleBtn.Text = "Circle";
            this.circleBtn.Click += new System.EventHandler(this.circleBtn_Click);
            this.circleBtn.ToolTipText = "Draw a filled circle (C)";
            // pushPullBtn
            this.pushPullBtn.Name = "pushPullBtn";
            this.pushPullBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.pushPullBtn.Text = "Push/Pull";
            this.pushPullBtn.Click += new System.EventHandler(this.pushPullBtn_Click);
            this.pushPullBtn.ToolTipText = "Extrude a filled face into a solid (P)";
            // moveBtn
            this.moveBtn.Name = "moveBtn";
            this.moveBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.moveBtn.Text = "Move";
            this.moveBtn.Click += new System.EventHandler(this.moveBtn_Click);
            this.moveBtn.ToolTipText = "Move a shape. Hold Ctrl when you click to copy it (M)";
            // undoBtn
            this.undoBtn.Name = "undoBtn";
            this.undoBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.undoBtn.Text = "Undo";
            this.undoBtn.Click += new System.EventHandler(this.undoBtn_Click);
            this.undoBtn.ToolTipText = "Undo (Ctrl+Z)";
            // redoBtn
            this.redoBtn.Name = "redoBtn";
            this.redoBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.redoBtn.Text = "Redo";
            this.redoBtn.Click += new System.EventHandler(this.redoBtn_Click);
            this.redoBtn.ToolTipText = "Redo (Ctrl+Y)";
            // frontViewBtn
            this.frontViewBtn.Name = "frontViewBtn";
            this.frontViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.frontViewBtn.Text = "Front";
            this.frontViewBtn.Click += new System.EventHandler(this.frontViewBtn_Click);
            this.frontViewBtn.ToolTipText = "Front view (Ctrl+1)";
            // topViewBtn
            this.topViewBtn.Name = "topViewBtn";
            this.topViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.topViewBtn.Text = "Top";
            this.topViewBtn.Click += new System.EventHandler(this.topViewBtn_Click);
            this.topViewBtn.ToolTipText = "Top view (Ctrl+2)";
            // leftViewBtn
            this.leftViewBtn.Name = "leftViewBtn";
            this.leftViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.leftViewBtn.Text = "Left";
            this.leftViewBtn.Click += new System.EventHandler(this.leftViewBtn_Click);
            this.leftViewBtn.ToolTipText = "Left view (Ctrl+3)";
            // rightViewBtn
            this.rightViewBtn.Name = "rightViewBtn";
            this.rightViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.rightViewBtn.Text = "Right";
            this.rightViewBtn.Click += new System.EventHandler(this.rightViewBtn_Click);
            this.rightViewBtn.ToolTipText = "Right view (Ctrl+4)";
            // isoViewBtn
            this.isoViewBtn.Name = "isoViewBtn";
            this.isoViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.isoViewBtn.Text = "Iso";
            this.isoViewBtn.Click += new System.EventHandler(this.isoViewBtn_Click);
            this.isoViewBtn.ToolTipText = "Isometric view (Ctrl+5)";
            // planeLabel
            this.planeLabel.Name = "planeLabel";
            this.planeLabel.Text = "Draw on:";
            // planeCombo
            this.planeCombo.Name = "planeCombo";
            this.planeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.planeCombo.Size = new System.Drawing.Size(120, 25);
            this.planeCombo.SelectedIndexChanged += new System.EventHandler(this.planeCombo_SelectedIndexChanged);
            this.planeCombo.ToolTipText = "Plane that new shapes are drawn on when the cursor is not over a face";
            // offsetLabel
            this.offsetLabel.Name = "offsetLabel";
            this.offsetLabel.Text = "at";
            // offsetBox
            this.offsetBox.Name = "offsetBox";
            this.offsetBox.Size = new System.Drawing.Size(50, 25);
            this.offsetBox.TextChanged += new System.EventHandler(this.offsetBox_TextChanged);
            this.offsetBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.offsetBox_KeyDown);
            this.offsetBox.ToolTipText = "Position of the drawing plane along its normal";
            // gridBtn
            this.gridBtn.Name = "gridBtn";
            this.gridBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.gridBtn.Text = "Grid";
            this.gridBtn.Click += new System.EventHandler(this.gridBtn_Click);
            this.gridBtn.ToolTipText = "Show the grid and snap to it (G)";
            // axesBtn
            this.axesBtn.Name = "axesBtn";
            this.axesBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.axesBtn.Text = "Axes";
            this.axesBtn.Click += new System.EventHandler(this.axesBtn_Click);
            this.axesBtn.ToolTipText = "Show the X/Y/Z axes (A)";
            // shapesLabel
            this.shapesLabel.Name = "shapesLabel";
            this.shapesLabel.Text = "Shapes:";
            // shapesCombo
            this.shapesCombo.Name = "shapesCombo";
            this.shapesCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.shapesCombo.Size = new System.Drawing.Size(160, 25);
            this.shapesCombo.SelectedIndexChanged += new System.EventHandler(this.shapesCombo_SelectedIndexChanged);
            this.shapesCombo.ToolTipText = "All shapes in the scene. Pick one to select it";
            // colorBtn
            this.colorBtn.Name = "colorBtn";
            this.colorBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.colorBtn.Text = "Color...";
            this.colorBtn.Click += new System.EventHandler(this.colorBtn_Click);
            this.colorBtn.ToolTipText = "Change the colour of the selected shape";
            // deleteBtn
            this.deleteBtn.Name = "deleteBtn";
            this.deleteBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.deleteBtn.Text = "Delete";
            this.deleteBtn.Click += new System.EventHandler(this.deleteBtn_Click);
            this.deleteBtn.ToolTipText = "Delete the selected shape (Del)";
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
            this.viewport.Location = new System.Drawing.Point(0, 25);
            this.viewport.Name = "viewport";
            this.viewport.Size = new System.Drawing.Size(1100, 653);
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
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStatus;
        private System.Windows.Forms.ToolStripStatusLabel measureStatus;
        private System.Windows.Forms.ToolStripStatusLabel saveStatus;
        private ThreeDApp2.ViewportPanel viewport;
    }
}
