using System;
using Lomont.Graphics;
using Lomont.SimpleShapes;
using static Lomont.SimpleShapes.SimpleShape3D;


namespace Lomont.Projects
{
    [Project("Chris Lomont", "testing 3D shapes", "6/28/2021")]
    class Testing3D
    {
        /// <summary>
        /// testing OpenSCAD generation
        /// </summary>
        public void Run()
        {

            var cube1 = Rotate(
                120,45,72,
                Cube(4,5,6).Color(Red)
                );
            var cube2 = Cube(Point(-1,-1,-1),Point(0.5,-2,-4));
            var sph1 = Sphere(Point(-10,-10,0),1.5,120,120);
            var cyl1 = Cylinder(5.0, 1.0).Color(Green);


            var p5 = Translate(Point(-5,5,0),NGon(7, 1.0).Color(Blue));
            var poly = LinearExtrude(p5, 0.5).Color(Cyan);

            var diff = 
                Translate(Point(-5,2,2),
                Difference(
                    Sphere().Color(Blue),
                    Cube().Color(Red)
                )
            );

            var cc = Group();
            var i = 0;
            foreach (var c in colors)
            {
                var angle = i* 2*Math.PI/ colors.Length;
                var s = 
                    Rotate( angle,0,angle,
                    Translate(
                    Point(8,0,-4),
                    Sphere().Color(c)
                    )
                );
                ++i;
                cc.Children.Add(s);
            }

            var p1 = Point(1, 1, 1);
            var a = Math.PI / 4;
            var c2 = 
                Union(
                    Cube(-p1,p1).Color(Red), 
                    Rotate(a,a,a, Cube(-p1,p1).Color(Green)),
                    Cube(Point(-2,-2,-2),Point(0,2,2)).Color(Yellow)
                    );
            var c3 =
                Translate(Point(-2,0,0),
                Difference(
                Xor(
                    Cube(-p1, p1).Color(Red),
                    Rotate(a, a, a, Cube(-p1, p1).Color(Green))
                    ),
                Cube(Point(-2, -2, -2), Point(0, 2, 2)).Color(Yellow)
                ));
            var c4 = Translate(Point(0,0,3),Group(c2, c3));

            var g = Group(cube1,cube2,sph1, cyl1, poly,diff,cc,c4);


            //Group(c2,c3).Save("Testing.scad");
            g.Save("Testing.scad");

        }


        ColorB [] colors = new []
        {
            Black,
            DarkBlue,DarkCyan,DarkGreen,DarkMagenta,DarkRed,DarkYellow,
            Gray,
            Blue,Cyan,Green,Magenta,Red,Yellow,
            White
        };


    }
}