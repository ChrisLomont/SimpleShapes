using Lomont.SimpleShapes;
using System;
using static Lomont.SimpleShapes.SimpleShape2D;


namespace Lomont.Projects
{
    [Project("Chris Lomont", "Grid panel for WS2812 LEDs", "4/17/21")]
    class LedGrid
    {
        /// <summary>
        /// Make transparent panels for a 6x6 WS2812 grid
        /// 60 LEDs/m style
        /// Chris Lomont 4/20/2021
        /// </summary>
        public void Run()
        {
            var stripLength = 100.0; // mm for 6, since 60/m
            var cornerRadius = 0.0; //5.0;
            var holeDiameter = Parts.ScrewClearanceDiameter["M3"];
            var holeBorder = 4.0;

            //holeBorder + holeDiameter / 2;

            // computed
            var borderSize = 10.0;
            var side = stripLength + 2 * borderSize;
            var cx = holeBorder + cornerRadius + holeDiameter / 2;

            Console.WriteLine($"Side {side}");

            // shape:
            var rect = Rect(0, 0, side, side, cornerRadius, cornerRadius).Fill(Blue);
            var holes = Group(
                Circle(cx, cx, holeDiameter / 2),
                Circle(side - cx, cx, holeDiameter / 2),
                Circle(cx, side - cx, holeDiameter / 2),
                Circle(side - cx, side - cx, holeDiameter / 2)
                );
            var panel = Difference(rect, holes)
                    .Stroke(0.1)
                    .Fill(None)
                ;
            var text = Text(cx * 2, cx - 2, 50, 0, "Chris Lomont 2021").Stroke(0.0).Fill(Blue);
            var final = Group(panel, text);


            final.Save("LEDPanel.svg");

        }

    }
}
