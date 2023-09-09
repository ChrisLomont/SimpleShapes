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
    [Project("Chris Lomont", "christmas light holder", "1/8/2023")]
    class ChristmasLightHolder
    {


        public void Run()
        {
            var (w, h) = (11.5, 10.0);

            var w1 = 3.0*25.4;
            var w2 = 4.0*25.4;
            var h1 = 3.0*25.4;
            w *= 25.4;
            h*=25.4;

            var p0 = Point(0,0);
            var p1 = Point(0, h);
            var p2 = Point(w1,h);
            var p3 = Point(w2, h-h1);
            var p4 = Point(w-w2, h-h1);
            var p5 = Point(w-w1, h );
            var p6 = Point(w, h);
            var p7 = Point(w, 0);
            var p8 = Point(w-w1, 0);
            var p9 = Point(w - w2, h1);
            var p10 = Point(w2, h1);
            var p11 = Point(w1, 0);

            var filletRadius = 0.5*25.4;
            var border = 2.0*25.4;

            var holeRad = 0.5 * 25.4;
            var dir1 = Point(w, h).Normalize();
            var dir2 = Point(w, -h).Normalize();

            var hole1 = Circle(dir1 * border, holeRad);
            var hole2 = Circle(p6-dir1*border, holeRad);
            var hole3 = Circle(p1+dir2* border, holeRad);
            var hole4 = Circle(p7 - dir2* border, holeRad);
            var holes = Group(hole1,hole2,hole3,hole4);

            var sideView1 = Path(
                p0, p1, p2, p3, p4, p5,p6,p7,p8,p9,p10,p11
            );

            var (nx, ny) = (7, 20);
            var notches = Union(
                Translate(w / 2 - nx / 2, h1 - ny / 3, MakeNotch()),
                Translate(w / 2 - nx / 2, h-h1 - 2*ny / 3, MakeNotch())
            );

            Node MakeNotch() => Fillet(Path(Point(0,0), Point(nx,0), Point(nx,ny), Point(0,ny)), nx/2);

            var sideView = Fillet(sideView1, filletRadius, 
                new[]
                {
                    true,true,
                    false,false,
                    true,true,true,true,
                    false,false,
                    true,true
                }
                );


            Node g = Group(sideView, holes);
            g = Difference(g, notches);
            g.Save("ChristmasLightHolder.svg");

        }
    }
}
