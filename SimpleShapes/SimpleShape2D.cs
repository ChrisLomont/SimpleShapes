using System;
using System.Collections.Generic;
using System.Linq;
using Lomont.Graphics;
using Lomont.Numerical;
using Lomont.SimpleShapes.Shape2D;
using Path = Lomont.SimpleShapes.Shape2D.Path;

namespace Lomont.SimpleShapes
{

    /// <summary>
    /// Static class to clean up usage syntax
    /// </summary>
    public static class SimpleShape2D
    {
        public static Vec2 Point(Vec2 p) => new(p.X, p.Y);
        public static Vec2 Point(double x = 0, double y = 0) => new(x, y);
        public static Vec2 Dir(double angle) => new(Math.Cos(angle), Math.Sin(angle));

        /// <summary>
        /// Direction rotated 90 degrees ccw from dir
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Vec2 Perp(Vec2 dir) => Point(-dir.Y, dir.X);


        /* TODO
         * Helpers:
         * 1. Arc connections to ends of things - auto tangent, etc..
         * 2. things to help align items - creates transforms...
         *
         */
        public static Group Group(params Node[] children)
        {
            var g = new Group();
            g.Children.AddRange(children);
            return g;
        }
        public static Group Group(IEnumerable<Node> children)
        {
            var g = new Group();
            g.Children.AddRange(children);
            return g;
        }

        #region Styles
        public static T Stroke<T>(this T c, double thickness, ColorB color) where T : Style
        {
            c.StrokeThickness = thickness;
            c.StrokeColor = color;
            return c;
        }
        public static T Stroke<T>(this T c, ColorB color, double thickness) where T : Style
        {
            c.StrokeThickness = thickness;
            c.StrokeColor = color;
            return c;
        }
        public static T Stroke<T>(this T c, double thickness) where T : Style
        {
            c.StrokeThickness = thickness;
            return c;
        }
        public static T Stroke<T>(this T c, ColorB color) where T : Style
        {
            c.StrokeColor = color;
            return c;
        }
        public static T Fill<T>(this T c, ColorB color) where T : Style
        {
            c.FillColor = color;
            return c;
        }



        #endregion



        #region Shapes

        public static Line Line(Vec2 p1, Vec2 p2)
        {
            return new Line(p1, p2);
        }
        public static Line Line(double x1, double y1, double x2, double y2)
        {
            return new Line(new Vec2(x1, y1), new Vec2(x2, y2));
        }

        public static Rect Rect(Vec2 p1, Vec2 p2, double rx = 0, double ry = 0)
        {
            return new Rect(p1, p2, rx, ry);
        }
        public static Rect Rect(double x1, double y1, double x2, double y2, double rx = 0, double ry = 0)
        {
            return new Rect(new Vec2(x1, y1), new Vec2(x2, y2), rx, ry);
        }

        public static Circle Circle(Vec2 center, double radius)
        {
            return new Circle(center.X, center.Y, radius);
        }
        public static Circle Circle(double cx, double cy, double radius)
        {
            return new Circle(cx, cy, radius);
        }
        public static Ellipse Ellipse(double cx, double cy, double a, double b, double rotation = 0, double? arcStart = null, double? arcEnd = null)
        {
            return new Ellipse(new Vec2(cx, cy), a, b);
        }

        /// <summary>
        /// Make a regular n-sided polygon
        /// </summary>
        /// <param name="numSides"></param>
        /// <param name="sideLength"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public static Path NGon(int numSides, double sideLength, Vec2 center = null)
        {
            center ??= Vec2.Origin;
            var (tHeight, radius) = PolyhedronInfo.TriangleSides(numSides, sideLength);
            var pts = new List<Vec2>();
            for (var i = 0; i < numSides; ++i)
            {
                var angle = i * Math.PI * 2 / numSides;
                pts.Add(radius * Dir(angle) + center);
            }
            return Path(pts).Closed();
        }

        public static Path Path()
        {
            return new Path();
        }
        public static Path Path(IEnumerable<Vec2> points)
        {
            return new Path(points.ToArray());
        }
        public static Path Path(params double[] vals)
        {
            var pts = new List<Vec2>();
            for (var i = 0; i < vals.Length; i += 2)
                pts.Add(new Vec2(vals[i], vals[i + 1]));
            return Path(pts);
        }
        public static Path Path(params Vec2[] points)
        {
            return new Path(points);
        }
        public static Path Path(IGetPoints g)
        {
            return new Path(g.GetPoints().ToArray());
        }
        public static Path Path(params Contour[] contours)
        {
            return new Path(contours);
        }

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
        /// <returns></returns>
        public static Text Text(double x, double y, double w, double h, string text,
            string fontName = "", double minFeatureSize = 0.0, double fontSize = 0.0
            )
        {
            return new Text(x, y, w, h, text, fontName, minFeatureSize);
        }

        /// <summary>
        /// Convert text to connected letters
        /// Requires installed font "Brannboll Connect PERSONAL USE"
        /// </summary>
        /// <param name="text">Text to convert</param>
        /// <param name="width">Width in mm</param>
        /// <param name="thicken">how much to thicken letters, default 1.0mm</param>
        /// <returns></returns>
        public static Node MakeConnectedText(string text, double width = 25.4 * 6, double thicken = 1.0)
        {
            Func<Node, Node> T = n1 => Thicken(n1, thicken);
            var f = Text(0, 0, width, 0, text, fontName: "Brannboll Connect PERSONAL USE", minFeatureSize: 0.01);

            Node cur = T(Path(f.Contours[0])); // start here
            Node root = null; // will hold final answer
            for (var index = 1; index < f.Contours.Count; ++index)
            {
                var nb = Path(f.Contours[index]);
                if (cur.Bounds().Contains(nb.Bounds()))
                    cur = Difference(cur, nb);
                else
                {
                    // next letter
                    root = root == null ? cur : Union(root, cur);
                    cur = T(nb);
                }
            }
            root = Union(root, cur);
            return root;
        }



