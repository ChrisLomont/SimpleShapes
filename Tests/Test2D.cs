#if false // todo - fix
using static Lomont.SimpleShape;

namespace Lomont.Lib2D
{


    public static class Test2D
    {

        /// <summary>
        /// Test 2d things library
        /// </summary>
        public static void Test()
        {
            // Test1();
            // Bouncer();
            // TestBoolean();
            //TestSyntax();
            
            //Test(MakeBoolean, "TestShapes.svg");
            // Test(MakeCircles, "TestShapes.svg");
            // Test(MakeLines, "TestShapes.svg");
            //Test(MakeTransforms, "TestShapes.svg");
            // Test(MakePaths, "TestShapes.svg");
            //Test(MakeOffsets, "TestShapes.svg");
            //Test(MakeText, "TestShapes.svg");
            Test(MakeFonts,"TestFonts.svg");
        }

        static Node MakeFonts()
        {
            double dy = 100.0;
            var t1 = Text(0, 0, 300, 0, "Bob0");
            var t2 = Text(0, 0, 300, 0, "Bob0",fontName : "Arial");
            return Group(
                t1,
                Translate(0,dy,t2)
            ).Stroke(0.5).Fill(None);
        }

        static void Test(Func<Node> func, string saveName)
        {
            var node = func();
            node.Save(saveName);
        }

        static Node MakeBoolean()
        {
            // some booleans
            var rad = 20;
            var c1 = Circle(0, 0, rad);
            var c2 = Circle(rad, 0, rad);
            var dx = Point(0, rad * 2.5);
            var hollowBooleans =
                Group(
                        Group(c1, c2),
                        Translate(1 * dx, Union(c1, c2)),
                        Translate(2 * dx, Intersection(c1, c2)),
                        Translate(3 * dx, Difference(c1, c2)),
                        Translate(4 * dx, Xor(c1, c2)).Fill(Green)
                    ) // group
                    .Stroke(0.5)
                    .Stroke(Blue)
                    .Fill(None);

            return hollowBooleans;
        }

        static Node MakeCircles()
        {
            var radius = 100.0;
            var circle1 = Circle(30.0, 40.0, radius); // default
            var circle2 = Translate(-50, -50, Circle(30.0, 40.0, radius)); // default
            // root.Add(circle1,circle2);

            //.Thickness(10.0);


            return Group(
                    Circle(20, 20, 50), // x,y,r
                    Circle(30, 30, 10)
                        .Stroke(5)
                        .Stroke(Blue)
                        .Fill(Yellow)
                    ,
                    Translate(-20, -40,
                        Circle(30, 30, 10)
                            .Stroke(Green)
                    )
                )
                .Stroke(0.5);
        }

        static Node MakeLines()
        {
            var line1 = Line(0, 0, 40, 0); // horiz
            var line2 = Rotate(
                FromDegrees(90),
                Line(0, 0, 40, 0)
                    .Stroke(Red)
            );
            return Group(line1, line2)
                    .Stroke(2)
                    .Stroke(Cyan)
                ;
        }

        static Node MakeTransforms()
        {
            return
                Group(
                        // todo - rect rotate fails
                        Rotate(FromDegrees(30), Rect(0, 0, 30, 40)),
                        Rect(new Vector2D(20, 20), new Vector2D(60, 70), 10, 10)
                    )
                    .Stroke(Gray)
                    .Fill(None)

                ;
        }

        static Node MakePaths()
        {
            // Paths

            // a path can consist of multiple sub-contours to make holes
            var path1 = Path(
                Contour(Circle(0, 0, 30)).Fill(None).Closed(),
                Contour(Rect(-10, -10, 10, 10)).Fill(None).Closed()
            ).Fill(Blue);

            var pt1 = new Vector2D(0, 0);
            var pt2 = new Vector2D(50, 0);
            var pt3 = new Vector2D(50, 50);
            var path2 = Translate(50, 0, Path(pt1, pt2, pt3).Closed());
            var path3 = Translate(100, 0, Path(pt1, pt2, pt3).Open());
            var paths = Translate(90, 90,
                Group(path1, path2, path3));
            return paths;
        }

