using System;
using System.Drawing;
using System.IO;
using Lomont.Numerical;
using Svg; // uses nuget package Svg 3.4.0 (or later?)
using Svg.Pathing;

namespace Lomont.SimpleShapes.Shape2D
{
    /// <summary>
    /// Wrap useful SVG functions
    /// </summary>
    static class SvgHelper
    {
        public static SvgDocument ToDoc(Node node, SvgUnitType units = SvgUnitType.Millimeter)
        {
            // 2D bounds
            var bounds = node.Bounds();

            var doc = new SvgDocument
            {
                // coords of view in user space
                ViewBox = new SvgViewBox(
                (float)bounds.Min.X - 10, (float)bounds.Min.Y - 10, // min
                (float)bounds.Width + 20, (float)bounds.Height + 20 // width, height
                )
            };

            // map this to mm
            doc.Width = new SvgUnit(units, doc.ViewBox.Width);
            doc.Height = new SvgUnit(units, doc.ViewBox.Height);

            //doc.Ppi = 96; // pixels per inch
            //doc.AspectRatio = 1.0;
            // walk all nodes
            node.Walk((n, t, s) =>
            {
                // todo - check unsupported things don't reach here
                // Trace.Assert(n.Lines.Count == 0);
                switch (n)
                {
                    case Circle c:
                        var sc = new SvgCircle();
                        var pt = t * c.Center;
                        sc.CenterX = new SvgUnit((float)pt.X);
                        sc.CenterY = new SvgUnit((float)pt.Y);
                        sc.Radius = (float)c.Radius;
                        SetStyle(sc, s);
                        doc.Children.Add(sc);
                        break;
                    case Line ln:
                        {
                            var sln = new SvgLine();
                            var p1 = t * ln.P1;
                            var p2 = t * ln.P2;
                            sln.StartX = (float)p1.X;
                            sln.StartY = (float)p1.Y;
                            sln.EndX = (float)p2.X;
                            sln.EndY = (float)p2.Y;
                            SetStyle(sln, s);
                            doc.Children.Add(sln);
                        }
                        break;
                    case Rect r:
                        {
                            // we must rotate our rects as shapes
#if false
                            var sr = new SvgRectangle();
                        var p1 = t * r.P1;
                        var p2 = t * r.P2;
                        sr.X = (float)Math.Min(p1.X, p2.X);
                        sr.Y = (float)Math.Min(p1.Y,p2.Y);
                        sr.Width = (float)Math.Abs(p1.X-p2.X);
                        sr.Height = (float)Math.Abs(p1.Y-p2.Y);
                        sr.CornerRadiusX = (float) r.Rx;
                        sr.CornerRadiusY = (float)r.Ry;
                        SetStyle(sr, s);
                        doc.Children.Add(sr);
#else
                            var sp = new SvgPath();
                            var sv = new SvgPathSegmentList();
                            var p1 = r.P1;
                            var p3 = r.P2;
                            var p2 = new Vec2(p1.X, p3.Y);
                            var p4 = new Vec2(p3.X, p1.Y);
                            p1 = t * p1;
                            p2 = t * p2;
                            p3 = t * p3;
                            p4 = t * p4;

                            sv.Add(new SvgMoveToSegment(ToSvgPt(p1)));
                            sv.Add(new SvgLineSegment(ToSvgPt(p1), ToSvgPt(p2)));
                            sv.Add(new SvgLineSegment(ToSvgPt(p2), ToSvgPt(p3)));
                            sv.Add(new SvgLineSegment(ToSvgPt(p3), ToSvgPt(p4)));
                            sv.Add(new SvgClosePathSegment());

                            sp.PathData = sv;

                            // todo - styles! on more pieces
                            SetStyle(sp, s);
                            doc.Children.Add(sp);

#endif
                        }
                        break;
                    case Path p:
                        {
                            var sp = new SvgPath();
                            var sv = new SvgPathSegmentList();
                            foreach (var c in p.Contours)
                            {
                                var pts = c.Points;
                                sv.Add(new SvgMoveToSegment(ToSvgPt(t * pts[0])));
                                for (var i = 0; i < pts.Count - 1; ++i)
                                {
                                    var p1 = t * pts[i];
                                    var p2 = t * pts[i + 1];
                                    sv.Add(new SvgLineSegment(
                                        new PointF((float)p1.X, (float)p1.Y),
                                        new PointF((float)p2.X, (float)p2.Y)
                                    ));
                                }

                                if (c.IsClosed)
                                    sv.Add(new SvgClosePathSegment());
                            }

                            sp.PathData = sv;

                            // todo - styles! on more pieces
                            SetStyle(sp, s);
                            doc.Children.Add(sp);
                        }
                        /*
                        sp.PathData.
                        sp.
                        var p1 = t * r.P1;
                        var p2 = t * r.P2;
                        sr.X = (float)Math.Min(p1.X, p2.X);
                        sr.Y = (float)Math.Min(p1.Y, p2.Y);
                        sr.Width = (float)Math.Abs(p1.X - p2.X);
                        sr.Height = (float)Math.Abs(p1.Y - p2.Y);
                        sr.CornerRadiusX = (float)r.Rx;
                        sr.CornerRadiusY = (float)r.Ry;
                        SetStyle(sr, s);
                        doc.Children.Add(sr);
                        */
                        break;
                    case Group g: break;
                    default:
                        throw new NotImplementedException($"SVG unimplemented switch on type {n.GetType().Name}");
                }
                /*
                                foreach (var polygon in n.Polygons)
                                {
                                    foreach (var contour in polygon.Contours)
                                    {
                                        for (var i =0; i < contour.Count; ++i)
                                        {
                                            var p1 = contour[i];
                                            var p2 = contour[(i+1)%contour.Count];
                                            var line = new SvgLine
                                            {
                                                StartX = new SvgUnit((float) (p1.Coordinate.X)),
                                                StartY = new SvgUnit((float) (p1.Coordinate.Y)),
                                                EndX = new SvgUnit((float) (p2.Coordinate.X)),
                                                EndY = new SvgUnit((float) (p2.Coordinate.Y)),

                                                // todo - style from object
                                                Stroke = new SvgColourServer(System.Drawing.Color.Black),
                                                StrokeWidth = new SvgUnit(1.0f),
                                                StrokeLineCap = SvgStrokeLineCap.Round,
                                                StrokeLineJoin = SvgStrokeLineJoin.Round
                                                // line.Fill = 
                                                // line.
                                                // line.ID // should be unique
                                                // line.CustomAttributes // todo
                                            };
                                            doc.Children.Add(line);
                                        }
                                    }
                                }
                */

            },
                Style.Default);

            return doc;
        }

