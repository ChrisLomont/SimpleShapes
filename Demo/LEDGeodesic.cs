using System;
using System.Linq;
using Lomont.Graphics;
using Lomont.Numerical;
using static Lomont.SimpleShapes.SimpleShape3D;
using D = Lomont.SimpleShapes.DodecahedronInfo;
using P = Lomont.SimpleShapes.PolyhedronInfo;
using static System.Math;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Lomont.Geometry;
using Lomont.SimpleShapes.Shape3D;
using Sphere = Lomont.SimpleShapes.Shape3D.Sphere;
using Lomont.Stats;
using Lomont.Utility;
using Color = Lomont.Graphics.Color;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "LED Geodesic", "9/3/2023")]
    class LEDGeodesic
    {
        /* things learned:
         1. for the 8mm leds, the kerf of 0.1mm worked great

         */



        public void Run()
        {
            //Geodesic("LEDGeodesic.scad"); // make geodesic of small balls and tubes for edges
            //SpherePoints("NPoints.scad"); // cover sphere with points in various methods
            //LEDBlock("BlockLEDs.scad"); // make a block of LEDs with different sizes

            if (false)
            {
                var top = LEDGeodesicBall_v1(false); // make the LED geodesic ball shape
                top.Save("LEDBallTop_v1.scad");
                var bot = LEDGeodesicBall_v1(true); // make the LED geodesic ball shape
                bot.Save("LEDBallBottom_v1.scad");
                Union(top, Translate(0, 0, 4.0, bot)).Save("LEDBallBoth_v1.scad");
            }

            if (true)
            {
                var top = LEDGeodesicBall_v2(false); // make the LED geodesic ball shape
                top.Save("LEDBallTop_v2.scad");
                var bot = LEDGeodesicBall_v2(true); // make the LED geodesic ball shape
                bot.Save("LEDBallBottom_v2.scad");
                Union(top, Translate(0, 5, -5.0, bot)).Save("LEDBallBoth_v2.scad");
            }


        }

        void LEDBlock(string filename)
        {
            var blockDelta = 7.0;
            double dx = 13.0;
            double dKerf = 0.10;
            double blockHt = 12.0;
            double dz = 0.05;
            int sides = 100;
            var leds = Union(
                Translate(Point(0 * dx, 0, -dz), LED(true, kerf: 0 * dKerf, sides)),
                Translate(Point(1 * dx, 0, -dz), LED(true, kerf: 1 * dKerf, sides)),
                Translate(Point(2 * dx, 0, -dz), LED(true, kerf: 2 * dKerf, sides)),
                Translate(Point(3 * dx, 0, -dz), LED(true, kerf: 3 * dKerf, sides)),
                Translate(Point(4 * dx, 0, -dz), LED(true, kerf: 4 * dKerf, sides)),
                Translate(Point(5 * dx, 0, -dz), LED(true, kerf: 5 * dKerf, sides))
            );
            var block = Cube(Point(-blockDelta, -blockDelta, 0), Point(5*dx+1*blockDelta, blockDelta, blockHt));
            block.Color(new ColorB(255, 0, 0));

            var shape = Difference(block, leds);
            shape.Save(filename);
        }


        // return LED pointing up on +z axis
        // tip at 0,0,0
        // is an 8mm LED, WS2812B
        Node LED(
            bool hasLeads = true, 
            double kerf = 0, 
            int sides = 40, 
            bool hole = false, 
            double shellThickness = 1.7,
            // prefent trapped air cups in PreForm
            bool preventCups = false,
            double excess=0.0 // length on top
        )
        {
            double radius = 4.0+kerf; // mm
            double ht = 11.0+kerf; // total height
            double baseRadius = 9.25/2+kerf;
            double baseHt = 2.0+kerf; // height of base cylinder
            double wireLen = 16.0; // wire length
            double wireRadius = 0.5/2; // wire radius

            var basePt = Point(0, 0, 0);
            var cylPt = Point(0,0,baseHt);
            var sphPt = Point(0,0,ht-radius);

            Node Wire(double x) => Cylinder(Point(x,0,-wireLen),Point(x,0,0.5),wireRadius,sides);

            Node ledShape = Group(
                Sphere(sphPt, radius, sides),
                Cylinder(sphPt,basePt,radius, sides),
                Cylinder(cylPt, basePt, baseRadius, sides)
            );

            if (hasLeads)
            {
                var leads = Group(
                    Wire(wireRadius * 2),
                    Wire(wireRadius * 6),
                    Wire(-wireRadius * 2)
                );
                ledShape = Union(ledShape, leads);
            }

            var led = Translate(0, 0, -ht, ledShape);
            if (hole)
            { // use LED to make hole
                var shell = Cylinder(Point(0,0,shellThickness+excess),Point(0,0,-ht+0.1),baseRadius+shellThickness).Color(Gray);
                led = Difference(shell, led);

                // cups
                if (preventCups)
                {
                    double cupRadius = 1.0;
                    double cupLength = 10.0;
                    double cupZ = 0.0;
                    led = Difference(led,
                        Group(
                            Cylinder(Point(-cupLength,0,cupZ),Point(cupLength,0,cupZ),cupRadius,12),
                            Cylinder(Point(0,-cupLength,cupZ), Point(0,cupLength,cupZ), cupRadius, 12)
                            ).Color(Red)
                    );
                }

                led = Translate(0, 0, -shellThickness, led);
            }

            return led;

        }

        void SpherePoints(string filename)
        {
            int numSides = 10;
            double radius = 1.0;
            double spacing = 40.0;
            //var (verts,edges) = SpherePoints(128);
            var (verts, edges) = FibonacciSpherePoints(42);

                        var t = verts.Select(p => new Sphere(spacing*p, radius, numSides, numSides)).ToList();
            var balls = Group(t);

            var tubes = Group(
                edges.Select(edge=>Cylinder(
                    spacing*verts[edge.Item1], spacing * verts[edge.Item2],
                    radius,numSides))
                );

            var shape = Union(balls, tubes);//, LED());
            //shape = balls;

            shape.Save(filename);
        }
        // make ball piece
        Node LEDGeodesicBall_v2(bool topPiece)
        {
            bool addEdges = false;
            bool addShell = true;
            bool addLeds = true;
            bool clipVerts = true;
            double wireHoleRadius = 4.0;
            bool addNubs = false;

            int numSides = 10; // sphere and cylinder divisions

            var radius = 60.0;   // mm radius size of outer object
            var cylRad = 2.0;  // tube radius
            var sphRad = cylRad + 1.5; // cylinder radius
            var shellThickness = 3.0;

            var ledDepth = 4.0; // led depth from surface
            var ledKerf = 0.1;
            var ledSides = 50;

            // geodesic vert, face definitions
            var poly = Polyhedra.Icosahedron;
            var (v, f) = GeodesicPolyhedron(poly, 2); // number of subdivisions
            Console.WriteLine($"Vertices {v.Count / 3}, faces {f.Count / 4}");

            // get nice vertex into Vec3
            var pts = Enumerable.Range(0, v.Count / 3)
                .Select(i => new Vec3(v[3 * i], v[3 * i + 1], v[3 * i + 2])).ToList();

            // the geodesic should be symmetric, so the bounding sphere
            // has diameter = max dist between verts
            var maxPairDistance = 0.0;
            for (int i = 0; i < pts.Count; ++i)
                for (int j = i + 1; j < pts.Count; ++j)
                {
                    var dist = (pts[i] - pts[j]).Length;
                    maxPairDistance = Max(dist, maxPairDistance);
                }

            // now scale points so enclosing diameter is the one requested
            pts = pts.Select(p => p * radius / (maxPairDistance / 2)).ToList();

            Node ball = Group();

            // track edges added to prevent dups 
            var seen = new HashSet<ulong>();

            var faces = new List<Node>();
            if (addEdges)
            {
                // create edges from tubes
                var index = 0;
                while (index < f.Count)
                {
                    var sides = f[index++];

                    for (var i = 0; i < sides; ++i)
                        AddTubeA(f[index + i], f[index + ((i + 1) % sides)]);
                    index += sides;
                }

                // sphere at each vertex
                var verts = new List<Node>();
                foreach (var c in pts)
                    verts.Add(Sphere(c, sphRad, numSides, numSides));
                ball = Union(ball, Group(faces), Group(verts));
            }

            var clipZ = -8.0; // 8mm leds, this plenty?

            // decide to clip or not
            bool ClipTop(Vec3 p)
            {
                if (Abs(p.Z) < 5.0)
                {
                    // center slice, split on y
                    return p.Y < 5.0;
                }
                return p.Z > clipZ;
            }

            List<Vec3> ClipPts(List<Vec3> pp)
            {
                return (!topPiece ? pp.Where(ClipTop) : pp.Where(p => !ClipTop(p))).ToList();
            }

            if (clipVerts)
            {
                // clip pts
                pts = ClipPts(pts);
            }

            var shellSides = 100;

            if (addLeds)
            {
                // leds at each vertex
                Node leds = Group(pts.Select(p =>
                {
                    var dz = radius - ledDepth;
                    var mat = Align(Vec3.ZAxis, p);
                    var led = LED(true, ledKerf, ledSides, true, preventCups:false, excess:3.6);
                    led = Translate(0, 0, dz, led);
                    led = Transform(mat, led);
                    return led;
                }));

                // clamp leds on outer ball
                if (addShell)
                {
                    //Node outer = OuterShell().Color(new ColorB(255,0,0,64));
                    //leds = Difference(leds,outer);
                    //return leds;
                }

                ball = Union(ball, leds);
            }

            Node OuterShell()
            {
                return Sphere(Vec3.Origin, radius, shellSides, shellSides).Color(topPiece ? Cyan : Blue);

            }

            if (addShell)
            {
                // shell
                Node outer = OuterShell();
                Node inner = Sphere(Vec3.Origin, radius - shellThickness, shellSides, shellSides).Color(Red);
                

                // side holes
                var sideHole = Cylinder(Point(-radius-5.0,0,0), Point(radius + 5.0, 0, 0), wireHoleRadius, 40);

                // clip top/bottom shape
                double zOffset = 12.0;
                var bb = radius + 10;
                var block1 = Cube(
                    Point(-bb, -bb, -bb),
                    Point( bb,  bb, -zOffset)
                );
                var block2 = Cube(
                    Point(-bb, ledKerf, -2*zOffset),
                    Point( bb, bb, zOffset)
                );
                var clip = Union(block1, block2, sideHole).Color(Green);
                if (topPiece)
                    clip = Rotate(PI, 0, 0, clip);

                // textured surface
                double nubRadius = 1.0;
                if (addNubs)
                {
                    var (nubPoints, _) = FibonacciSpherePoints(4000);
                    //nubPoints = ClipPts(nubPoints);
                    var nubs = Group(nubPoints.Select(p => Sphere(p.Unit() * radius, nubRadius))).Color(Magenta);
                    outer = Union(outer, nubs);
                }

                var shell = Difference(outer, Union(inner, clip));
                ball = Union(ball, shell);

                // small holes to prevent air cups
                double cupHoleRadius = 0.5;
                double cupExcess = 5.0; // length outside
                Node cupHoles = Group(pts.Select(
                    p => Cylinder(p + p.Unit() * cupExcess, p - p.Unit() * (shellThickness + cupExcess), cupHoleRadius,
                        12)
                )).Color(White);
                
                // one at tip to allow air exit
                double scz = topPiece ? -1 : 1;
                double zDepth = shellThickness+4.0;
                double yDepth = 10.0;
                cupHoles = Union(cupHoles,
                    Cylinder(Point(0,0,scz*(radius-zDepth)),Point(0,scz*yDepth,scz*(radius-zDepth)),2*cupHoleRadius,12)
                ).Color(White);
                ball = Difference(ball, cupHoles);

            }

            
            // create a matrix that orients startDir into endDir
            Mat4 Align(Vec3 startDir, Vec3 endDir)
            {

                var m1 = XyzToFrame(startDir);
                var m2 = XyzToFrame(endDir);
                var inv = m1.Inverse();
                var ans = m2 * inv;

                CheckOrtho(ans * Vec3.XAxis, ans * Vec3.YAxis, ans * Vec3.ZAxis);

                Trace.Assert(Abs((ans * Vec3.XAxis).Length - 1) < 1e-3);
                Trace.Assert(Abs((ans * Vec3.YAxis).Length - 1) < 1e-3);
                Trace.Assert(Abs((ans * Vec3.ZAxis).Length - 1) < 1e-3);

                return ans;

                // maps Z -> dir, creates right hand ortho X,Y,Z frame
                static Mat4 XyzToFrame(Vec3 dir)
                {
                    // want x cross y = z, thus
                    // z cross x = y, and
                    // y cross z = x

                    var startZ = dir.Unit();
                    // need nonzero X, ortho to Z.
                    var startX = Vec3.Cross(Vec3.YAxis, startZ);
                    if (startX.Length < 1e-1) // not much in y direction, try x direction
                        startX = Vec3.Cross(startZ, Vec3.XAxis);
                    if (startX.Length < 1e-1) // not much in x or y direction, must be in z direction
                        startX = Vec3.Cross(startZ, Vec3.ZAxis);
                    startX = startX.Unit();

                    var startY = Vec3.Cross(startZ, startX);
                    startY = startY.Unit();

                    CheckOrtho(startX, startY, startZ);

                    // converts XYZ into start frame
                    var m4 = new Mat4(startX, startY, startZ, new Vec3(0, 0, 0));
                    m4[3, 3] = 1.0;
                    return m4;
                }

            }
            static void CheckOrtho(Vec3 startX, Vec3 startY, Vec3 startZ)
            {
                Trace.Assert(Abs((startX.Length - 1)) < 1e-3);
                Trace.Assert(Abs((startY.Length - 1)) < 1e-3);
                Trace.Assert(Abs((startZ.Length - 1)) < 1e-3);
                Trace.Assert((Vec3.Cross(startX, startY) - startZ).Length < 1e-3);
                Trace.Assert((Vec3.Cross(startZ, startX) - startY).Length < 1e-3);
                Trace.Assert((Vec3.Cross(startY, startZ) - startX).Length < 1e-3);
            }

            return ball;

            void AddTubeA(int i1, int i2)
            {
                var hash = (ulong)(Math.Min(i1, i2) * 1000 + Math.Max(i1, i2));
                if (!seen.Contains(hash))
                {
                    var (p1, p2) = (pts[i1], pts[i2]);
                    faces.Add(Cylinder(p1, p2, cylRad, numSides));
                    seen.Add(hash);
                }
            }

        }

        // make ball piece
        Node LEDGeodesicBall_v1(bool topPiece)
        {
            int numSides = 10; // sphere and cylinder divisions

            var radius = 60.0;   // mm radius size of outer object
            var cylRad = 2.0;  // tube radius
            var sphRad = cylRad + 1.5; // cylinder radius
            var shellThickness = 4.0;

            var ledDepth = 4.0; // led depth from surface
            var ledKerf = 0.1;
            var ledSides = 50;

            // geodesic vert, face definitions
            var poly = Polyhedra.Icosahedron;
            var (v, f) = GeodesicPolyhedron(poly, 2); // number of subdivisions
            Console.WriteLine($"Vertices {v.Count / 3}, faces {f.Count / 4}");

            // get nice vertex into Vec3
            var pts = Enumerable.Range(0, v.Count / 3)
                .Select(i => new Vec3(v[3 * i], v[3 * i + 1], v[3 * i + 2])).ToList();

            // the geodesic should be symmetric, so the bounding sphere
            // has diameter = max dist between verts
            var maxPairDistance = 0.0;
            for (int i = 0; i < pts.Count; ++i)
            for (int j = i + 1; j < pts.Count; ++j)
            {
                var dist = (pts[i] - pts[j]).Length;
                maxPairDistance = Max(dist, maxPairDistance);
            }

            // now scale points so enclosing diameter is the one requested
            pts = pts.Select(p => p*radius/(maxPairDistance/2)).ToList();

            var clipZ = -8.0; // 8mm leds, this plenty?

            // clip pts
            if (topPiece)
                pts = pts.Where(p => p.Z > clipZ).ToList();
            else
                pts = pts.Where(p => p.Z <= clipZ).ToList();

            // track edges added to prevent dups 
            var seen = new HashSet<ulong>();

            // create faces from tubes
            var index = 0;
            var faces = new List<Node>();
            while (index < f.Count && false)
            {
                var sides = f[index++];

                for (var i = 0; i < sides; ++i)
                    AddTubeA(f[index + i], f[index + ((i + 1) % sides)]);
                index += sides;
            }

            // leds at each vertex
            var leds = Group(pts.Select(p =>
            {
                var dz = radius - ledDepth;
                var mat = Align(Vec3.ZAxis, p);
                var led = LED(true, ledKerf, ledSides, true);
                led = Translate(0,0,dz, led);
                led = Transform(mat, led);
                return led;
            }));


            // sphere at each vertex
            var verts = new List<Node>();
          //  foreach (var c in pts)
          //      verts.Add(Sphere(c, sphRad, numSides, numSides));

            // shell
            var shellSides = 100;
            var outer = Sphere(Vec3.Origin, radius, shellSides, shellSides).Color(topPiece?Cyan:Blue);
            var inner = Sphere(Vec3.Origin, radius-shellThickness, shellSides, shellSides).Color(Red);
            var bb = radius + 10;
            var half =
                topPiece
                    ? Cube(Point(-bb, -bb, clipZ), Point(bb, bb, -bb)).Color(Green)
                    : Cube(Point(-bb, -bb, clipZ), Point(bb, bb, bb)).Color(Green);
            var shell = Difference(outer, Union(inner,half));

            // make solid shape
            Node ball = Union(shell, Group(faces), Group(verts),leds);

            // create a matrix that orients startDir into endDir
            Mat4 Align(Vec3 startDir, Vec3 endDir)
            {
                
                var m1 = XyzToFrame(startDir);
                var m2 = XyzToFrame(endDir);
                var inv = m1.Inverse();
                var ans = m2*inv;

                CheckOrtho(ans * Vec3.XAxis, ans * Vec3.YAxis, ans * Vec3.ZAxis);

                Trace.Assert(Abs((ans * Vec3.XAxis).Length - 1) < 1e-3);
                Trace.Assert(Abs((ans * Vec3.YAxis).Length - 1) < 1e-3);
                Trace.Assert(Abs((ans * Vec3.ZAxis).Length - 1) < 1e-3);

                return ans;

                // maps Z -> dir, creates right hand ortho X,Y,Z frame
                static Mat4 XyzToFrame(Vec3 dir)
                {
                    // want x cross y = z, thus
                    // z cross x = y, and
                    // y cross z = x

                    var startZ = dir.Unit();
                    // need nonzero X, ortho to Z.
                    var startX = Vec3.Cross(Vec3.YAxis, startZ);
                    if (startX.Length < 1e-1) // not much in y direction, try x direction
                        startX = Vec3.Cross(startZ, Vec3.XAxis);
                    if (startX.Length < 1e-1) // not much in x or y direction, must be in z direction
                        startX = Vec3.Cross(startZ, Vec3.ZAxis);
                    startX = startX.Unit();

                    var startY = Vec3.Cross(startZ, startX);
                    startY = startY.Unit();

                    CheckOrtho(startX, startY, startZ);

                    // converts XYZ into start frame
                    var m4 = new Mat4(startX, startY, startZ, new Vec3(0, 0, 0));
                    m4[3, 3] = 1.0;
                    return m4;
                }

            }
            static void CheckOrtho(Vec3 startX, Vec3 startY, Vec3 startZ)
            {
                Trace.Assert(Abs((startX.Length - 1)) < 1e-3);
                Trace.Assert(Abs((startY.Length - 1)) < 1e-3);
                Trace.Assert(Abs((startZ.Length - 1)) < 1e-3);
                Trace.Assert((Vec3.Cross(startX, startY) - startZ).Length < 1e-3);
                Trace.Assert((Vec3.Cross(startZ, startX) - startY).Length < 1e-3);
                Trace.Assert((Vec3.Cross(startY, startZ) - startX).Length < 1e-3);
            }

            return ball;

            void AddTubeA(int i1, int i2)
            {
                var hash = (ulong)(Math.Min(i1, i2) * 1000 + Math.Max(i1, i2));
                if (!seen.Contains(hash))
                {
                    var (p1, p2) = (pts[i1], pts[i2]);
                    faces.Add(Cylinder(p1, p2, cylRad, numSides));
                    seen.Add(hash);
                }
            }

        }

        void Geodesic(string filename){
            int numSides = 10; // sphere and cylinder divisions
            var radius = 50; // mm radius size of outer object
            var cylRad = 2.0; // tube radius
            var sphRad = cylRad + 1.5; // cylinder radius

            // polyhedra vert, face definitions
            // var poly = Polyhedra.Cube;
            var poly = Polyhedra.Icosahedron;
            var (v, f) = GeodesicPolyhedron(poly,2);

            Console.WriteLine($"Vertices {v.Count/3}, faces {f.Count/4}");

            // track edges added to prevent dups 
            var seen = new HashSet<ulong>();

            // scale points into box [-1,1]
            var pts = Enumerable.Range(0, v.Count / 3)
                .Select(i => new Vec3(v[3 * i], v[3 * i + 1], v[3 * i + 2])).ToList();
            var maxLen = pts.Max(p => p.Length);
            pts = pts.Select(p => p / maxLen).ToList();

            // create faces from tubes
            var index = 0;
            var faces = new List<Node>();
            while (index < f.Count)
            {
                var sides = f[index++];

                for (var i = 0; i < sides; ++i)
                    AddTubeA(f[index + i], f[index + ((i + 1) % sides)]);
                index += sides;
            }

            // sphere at each vertex
            var verts = new List<Node>();
            for (var i = 0; i < v.Count; i += 3)
            {
                var c = Pt(i / 3);
                verts.Add(Sphere(c, sphRad, numSides, numSides));
            }

            // make solid shape
            Node ball = Union(Group(faces), Group(verts));


            ball.Save(filename);

            Vec3 Pt(int index)
            {
                return radius * pts[index];
                //var i = index * 3;
                //return radius*(new Vec3(v[i], v[i + 1], v[i + 2]));
            }
            void AddTubeA(int i1, int i2)
            {
                var hash = (ulong)(Math.Min(i1, i2) * 1000 + Math.Max(i1, i2));
                if (!seen.Contains(hash))
                {
                    var (p1, p2) = (Pt(i1), Pt(i2));
                    faces.Add(Cylinder(p1, p2, cylRad, numSides));
                    seen.Add(hash);
                }
            }
        }

        // geom helper
        class GH
        {
            public List<Vec3> vertices = new();

            // get vertex index, adds if needed
            public int VertexIndex(Vec3 v, double tolerance = 1e-5)
            {
                // todo - speed up if needed
                for (int i = 0; i < vertices.Count; i++)
                {
                    if ((v - vertices[i]).Length < tolerance)
                        return i;
                }
                vertices.Add(v);
                return vertices.Count - 1;
            }

            public List<List<int>> faces = new();

            public void AddFace(params Vec3[] pts) =>
                faces.Add(pts.Select(v => VertexIndex(v)).ToList());
        }

        // make geodesic polyhedron
        // todo - move to Lomont.Geometry
        // todo - implement all types at 
        // https://en.wikipedia.org/wiki/Geodesic_polyhedron
        static (List<double> vertices, List<int> faces) GeodesicPolyhedron(
            PolyhedraData basePolyhedron,
            int edgeSubdivisions
            )
        {
            // base shape is Icosahedron
            //var v = Lomont.Geometry.Polyhedra.Icosahedron.Vertices;
            //var f = Lomont.Geometry.Polyhedra.Icosahedron.FaceDefinitions;
            //v = Lomont.Geometry.Polyhedra.Octahedron.Vertices;
            //f = Lomont.Geometry.Polyhedra.Octahedron.FaceDefinitions;
            //v = Lomont.Geometry.Polyhedra.Tetrahedron.Vertices;
            //f = Lomont.Geometry.Polyhedra.Tetrahedron.FaceDefinitions;
            //v = Lomont.Geometry.Polyhedra.Dodecahedron.Vertices;
            //f = Lomont.Geometry.Polyhedra.Dodecahedron.FaceDefinitions;
            var v = basePolyhedron.Vertices;
            var f = basePolyhedron.FaceDefinitions;

            var gh = new GH();

            Vec3 GetV(int index)
            {
                index *= 3;
                double x = v[index];
                double y = v[index+1];
                double z = v[index+2];
                return new Vec3(x, y, z);
            }

            // subdivide each face edge into n pieces
            int n = edgeSubdivisions;

            int flen = f.Count;
            //Trace.Assert((flen%4) == 0); // should all of of form 3,a,b,c where a,b,c is vertex index

            int fIndex = 0;
            // for (int fIndex = 0; fIndex < flen; fIndex += 4)
            while (fIndex < flen)
            {
                int sides = f[fIndex++];
                List<Vec3> tris = new();
                if (sides > 3)
                {
                    var center = new Vec3(0,0,0);
                    for (int i = 0; i < sides; ++i)
                        center += GetV(f[i+fIndex]);
                    center /= sides;

                    for (int i = 0; i < sides; ++i)
                    {
                        tris.Add(center);
                        tris.Add(GetV(f[fIndex+i]));
                        tris.Add(GetV(f[fIndex + ((i+1)%sides)]));
                    }

                    fIndex += sides;

                }
                else
                {
                    tris.Add(GetV(f[fIndex++]));
                    tris.Add(GetV(f[fIndex++]));
                    tris.Add(GetV(f[fIndex++]));
                }

                for (int triIndex = 0; triIndex< tris.Count; triIndex += 3)
                {
                    Vec3 p0 = tris[triIndex], p1 = tris[triIndex + 1], p2 = tris[triIndex + 2];
                    var dist = p0.Length; // dist from origin

                    Vec3 d1 = (p1 - p0) / n, d2 = (p2 - p0) / n, d3 = (p2 - p1) / n; // edge directions

                    for (int i = 0; i < n; ++i)
                    for (int j = 0; j < n - i; ++j)
                    {
                        var v0 = p0 + i * d1 + j * d2;
                        var v1 = v0 + d1;
                        var v2 = v0 + d2;
                        v0 *= dist / v0.Length;
                        v1 *= dist / v1.Length;
                        v2 *= dist / v2.Length;
                        gh.AddFace(v0, v1, v2); // counter clockwise
                        if (i != 0)
                        {
                            v0 = p0 + i * d1 + j * d2;
                            v1 = v0 + d2;
                            v2 = v0 + d3;
                            v0 *= dist / v0.Length;
                            v1 *= dist / v1.Length;
                            v2 *= dist / v2.Length;
                            gh.AddFace(v0, v1, v2); // counter clockwise
                        }
                    }
                }
            }

            // make list of vertices and list of faces
            var outFaces = new List<int>();
            foreach (var face in gh.faces)
            {
                outFaces.Add(face.Count);
                outFaces.AddRange(face);
            }

            var outVerts = new List<double>();
            foreach (var vt in gh.vertices)
            {
                outVerts.Add(vt.X);
                outVerts.Add(vt.Y);
                outVerts.Add(vt.Z);
            }

            return (outVerts, outFaces);
        }

     

        static (List<Vec3> points, List<(int, int)> edges) FibonacciSpherePoints(int n)
        {
            var phi = Math.PI * (Math.Sqrt(5) - 1.0); // golden mean * 2pi
            List<Vec3> verts = new();
            for (int i = 0; i < n; ++i)
            {
                double y = 1 - i*(2.0 / (n - 1)); // 1 to -1 uniformly
                double radius = Math.Sqrt(1 - y * y); // radius at ht y
                double theta = phi * i;
                double x = Math.Cos(theta) * radius;
                double z = Math.Sin(theta) * radius;
                verts.Add(new (x, y, z));
            }

            return (verts, Edges(verts));

        }


        // compute n points spaced nicely on a sphere
        // follows a spiral
        static (List<Vec3> points, List<(int, int)> edges) SpherePoints(int n)
        {
            var verts = new List<Vec3>();

            // usual spherical coords
            Vec3 Sph(double x, double y) => new(Cos(x) * Cos(y), Sin(x) * Cos(y), Sin(y));
            double s0 = -1 + 1.0 / (n - 1);
            double ds = (2 - 2.0 / (n - 1)) / (n - 1);
            double x = 0.1 + 1.2 * n;
            for (int i = 0; i < n; ++i)
            {
                double s = s0 + i * ds;
                verts.Add(Sph(s * x, Math.PI / 2 * Math.Sign(s) * (1 - Math.Sqrt(1 - Abs(s)))));
            }

            var edges = Edges(verts);
            return (verts, edges);
        }

        static List<(int, int)> Edges(List<Vec3> verts)
        {
            int n = verts.Count;

         

            //var e1 = new List<(int, int)>();
            //for (int i = 0; i < n-1; ++i)
            //    e1.Add((i,i+1));
            //return (verts, e1);

            List<(int i,int j,double d)> dists = new();

            // compute edges:
            for (int i =0 ; i < n; ++i)
            for (int j = i + 1; j < n; ++j)
            {
                var dist = (verts[i] - verts[j]).Length;
                dists.Add((i, j, dist));
            }
            dists.Sort((p1,p2)=>p1.d.CompareTo(p2.d));
            
            //Console.WriteLine(dists.Aggregate("", (s, p) => s + ", " + p.d));


            // add edges short to large until every vert has at least 3 neighbors
            // AND some min val passed
            int nextDistIndex = 0;
            int[] neighborCount = new int[n];
            List<(int, int)> edges = new List<(int, int)>();

            var minD = dists[0].d;
            double lastD = 0, cutoff = 2.5;

            do
            {
                bool updated = false;
                while (!updated && nextDistIndex < dists.Count)
                {
                    var (i, j, d) = dists[nextDistIndex++];
                    lastD = d;
                    if (neighborCount[i] < 5 && neighborCount[j] < 5)
                    {
                        neighborCount[i]++;
                        neighborCount[j]++;
                        updated = true;
                        edges.Add((i,j));
                    }

                }

                if (nextDistIndex >= dists.Count) break;
            } while ((neighborCount.Any(c=>c<3) || lastD < minD * cutoff));// && (nextDistIndex < n));

            return edges;
        }


    }
}
