using System;
using System.Collections.Generic;
using System.Drawing;

namespace ThreeDApp2
{
    public enum IconPrimKind { Lines, Polygon, Ellipse }

    /// <summary>One stroke or filled shape of a toolbar icon, in a 24 x 24 coordinate box.</summary>
    public class IconPrim
    {
        public IconPrimKind Kind;
        public PointF[] Points;       // Lines / Polygon: the points. Ellipse: top-left and bottom-right.
        public Color Fill = Color.Empty;
        public Color Stroke = Color.Empty;
        public float Width = 1.5f;
    }

    /// <summary>
    /// Vector definitions of the toolbar icons. Kept as plain data (no GDI) so the same shapes are
    /// drawn by the app (ToolIcons) and can be previewed or tested anywhere.
    /// </summary>
    public static class IconShapes
    {
        static readonly Color Ink = Color.FromArgb(45, 45, 50);
        static readonly Color Red = Color.FromArgb(215, 45, 45);
        static readonly Color Green = Color.FromArgb(40, 160, 70);
        static readonly Color Blue = Color.FromArgb(40, 90, 220);
        static readonly Color Skin = Color.FromArgb(252, 222, 190);

        public static readonly string[] Names =
        {
            "select", "line", "rectangle", "circle", "pushpull", "move", "pan", "pan-grab", "orbit", "import", "export", "render",
            "undo", "redo", "view-front", "view-top", "view-left", "view-right", "view-iso",
            "grid", "axes", "color", "delete", "options",
        };

