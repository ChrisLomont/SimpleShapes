using Lomont.SimpleShapes;
using Lomont.SimpleShapes.Shape2D;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc.Routing;
using static Lomont.SimpleShapes.SimpleShape2D;
//using Point = Lomont.Lib3D.Point;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "Cover for 64x64 leds", "9/26/21")]
    class Leds64x64
    {
        public void Run()
        {
            // one of the panels
            var w = 95.8; // 
            var h = 191.8; // 
            Node MakeOne(bool left)
            {

                // from center of panels:
                var screenHole = Point(30.0, 87.0); // 4, symmetric
                var nub = Point(15.0, 88.0); // 2, anti symmetric
                var revHole = Point(39.0,87.0); // 4, symmetric


                var holeDiameter = Parts.ScrewClearanceDiameter["M3"];
                var nubDiameter = holeDiameter;
                var cornerRadius = 10.0;



                // spacing
                var outBorder = 10.0; // extra on each side
                var inBorder = 15.0; // amount inside, then box in middle



                //holeBorder + holeDiameter / 2;


                // shape:
                var rect = Rect(-outBorder, -outBorder, w + outBorder, h + outBorder, cornerRadius, cornerRadius)
                    .Fill(Blue);

                var inner = Rect(inBorder, inBorder, w - inBorder, h - inBorder, cornerRadius, cornerRadius).Fill(Blue);



                // holes to attach to screens
                var (hx, hy) = screenHole;
                var screenHoles = Group(
                    Circle(w / 2 + hx, h / 2 + hy, holeDiameter / 2),
                    Circle(w / 2 - hx, h / 2 + hy, holeDiameter / 2),
                    Circle(w / 2 + hx, h / 2 - hy, holeDiameter / 2),
                    Circle(w / 2 - hx, h / 2 - hy, holeDiameter / 2)
                );

                // holes to attach to backing
                var (rx, ry) = revHole;
                var revHoles = !left
                    ? Group(
                        Circle(w / 2 + rx, h / 2 + ry, holeDiameter / 2),
                        //Circle(w / 2 - rx, h / 2 + ry, holeDiameter / 2),
                        Circle(w / 2 + rx, h / 2 - ry, holeDiameter / 2)
                        //Circle(w / 2 - rx, h / 2 - ry, holeDiameter / 2)
                    )
                        : Group(
                            //Circle(w / 2 + rx, h / 2 + ry, holeDiameter / 2),
                            Circle(w / 2 - rx, h / 2 + ry, holeDiameter / 2),
                            //Circle(w / 2 + rx, h / 2 - ry, holeDiameter / 2),
                            Circle(w / 2 - rx, h / 2 - ry, holeDiameter / 2)
                        );


                // nubs for alignment
                var (nx, ny) = nub;
                var nubs = Group(
                    Circle(w / 2 - nx, h / 2 + ny, nubDiameter / 2),
                    Circle(w / 2 + nx, h / 2 - ny, nubDiameter / 2)
                );

                // outermost holes to attach to top
                var s = left ? -1 : 1;
                var (ox, oy) = (w/2+outBorder/2,h/2+outBorder/2);
                var outer = Group(
                    Circle(w / 2 + s*ox, h / 2 + oy, nubDiameter / 2),
                    Circle(w / 2 + s*ox, h / 2 - oy, nubDiameter / 2)
                );

                var panel = Difference(rect, Group(screenHoles, inner, nubs, revHoles,outer))
                    //.Stroke(0.1)
                    //.Fill(None)
                    ;
                //var text = Text(cx * 2, cx - 2, 50, 0, "Chris Lomont 2021").Stroke(0.0).Fill(Blue);
                //var final = Group(panel, text);
                return panel;
            }

            var left  = MakeOne(true);
            var right = MakeOne(false);
            var dual = Union(
                left,
                Translate(h-w,0,right)
                )
                .Stroke(0.1)
                .Fill(None)
                ;
            dual.Save("Leds64x64.svg");
        }
    }
}
