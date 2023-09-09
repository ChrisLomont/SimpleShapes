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
    [Project("Chris Lomont", "Random Circle Art II", "1/14/2023")]
    class RandomCirclesII
    {


        public void Run()
        {
            //RR(1234, 0, 1.0 / 2, 1.0 / 6, 350, 1.00001, 2.0, 1).Save("CirclesII.svg");
            RR(
                1234, 
                1, 
                1.0 / 2, 1.0 / 7, 
                300, 
                1.0000001, 
                2.0, 
                1)
                .Save("CirclesII.svg");
        }

        Node RR(
            int seed, 
            int startPass, 
            double outerRadiusRatio, 
            double innerRadiusRatio,
            int numCircs,
            double scaling, // value > 1 to converge shapes
            double thickness,
            int stepDivisor

            )
            {
            // size of outside - size to cut from :)
            var (w, h) = (11.5 * 25.4, 11.5 * 25.4);

            var r = new Random(seed); // reproducible
            double Rnd(double min, double max) => r.NextDouble() * (max - min) + min;

            // define region to fill: 
            // - square, disk, ring, looped rings....

            var outerRad = w * outerRadiusRatio;
            var innerRad = w* innerRadiusRatio;

            var area = Math.PI*(outerRad*outerRad-innerRad*innerRad); // area to fill

            var n = numCircs; // items to try and fill

            var r2 = (outerRad - innerRad) / 2;
            var baseArea = Math.PI*r2*r2*0.95; // area of first item


            List<(Vec2 c, double r)> circles = new List<(Vec2 c, double r)>();
            // add center circle to skip
            circles.Add(new(new Vec2(w/2,h/2),innerRad));


            bool Intersects(Vec2 p, double rad, List<(Vec2 c, double r)> circles, int ignore = -1)
            {
                // ensure in bounds:
                if ((p - new Vec2(w / 2, h / 2)).Length + rad > outerRad)
                    return true;

                var index = -1;
                foreach (var (c, r) in circles)
                {
                    ++index;
                    if (index == ignore) continue;
                    var d = (p - c).Length;
                    if (d < rad + r) return true;
                }
                return false;
            }
            for (int pass = startPass; pass < n+ startPass; ++pass)
            {
                // new area:
                var curArea = baseArea * Math.Pow((pass + stepDivisor) /stepDivisor, -scaling);

                // fill with circles:
                // A = pi r^2
                var curRad = Math.Sqrt(curArea / Math.PI);

                Vec2 p = new Vec2();
                while (true)
                {
                    var x = Rnd(0, w);
                    var y = Rnd(0, h);
                    p = new Vec2(x, y);
                    if (!Intersects(p, curRad, circles))
                        break;
                }
                circles.Add(new(p, curRad));
            }

            double ComputeMaxRad(int circleIndex)
            {
                var (p, rad) = circles[circleIndex];
                var rad2 = rad;

                // mult by 1.5 till intersects something
                while (!Intersects(p, rad2, circles, circleIndex))
                    rad2 *= 1.5;

                // binary search to edge
                var lo = rad;
                var hi = rad2;
                while (hi - lo > 1e-4)
                {
                    var mid = (lo+hi)/2;
                    if (Intersects(p, mid, circles, circleIndex))
                        hi = mid;
                    else
                        lo = mid;
                }

                return (lo + hi) / 2;
            }

            // percolate - enlarge circles
            for (var k = 0; k < circles.Count*4; ++k)
            {
                var i = r.Next(circles.Count - 1) + 1; // skip circle 0 center
                i = ((k) % (circles.Count - 1)) + 1; // all of them, above is rand order
#if true
                circles[i] = circles[i] with { r = ComputeMaxRad(i) };
#else

                var (p,rad) = circles[i];
                var rad2 = rad;
                int pass = 0;
                while (pass++ < 1000)
                {
                    rad2 = rad * Rnd(1,1.1);
                    if (!Intersects(p, rad2, circles, i))
                        break;
                }

                if (!Intersects(p, rad2, circles, i))
                    circles[i] = circles[i] with {r=rad2 };
#endif
            }

            // add outer
            circles.Add(new(new Vec2(w / 2, h / 2), outerRad));

            // draw circles as disks
            var disks = new List<Node>();
            foreach (var (p, rad) in circles)
            {
                if (2*rad < thickness) continue;
                var co = Circle(p, rad + thickness / 2);
                var ci = Circle(p,rad-thickness/2);
                var disk = Difference(co, ci);
                disk.Fill(Blue).Stroke(Blue);
                disks.Add(disk);
            }

            Console.WriteLine("Making union");
            var g = Union(disks[0], disks[1]);
            for (var k = 2; k < disks.Count; k++)
            {
                g = Union(g, disks[k]);
                Console.WriteLine($"{k} of {disks.Count}");
            }

            return g;


        }
    }
}