        public static List<IconPrim> Get(string name)
        {
            List<IconPrim> p = new List<IconPrim>();
            switch (name)
            {
                case "select":
                    p.Add(Poly(Ink, Color.White, 1.2f, 6, 2.5f, 6, 19.5f, 10, 15.5f, 13, 21.5f, 15.8f, 20.2f, 12.8f, 14.3f, 18.3f, 14.3f));
                    break;

                case "line":
                    p.Add(Line(Color.FromArgb(120, 120, 130), 1.5f, 2, 22, 22, 22));
                    p.Add(Poly(Color.FromArgb(240, 185, 60), Ink, 1.1f, 15, 3, 20, 8, 8.5f, 19.5f, 3.5f, 14.5f));         // body
                    p.Add(Poly(Color.FromArgb(240, 140, 160), Ink, 1.1f, 15, 3, 17, 1, 22, 6, 20, 8));                   // eraser
                    p.Add(Poly(Color.FromArgb(235, 210, 170), Ink, 1.1f, 3.5f, 14.5f, 8.5f, 19.5f, 2, 21));               // wood tip
                    p.Add(Poly(Ink, Color.Empty, 0, 2, 21, 2.9f, 17.9f, 5.1f, 20.1f));                                    // graphite
                    break;

                case "rectangle":
                    p.Add(Poly(Color.FromArgb(255, 205, 130), Ink, 1.5f, 3, 6, 21, 6, 21, 18, 3, 18));
                    p.Add(Dot(Ink, 3, 6, 1.6f)); p.Add(Dot(Ink, 21, 18, 1.6f));
                    break;

                case "circle":
                    p.Add(Ellipse(Color.FromArgb(200, 180, 245), Ink, 1.5f, 3, 3, 21, 21));
                    p.Add(Dot(Ink, 12, 12, 1.4f));
                    p.Add(Line(Ink, 1f, 12, 12, 21, 12));
                    break;

                case "pushpull":
                    p.Add(Poly(Color.FromArgb(230, 200, 140), Ink, 1.1f, 3, 12, 9, 9, 21, 9, 15, 12));      // top
                    p.Add(Poly(Color.FromArgb(205, 170, 105), Ink, 1.1f, 3, 12, 15, 12, 15, 22, 3, 22));    // front
                    p.Add(Poly(Color.FromArgb(165, 130, 75), Ink, 1.1f, 15, 12, 21, 9, 21, 19, 15, 22));    // side
                    p.Add(Line(Red, 2.2f, 12, 10.5f, 12, 3));
                    p.Add(Poly(Red, Color.Empty, 0, 8, 5, 12, 0.8f, 16, 5));
                    break;

                case "move":
                    p.Add(Line(Red, 2f, 12, 4, 12, 20));
                    p.Add(Line(Red, 2f, 4, 12, 20, 12));
                    p.Add(Poly(Red, Color.Empty, 0, 8.5f, 5, 12, 1, 15.5f, 5));
                    p.Add(Poly(Red, Color.Empty, 0, 8.5f, 19, 12, 23, 15.5f, 19));
                    p.Add(Poly(Red, Color.Empty, 0, 5, 8.5f, 1, 12, 5, 15.5f));
                    p.Add(Poly(Red, Color.Empty, 0, 19, 8.5f, 23, 12, 19, 15.5f));
                    break;

                case "pan":
                case "pan-grab":
                    Hand(p, name == "pan-grab");
                    break;

                case "orbit":
                    p.Add(Ellipse(Color.FromArgb(205, 225, 250), Ink, 1.1f, 6.5f, 6.5f, 17.5f, 17.5f));        // the model
                    p.Add(Ellipse(Color.Empty, Color.FromArgb(120, 45, 45, 50), 0.9f, 6.5f, 10.2f, 17.5f, 13.8f)); // its equator
                    ArrowArc(p, Red, 12, 12, 10, 200, 340);                                                      // round the bottom
                    ArrowArc(p, Red, 12, 12, 10, 20, 160);                                                       // round the top
                    break;

                case "import":
                case "export":
                    {
                        Color tray = Color.FromArgb(110, 115, 125);
                        p.Add(Poly(Color.FromArgb(225, 230, 238), tray, 1.3f, 3, 14, 6.5f, 14, 8, 17, 16, 17, 17.5f, 14, 21, 14, 21, 21, 3, 21));
                        Color arrow = name == "import" ? Green : Blue;
                        if (name == "import")
                        {
                            p.Add(Line(arrow, 2.6f, 12, 2, 12, 10));
                            p.Add(Poly(arrow, Color.Empty, 0, 7.5f, 9, 16.5f, 9, 12, 14.5f));
                        }
                        else
                        {
                            p.Add(Line(arrow, 2.6f, 12, 14, 12, 6.5f));
                            p.Add(Poly(arrow, Color.Empty, 0, 7.5f, 7.5f, 16.5f, 7.5f, 12, 1.5f));
                        }
                    }
                    break;

                case "render":
                    // A graphics card: board, fan and connector pins.
                    p.Add(Poly(Color.FromArgb(40, 120, 80), Ink, 1.1f, 2, 6, 22, 6, 22, 17, 2, 17));
                    p.Add(Ellipse(Color.FromArgb(70, 75, 85), Color.FromArgb(200, 205, 215), 1f, 10, 7.5f, 19, 15.5f));
                    p.Add(Ellipse(Color.FromArgb(200, 205, 215), Color.Empty, 0, 13.6f, 10.6f, 15.4f, 12.4f));
                    p.Add(Poly(Color.FromArgb(240, 185, 40), Color.Empty, 0, 4, 8.5f, 8, 8.5f, 8, 11, 4, 11));
                    for (int i = 0; i < 5; i++) p.Add(Line(Color.FromArgb(230, 180, 40), 1.4f, 5 + i * 3, 17.5f, 5 + i * 3, 20.5f));
                    break;

                case "undo":
                case "redo":
                    {
                        List<PointF> arc = Arc(12.5f, 13, 6.5f, 205, -40, 14);
                        PointF tip = arc[0];
                        p.Add(new IconPrim { Kind = IconPrimKind.Lines, Points = arc.ToArray(), Stroke = Blue, Width = 2.4f });
                        p.Add(Poly(Blue, Color.Empty, 0, tip.X - 4.5f, tip.Y - 3.5f, tip.X + 3.5f, tip.Y - 2.5f, tip.X - 0.5f, tip.Y + 5f));
                        if (name == "redo") Mirror(p);
                    }
                    break;

                case "view-front": Cube(p, 1); break;
                case "view-top": Cube(p, 0); break;
                case "view-right": Cube(p, 2); break;
                case "view-left": Cube(p, 2); Mirror(p); break;
                case "view-iso": Cube(p, -1); break;

                case "grid":
                    p.Add(Poly(Color.FromArgb(235, 240, 245), Color.FromArgb(110, 110, 120), 1.2f, 3, 3, 21, 3, 21, 21, 3, 21));
                    for (int i = 1; i < 3; i++)
                    {
                        float t = 3 + i * 6;
                        p.Add(Line(Color.FromArgb(110, 110, 120), 1f, t, 3, t, 21));
                        p.Add(Line(Color.FromArgb(110, 110, 120), 1f, 3, t, 21, t));
                    }
                    break;

                case "axes":
                    p.Add(Line(Red, 2f, 7, 17, 22, 17));
                    p.Add(Line(Blue, 2f, 7, 17, 7, 2));
                    p.Add(Line(Green, 2f, 7, 17, 17, 7));
                    p.Add(Line(Color.FromArgb(150, 215, 45, 45), 1f, 7, 17, 2, 17));
                    p.Add(Line(Color.FromArgb(150, 40, 90, 220), 1f, 7, 17, 7, 22));
                    p.Add(Line(Color.FromArgb(150, 40, 160, 70), 1f, 7, 17, 3, 21));
                    break;

                case "color":
                    p.Add(Ellipse(Color.FromArgb(245, 235, 215), Ink, 1.2f, 2, 3, 22, 21));
                    p.Add(Dot(Red, 7.5f, 9, 2.2f));
                    p.Add(Dot(Blue, 12.5f, 7, 2.2f));
                    p.Add(Dot(Green, 17, 10, 2.2f));
                    p.Add(Dot(Color.FromArgb(240, 185, 40), 16.5f, 15.5f, 2.2f));
                    p.Add(Ellipse(Color.White, Ink, 1f, 6, 13, 10.5f, 17.5f));
                    break;

                case "delete":
                    p.Add(Line(Red, 3.2f, 5, 5, 19, 19));
                    p.Add(Line(Red, 3.2f, 19, 5, 5, 19));
                    break;

                case "options":
                    Gear(p);
                    break;
            }
            return p;
        }

