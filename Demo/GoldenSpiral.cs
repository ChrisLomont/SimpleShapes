using Lomont.Numerical;
using static Lomont.SimpleShapes.SimpleShape2D;
using static System.Math;

namespace Lomont.Projects
{

    [Project("Chris Lomont", "Golden mean spiral art", "4/28/21")]
    class GoldenSpiral
    {
        static double phi = (1 + Sqrt(5)) / 2;
        public void Run()
        {
            // spiral grows by phi every 1/4 turn

            var angle = PI * 4; // amount of turn
            var steps = 100; // steps 
            var pts1 = new List<Vec2>();
            var pts2 = new List<Vec2>();
            for (var i = 0; i < steps; ++i)
            {
                var theta = i * angle / (steps-1)-PI;
                var r = Pow(phi, 2*theta / PI);
                pts1.Add(Dir(theta) * r);
                pts2.Add(-Dir(theta) * r);
            }

            var p1 = Path(pts1);
            var p2 = Path(pts2);

            var final = Group(p1,p2);
            final.Save("Golden.svg");

        }
    }
}
