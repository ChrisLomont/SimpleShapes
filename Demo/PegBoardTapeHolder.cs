using System;
using System.Linq;
using Lomont.Graphics;
using Lomont.Numerical;
using static Lomont.SimpleShapes.SimpleShape2D;
using D = Lomont.SimpleShapes.DodecahedronInfo;
using P = Lomont.SimpleShapes.PolyhedronInfo;
using static System.Math;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using Lomont.SimpleShapes.Shape2D;
using Path = Lomont.SimpleShapes.Shape2D.Path;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "peg board tape holder", "12/28/2022")]
    class PegBoardTapeHolder
    {
        static double kerf = (1.0 / 20); // gap in inches


        static double depth = 4 * 25.4; //  // bottom lip length (without fillet)
        static double width = 0.80 * 25.4;   // thickness of wood to cut from
        static double filletRadius = 0.4 * 25.4; // fillet radius
        static double slatThickness = (0.25+kerf) * 25.4; // how thick insert is
        static double slatDelta = slatThickness * 0.7; // how deep from edge
        static double notchRadius = slatThickness * 0.75; // hole at slat end
        static double height = 6 * 25.4; // this many pegs tall
        static double angle = FromDegrees(20); // angle from horiz to base
        static double drillRadius = (0.25 / 2.0)*25.4;


        public void Run()
        {
            var item1 = MakeOne();
            item1.Save("PegBoardTapeHolder1.svg");
            var item2 =
                    Scale(1, -1, MakeOne()
                    );
            item2.Save("PegBoardTapeHolder2.svg");

            var drill = Group(
                MakeDrill()
                    );
            
            drill.Save("PegBoardTapeHolder3.svg");

            var drillAndOne = Group(
                MakeOne(),
                MakeDrill()
            );

            drillAndOne.Save("PegBoardTapeHolder4.svg");


        }

        Node MakeDrill()
        {
            var rect = Rect(0, 0, width, height);
            Node c = Group();
            var spacing = 1 * 25.4;
            for (var i = spacing; i <= height-spacing; i += spacing)
            {
                c = Group(c,
                    Circle(width/2,i,drillRadius)
                );

            }
            return Group(rect,c);
        }


        Node MakeOne()
        {

            var boxView = Group(
                Line(0, 0, depth, 0),
                Line(0, 0, 0, height)
                //Rect(0,0,depth,height)
                );



            var p1 = Point(0, 0);
            var p2 = depth * Point(Cos(angle), Sin(angle));
            var p3 = Point(0,height);
            var sideView1 = Path(
                p1, p2, p3
            );
            var sideView = Fillet(sideView1, filletRadius);


            // slat markers
            var inset = slatDelta + slatThickness;
            Node slat1 = Union(
                Rect(
                    slatDelta,inset,
                    slatDelta+slatThickness,height+slatThickness
                ),

                Rotate(angle,
                Rect(
                    inset,slatDelta,
                    depth + slatThickness, slatDelta + slatThickness
                ))
                //,Circle(
                //    slatDelta+slatThickness/2, 
                //    inset+slatThickness/2, 
                //    notchRadius)
                );

            var slx = Point(slatThickness, 0);
            var sly = Point(0,slatThickness);


            var (int1, int2, circ) = FilletInfo(p2, p1, p3, 5);
            
            Path slat2 = Path(
                int2,p3,p3+slx,p1+slx+sly,int2
            );
            var perp = slatThickness * Perp(Dir(angle));
            var p4 = p2 + perp;
            Path slat3 = Path(
                p4,
                p2,int1,
                
                slatThickness*(p2-p1).Normalize()+perp,

                p4
            );

            var slat = Union(slat2, slat3, circ);

            var dy1 = slatDelta * Math.Tan(angle);
            var dy2 = Math.Sqrt(dy1*dy1+ slatDelta * slatDelta);

            slat = Translate(slatDelta, dy1+dy2, slat);

            //Node slat = Fillet(slatPath, 0.1);

            slat = Intersection(slat, sideView);

            var item1 = Group(
                sideView,
                slat
                ,boxView
                );
            return item1;
            

#if false

            int numSides = 100; // sphere and cylinder divisions

            var radius = 50; // mm radius size of outer object
            var cylRad = 2.5; // tube radius

            // polyhedra vert, face definitions
            var v = Lomont.Geometry.Polyhedra.DeltoidalIcositetrahedron.Vertices;
            var f = Lomont.Geometry.Polyhedra.DeltoidalIcositetrahedron.FaceDefinitions;

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
                verts.Add(Sphere(c, cylRad, numSides, numSides));
            }

            // axes
            var tz = cylRad;
            var ty = 12.0;
            var tx = 2 * radius - cylRad;

            faces.Add(Cube(tx, ty, tz));
            faces.Add(Cube(ty, tx, tz));

            // make solid shape
            Node ball = Union(Group(faces), Group(verts));


            // make holes
            var holes = new List<Node>();
            var holeDiam = 3.4; // mm
            var numHoles = 5; // make odd
            for (var i = 0; i < numHoles; ++i)
            {
                var c = Cylinder(2 * tz, holeDiam / 2);
                var del = (i - numHoles / 2) * (tx - cylRad * 5) / numHoles;
                holes.Add(Translate(del, 0, -tz, c));

                c = Cylinder(2 * tz, holeDiam / 2);
                holes.Add(Translate(0, del, -tz, c));
            }


            var g = Difference(ball, Group(holes));

            g.Save("IMUCalibrator.scad");

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
#endif

        }
    }
}