        static PointF ToSvgPt(Vec2 pt) => new PointF((float)pt.X, (float)pt.Y);


        static void SetStyle<T>(T item, Style style) where T : SvgPathBasedElement
        {
            item.Stroke = style.StrokeColor.Alpha == 0 ? SvgPaintServer.None :
                new SvgColourServer(
                Color.FromArgb(255,
                    style.StrokeColor.Red,
                    style.StrokeColor.Green, style.StrokeColor.Blue)
                );
            item.StrokeWidth = new SvgUnit((float)style.StrokeThickness);
            item.StrokeLineCap = SvgStrokeLineCap.Round;
            item.StrokeLineJoin = SvgStrokeLineJoin.Round;
            item.Fill = style.FillColor.Alpha == 0 ? SvgPaintServer.None :
            new SvgColourServer(
                Color.FromArgb(255,
                style.FillColor.Red,
                style.FillColor.Green,
                style.FillColor.Blue)
            );
        }

        /// <summary>
        /// Save node (and all transformed descendants) to SVG
        /// </summary>
        /// <param name="node"></param>
        /// <param name="filename"></param>
        public static void SaveSvg(Node node, string filename, SvgUnitType units = SvgUnitType.Millimeter)
        {
            var doc = ToDoc(node, units);
            using var fs = File.Create(filename);
            doc.Write(fs);
        }
        /// <summary>
        /// Save node (and all transformed descendants) to PNG
        /// </summary>
        /// <param name="node"></param>
        /// <param name="filename"></param>
        public static void SavePng(Node node, string filename)
        {
            var doc = ToDoc(node);
            using var bitmap = doc.Draw();
            bitmap.Save(filename);
        }
    }
}
