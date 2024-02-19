using Lomont.Graphics;
using Lomont.Numerical;
using Lomont.SimpleShapes.Shape3D;
using Boolean = Lomont.SimpleShapes.Shape3D.Boolean;
using Path = Lomont.SimpleShapes.Shape3D.Path;

namespace Lomont.SimpleShapes
{

    /// <summary>
    /// Static class to clean up usage syntax
    /// </summary>
    public static class SimpleShape3D
    {
        #region 2D Shapes

        // Path, Rect, Circle, Ellipse, NGon, Polygon, Text

        public static Path Path()
        {
            return new Path();
        }
        public static Path Path(IEnumerable<Vec3> points)
        {
            return new Path(points.ToArray());
        }
        public static Path Path(params double[] vals)
        {
            var pts = new List<Vec3>();
            for (var i = 0; i < vals.Length; i += 2)
                pts.Add(new Vec3(vals[i], vals[i + 1]));
            return Path(pts);
        }
        public static Path Path(params Vec3[] points)
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
        public static Rect Rect(Vec3 p1, Vec3 p2)
        {
            return new Rect(p1, p2);
        }
        public static Rect Rect(double x1, double y1, double x2, double y2)
        {
            return new Rect(new Vec3(x1, y1), new Vec3(x2, y2));
        }
        public static Circle Circle(Vec3 center, double radius)
        {
            return new Circle(center.X, center.Y, radius);
        }
        public static Circle Circle(double cx, double cy, double radius)
        {
            return new Circle(cx, cy, radius);
        }
        public static Ellipse Ellipse(double cx, double cy, double a, double b, double rotation = 0, double? arcStart = null, double? arcEnd = null)
        {
            return new Ellipse(a, b, cx, cy);
        }

