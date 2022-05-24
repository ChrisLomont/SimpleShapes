using Lomont.SimpleShapes;
using static Lomont.SimpleShapes.SimpleShape2D;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "Simple box with text around it", "4/16/21")]
    class TextAndBox
    {
        // Text with box around it
        public void Run()
        {
            var text = Union(
                Text(0, 0, 100, 0, "CHRIS"),
                Text(0, 30, 100, 0, "LOMONT")
                );

            var thickness = 1.0;
            var height = 51;
            var rect = Difference(
                Rect(0, 0, 100, height),
                Rect(thickness, thickness, 100 - thickness, height - thickness)
            );

            var final = Union(text, rect).Fill(None).Stroke(0.1);
            final.Save("Art2.svg");
            final.Save("Art2.png");

        }
        // test art piece

    }
}
