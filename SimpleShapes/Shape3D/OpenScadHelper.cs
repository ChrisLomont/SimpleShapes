using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lomont.Graphics;
using Lomont.Numerical;
using Svg;
using Boolean = Lomont.SimpleShapes.Shape3D.Boolean;
using Path = Lomont.SimpleShapes.Shape3D.Path; //using System.Drawing;

namespace Lomont.SimpleShapes.Shape3D
{
    /// <summary>
    /// Wrap useful OpenSCAD functions
    /// </summary>
    static class OpenScadHelper
    {
#if false
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
                            var p2 = new Vector2D(p1.X, p3.Y);
                            var p4 = new Vector2D(p3.X, p1.Y);
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

        static PointF ToSvgPt(Vector2D pt) => new PointF((float)pt.X, (float)pt.Y);


        static void SetStyle<T>(T item, Style style) where T : SvgPathBasedElement
        {
            item.Stroke = style.StrokeColor.Alpha == 0 ? SvgPaintServer.None :
                new SvgColourServer(
                System.Drawing.Color.FromArgb(255,
                    style.StrokeColor.Red,
                    style.StrokeColor.Green, style.StrokeColor.Blue)
                );
            item.StrokeWidth = new SvgUnit((float)style.StrokeThickness);
            item.StrokeLineCap = SvgStrokeLineCap.Round;
            item.StrokeLineJoin = SvgStrokeLineJoin.Round;
            item.Fill = style.FillColor.Alpha == 0 ? SvgPaintServer.None :
            new SvgColourServer(
                System.Drawing.Color.FromArgb(255,
                style.FillColor.Red,
                style.FillColor.Green,
                style.FillColor.Blue)
            );
        }
#endif

        /// <summary>
        /// Save node (and all transformed descendants) to OpenSCAD
        /// </summary>
        /// <param name="node"></param>
        /// <param name="filename"></param>
        public static void Save(Node node, string filename)
        {
            using var fs = File.CreateText(filename);
            fs.WriteLine($"// File auto generated by Chris Lomont 3d tools on {DateTime.Now}");
            WriteNode(0, new Mat4(), fs,node);
            fs.WriteLine("// end of file");
        }

