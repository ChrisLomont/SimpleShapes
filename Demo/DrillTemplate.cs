using Lomont.SimpleShapes;
using static Lomont.SimpleShapes.SimpleShape2D;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "Drill templates for laser table", "4/24/21")]
    class DrillTemplate
    {
        /// <summary>
        /// Template for drilling holes for 4x4 ends on table
        /// Chris Lomont 4/25/2021
        /// </summary>
        public void Run()
        {
            var s = 92.4; // side length
            var r = InToMM("1/8") / 2; // todo - drill hole radius
            var amt = 1.0 / 3.0; // ratio in for drill holes along diagonal

            var p1 = Point(s * amt, s * amt); // one corner in
            var p2 = Point(s, s) - p1; // other corner in

            var rect1 = Rect(0, 0, s, s);
            var c1 = Circle(p1, r);
            var c2 = Circle(p2, r);

            var template1 = Difference(rect1, Group(c1, c2)).Stroke(0.5).Fill(None);
            template1.Save("DrillTemplate4x4.svg");

            // for 2x4 pieces (assuming actually 3.5" x 1.5"
            var s2 = 20.0; // 1.5 * 25.4;
            var s4 = 3.5 * 25.4;
            var rect2 = Rect(0, 0, s2, s4);
            var pb1 = Point(s2 / 2, s4 * amt); // one edge in
            var pb2 = Point(s2 / 2, s4 * (1 - amt)); // other edge in
            var cb1 = Circle(pb1, r);
            var cb2 = Circle(pb2, r);

            var template2 = Difference(rect2, Group(cb1, cb2)).Stroke(0.5).Fill(None);
            template2.Save("DrillTemplate2x4.svg");
        }


    }
}
