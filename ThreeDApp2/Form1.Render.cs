using System;
using System.Drawing;
using System.Windows.Forms;

namespace ThreeDApp2
{
    /// <summary>Switching between the standard (GDI+) renderer and the GPU (OpenGL) renderer.</summary>
    public partial class Form1
    {
        GlRenderer mGpu;            // Null while the standard renderer is in use
        string mGpuDescription;     // Graphics card name, once the GPU renderer has started

        /// <summary>Turns the GPU renderer on or off. Falls back to Standard (with the reason) if it can't start.</summary>
        private void UseGpuRenderer(bool on, bool fromUser)
        {
            if (on && mGpu == null)
            {
                string error;
                mGpu = GlRenderer.TryCreate(out error);
                if (mGpu == null)
                {
                    saveStatus.Text = "GPU renderer unavailable, using Standard";
                    if (fromUser)
                        MessageBox.Show(this, "The GPU renderer couldn't start on this PC:\n" + error + "\n\nThe Standard renderer is still in use.",
                                        "GPU renderer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    on = false;
                }
                else mGpuDescription = mGpu.Description;
            }
            else if (!on && mGpu != null)
            {
                mGpu.Dispose();
                mGpu = null;
            }
            mSettings.Renderer = on ? "gpu" : "standard";
            if (fromUser) SaveSettings();
            UpdateRendererUi();
            viewport.Invalidate();
        }

        private void UpdateRendererUi()
        {
            bool gpu = mGpu != null;
            renderBtn.Text = gpu ? "GPU" : "Standard";
            standardRenderItem.Checked = !gpu;
            gpuRenderItem.Checked = gpu;
            renderBtn.ToolTipText = gpu ? "Renderer: GPU (OpenGL) on " + mGpuDescription : "Renderer: Standard (GDI+)";
        }

        /// <summary>Switches between perspective and orthographic (parallel) projection.</summary>
        private void SetOrthographic(bool on, bool fromUser)
        {
            mScene.Orthographic = on;
            mSettings.Projection = on ? "orthographic" : "perspective";
            if (fromUser)
            {
                SaveSettings();
                saveStatus.Text = on ? "Orthographic view" : "Perspective view";
            }
            UpdateProjectionUi();
            NoteViewChange();
            UpdateHover();
            viewport.Invalidate();
        }

        private void UpdateProjectionUi()
        {
            bool ortho = mScene.Orthographic;
            projectionBtn.Text = ortho ? "Ortho" : "Perspective";
            perspectiveItem.Checked = !ortho;
            orthographicItem.Checked = ortho;
            if (projectionBtn.Image != null)
            {
                projectionBtn.Image.Dispose();
                projectionBtn.Image = ToolIcons.Render(ortho ? "ortho" : "perspective", toolStrip1.ImageScalingSize.Width);
            }
        }

        private void perspectiveItem_Click(object sender, EventArgs e) { SetOrthographic(false, true); }
        private void orthographicItem_Click(object sender, EventArgs e) { SetOrthographic(true, true); }

        private void standardRenderItem_Click(object sender, EventArgs e) { UseGpuRenderer(false, true); }
        private void gpuRenderItem_Click(object sender, EventArgs e) { UseGpuRenderer(true, true); }

        /// <summary>
        /// Draws the shapes with the GPU into the viewport (which has no transform applied).
        /// Returns false, after switching back to Standard, if the GPU fails mid-way.
        /// </summary>
        private bool PaintShapesGpu(Graphics g, out int visible)
        {
            visible = 0;
            try
            {
                RenderStyle style = new RenderStyle
                {
                    Wireframe = mWireframe,
                    Translucent = mTranslucent,
                    Selected = SelectionIndices(),
                    Hover = -1,   // Hover is drawn as an overlay, so it doesn't force a redraw
                    ColorOf = EffectiveColor,
                };
                GpuFrame frame = GpuScene.Build(mScene, viewport.ClientSize.Width, viewport.ClientSize.Height, style);
                Bitmap picture = mGpu.Render(frame);
                g.DrawImageUnscaled(picture, 0, 0);
                visible = frame.ShapeCount;
                return true;
            }
            catch (Exception ex)
            {
                mGpu.Dispose();
                mGpu = null;
                mSettings.Renderer = "standard";
                UpdateRendererUi();
                saveStatus.Text = "GPU renderer stopped (" + ex.Message + "), using Standard";
                return false;
            }
        }
    }
}
