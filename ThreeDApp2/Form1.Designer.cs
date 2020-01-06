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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.zoomInBtn = new System.Windows.Forms.ToolStripButton();
            this.zoomOutBtn = new System.Windows.Forms.ToolStripButton();
            this.moveBtn = new System.Windows.Forms.ToolStripButton();
            this.scaleBtn = new System.Windows.Forms.ToolStripButton();
            this.panBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.topViewBtn = new System.Windows.Forms.ToolStripButton();
            this.frontViewBtn = new System.Windows.Forms.ToolStripButton();
            this.leftViewBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton8 = new System.Windows.Forms.ToolStripButton();
            this.rightViewBtn = new System.Windows.Forms.ToolStripButton();
            this.bottomViewBtn = new System.Windows.Forms.ToolStripButton();
            this.drawBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton11 = new System.Windows.Forms.ToolStripButton();
            this.orbitBtn = new System.Windows.Forms.ToolStripButton();
            this.selectBtn = new System.Windows.Forms.ToolStripButton();
            this.polygonListCombobox = new System.Windows.Forms.ToolStripComboBox();
            this.resetBtn = new System.Windows.Forms.ToolStripButton();
            this.undoBtn = new System.Windows.Forms.ToolStripButton();
            this.redoBtn = new System.Windows.Forms.ToolStripButton();
            this.rectBtn = new System.Windows.Forms.ToolStripButton();
            this.deleteBtn = new System.Windows.Forms.ToolStripButton();
            this.settingsBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomInBtn,
            this.zoomOutBtn,
            this.moveBtn,
            this.scaleBtn,
            this.panBtn,
            this.orbitBtn,
            this.toolStripSeparator1,
            this.topViewBtn,
            this.frontViewBtn,
            this.leftViewBtn,
            this.toolStripButton8,
            this.rightViewBtn,
            this.bottomViewBtn,
            this.toolStripButton11,
            this.toolStripSeparator3,
            this.selectBtn,
            this.polygonListCombobox,
            this.resetBtn,
            this.undoBtn,
            this.redoBtn,
            this.drawBtn,
            this.rectBtn,
            this.deleteBtn,
            this.settingsBtn,
            this.toolStripSeparator2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1067, 28);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // zoomInBtn
            // 
            this.zoomInBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomInBtn.Image = ((System.Drawing.Image)(resources.GetObject("zoomInBtn.Image")));
            this.zoomInBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomInBtn.Name = "zoomInBtn";
            this.zoomInBtn.Size = new System.Drawing.Size(24, 25);
            this.zoomInBtn.Text = "Zoom In";
            this.zoomInBtn.Click += new System.EventHandler(this.zoomInBtn_Click);
            // 
            // zoomOutBtn
            // 
            this.zoomOutBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomOutBtn.Image = ((System.Drawing.Image)(resources.GetObject("zoomOutBtn.Image")));
            this.zoomOutBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomOutBtn.Name = "zoomOutBtn";
            this.zoomOutBtn.Size = new System.Drawing.Size(24, 25);
            this.zoomOutBtn.Text = "Zoom Out";
            this.zoomOutBtn.Click += new System.EventHandler(this.zoomOutBtn_Click);
            // 
            // moveBtn
            // 
            this.moveBtn.CheckOnClick = true;
            this.moveBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.moveBtn.Image = ((System.Drawing.Image)(resources.GetObject("moveBtn.Image")));
            this.moveBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.moveBtn.Name = "moveBtn";
            this.moveBtn.Size = new System.Drawing.Size(24, 25);
            this.moveBtn.Text = "Move";
            this.moveBtn.Click += new System.EventHandler(this.moveBtn_Click);
            // 
            // scaleBtn
            // 
            this.scaleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.scaleBtn.Image = ((System.Drawing.Image)(resources.GetObject("scaleBtn.Image")));
            this.scaleBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.scaleBtn.Name = "scaleBtn";
            this.scaleBtn.Size = new System.Drawing.Size(24, 25);
            this.scaleBtn.Text = "Scale";
            // 
            // panBtn
            // 
            this.panBtn.CheckOnClick = true;
            this.panBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.panBtn.Image = ((System.Drawing.Image)(resources.GetObject("panBtn.Image")));
            this.panBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.panBtn.Name = "panBtn";
            this.panBtn.Size = new System.Drawing.Size(24, 25);
            this.panBtn.Text = "Pan";
            this.panBtn.Click += new System.EventHandler(this.panBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // topViewBtn
            // 
            this.topViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.topViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("topViewBtn.Image")));
            this.topViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.topViewBtn.Name = "topViewBtn";
            this.topViewBtn.Size = new System.Drawing.Size(24, 25);
            this.topViewBtn.Text = "Top";
            this.topViewBtn.Click += new System.EventHandler(this.topViewBtn_Click);
            // 
            // frontViewBtn
            // 
            this.frontViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.frontViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("frontViewBtn.Image")));
            this.frontViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.frontViewBtn.Name = "frontViewBtn";
            this.frontViewBtn.Size = new System.Drawing.Size(24, 25);
            this.frontViewBtn.Text = "Front";
            // 
            // leftViewBtn
            // 
            this.leftViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.leftViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("leftViewBtn.Image")));
            this.leftViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.leftViewBtn.Name = "leftViewBtn";
            this.leftViewBtn.Size = new System.Drawing.Size(24, 25);
            this.leftViewBtn.Text = "Left";
            this.leftViewBtn.Click += new System.EventHandler(this.leftViewBtn_Click);
            // 
            // toolStripButton8
            // 
            this.toolStripButton8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton8.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton8.Image")));
            this.toolStripButton8.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton8.Name = "toolStripButton8";
            this.toolStripButton8.Size = new System.Drawing.Size(24, 25);
            this.toolStripButton8.Text = "toolStripButton8";
            // 
            // rightViewBtn
            // 
            this.rightViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rightViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("rightViewBtn.Image")));
            this.rightViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rightViewBtn.Name = "rightViewBtn";
            this.rightViewBtn.Size = new System.Drawing.Size(24, 25);
            this.rightViewBtn.Text = "Right";
            this.rightViewBtn.Click += new System.EventHandler(this.rightViewBtn_Click);
            // 
            // bottomViewBtn
            // 
            this.bottomViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bottomViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("bottomViewBtn.Image")));
            this.bottomViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bottomViewBtn.Name = "bottomViewBtn";
            this.bottomViewBtn.Size = new System.Drawing.Size(24, 25);
            this.bottomViewBtn.Text = "Bottom";
            this.bottomViewBtn.Click += new System.EventHandler(this.bottomViewBtn_Click);
            // 
            // drawBtn
            // 
            this.drawBtn.CheckOnClick = true;
            this.drawBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.drawBtn.Image = ((System.Drawing.Image)(resources.GetObject("drawBtn.Image")));
            this.drawBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.drawBtn.Name = "drawBtn";
            this.drawBtn.Size = new System.Drawing.Size(24, 25);
            this.drawBtn.Text = "Line";
            this.drawBtn.Click += new System.EventHandler(this.drawBtn_Click);
            this.drawBtn.DisplayStyleChanged += new System.EventHandler(this.drawBtn_DisplayStyleChanged);
            // 
            // toolStripButton11
            // 
            this.toolStripButton11.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton11.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton11.Image")));
            this.toolStripButton11.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton11.Name = "toolStripButton11";
            this.toolStripButton11.Size = new System.Drawing.Size(24, 25);
            this.toolStripButton11.Text = "toolStripButton11";
            this.toolStripButton11.Click += new System.EventHandler(this.toolStripButton11_Click);
            // 
            // orbitBtn
            // 
            this.orbitBtn.CheckOnClick = true;
            this.orbitBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.orbitBtn.Image = ((System.Drawing.Image)(resources.GetObject("orbitBtn.Image")));
            this.orbitBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.orbitBtn.Name = "orbitBtn";
            this.orbitBtn.Size = new System.Drawing.Size(24, 25);
            this.orbitBtn.Text = "Orbit";
            // 
            // selectBtn
            // 
            this.selectBtn.CheckOnClick = true;
            this.selectBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.selectBtn.Image = ((System.Drawing.Image)(resources.GetObject("selectBtn.Image")));
            this.selectBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.selectBtn.Name = "selectBtn";
            this.selectBtn.Size = new System.Drawing.Size(24, 25);
            this.selectBtn.Text = "Select";
            this.selectBtn.Click += new System.EventHandler(this.selectBtn_Click);
            // 
            // polygonListCombobox
            // 
            this.polygonListCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.polygonListCombobox.Name = "polygonListCombobox";
            this.polygonListCombobox.Size = new System.Drawing.Size(160, 28);
            this.polygonListCombobox.SelectedIndexChanged += new System.EventHandler(this.polygonListCombobox_SelectedIndexChanged);
            // 
            // resetBtn
            // 
            this.resetBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.resetBtn.Image = ((System.Drawing.Image)(resources.GetObject("resetBtn.Image")));
            this.resetBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.resetBtn.Name = "resetBtn";
            this.resetBtn.Size = new System.Drawing.Size(24, 25);
            this.resetBtn.Text = "Reset";
            this.resetBtn.Click += new System.EventHandler(this.resetBtn_Click);
            // 
            // undoBtn
            // 
            this.undoBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.undoBtn.Image = ((System.Drawing.Image)(resources.GetObject("undoBtn.Image")));
            this.undoBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.undoBtn.Name = "undoBtn";
            this.undoBtn.Size = new System.Drawing.Size(24, 25);
            this.undoBtn.Text = "Undo";
            this.undoBtn.Click += new System.EventHandler(this.undoBtn_Click);
            // 
            // redoBtn
            // 
            this.redoBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.redoBtn.Image = ((System.Drawing.Image)(resources.GetObject("redoBtn.Image")));
            this.redoBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.redoBtn.Name = "redoBtn";
            this.redoBtn.Size = new System.Drawing.Size(24, 25);
            this.redoBtn.Text = "Redo";
            this.redoBtn.Click += new System.EventHandler(this.redoBtn_Click);
            // 
            // rectBtn
            // 
            this.rectBtn.CheckOnClick = true;
            this.rectBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rectBtn.Image = ((System.Drawing.Image)(resources.GetObject("rectBtn.Image")));
            this.rectBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rectBtn.Name = "rectBtn";
            this.rectBtn.Size = new System.Drawing.Size(24, 25);
            this.rectBtn.Text = "Rectangle";
            this.rectBtn.Click += new System.EventHandler(this.rectBtn_Click);
            // 
            // deleteBtn
            // 
            this.deleteBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteBtn.Image = ((System.Drawing.Image)(resources.GetObject("deleteBtn.Image")));
            this.deleteBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteBtn.Name = "deleteBtn";
            this.deleteBtn.Size = new System.Drawing.Size(24, 25);
            this.deleteBtn.Text = "Delete";
            // 
            // settingsBtn
            // 
            this.settingsBtn.CheckOnClick = true;
            this.settingsBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.settingsBtn.Image = ((System.Drawing.Image)(resources.GetObject("settingsBtn.Image")));
            this.settingsBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.settingsBtn.Name = "settingsBtn";
            this.settingsBtn.Size = new System.Drawing.Size(24, 25);
            this.settingsBtn.Text = "toolStripButton1";
            this.settingsBtn.Click += new System.EventHandler(this.settingsBtn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton zoomInBtn;
        private System.Windows.Forms.ToolStripButton zoomOutBtn;
        private System.Windows.Forms.ToolStripButton moveBtn;
        private System.Windows.Forms.ToolStripButton scaleBtn;
        private System.Windows.Forms.ToolStripButton panBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton topViewBtn;
        private System.Windows.Forms.ToolStripButton frontViewBtn;
        private System.Windows.Forms.ToolStripButton leftViewBtn;
        private System.Windows.Forms.ToolStripButton toolStripButton8;
        private System.Windows.Forms.ToolStripButton drawBtn;
        private System.Windows.Forms.ToolStripButton toolStripButton11;
        private System.Windows.Forms.ToolStripButton rightViewBtn;
        private System.Windows.Forms.ToolStripButton bottomViewBtn;
        private System.Windows.Forms.ToolStripButton orbitBtn;
        private System.Windows.Forms.ToolStripButton selectBtn;
        private System.Windows.Forms.ToolStripComboBox polygonListCombobox;
        private System.Windows.Forms.ToolStripButton resetBtn;
        private System.Windows.Forms.ToolStripButton undoBtn;
        private System.Windows.Forms.ToolStripButton redoBtn;
        private System.Windows.Forms.ToolStripButton rectBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton deleteBtn;
        private System.Windows.Forms.ToolStripButton settingsBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}