        static void WriteNode(int indent, Mat4 transform, TextWriter w, Node node)
        {
            // todo - make indenting nicer someday
            var ind = new string(' ',indent*3);
            var childTransform = transform * node.Transform; // apply to here, and all children
            WriteTransform(childTransform);

            switch (node)
            {
                case Sphere s:
                {
                    DoColor(s.FillColor);
                    var cc = s.Center;
                    w.WriteLine($"{ind}translate([{cc}])");
                    w.WriteLine($"sphere(r={s.Radius}, $fn={s.U});");
                }
                    break;
                case Cube c:
                {
                        DoColor(c.FillColor);
                        var d = c.Max - c.Min;
                        w.WriteLine($"{ind}translate([{c.Min.X},{c.Min.Y},{c.Min.Z}])");
                        w.WriteLine($"{ind}{ind}cube(size = [{d.X},{d.Y},{d.Z}], center = false);");
                } break;
                case Cylinder c:
                {
                    DoColor(c.FillColor);
                    w.WriteLine($"{ind}{ind}cylinder(h={c.Height},r1={c.Radius1},r2={c.Radius2},center=false,$fn={c.Sides});");
                }
                    break;
                case Polyhedron h:
                {
                    DoColor(h.FillColor);
                    w.Write($"{ind}polyhedron( points = [");
                    var pts = h.Points;
                    for (var i = 0; i < pts.Count; i++)
                    {
                        var p = pts[i];
                        w.Write($"[{p.X}, {p.Y}, {p.Z}]");
                        if (i < pts.Count - 1)
                            w.Write(",");
                        w.Write(' ');
                    }

                    w.Write("], faces = [ ");
                    for (var j = 0; j < h.Faces.Count; ++j)
                    {
                        var f = h.Faces[j];
                        w.Write("[");
                        for (var i = 0; i < f.Count; i++)
                        {
                            w.Write(f[i]);
                            if (i < f.Count - 1)
                                w.Write(",");
                            w.Write(' ');
                        }

                        w.Write("]");
                        if (j < h.Faces.Count - 1)
                            w.Write(",");
                        w.Write(' ');
                    }

                    w.WriteLine(" ], convexity = 10);");

                }
                    break;
                case Path p:
                {
                    DoColor(p.FillColor);
                    w.Write($"polygon([");
                    // todo - doesn't handle holes, etc...
                    var firstPass = true;
                    foreach (var c in p.Contours)
                    foreach (var pt in c.Points)
                    {
                        Trace.Assert(pt.Z == 0, "Polygon not planar");
                        if (!firstPass)
                            w.Write(",");
                        w.Write($"[{pt.X},{pt.Y}]");
                        firstPass = false;
                    }
                    w.WriteLine("]);");
                }
                    break;
                case LinearExtrude e:
                {
                    DoColor(e.FillColor);
                    w.WriteLine($"linear_extrude(height={e.Height},center=false){{");
                    WriteNode(indent+1,/*childTransform*/Mat4.Identity,w,e.Path);
                    w.WriteLine("}");
                } break;
                case Polygon poly:
                    DoColor(poly.FillColor);
                    var (p0, p1, p2) = (poly.Points[0], poly.Points[1], poly.Points[2]);
                    var (m,n) = Mat4.MapFrame(p0,p1,p2);// maps polygon in 3D to xy plane
                    WriteTransform(n);
                    w.Write("polygon(points=[");
                    var first = true;
                    foreach (var pt in poly.Points.Select(p => m * p))
                    {
                        Debug.Assert(Math.Abs(pt.Z)<0.001);
                        if (!first)
                            w.Write(",");
                        first = false;
                        w.Write($"[{pt.X},{pt.Y}]");
                    }
                    w.WriteLine("]);");
                    break;
                case Boolean {Op: Boolean.Type.Xor} b:
                { // XOR is union (A/B, B/A)
                    w.WriteLine($"union(){{");
                    
                    // diff A/B
                    w.WriteLine($"difference(){{");
                    foreach (var c in b.Nodes)
                        WriteNode(indent + 1, childTransform, w, c);
                    w.WriteLine("}"); // difference

                    // diff B/A 
                    w.WriteLine($"difference(){{");
                        // - note B may be many items, so union them
                        w.WriteLine($"union(){{");
                        for (var i = 1; i < b.Nodes.Length; ++i)
                        {
                            WriteNode(indent + 1, childTransform, w, b.Nodes[i]);
                        }
                        w.WriteLine("}"); // union


                        WriteNode(indent + 1, childTransform, w, b.Nodes[0]); // A
                        w.WriteLine("}"); // difference


                        w.WriteLine("}"); // close union
                    }
                    break;
                case Boolean b:
                {
                    var ch = b.Op switch
                    {
                        Boolean.Type.Union => "union",
                        Boolean.Type.Intersection => "intersection",
                        Boolean.Type.Difference => "difference",
                        // todo - implement type XOR
                        _ => throw new NotImplementedException($"Unknown boolean {b.Op}")
                    };
                    w.WriteLine($"{ch}(){{");
                    foreach (var c in b.Nodes)
                        WriteNode(indent+1,Mat4.Identity, w, c);
                    w.WriteLine("}");
                }
                    break;
                case Group g:
                {
                    w.WriteLine("{");
                    foreach (var c in g.Children)
                        WriteNode(indent + 1, Mat4.Identity, w, c);
                    w.WriteLine("}");
                    }
                    break;


                default:
                    throw new NotImplementedException($"OpenSCAD unimplemented switch on type {node.GetType().Name}");
            }

            void WriteTransform(Mat4 transform)
            {
                if (transform != Mat4.Identity)
                {
                    w.Write("multmatrix(m=[");
                    for (var i = 0; i < 4; ++i)
                    {
                        w.Write("[");
                        for (var j = 0; j < 4; ++j)
                            w.Write(transform[i, j] + (j != 3 ? "," : ""));
                        w.Write("]");
                        if (i != 3)
                            w.Write(",");
                    }
                    w.WriteLine("])");
                }
            }

        void DoColor(ColorB color)
            {
                if (color != null)
                    w.WriteLine($"color([{color.Red/255.0},{color.Green/255.0},{color.Blue/255.0}])");

            }

        }

    }
}
