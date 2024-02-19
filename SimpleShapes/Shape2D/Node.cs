using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using Lomont.Numerical;
using FontFamily = System.Drawing.FontFamily;

namespace Lomont.SimpleShapes.Shape2D
{

    public interface IGetPoints
    {
        public IEnumerable<Vec2> GetPoints();
    }

    /// <summary>
    /// General shape 2d node
    /// Derived from Style
    /// </summary>
    public class Node : Style
    {

        /// <summary>
        /// Walk nodes, returning a composed transform and node at each level
        /// </summary>
        public void Walk(Action<Node, Mat3, Style> action, Style startStyle)
        {
            Helper(this, new Mat3(), startStyle);

            void Helper(Node node, Mat3 parentTransform, Style parentStyle)
            {
                var thisStyle = parentStyle?.Append(node);

                var thisTransform = parentTransform * node.Transform;

                // act on self
                action(node, thisTransform, thisStyle);


                if (node is Group g)
                {
                    foreach (var c in g.Children)
                        Helper(c, thisTransform, thisStyle);
                }
            }
        }

        /// <summary>
        /// Bounds of node and all transformed children
        /// </summary>
        /// <returns></returns>
        public BoundingBox Bounds()
        {
            var b = new BoundingBox();
            Walk((n, t, _) =>
                {
                    if (n is IGetPoints pts)
                    {
                        foreach (var p in pts.GetPoints())
                            b.Add(t * p);
                    }
                },
                null // don't care about styles
            );
            return b;
        }

        /// <summary>
        /// Transform to apply to all children
        /// </summary>
        public Mat3 Transform { get; set; } = new Mat3();

        /// <summary>
        /// Save node and below to SVG or PNG
        /// </summary>
        /// <param name="filename"></param>
        public void Save(string filename)
        {
            var ext = System.IO.Path.GetExtension(filename).ToUpper();
            switch (ext)
            {
                //case "png": break;
                case ".SVG":
                    SvgHelper.SaveSvg(this, filename);
                    break;
                case ".PNG":
                    SvgHelper.SavePng(this, filename);
                    break;
                default:
                    throw new NotImplementedException($"Unknown extension {ext} in Node.Save");
            }
        }
    }

    // child classes?
    // Circle, Group, Ellipse, Polygon, Polyline, Line, Rect, Text
    public class Ellipse : Node, IGetPoints
    {
        public Ellipse(Vec2 center, double a, double b)
        {

        }

        public IEnumerable<Vec2> GetPoints()
        {
            throw new NotImplementedException("");
        }


    }

    public class Circle : Node, IGetPoints
    {
        public Vec2 Center;
        public double Radius;

        public Circle(double cx, double cy, double radius)
        {
            Center = new Vec2(cx, cy);
            Radius = radius;
        }

        public IEnumerable<Vec2> GetPoints()
        {
            int sides = 100; // todo - from style, else default somehow?
            for (var i = 0; i < sides; ++i)
            {
                var angle = i * Math.PI * 2 / sides; // - angle for orientation
                var (c, s) = (Math.Cos(angle), Math.Sin(angle));

                yield return Center + Radius * new Vec2(c, s);
            }
        }
    }

    public class Line : Node, IGetPoints
    {
        public Vec2 P1, P2;

        public Line(Vec2 p1, Vec2 p2)
        {
            P1 = p1;
            P2 = p2;

        }

        public IEnumerable<Vec2> GetPoints()
        {
            yield return P1;
            yield return P2;
        }




    }

    public class Polyline : Node, IGetPoints
    {
        public List<Vec2> Points { get; } = new List<Vec2>();
        public bool IsClosed { get; set; }

        public Polyline Closed(bool closed = true)
        {

            IsClosed = closed;
            return this;
        }

        public IEnumerable<Vec2> GetPoints()
        {
            foreach (var p in Points)
                yield return p;
            if (IsClosed && Points.Count > 0)
                yield return Points[0];
        }

    }

    public class Rect : Node, IGetPoints
    {
        public Vec2 P1 { get; set; }
        public Vec2 P2 { get; set; }
        public double Rx { get; set; }
        public double Ry { get; set; }

        public Rect(Vec2 p1, Vec2 p2, double rx, double ry)
        {
            P1 = p1;
            P2 = p2;
            Rx = rx;
            Ry = ry;
        }

        // todo - rounded corners?
        public IEnumerable<Vec2> GetPoints()
        {
            yield return P1;
            yield return new Vec2(P1.X, P2.Y);
            yield return P2;
            yield return new Vec2(P2.X, P1.Y);
        }

    }

    public class Group : Node
    {
        /// <summary>
        /// Children of this node
        /// </summary>
        public List<Node> Children { get; } = new List<Node>();

        /// <summary>
        /// add child node(s)
        /// </summary>
        /// <param name="nodes"></param>
        public void Add(params Node[] nodes)
        {
            Children.AddRange(nodes);
        }

    }

    /// <summary>
    /// Collection of line segments
    /// todo - add more types later
    /// </summary>
    public class Contour : Style
    {
        public List<Vec2> Points { get; } = new List<Vec2>();
        public Contour(params Vec2[] points)
        {
            Points.AddRange(points);
        }

        public bool IsClosed { get; set; }