        #region Builders

        static IconPrim Line(Color stroke, float width, params float[] xy)
        {
            return new IconPrim { Kind = IconPrimKind.Lines, Points = Pts(xy), Stroke = stroke, Width = width };
        }

        static IconPrim Poly(Color fill, Color stroke, float width, params float[] xy)
        {
            return new IconPrim { Kind = IconPrimKind.Polygon, Points = Pts(xy), Fill = fill, Stroke = stroke, Width = width };
        }

        static IconPrim Ellipse(Color fill, Color stroke, float width, float x1, float y1, float x2, float y2)
        {
            return new IconPrim { Kind = IconPrimKind.Ellipse, Points = Pts(new[] { x1, y1, x2, y2 }), Fill = fill, Stroke = stroke, Width = width };
        }

        static IconPrim Dot(Color fill, float x, float y, float r)
        {
            return Ellipse(fill, Color.Empty, 0, x - r, y - r, x + r, y + r);
        }

        static PointF[] Pts(float[] xy)
        {
            PointF[] pts = new PointF[xy.Length / 2];
            for (int i = 0; i < pts.Length; i++) pts[i] = new PointF(xy[2 * i], xy[2 * i + 1]);
            return pts;
        }

        /// <summary>Points on a circular arc (screen coordinates, angles in degrees, counter-clockwise positive).</summary>
        static List<PointF> Arc(float cx, float cy, float r, float fromDeg, float toDeg, int steps)
        {
            List<PointF> pts = new List<PointF>();
            for (int i = 0; i <= steps; i++)
            {
                double a = (fromDeg + (toDeg - fromDeg) * i / steps) * Math.PI / 180;
                pts.Add(new PointF(cx + r * (float)Math.Cos(a), cy - r * (float)Math.Sin(a)));
            }
            return pts;
        }

        /// <summary>An arc with an arrowhead at its end.</summary>
        static void ArrowArc(List<IconPrim> p, Color c, float cx, float cy, float r, float fromDeg, float toDeg)
        {
            List<PointF> arc = Arc(cx, cy, r, fromDeg, toDeg - 14, 12);
            p.Add(new IconPrim { Kind = IconPrimKind.Lines, Points = arc.ToArray(), Stroke = c, Width = 2f });
            double a = toDeg * Math.PI / 180, b = (toDeg - 14) * Math.PI / 180;
            PointF tip = new PointF(cx + r * (float)Math.Cos(a), cy - r * (float)Math.Sin(a));
            PointF bas = new PointF(cx + r * (float)Math.Cos(b), cy - r * (float)Math.Sin(b));
            float nx = (float)Math.Cos(b) * 3.2f, ny = -(float)Math.Sin(b) * 3.2f;   // Outward normal at the base
            p.Add(Poly(c, Color.Empty, 0, tip.X, tip.Y, bas.X + nx, bas.Y + ny, bas.X - nx, bas.Y - ny));
        }

