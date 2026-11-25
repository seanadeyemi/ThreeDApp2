using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ThreeDApp2
{
    /// <summary>Draws the vector icons from <see cref="IconShapes"/> into bitmaps and cursors.</summary>
    public static class ToolIcons
    {
        public static Bitmap Render(string name, int size)
        {
            Bitmap bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);
                float s = size / 24f;
                g.ScaleTransform(s, s);
                foreach (IconPrim prim in IconShapes.Get(name)) Draw(g, prim);
            }
            return bmp;
        }

        static void Draw(Graphics g, IconPrim prim)
        {
            switch (prim.Kind)
            {
                case IconPrimKind.Lines:
                    if (prim.Points.Length < 2) return;
                    using (Pen pen = MakePen(prim.Stroke, prim.Width))
                        g.DrawLines(pen, prim.Points);
                    break;
                case IconPrimKind.Polygon:
                    if (!prim.Fill.IsEmpty)
                        using (SolidBrush b = new SolidBrush(prim.Fill)) g.FillPolygon(b, prim.Points);
                    if (!prim.Stroke.IsEmpty && prim.Width > 0)
                        using (Pen pen = MakePen(prim.Stroke, prim.Width)) g.DrawPolygon(pen, prim.Points);
                    break;
                case IconPrimKind.Ellipse:
                    RectangleF r = RectangleF.FromLTRB(prim.Points[0].X, prim.Points[0].Y, prim.Points[1].X, prim.Points[1].Y);
                    if (!prim.Fill.IsEmpty)
                        using (SolidBrush b = new SolidBrush(prim.Fill)) g.FillEllipse(b, r);
                    if (!prim.Stroke.IsEmpty && prim.Width > 0)
                        using (Pen pen = MakePen(prim.Stroke, prim.Width)) g.DrawEllipse(pen, r);
                    break;
            }
        }

        static Pen MakePen(Color c, float width)
        {
            Pen pen = new Pen(c, width);
            pen.StartCap = pen.EndCap = LineCap.Round;
            pen.LineJoin = LineJoin.Round;
            return pen;
        }

        #region Cursors from icons

        [StructLayout(LayoutKind.Sequential)]
        struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")] static extern bool GetIconInfo(IntPtr hIcon, ref ICONINFO info);
        [DllImport("user32.dll")] static extern IntPtr CreateIconIndirect(ref ICONINFO info);
        [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr hIcon);
        [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr hObject);

        /// <summary>Makes a cursor from an icon with the hotspot in its centre. Falls back to <paramref name="fallback"/>.</summary>
        public static Cursor MakeCursor(string name, int size, Cursor fallback)
        {
            try
            {
                using (Bitmap bmp = Render(name, size))
                {
                    IntPtr hIcon = bmp.GetHicon();
                    ICONINFO info = new ICONINFO();
                    GetIconInfo(hIcon, ref info);
                    info.fIcon = false;
                    info.xHotspot = size / 2;
                    info.yHotspot = size / 2;
                    IntPtr hCursor = CreateIconIndirect(ref info);
                    DestroyIcon(hIcon);
                    if (info.hbmMask != IntPtr.Zero) DeleteObject(info.hbmMask);
                    if (info.hbmColor != IntPtr.Zero) DeleteObject(info.hbmColor);
                    return hCursor == IntPtr.Zero ? fallback : new Cursor(hCursor);
                }
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        #endregion
    }
}