        public Contour Open()
        {
            IsClosed = false;
            return this;
        }
        public Contour Closed()
        {
            IsClosed = true;
            return this;
        }
    }
    public class Path : Node, IGetPoints
    {
        public List<Contour> Contours { get; } = new List<Contour>();
        public Path(params Vec2[] points) : this(new Contour(points))
        {

        }
        public Path(params Contour[] contours)
        {
            Contours.AddRange(contours);
        }

        public IEnumerable<Vec2> GetPoints()
        {
            foreach (var c in Contours)
                foreach (var pt in c.Points)
                {
                    yield return pt;
                }
        }

        public Path()
        {

        }

        public Path Open()
        {
            foreach (var c in Contours)
                c.Open();
            return this;
        }
        public Path Closed()
        {
            foreach (var c in Contours)
                c.Closed();
            return this;
        }

    }

    public class Text : Path
    {
        public Vec2 Pt;
        public Vec2 Size;
        public string Message;
        public double Scaling; // scaling used

        /// <summary>
        /// Render text to fix in box w,h, start x,y upper left
        /// If w or h is zero, ignore that dimension
        /// </summary>
        /// <param name="x">Left position</param>
        /// <param name="y">Right top position</param>
        /// <param name="w">Width, 0 for size as needed</param>
        /// <param name="h">Height, 0 for size as needed</param>
        /// <param name="text">Text to write</param>
        /// <param name="fontName">optional font name</param>
        /// <param name="minFeatureSize">smallest feature to subdivide to, 0 for default</param>
        /// <param name="fontSize">font size to use if w and h both 0</param>
        public Text(double x, double y, double w, double h, string text, string fontName = "", double minFeatureSize = 0.0, double fontSize = 0.0)
        {
            Pt = new Vec2(x, y);
            Size = new Vec2(w, h);
            Message = text;
            InitPoints(
                string.IsNullOrEmpty(fontName) ? "Consolas" : fontName,
                minFeatureSize,
                fontSize
                );
        }

        void InitPoints(string fontName, double minFeatureSize, double fontSize)
        {
            if (minFeatureSize == 0.0)
                minFeatureSize = 0.25; // default
            // todo - use font size and min feature size
            var fontsize = 30.0;
            var ff = new FontFamily(fontName);
            var gp = new GraphicsPath();
            gp.AddString(
                Message,
                ff,
                (int)FontStyle.Regular,
                (float)(96.0 * fontsize / 72.0), // em size
                new Point(0, 0), // draw here
                new StringFormat() // options, like center alignment
            );

            // this flattens out the bezier curves
            gp.Flatten(new System.Drawing.Drawing2D.Matrix(), flatness: (float)minFeatureSize);

            // gp now has outline
            int line = 0, bezier = 0; // current count for an item
            var c = gp.PointCount;
            Contour contour = null; // current contour
            Vec2 lastPoint = new Vec2();
            var bezierPtrs = new Vec2[4];
            for (var i = 0; i < c; ++i)
            {
                var pt = gp.PathPoints[i];
                var tp = gp.PathTypes[i];
                var nextPoint = new Vec2(pt.X, pt.Y);
                switch (tp & 7)
                {
                    case 0: // start point

                        // todo - figure out bezier points
                        // need 4 per curve, perhaps each start uses last point if one exists
                        //Debug.Assert((bezier % 4) == 0);
                        //bezier = 0;

                        contour = new Contour();
                        contour.Points.Add(nextPoint);
                        break;
                    case 1: // line end point
                        contour.Points.Add(nextPoint);
                        line++;
                        break;
                    case 3: // cubic bezier control point
                        // cubic bezier is 4 control points
                        contour.Points.Add(nextPoint); // todo - cubic
                        bezier++;
                        break;
                }
                var end = (tp & 0x80) == 0x80;
                var marker = (tp & 0x20) == 0x20;
                if (end)
                {
                    if (contour.Points.Count > 0)
                        Contours.Add(contour);
                    contour = null;
                }

                lastPoint = nextPoint;

                // todo - implement bezier for better fonts

                /* types:
                 3 = cubic bezier control pt
                 0 = start point
                 1 = line endpoint
                 163?= 0xA3
                 131 = 0x83
                 129 = 0x81
                 161 = 0xA1

                flags:
                0x20 - point is a marker
                0x80 - last point in closed subpath        
                
                // cubic bezier
                B(t) = (1-t)^3P0 + 3(1-t)^2 t P1 + 3(1-t)t^2 P2 + t^3 P3, 0 <= t <= 1
                 */


                Debug.WriteLine($"{i}: {pt} {tp}");
            }
            if (contour != null && contour.Points.Count > 0)
                Contours.Add(contour); // add last one
            Closed(); // all contours closed

            var bb = Bounds();
            var del = bb.Max - bb.Min; // todo - deconstructor
            var (curW, curH) = (del.X, del.Y);
            var scaleX = Size.X > 0 ? Size.X / curW : double.MaxValue;
            var scaleY = Size.Y > 0 ? Size.Y / curH : double.MaxValue;
            var scale = Math.Min(scaleX, scaleY);

            Scaling = 1.0; // current scale
            PositionPoints(scale); // scale and move
        }


        // rescale points
        public void PositionPoints(double rescale)
        { // scale all points
            var bb = Bounds();

            foreach (var c in Contours)
            {
                for (var i = 0; i < c.Points.Count; ++i)
                    c.Points[i] = rescale * (c.Points[i] - bb.Min) + Pt;
            }
        }


    }

}