        static void Mirror(List<IconPrim> prims)
        {
            foreach (IconPrim prim in prims)
            {
                for (int i = 0; i < prim.Points.Length; i++) prim.Points[i].X = 24 - prim.Points[i].X;
                if (prim.Kind == IconPrimKind.Ellipse)
                {
                    PointF a = prim.Points[0], b = prim.Points[1];
                    prim.Points[0] = new PointF(b.X, a.Y);
                    prim.Points[1] = new PointF(a.X, b.Y);
                }
            }
        }

        /// <summary>Isometric cube; face 0 = top, 1 = front, 2 = right side is highlighted (-1 = none).</summary>
        static void Cube(List<IconPrim> p, int highlight)
        {
            Color hi = Color.FromArgb(70, 140, 235);
            Color[] plain = { Color.FromArgb(235, 235, 240), Color.FromArgb(205, 205, 212), Color.FromArgb(170, 170, 180) };
            if (highlight < 0) plain = new[] { Color.FromArgb(150, 195, 245), Color.FromArgb(90, 150, 230), Color.FromArgb(60, 115, 200) };
            p.Add(Poly(highlight == 0 ? hi : plain[0], Ink, 1.1f, 3, 7.5f, 12, 3, 21, 7.5f, 12, 12));
            p.Add(Poly(highlight == 1 ? hi : plain[1], Ink, 1.1f, 3, 7.5f, 12, 12, 12, 22, 3, 17.5f));
            p.Add(Poly(highlight == 2 ? hi : plain[2], Ink, 1.1f, 12, 12, 21, 7.5f, 21, 17.5f, 12, 22));
        }

        static void Hand(List<IconPrim> p, bool grab)
        {
            // Fingers as thick round-capped strokes: a dark outline stroke under a skin-coloured one.
            float[][] fingers = grab
                ? new[] { new float[] { 7.5f, 11, 7.5f, 9 }, new float[] { 10.5f, 10, 10.5f, 7.5f }, new float[] { 13.5f, 10, 13.5f, 7.5f }, new float[] { 16.5f, 11, 16.5f, 9 } }
                : new[] { new float[] { 7.5f, 12, 7.5f, 5 }, new float[] { 10.5f, 11, 10.5f, 2.5f }, new float[] { 13.5f, 11, 13.5f, 3 }, new float[] { 16.5f, 12, 16.5f, 5.5f } };
            foreach (float[] f in fingers) p.Add(Line(Ink, 3.9f, f));
            p.Add(Line(Ink, 3.9f, 6, 16.5f, grab ? 4.5f : 3.2f, grab ? 13.5f : 11.5f));                    // thumb outline
            p.Add(Poly(Skin, Ink, 1.1f, 5.6f, 11.5f, 18.4f, 11.5f, 18.4f, 16, 16, 21.5f, 8.5f, 21.5f, 4.8f, 16.5f));
            foreach (float[] f in fingers) p.Add(Line(Skin, 2.2f, f));
            p.Add(Line(Skin, 2.2f, 6, 16.5f, grab ? 4.5f : 3.2f, grab ? 13.5f : 11.5f));
        }

        static void Gear(List<IconPrim> p)
        {
            List<float> xy = new List<float>();
            int teeth = 8;
            for (int i = 0; i < teeth * 2; i++)
            {
                double a0 = Math.PI * 2 * i / (teeth * 2) - 0.12, a1 = a0 + 0.24 + Math.PI * 2 / (teeth * 2) - 0.24;
                float r = (i % 2 == 0) ? 10 : 7.2f;
                xy.Add(12 + r * (float)Math.Cos(a0)); xy.Add(12 + r * (float)Math.Sin(a0));
                xy.Add(12 + r * (float)Math.Cos(a1)); xy.Add(12 + r * (float)Math.Sin(a1));
            }
            p.Add(Poly(Color.FromArgb(150, 155, 165), Ink, 1.1f, xy.ToArray()));
            p.Add(Ellipse(Color.FromArgb(245, 245, 248), Ink, 1.1f, 8.5f, 8.5f, 15.5f, 15.5f));
        }

        #endregion
    }
}
