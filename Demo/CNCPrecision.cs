using Lomont.SimpleShapes;
using System;
using System.Runtime.InteropServices;
using Lomont.Graphics;
using Lomont.SimpleShapes.Shape2D;
using static Lomont.SimpleShapes.SimpleShape2D;
using System.Drawing;
using Lomont.Numerical;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "CNC Precision Test for Shaper Origin", "9/22/2022")]

    public class CNCPrecision
    {
        // todo - move to some central place
        static class ShaperOriginColors
        {
            public record Style(ColorB Fill, ColorB Stroke);

            public static Style InteriorCut = new(White, Black);
            public static Style ExteriorCut = new(Black, Black);
            public static Style OnLineCut = new(White, Gray);
            public static Style PocketingCut = new(Gray, Gray);
            public static Style Guide = new(Blue, Blue);

        }

        /// <summary>
        /// Make CNC Precision test piece
        /// Chris Lomont 9/22/2022
        /// </summary>
        public void Run()
        {
            // todo integrate single line engraving fonts from 
            // http://imajeenyus.com/computer/20150110_single_line_fonts/index.shtml

            var inc = ShaperOriginColors.InteriorCut;
            var ouc = ShaperOriginColors.ExteriorCut;
            var tc = ShaperOriginColors.OnLineCut; // text cut
            var fontName = "OrachTechDemo2Lttf"; // todo - engraving font
            var fontSize = 10.0;
            const double fontHeight = 25.4 / 6;
            const double fontHeight2 = 25.4 / 4;

            var diam = 25.4; // piece feature size in mm = 1"
            var d8 = 25.4 / 8; // 1/8"
            var d4 = 25.4 / 4; // 1/4"
            var d2 = 25.4 / 2; // 1/2"

            var x0 = diam * 0.6;
            var y0 = diam * 1.5;
            //var (dx0,dy0) = (diam+2.5*d4,diam+2.5*d4);

            // outer edge
            var stock = 6.0; // inches
            var (rx, ry) = (stock * 25.4, stock * 25.4);
            var outer = Rect(0, 0, rx, ry)
                .Stroke(inc.Stroke)
                .Fill(inc.Fill);


            Node Txt(double x, double y, string txt, double ht = fontHeight)
            {
                return Text(x, y, 0, ht, txt, fontName: fontName)
                    .Stroke(tc.Stroke)
                    .Stroke(0.1)
                    .Fill(tc.Fill);
            }


            Node Fun1(double drad, double y0, string txt, bool cuts = true, double dd = 0)
            {
                var c8i = Circle(x0 + diam / 2, y0 + diam / 2 - dd, diam / 2)
                    .Stroke(inc.Stroke)
                    .Fill(inc.Fill);

                var (x1, y1) = (x0 + diam + d4, y0 - dd);
                var r8i = Rect(x1, y1, x1 + diam, y1 + diam, drad, drad)
                    .Stroke(inc.Stroke)
                    .Fill(inc.Fill);

                var (x2, y2) = (x1 + diam + d2, y0);
                var r8o2 = Rect(x2, y2, x2 + diam, y2 + diam)
                    .Stroke(inc.Stroke)
                    .Fill(inc.Stroke);

                var r8o = Rect(x2 - drad, y2 - drad, x2 + diam + drad, y2 + diam + drad, drad, drad)
                    .Stroke(inc.Stroke);

                var t8 = Txt(x0 - 10, y0 + diam / 2 - fontHeight / 2 - dd, txt);

                // cut types
                Node tt;
                if (cuts)
                {
                    var dt = d4 * 2;
                    var tx = d8;
                    var t1 = Txt(x0 + tx, y0 - dt, " Inside 1\"");
                    var t2 = Txt(x1 + tx, y1 - dt, " Inside 1\"");
                    var t3 = Txt(x2 + tx, y2 - dt, "Outside 1\"");
                    tt = Group(t1, t2, t3, t8);
                }
                else
                    tt = t8;

                return Group(c8i, r8i, r8o, r8o2, tt);
            }

            // 1/8" cuts
            var f8 = Fun1(d8, y0, "1/8\"", true);
            // 1/4" cuts
            var f4 = Fun1(d4, y0 + diam + d2 + d8, "1/4\"", false, diam / 2 - d8);

            // engraving
            Node Eng(string txt, double x, double y)
            {
                return Txt(x, y, txt);
            }

            var ex = 1 * diam;
            var ey = 4.5 * diam - d4;
            var dex = diam;
            var dey = 8;
            var eng = Group(
                Eng("Engraving", ex, ey - dey)
            );
            var engSizes = new[] {"0.005", "0.008", "0.010", "0.012", "0.015", "0.020", "0.025", "0.030"};
            var k = 0;
            for (var i = 0; i < 2; ++i)
            for (var j = 0; j < 4; ++j)
            {
                if (k < engSizes.Length)
                    eng.Children.Add(Eng(engSizes[k++] + '\"', ex + i * dex, ey + j * dey));
            }



            // depths
            Node Depth(string txt, double x, double y)
            {
                var size = 25.4 / 2; // 1/2" inch
                var fh = fontHeight * 0.75;
                var d = Group(
                        Rect(0, 0, d8, size),
                        Rect(d4, 0, size + d8 * 2, size, d8, d8),
                        Txt(d8 * 2 + size + d4 / 3, size / 2 - fh / 2, txt, ht: fh)
                    )
                    .Stroke(inc.Stroke)
                    .Fill(inc.Fill);
                return Translate(x, y, d);
            }

            var (px, py) = (5 * diam - d2 + d4, 1 * diam);
            var (dpx, dpy) = (diam + diam / 2, diam / 2 + d8);
            var depths = Group(
                // depths
                Txt(px, py - d4, "Depths x 1/2\"")
            );

            var depSizes = new[] {"0.02", "0.05", "0.10", "1/8", "0.20", "1/4", "1/2"};
            k = 0;
            for (var i = 0; i < 1; ++i)
            for (var j = 0; j < 8; ++j)
            {
                if (k < depSizes.Length)
                    depths.Children.Add(Depth(depSizes[k++] + '\"', px + i * dpx, py + j * dpy));
            }


            // misc text
            var (tx, ty) = (10, 6 * diam);
            var (dtx, dty) = (0, fontHeight * 1.8);

            var text = Group(
                // top text
                Txt(diam, d4, "Shaper Origin CNC Precision Test", fontHeight2),


                Txt(diam * 0.75, ry - d2, "Chris Lomont, www.Lomont.org", fontHeight),
                Txt(diam * 0.75, ry - d2 + fontHeight, "Sept 2022, v0.1"),

                Txt(diam * 4 - d8 - d2, ry - fontHeight - d4, $"{stock}\"x{stock}\" stock")
            );

            // todo
            // engraving lines

            // corner holes
            var cr = d8;
            var cx = 3 * d4 / 2;
            var corners = Group(
                Circle(cx, cx, cr),
                Circle(cx, rx - cx, cr),
                Circle(rx - cx, cx, cr),
                Circle(rx - cx, rx - cx, cr)
            );

            Node Lissa(double x, double y, double A, double B, double a, double b, double d)
            {
                var pts = new List<Vec2>();

                var steps = 500;
                var loops = 1;
                for (var i = 0; i <= steps; ++i)
                {
                    var t = Math.PI * 2 * i / steps * loops;
                    var p = new Vec2(
                        x + A * Math.Sin(a * t + d),
                        y + B * Math.Sin(b * t)
                    );
                    pts.Add(p);
                }

                return Path(pts)
                    .Stroke(ShaperOriginColors.OnLineCut.Stroke)
                    .Stroke(0.3);
            }


            var (lx, ly) = (diam * 4 - d8, ry - diam);
            var lr = d2;
            var lissa = Group(
                Lissa(lx, ly, lr, lr, 5, 6, Math.PI / 2)
            );

            var final = Group(
                outer,
                text,
                f8, f4,
                eng,
                depths,
                corners,
                lissa
            );




            final.Save("ShaperOriginPrecisionTest.svg");
        }
    }
}
