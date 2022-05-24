using static Lomont.SimpleShapes.SimpleShape2D;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "one off projects", "5/20/21")]
    class OneOffs

    {
        /// <summary>
        /// Shiela birthday 5/22/21
        /// </summary>
        public void Run()
        {
            MakeConnectedText("Chris", thicken: 1.0).Save("Chris2021.svg");
            MakeConnectedText("Sheila", thicken: 1.0).Save("Sheila2021.svg");
            MakeConnectedText("Stacie", thicken: 1.0).Save("Stacie2021.svg");
            MakeConnectedText("Brennan", thicken: 1.0).Save("Brennan2021.svg");
            MakeConnectedText("Scott", thicken: 1.0).Save("Scott2021.svg");
            MakeConnectedText("Stephen", thicken: 1.0).Save("Stephen2021.svg");
            MakeConnectedText("Sarah", thicken: 1.0).Save("Sarah2021.svg");
        }



        // circles, for testing mirrors
        public void Run1()
        {
            var radius = 25.4; // 1"
            var g1 = Circle(0, 0,radius);
            var g2 = Circle(0, 2*radius+1, radius);
            Group(g1,g2).Save("MirrorCircles.svg");
        }
    }
}
