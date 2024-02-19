using Lomont.SimpleShapes;
using Lomont.SimpleShapes.Shape2D;

// todo - simplify includes....
using static Lomont.SimpleShapes.SimpleShape2D;


namespace Lomont.Projects
{
    [Project("Chris Lomont", "Spiral N-gon art", "4/21/21")]
    class SpiralNGon
    {
        // spiral ngon
        public void Run()
        {
            // center of ngon to center of face
            double Height(double sideLength, int sides)
            {
                return (sideLength / 2) * Math.Sin(Math.PI * (0.5 - 1.0 / sides)) / Math.Sin(Math.PI / sides);
            }
            Node NGon(
                int sides,
                int steps = 12,
                double sideLength = 100.0,
                double thick = 4.0,
                double scaling = 0.875,
                double rotation = Math.PI / 2
                )
            {
                var angle = 0.0;
                var da = rotation / (steps - 1);
                var (max, min) = (100.0, 20.0);
                Node rings = Group();
                var ht = Height(sideLength, sides);
                var radius = Math.Sqrt(sideLength * sideLength / 4 + ht * ht);
                for (var i = 0; i < steps; ++i)
                {
                    var p1 = Path(new Contour());
                    var p2 = Path(new Contour());
                    for (var j = 0; j < sides; ++j)
                    {
                        var sa = j * Math.PI * 2 / sides;
                        var v = Point(Math.Cos(sa), Math.Sin(sa));
                        p1.Contours[0].Points.Add(radius * v);
                        p2.Contours[0].Points.Add((radius - thick) * v);
                    }
                    var ring = Difference(p1, p2);
                    rings = Union(rings, Rotate(angle, ring));
                    angle += da;
                    radius *= scaling;
                }
                return Group(rings).Stroke(0.1).Fill(None);
            }

            var sideLength = 100.0;
            var steps = 12;
            var tri = NGon(3, steps, sideLength);
            var square = NGon(4, steps, sideLength);
            var hex = NGon(6, steps, sideLength);

            // squares are top and sides of hex
            // var ang = Math.PI / 6;
            var h6 = Height(sideLength, 6);
            var squares = //Group(
                          //Transform(Math.PI/4,0,-h6-sideLength/2,square),
                Transform(Math.PI / 4, 0, h6 + sideLength / 2, square)
            //)
            ;

            var final = Group(
                //Translate(0,0,tri),
                hex,
                squares
            );

            final.Save("Rects.svg");
            final.Save("Rects.png");
        }


    }
}