        static Node MakeOffsets()
        {
            var r1 = Rect(0,0,20,30);
            var r2 = Thicken(5, r1);
            var line1 = Thicken(7,Line(4, 4, 50, 70));
            return Group(r1,Translate(0,40,r2), line1);
        }
        static Node MakeText()
        {
            var r = Rect(0, 0, 100, 100).Fill(None).Stroke(Gray);
            var t = Text(0, 0, 100, 100, "[ Testing 0 1 2 3 ! @ # $ ]\nBOB").Fill(None).Stroke(0.1);

            return Group(r,t);
        }


        // how we want code to look
        static void TestSyntax()
        {
#if false
            var (x, y) = (1.0, 2.0);
            var pt = new Vector2D(1, 2);
            var r = 10.0;

            // todo - make Point(1,2) creator...
            var p1 = new Vector2D(1, 2);
            var p2 = new Vector2D(1, 2);
            var p3 = new Vector2D(1, 2);
            var p4 = new Vector2D(1, 2);

                //  todo .FillRule() // style, even odd, nonzero, ?
                // todo .LineCap()
                // todo .LineJoin()


            double a1 = 0, a2 = 20;
            double a = 3, b = 4;
            double rotation = 10;
            var e = Ellipse(x, y, r, a, b, rotation, a1, a2 /*, order*/);

            var p = Polygon(p1, p2, p3, p4); // closed default
            // todo - make these constructors more available...
            //var p2 = Polygon(from number in Enumerable.Range(0, 10) select new Vector2D(...));
            //var p3 = Polygon(" 1 2 3 4 5 6"); // same for others?

            var ln = Line(p1, p2);
            var pl1 = Polyline(p1, p2, p3); // polyline
            var pl2 = Polyline(p1, p2, p3).Closed(); // closed polyline

            // todo - with holes?
            var pth = Path(
                        Ellipse(),
                        Line(),
                        Polyline()
                    )
                    .Stroke(Blue)
                    .Thickness(20.0)
                as Path;

            // Text, Font and Fontsize and FontWeight in style
            // Union, Intersection,

            var g2 = Group(c, g, e, pth);

            // use https://github.com/WaterTrans/GlyphLoader
            // to implement outlines
            // var t1 = Translate(
            //     Text("Testing")
            //         .Font()
            //         .FontSize()
            //         .FontWeight()
            //         .FontStyle()
            // ) as Text;


        g2.Save("Testing.svg");
#endif
            //root.Save("TestShapes.svg");
            //root.Save("TestShapes.png");
        }

#if false // old stuff
        static void TestBoolean()
        {
            var rand = new Random();
            var radius = 100;

            var left = new Vector2D(0, 0);
            var right = new Vector2D(radius, 0);
            var dy = new Vector2D(0, -radius * 2.5);

            var root = new Node();

            if (true)
            {
                var c1 = Node();
                c1.Circle(left, radius);
                var c2 = Node();
                c2.Circle(right, radius);
                root.Add(Union(c1,c2));
            }

            if (true)
            {
                var c2 = Node();
                c2.Circle(left + dy, radius, reverse:false);
                var c3 = Node();
                c3.Circle(right + dy, radius,reverse:false);
                //c2.Add(c3);
                root.Add(Intersection(c2, c3));
                //c2.Add(c3);
                //root.Add(Union(c2));
            }

            if (true)
            {
                var c4 = Node();
                c4.Circle(left + 2 * dy, radius);
                var c5 = Node();
                c5.Circle(right + 2 * dy, radius);
                root.Add(Difference(c4, c5));
            }

            if (true)
            {
                var c6 = Node();
                c6.Circle(left + 3 * dy, radius);
                var c7 = Node();
                c7.Circle(right + 3 * dy, radius);
                root.Add(Xor(c6, c7));
            }


            root.Save("Boolean.svg");

        }