        /// <summary>
        /// Make a regular n-sided polygon
        /// </summary>
        /// <param name="numSides"></param>
        /// <param name="sideLength"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public static Path NGon(int numSides, double sideLength = 5.0, Vec3 center = null)
        {
            center ??= Vec3.Origin;
            var (tHeight, radius) = PolyhedronInfo.TriangleSides(numSides, sideLength);
            var pts = new List<Vec3>();
            for (var i = 0; i < numSides; ++i)
            {
                var angle = i * Math.PI * 2 / numSides;
                pts.Add(radius * Dir(angle) + center);
            }
            return Path(pts).Closed();
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
            throw new NotImplementedException("");
            // todo
            Func<Node, Node> T = n1 => null; // todo Thicken(n1, thicken);
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

        public static Contour Contour(IEnumerable<Vec3> points)
        {
            return new Contour(points.ToArray());
        }
        public static Contour Contour(IGetPoints g)
        {
            return new Contour(g.GetPoints().ToArray());
        }

        public static Contour Contour(params Vec3[] points)
        {
            return new Contour(points);
        }

        #endregion

        #region 3D Shapes

        // cube, sphere, cylinder, ngon, polygon
        // linear_extrude, rotate_extrude

        /// <summary>
        /// Axis aligned cuboid given opposite diagonal corners 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Cube Cube(Vec3 a, Vec3 b)
        {
            return new Cube(a, b);
        }

        /// <summary>
        /// Create a origin centered cuboid
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="dz"></param>
        /// <returns></returns>
        public static Cube Cube(double dx = 1, double dy = 1, double dz = 1)
        {
            var p = Point(dx, dy, dz);
            return new Cube(-p / 2, p / 2);
        }

        public static Sphere Sphere(Vec3 center, double radius, int u = 10, int v = 10)
        {
            return new Sphere(center, radius, u, v);
        }

        public static Sphere Sphere()
        {
            return Sphere(Vec3.Origin, 1, 10, 10);
        }

        /// <summary>
        /// Cylinder of given height and radius, axis along z axis
        /// </summary>
        /// <param name="height"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Cylinder Cylinder(double height, double radius)
        {
            return new Cylinder(height, radius, radius);
        }
        public static Cylinder Cylinder(double height, double radius1, double radius2, int sides = 50)
        {
            return new Cylinder(height, radius1, radius2, sides);
        }

        public static Cylinder Cylinder(Vec3 p1, Vec3 p2, double radius, int sides = 50)
        {
            // default cylinder points in +z direction....
            var dir = p2 - p1;
            var rot = Mat4.CreateRotation(Vec3.ZAxis, dir);
            var ht = (p2 - p1).Length;
            return Translate(p1, Transform(rot, new Cylinder(ht, radius, radius, sides)));
        }

        public static Polygon Polygon(IEnumerable<Vec3> points)
        {
            return new Polygon(points);
        }

        public static Polyhedron Polyhedron(List<Vec3> points, List<List<int>> faces)
        {
            return new Polyhedron(points, faces);

        }


        // public Polygon NGon(double thickness, )
        // {
        //     throw new NotImplementedException("Direction cylinder");
        // }

        public static Node LinearExtrude(Path path, double height)
        {
            return new LinearExtrude(path, height);
        }

        #endregion

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


        #region Boolean
        /// <summary>
        /// make an Union of the subject and clip, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Union(Node subject, Node clip)
        {
            return new Boolean(Boolean.Type.Union, subject, clip);
        }
        public static Node Union(Node subject, params Node[] clips)
        {
            var list = new List<Node> { subject };
            list.AddRange(clips);
            return new Boolean(Boolean.Type.Union, list.ToArray());
        }

        /// <summary>
        /// make an XOR of the subject and clip, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Xor(Node subject, Node clip)
        {
            return new Boolean(Boolean.Type.Xor, subject, clip);
        }

        /// <summary>
        /// make a intersection of the subject and clip, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Intersection(Node subject, Node clip)
        {
            return new Boolean(Boolean.Type.Intersection, subject, clip);
        }

        /// <summary>
        /// make a difference of the nodes, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Difference(Node subject, Node clip)
        {
            return new Boolean(Boolean.Type.Difference, subject, clip);
        }

        public static Node Difference(Node subject, params Node[] clips)
        {
            var list = new List<Node> { subject };
            list.AddRange(clips);
            return new Boolean(Boolean.Type.Difference, list.ToArray());
        }


        #endregion

        #region Modify
        //public static Node Thicken(Node node, double amount)
        //{
        //    return ClipperHelper.Thicken(amount, node);
        //}

        // Offset, Hull, Minkowski
        #endregion


        #region Transforms
        public static T Rotate<T>(double angleX, double angleY, double angleZ, T item) where T : Node
        {
            item.Transform = Mat4.RotationXYZ(angleX, angleY, angleZ) * item.Transform;
            return item;
        }
        public static T Transform<T>(Mat4 transform, T item) where T : Node
        {
            item.Transform = transform * item.Transform;
            return item;
        }
        public static T Translate<T>(double dx, double dy, double dz, T item) where T : Node
        {
            item.Transform = Mat4.Translation(dx, dy, dz) * item.Transform;
            return item;
        }
        public static T Translate<T>(Vec3 delta, T item) where T : Node
        {
            var m = Mat4.Translation(delta);
            item.Transform = m * item.Transform;
            return item;
        }

        #endregion

        #region Styles
        public static T Color<T>(this T c, ColorB color) where T : Style
        {
            c.FillColor = color;
            return c;
        }

        #endregion

        #region Colors

        //static Color Convert(System.Windows.Media.Color color)
        //{
        //    return new(color.R,color.G,color.B);
        //}

        public static ColorB Black => new(0, 0, 0);
        public static ColorB Blue => new(0, 0, 255);
        public static ColorB Cyan => new(0, 255, 255);
        public static ColorB DarkBlue => new(0, 0, 128);
        public static ColorB DarkCyan => new(0, 128, 128);
        public static ColorB DarkGreen => new(0, 128, 0);
        public static ColorB DarkMagenta => new(128, 0, 128);
        public static ColorB DarkRed => new(128, 0, 0);
        public static ColorB DarkYellow => new(128, 128, 0);
        public static ColorB Gray => new(128, 128, 128);
        public static ColorB Green => new(0, 255, 0);
        public static ColorB Magenta => new(255, 0, 255);
        public static ColorB Red => new(255, 0, 0);
        public static ColorB Yellow => new(255, 255, 0);
        public static ColorB White => new(255, 255, 255);
        public static ColorB None => new(0, 0, 0, 0);

        public static ColorB RandomColor => new(c, c, c);
        static readonly Random rand = new(1234); // make reproducible
        static byte c => (byte)rand.Next(256);


        #endregion

        #region Utils
        public static Vec3 Point(Vec3 p) => new(p.X, p.Y, p.Z);
        public static Vec3 Point(double x = 0, double y = 0, double z = 0) => new(x, y, z);

        /// <summary>
        /// 2D direction in x,y plane
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns></returns>
        public static Vec3 Dir(double angle, double radius = 1.0) => radius * new Vec3(Math.Cos(angle), Math.Sin(angle), 0);

        /* TODO
         * Helpers:
         * 1. Arc connections to ends of things - auto tangent, etc..
         * 2. things to help align items - creates transforms...
         *
         */

        public static double FromDegrees(double degrees) => Math.PI * degrees / 180.0;

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
        #endregion

    }
}
