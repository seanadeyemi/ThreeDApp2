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
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomInBtn,
            this.zoomOutBtn,
            this.moveBtn,
            this.scaleBtn,
            this.panBtn,
            this.toolStripSeparator1,
            this.topViewBtn,
            this.frontViewBtn,
            this.leftViewBtn,
            this.toolStripButton8,
            this.rightViewBtn,
            this.bottomViewBtn,
            this.drawBtn,
            this.toolStripButton11});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // zoomInBtn
            // 
            this.zoomInBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomInBtn.Image = ((System.Drawing.Image)(resources.GetObject("zoomInBtn.Image")));
            this.zoomInBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomInBtn.Name = "zoomInBtn";
            this.zoomInBtn.Size = new System.Drawing.Size(23, 22);
            this.zoomInBtn.Text = "Zoom In";
            this.zoomInBtn.Click += new System.EventHandler(this.zoomInBtn_Click);
            // 
            // zoomOutBtn
            // 
            this.zoomOutBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomOutBtn.Image = ((System.Drawing.Image)(resources.GetObject("zoomOutBtn.Image")));
            this.zoomOutBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomOutBtn.Name = "zoomOutBtn";
            this.zoomOutBtn.Size = new System.Drawing.Size(23, 22);
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
            this.moveBtn.Size = new System.Drawing.Size(23, 22);
            this.moveBtn.Text = "Move";
            // 
            // scaleBtn
            // 
            this.scaleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.scaleBtn.Image = ((System.Drawing.Image)(resources.GetObject("scaleBtn.Image")));
            this.scaleBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.scaleBtn.Name = "scaleBtn";
            this.scaleBtn.Size = new System.Drawing.Size(23, 22);
            this.scaleBtn.Text = "Scale";
            // 
            // panBtn
            // 
            this.panBtn.CheckOnClick = true;
            this.panBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.panBtn.Image = ((System.Drawing.Image)(resources.GetObject("panBtn.Image")));
            this.panBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.panBtn.Name = "panBtn";
            this.panBtn.Size = new System.Drawing.Size(23, 22);
            this.panBtn.Text = "Pan";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // topViewBtn
            // 
            this.topViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.topViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("topViewBtn.Image")));
            this.topViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.topViewBtn.Name = "topViewBtn";
            this.topViewBtn.Size = new System.Drawing.Size(23, 22);
            this.topViewBtn.Text = "Top";
            this.topViewBtn.Click += new System.EventHandler(this.topViewBtn_Click);
            // 
            // frontViewBtn
            // 
            this.frontViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.frontViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("frontViewBtn.Image")));
            this.frontViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.frontViewBtn.Name = "frontViewBtn";
            this.frontViewBtn.Size = new System.Drawing.Size(23, 22);
            this.frontViewBtn.Text = "Front";
            // 
            // leftViewBtn
            // 
            this.leftViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.leftViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("leftViewBtn.Image")));
            this.leftViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.leftViewBtn.Name = "leftViewBtn";
            this.leftViewBtn.Size = new System.Drawing.Size(23, 22);
            this.leftViewBtn.Text = "Left";
            this.leftViewBtn.Click += new System.EventHandler(this.leftViewBtn_Click);
            // 
            // toolStripButton8
            // 
            this.toolStripButton8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton8.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton8.Image")));
            this.toolStripButton8.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton8.Name = "toolStripButton8";
            this.toolStripButton8.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton8.Text = "toolStripButton8";
            // 
            // rightViewBtn
            // 
            this.rightViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rightViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("rightViewBtn.Image")));
            this.rightViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rightViewBtn.Name = "rightViewBtn";
            this.rightViewBtn.Size = new System.Drawing.Size(23, 22);
            this.rightViewBtn.Text = "Right";
            this.rightViewBtn.Click += new System.EventHandler(this.rightViewBtn_Click);
            // 
            // bottomViewBtn
            // 
            this.bottomViewBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bottomViewBtn.Image = ((System.Drawing.Image)(resources.GetObject("bottomViewBtn.Image")));
            this.bottomViewBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bottomViewBtn.Name = "bottomViewBtn";
            this.bottomViewBtn.Size = new System.Drawing.Size(23, 22);
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
            this.drawBtn.Size = new System.Drawing.Size(23, 22);
            this.drawBtn.Text = "toolStripButton10";
            this.drawBtn.Click += new System.EventHandler(this.drawBtn_Click);
            this.drawBtn.DisplayStyleChanged += new System.EventHandler(this.drawBtn_DisplayStyleChanged);
            // 
            // toolStripButton11
            // 
            this.toolStripButton11.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton11.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton11.Image")));
            this.toolStripButton11.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton11.Name = "toolStripButton11";
            this.toolStripButton11.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton11.Text = "toolStripButton11";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.toolStrip1);
            this.Name = "Form1";
            this.Text = "Form1";
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
    }
}

