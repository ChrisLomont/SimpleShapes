using System;
using Lomont.Numerical;
using Lomont.SimpleShapes;
using Lomont.SimpleShapes.Shape2D;

// todo - simplify includes....
using static Lomont.SimpleShapes.SimpleShape2D;


namespace Lomont.Projects
{
    [Project("Chris Lomont", "Random circles test", "4/15/21")]
    class RandomCircles
    {
        public void Run()
        {
            var size = 100.0;
            var border = 5.0;
            var outerRect = Rect(0, 0, size, size).Fill(None);
            var innerRect = Rect(border, border, size - border, size - border).Fill(None);
            var finalRect = Difference(outerRect, innerRect);

            // random circles
            var rand = new Random(34546);

            double Rnd(double a, double b) => (b - a) * rand.NextDouble() + a;
            Vec2 Pt(double a, double b) => Point(Rnd(a, b), Rnd(a, b));

            Node rings = Group();//Group().Thickness(0.1).Fill(Green);
            var thick = 2.0;
            for (var i = 0; i < 80; ++i)
            {
                var radius = Rnd(size / 20, size / 10);
                var center = Pt(border, size - border);
                var ring = Difference(
                    Circle(center, radius),
                    Circle(center, radius - thick / 2)
                    );
                rings = Union(rings, ring);
                //rings.Add(ring);
            }

            // clip against outer rect
            var clip = Intersection(rings, outerRect).Stroke(0.1).Fill(None);

            // union all
            var final = Union(clip, finalRect).Stroke(0.1).Fill(None);

            //var final = Group(finalRect,clip);
            final.Save("Art1.svg");
            final.Save("Art1.png");

        }


    }
}
