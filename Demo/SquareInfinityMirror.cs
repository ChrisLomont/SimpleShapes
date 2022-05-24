using static Lomont.SimpleShapes.SimpleShape2D;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "Square inifnity mirror sides", "5/19/21")]
    class SquareInfinityMirror
    {
        double sideLen = 101.70; // side length
        double stripWidth = 12.0;
        double thickness = 2.9; // 1/8" thick acrylic
        double hole = 6.0;
        double spacing = 1.0;
        public void Run()
        {

            var g = Group();
            for (var i = 0; i < 4; ++i)
            {
                var w = sideLen;
                var h = stripWidth;

                if (i > 1)
                    w -= 2 * thickness; // shorter
                if (i == 3)
                    w -= hole;

                var r = Translate(
                    0,(h+spacing)*i,
                    Rect(0, 0, w, h)
                );
                g.Add(r);
            }
            //g.Stroke(Red,)
            g.Save("MirrorTemplate.svg");
        }
    }
}