        public static void Bouncer()
        {
            var root = Node();
            var rand = new Random();
            var edge = 1000; // box size
            var (w, h) = (edge,edge); // box size
            Func<double> r = () => rand.NextDouble() * w;
            // deltas
            var dd = w / 20.0;
            Func<double> d = () => rand.NextDouble() * dd*2-dd;


            // helper function
            bool OutOfBounds(Vector2D v, double b)
            {
                bool Check(double val, double mx) => 0 <= val && val <= mx;
                return !(Check(v.X, b) && Check(v.Y, b) && Check(v.Z, b));
            }

            void AddThickLine(Vector2D pt1, Vector2D pt2, double thick)
            {
                var dir = (pt2 - pt1).Normalize();
                var perp = new Vector2D(dir.Y, -dir.X);
                var poly = new Polygon();
                poly.AddPoint(pt1 + thick * perp);
                poly.AddPoint(pt2 + thick * perp);
                poly.AddPoint(pt2 - thick * perp);
                poly.AddPoint(pt1 - thick * perp);
                root.Polygons.Add(poly);

                // circle the ends
                root.Circle(pt1, thick,10);
                root.Circle(pt2, thick, 10);
            }

            for (var j = 0; j < 5; ++j)
            {
                // line endpoints and step size
                var p1 = new Vector2D(r(), r());
                var p2 = new Vector2D(r(), r());
                var (d1, d2) = (new Vector2D(d(), d()), new Vector2D(d(), d()));

                for (var i = 0; i < 40; ++i)
                {
                    AddThickLine(p1, p2, 2.0);
                    if (OutOfBounds(p1 + d1, edge))
                        d1 = -d1;
                    p1 += d1;
                    if (OutOfBounds(p2 + d2, edge))
                        d2 = -d2;
                    p2 += d2;
                }
            }

            // crop to a big circle
            var c = Node();
            c.Circle(new Vector2D(edge/2,edge/2), edge/2, reverse:false);

            var clip = Intersection(root, c);
            clip.Circle(new Vector2D(edge / 2, edge / 2), edge / 2);

            clip.Save("Bouncer.svg");

        }

        static void Test1()
        { 

        // get a root node
            var root = Node();

            // helper for random points
            var rand = new Random(1234);
            Func<double> r = () => rand.NextDouble() * 500 - 250;
            Func<double> dr = () => rand.NextDouble() * 300 + 100;

            // make some rectangles, store under root
            for (var i = 0; i < 5; ++i)
            {
                var (x1, y1) = (r(), r());
                var (x2, y2) = (x1 + dr(), y1 + dr());
                var rect = Rect(x1, y1, x2, y2);
                root.Add(rect);
            }

            // merge rects
            var merged = Union(root,null);

            // punch a hole
            var hole = Union(Node(Rect(30,30,50,50),Rect(40,40,90,90)),null);

            // var shape2 = Difference(merged, hole); // todo - testing
            var shape2 = Node(merged, hole);

            // write SVG and PNG
            shape2.Save("clip.svg");
            // merged.Save("clip.png");
        }

        public static void TODO()
        {


            var r = new Random(1234);

            // get clipping item
            var clipper = new ClipperLib.ClipperLib();

            // add some paths 
            var subs = new Paths();
            for (var i = 0; i < 10; ++i)
            {
                var x1 = r.Next(1000);
                var y1 = r.Next(1000);
                var x2 = r.Next(1000);
                var y2 = r.Next(1000);

                var p1 = new ClipperLib.IntPoint(x1, y1);
                var p2 = new ClipperLib.IntPoint(x1, y1);
                subs.Add(new Path {p1, p2});
            }

            // thicken around a square
            var ss = new Path();
            var d = 5;
            ss.Add(new IntPoint(d, d));
            ss.Add(new IntPoint(-d, d));
            ss.Add(new IntPoint(-d, -d));
            ss.Add(new IntPoint(d, -d));

            var output = ClipperLib.ClipperLib.MinkowskiSum(ss, subs, false);
        }
#endif
    }
}
#endif