        public static Contour Contour(IEnumerable<Vec2> points)
        {
            return new Contour(points.ToArray());
        }
        public static Contour Contour(IGetPoints g)
        {
            return new Contour(g.GetPoints().ToArray());
        }

        public static Contour Contour(params Vec2[] points)
        {
            return new Contour(points);
        }

#if false
        public static Polygon Polygon(params Vector2D[] points)
        {

        }
        public static Polyline Polyline(params Vector2D[] points)
        {

        }
#endif
        #endregion
        #region Transforms
        public static T Rotate<T>(double angle, T item) where T : Node
        {
            item.Transform = Mat3.ZRotation(angle) * item.Transform;
            return item;
        }
        public static T Transform<T>(double angle, double dx, double dy, T item) where T : Node
        {
            item.Transform = Mat3.Translation(dx, dy) * Mat3.ZRotation(angle) * item.Transform;
            return item;
        }
        public static T Transform<T>(double angle, Vec2 p, T item) where T : Node
        {
            item.Transform = Mat3.Translation(p) * Mat3.ZRotation(angle) * item.Transform;
            return item;
        }

        public static T Translate<T>(double dx, double dy, T item) where T : Node
        {
            item.Transform = Mat3.Translation(dx, dy) * item.Transform;
            return item;
        }
        public static T Translate<T>(Vec2 delta, T item) where T : Node
        {
            var m = Mat3.Translation(delta.X, delta.Y);
            item.Transform = m * item.Transform;
            return item;
        }

        /// <summary>
        /// Compute translation vector to apply to inner to center on outer
        /// </summary>
        /// <param name="outer"></param>
        /// <param name="inner"></param>
        /// <returns></returns>
        public static Vec2 Center(Node outer, Node inner)
        {
            return outer.Bounds().Center(inner.Bounds());
        }

        #endregion

        #region Math
        public static double FromDegrees(double degrees) => Math.PI * degrees / 180.0;
        #endregion

        #region Colors

        //static Color Convert(System.Windows.Media.Color color)
        //{
        //    return new(color.R,color.G,color.B);
        //}

        public static ColorB Red => new(255, 0, 0);
        public static ColorB Green => new(0, 255, 0);
        public static ColorB Blue => new(0, 0, 255);
        public static ColorB Yellow => new(255, 255, 0);
        public static ColorB Cyan => new(0, 255, 255);
        public static ColorB White => new(255, 255, 255);
        public static ColorB Black => new(0, 0, 0);
        public static ColorB Gray => new(128, 128, 128);
        public static ColorB None => new(0, 0, 0, 0);
        #endregion


        #region Boolean
        /// <summary>
        /// make an Union of the subject and clip, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Union(Node subject, Node clip)
        {
            return ClipperHelper.Union(subject, clip);
        }
        public static Node Union(Node subject, params Node[] clips)
        {
            Node ans = subject;
            foreach (var n in clips)
                ans = ClipperHelper.Union(ans, n);
            return ans;
        }

        /// <summary>
        /// make an XOR of the subject and clip, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Xor(Node subject, Node clip)
        {
            return ClipperHelper.Xor(subject, clip);
        }

        /// <summary>
        /// make a intersection of the subject and clip, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Intersection(Node subject, Node clip)
        {
            return ClipperHelper.Intersection(subject, clip);
        }

        /// <summary>
        /// make a difference of the nodes, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Difference(Node source, Node hole)
        {
            return ClipperHelper.Difference(source, hole);
        }

        public static Node Thicken(Node node, double amount)
        {
            return ClipperHelper.Thicken(amount, node);
        }

        #endregion

        #region Utils

        /// <summary>
        /// Convert string for inches like "1/8" to millimeters
        /// </summary>
        /// <param name="inches"></param>
        /// <returns></returns>
        public static double InToMM(string inches) => inches switch
        {
            "1/8" => 0.125 * 25.4,
            _ => throw new NotImplementedException("Conversion not yet implemented")
        };

        /// <summary>
        /// Make a notch shape
        /// </summary>
        /// <param name="p1">First edge point</param>
        /// <param name="p2">Second edge point</param>
        /// <param name="notchDistFromEnd">Dist from first point to notch start</param>
        /// <param name="notchThickness1">Notch thickness along edge</param>
        /// <param name="notchDepth1">Notch depth cut int/out of edge</param>
        /// <param name="directions">direction to face: 1 to right of line, -1 to left, 0 for both sides</param>
        /// <returns></returns>
        public static Node MakeNotch(
            Vec2 p1, Vec2 p2,
            double notchDistFromEnd,
            double notchThickness1,
            double notchDepth1,
            int directions
        )
        {

            var dir = (p2 - p1).Normalize(); // dir p1 -> p2
            var perp = Perp(dir); // perp to that, rotated 90 degrees ccw

            // where notch intersects p1->p2 first time
            var mid1 = p1 + dir * notchDistFromEnd;

            var toRt = directions == -1 ? 0.0 : 1.0; // do we go right?
            var toLf = directions == +1 ? 0.0 : 1.0; // do we go left?

            // 4 point quad, has depth in each direction
            var q1 = mid1 + toRt * perp * notchDepth1; // to right (or not)
            var q2 = q1 + dir * notchThickness1; // along p1->p2 dir
            var q3 = q2 - perp * (toRt + toLf) * notchDepth1; // other way
            var q4 = q3 - dir * notchThickness1; // and back

            return Path(q1, q2, q3, q4).Closed();
        }
        #endregion

    }
}
