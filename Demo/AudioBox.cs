using Lomont.Numerical;
using Lomont.SimpleShapes;
using Lomont.SimpleShapes.Shape2D;
using static Lomont.SimpleShapes.SimpleShape2D;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "Audio box for 6-pole double throw audio switch", "8/10/21")]
    class AudioBox
    {
        /// <summary>
        /// Make 6 sides for an audio switcher box
        /// Switches L+R (3 wires each) between two outputs
        /// Chris Lomont 8/10/2021
        /// </summary>
        public void Run()
        {

            var t = InToMM("1/8"); // material thickness
            var cutWidth = LaserMaterials.cutSize; // press fit 1/8" cut size

            var b = 2 * t / 3; // border around holes
            var holeSize = Point(cutWidth, 10.0); // tab holes

            // spacing for a PCB and mount holes 
            var pcbThroughHoleSpacing = 2.54; // 0.1 inches = 2.54mm
            var pcbMountSize = Point(38.5, 43.9); // mm for holes, long dir front to back (front = switch)
            var pcbSize = Point(45, 48.3);        // PCB outer diameter
            var pcbHoleRadius = Parts.ScrewClearanceDiameter["M2"] / 2;



            // outer dimensions, eyeballed
            var w = 140; // mm width (left to right)
            var h = 90.0; // mm width (front to back)
            var d = 30.0; // mm depth (top to bottom)

            var audioHoleDiameter = 9.3; // for audio plugs

            var kerf = 0.1; // pressfit kerf?!
            var tabLength = 5 * t; // notch tabs - center size, add sub kerf
            var tabDepth = t - kerf; // how deep to cut


            // make a hole with given center
            Node Tab(Vec2 center, bool rot = false)
            {
                var angle = rot ? FromDegrees(90.0) : 0.0;
                return
                    Translate(
                        center,
                        Rotate(angle, Translate(
                                -holeSize / 2,
                                Rect(0, 0, holeSize.X, holeSize.Y)
                            )
                        )
                    );
            }

            // want: border, delta, hole, delta, hole, delta, border sum to h
            // thus delta = (h-2*(border+hole)]/3
            var delta = (h - (2 * (b + holeSize.Y))) / 3;
            var cy = b + delta + holeSize.Y / 2;
            var cx = w / 3;

            // notch
            var hx = w / 5;
            var hy = d / 2 - (tabLength - kerf) / 2;

            Node Top(bool top)
            {
                var ep = t;
                var p1 = Point(0,ep);
                var p2 = Point(w,ep);
                var p3 = Point(w,h-ep);
                var p4 = Point(0,h-ep);
                Node panel = Rect(0, ep, w, h-ep);

                // tabs
                var notches = Union(
                    MakeNotch(p1, p2, hx, tabLength + kerf, tabDepth, -1),
                    MakeNotch(p2, p1, hx, tabLength + kerf, tabDepth,  1),
                    MakeNotch(p3, p4, hx, tabLength + kerf, tabDepth, -1),
                    MakeNotch(p4, p3, hx, tabLength + kerf, tabDepth,  1)
                    );
                panel = Union(panel,notches);

                // cut press fit tabs

                if (!top)
                {
                    // spacing for a PCB hole
                    // var pcbThroughHoleSpacing = 2.54; // 0.1 inches = 2.54mm
                    // var pcbMountSize = Point(38.5, 43.9); // mm for holes, long dir front to back (front = switch)
                    // var pcbSize = Point(45, 48.3);        // PCB outer diameter
                    // var pcbHoleRadius = Parts.ScrewClearanceDiameter["M2"] / 2;

                    var pr = pcbHoleRadius;
                    var (px, py) = pcbMountSize;

                    // pcb mounting holes, delta from centered
                    var cx = (w-px)/2; // center
                    var offset = 1.5 * (0.1) * 25.4; // 0.1" per pcb hole, off 1.5 of them
                    var mounts =
                        Translate(cx-offset,t*3,
                            Group(
                                // screw holes for corners
                                Circle(0, 0, pr),
                                Circle(0, py, pr),
                                Circle(px, 0, pr),
                                Circle(px, py, pr)
                                //,Circle(px/2+offset,0,pr*2) // text center for debugging
                            ));

                    var label =
                        Text(w / 8, h / 3, 3 * w / 4, h / 3, "Audio Board  v1.0\nChris Lomont 2021")
                            .Fill(Blue)
                            .Stroke(0.2,Blue);
                    panel = Difference(panel, mounts);
                    panel = Group(panel, label);
                }

                return panel;
            }


            Node Right()
            {
                var ep = t;
                var p1 = Point(ep, ep);
                var p2 = Point(d-ep,ep);
                var p3 = Point(d-ep,h-ep);
                var p4 = Point(ep,h-ep);
                var r = Rect(ep, ep, d - ep, h - ep);
                var hy = ((d-2*ep) / 2) - (tabLength + kerf) / 2;
                var notches = Union(
                    MakeNotch(p3, p4, hy, tabLength + kerf, tabDepth, -1),
                    MakeNotch(p1, p2, hy, tabLength + kerf, tabDepth, -1)
                    //MakeNotch(p4, p1, 0, tabLength + kerf, tabDepth, 1)
                //MakeNotch(p3, p4, hy, tabLength - kerf, tabDepth, -1)
                );
                return Union(r,notches);

                return Union(
                    r,
                    // left (in image)
                    Tab(Point(-t + b, cy)),
                    Tab(Point(-t + b, h - cy)),
                    // right (in image)
                    Tab(Point(d + t - b, cy)),
                    Tab(Point(d + t - b, h - cy))
                    );
            }


            Node Front(bool front)
            {

                var p1 = Point(0, 0);
                var p2 = Point(0, d);
                var p3 = Point(w, d);
                var p4 = Point(w, 0);
                Node r = Rect(0, 0, w , d);

                var notches = Union(
                    MakeNotch(p1, p4, hx, tabLength - kerf, tabDepth, 1),
                    MakeNotch(p2, p3, hx, tabLength - kerf, tabDepth, -1),
                    MakeNotch(p4, p1, hx, tabLength - kerf, tabDepth, -1),
                    MakeNotch(p3, p2, hx, tabLength - kerf, tabDepth, 1),
                    
                    MakeNotch(p1, p2, hy, tabLength - kerf, tabDepth, -1),
                    MakeNotch(p3, p4, hy, tabLength - kerf, tabDepth, -1)
                );

                r = Difference(r,notches);

                if (front)
                { // button hole

                    var boxhole1 = 6.0 + pcbThroughHoleSpacing; // measured for plug, add some spacing for errors
                    var bx1 = w / 2 - boxhole1/2; // centered
                    var hole2 = Rect(bx1, d / 2 - boxhole1 / 2, bx1 + boxhole1, d / 2 + boxhole1 / 2);

                    return Difference(r,hole2);
                }
                else
                { // jack holes

                    // spacing D = audio diam, x = intra spacing, t = edge thickness
                    // t+(x+D)*6+x+t=w
                    // x=(w-2*t-6*D)/7
                    var D = audioHoleDiameter;
                    var dx = (w - 2 * t - 6 * D) / 7;
                    var rad = audioHoleDiameter / 2;

                    var cy = d / 2;
                    var c1 = t + dx + rad;
                    var c2 = c1 + D + dx;
                    var c3 = c2 + D + dx;
                    var c4 = c3 + D + dx;
                    var c5 = c4 + D + dx;
                    var c6 = c5 + D + dx;

                    var holes1 = Union(
                        Circle(c1,cy,rad),
                        Circle(c2, cy, rad),
                        Circle(c3, cy, rad),
                        Circle(c4, cy, rad),
                        Circle(c5, cy, rad),
                        Circle(c6, cy, rad)
                    );

                    return Difference(r,holes1);
                }

                var panel = Union(
                    r
                    // top tabs
                    //,Tab(Point(cx, -t + b), true),
                    //Tab(Point(w - cx, -t + b), true),

                    // bottom tabs
                    //Tab(Point(cx, d + t - b), true),
                    //Tab(Point(w - cx, d + t - b), true)
                );
                if (!front)
                    return panel;

                // holes: 
                // spacing: b,t,b,DEL1,hole,(DEL2,hole)x3,DEL1,boxhole,DEL1,hole,DEL2,hole,DEL1,b,t,b = w
                // 4b+2t+4DEL1+6hole+boxhole+4DEL2 = w
                // del1 = (w-4b-2t-6hole-boxhole-4del2)/4
                // DEL1 = spacing to make all look ok
                // DEL2 = forced by audio jacks spacing
                var hole = audioHoleDiameter;
                var del2 = 16.0;   // measured from plugs (is DEL2 above + hole)
                var boxhole = 6.0 + pcbThroughHoleSpacing; // measured for plug, add some spacing for errors
                var del1 = (w - 4 * b - 2 * t - 6 * hole - boxhole - 4 * (del2 - hole)) / 4;

                var cx1 = b + t + b + del1;
                var cx2 = cx1 + del2;
                var cx3 = cx2 + del2;
                var cx4 = cx3 + del2;
                var bx = cx4 + hole + del1;
                var cx5 = bx + boxhole + del1;
                var cx6 = cx5 + del2;

                var cy1 = d / 2 - t * 1.5;

                var holes = Union(
                    Circle(cx1 + hole / 2, cy1, hole / 2),
                    Circle(cx2 + hole / 2, cy1, hole / 2),
                    Circle(cx3 + hole / 2, cy1, hole / 2),
                    Circle(cx4 + hole / 2, cy1, hole / 2),
                    Rect(bx, d / 2 - boxhole / 2, bx + boxhole, d / 2 + boxhole / 2),
                    Circle(cx5 + hole / 2, cy1, hole / 2),
                    Circle(cx6 + hole / 2, cy1, hole / 2)
                );
                return Difference(panel, holes);

            }

            var final1 = Group(
                // top and bottom
                Top(true)
                ,Translate(0, h + 2 * b, Top(false))
                // left and right
                ,Translate(w + 2 * b + t, 0, Right())
                ,Translate(w + 2 * b + t, h + 2 * b, Right())
                // front and back
                ,Translate(0, 2 * (h + 2 * b), Front(true))
                ,Translate(w + 2 * b + t, 2 * (h + 2 * b) , Front(false))
            );

            final1.Save("AudioBox.svg");
        }

#if false
        public void RunOLD()
        {
            var t = InToMM("1/8"); // material thickness
            var b = 2*t/3; // border around holes
            var holeSize = Point(t,10.0);

            // spacing for a PCB hole
            var pcbThroughHoleSpacing = 2.54; // 0.1 inches = 2.54mm
            var pcbMountSize = Point(2.21,1.41) * 25.4; // inches to mm
            var pcbHoleRadius = Parts.ScrewClearanceDiameter["M2"]/2;



            // outer dimensions
            var w = 120.0; // mm width (left to right)
            var h =  60.0; // mm width (front to back)
            var d =  25.0; // mm depth (top to bottom)

            var audioHoleDiameter = 9.3; // for audio plugs

            // make a hole with given center
            Node Tab(Vector2D center, bool rot = false)
            {
                var angle = rot ? FromDegrees(90.0) : 0.0;
                return
                    Translate(
                        center,
                        Rotate(angle, Translate(
                                -holeSize / 2,
                                Rect(0, 0, holeSize.X, holeSize.Y)
                            )
                        )
                    );
            }

            // want: border, delta, hole, delta, hole, delta, border sum to h
            // thus delta = (h-2*(border+hole)]/3
            var delta = (h - (2 * (b + holeSize.Y))) / 3;
            var cy = b + delta + holeSize.Y / 2;
            var cx = w / 3;

            Node Top(bool top)
            {
                Node panel = Rect(0, 0, w, h);

                // thread holes
                var cr = Parts.ScrewClearanceDiameter["M3"] / 2;
                var d = b + cr;
                Node holes = Group(
                    // screw holes for corners
                    Circle(d, d, cr),
                    Circle(d, h - d, cr),
                    Circle(w - d, d, cr),
                    Circle(w - d, h - d, cr)
                );
                if (!top)
                {
                    // spacing for a PCB hole
                    // var pcbThroughHoleSpacing = 2.54; // 0.1 inches = 2.54mm
                    // var pcbMountSize = Point(1.41, 2.21) * 25.4; // inches to mm
                    // var pcbHoleRadius = Parts.ScrewClearanceDiameter["M2"] / 2;

                    var pr = pcbHoleRadius;
                    var (px, py) = pcbMountSize;

                    // pcb mounting holes
                    var (pcx, pcy) = (2*w/3-2*px/3,h/2-py/2);
                    var mounts =
                        Translate(pcx, pcy,
                            Group(
                                // screw holes for corners
                                Circle(0, 0, pr),
                                Circle(0, py, pr),
                                Circle(px, 0, pr),
                                Circle(px, py, pr)
                            ));
                    holes = Union(holes, mounts);
                    
                    //var label =
                    //    Text(w/8,h/2, 3*w / 4, h / 3, "Chris Lomont Audio Board, v1.0, 2021").Fill(Blue).Stroke(0);
                    //panel = Group(panel, label);

                }

                return Difference(panel,
                    Union(
                        // left holes
                        Tab(Point(b * 1.5, cy)),
                        Tab(Point(b * 1.5, h - cy)),
                        // right holes
                        Tab(Point(w - b * 1.5, cy)),
                        Tab(Point(w - b * 1.5, h - cy)),
                        // top holes
                        Tab(Point(cx, b*1.5),true),
                        Tab(Point(w -cx, b * 1.5), true),
                        // bottom holes
                        Tab(Point(cx, h-b * 1.5), true),
                        Tab(Point(w - cx, h-b * 1.5), true),

                        // threads
                        holes
                    )
                );
            }

            Node Right()
            {
                var ep = b + t;
                var r = Rect(0,ep,d,h-ep);

                return Union(
                    r,
                    // left (in image)
                    Tab(Point(-t+b,cy)),
                    Tab(Point(-t + b, h-cy)),
                    // right (in image)
                    Tab(Point(d+t - b, cy)),
                    Tab(Point(d+t - b, h - cy))
                    );
            }

            Node Front(bool makeHoles)
            {
                var ep = b + t;
                var r = Rect(ep,0,w-ep,d);
                var panel = Union(
                    r,
                    // top tabs
                    Tab(Point(cx,-t+b),true),
                    Tab(Point(w-cx, -t + b), true),

                    // bottom tabs
                    Tab(Point(cx, d+t - b), true),
                    Tab(Point(w-cx, d+t - b), true)
                );
                if (!makeHoles)
                    return panel;

                // holes: 
                // spacing: b,t,b,DEL1,hole,(DEL2,hole)x3,DEL1,boxhole,DEL1,hole,DEL2,hole,DEL1,b,t,b = w
                // 4b+2t+4DEL1+6hole+boxhole+4DEL2 = w
                // del1 = (w-4b-2t-6hole-boxhole-4del2)/4
                // DEL1 = spacing to make all look ok
                // DEL2 = forced by audio jacks spacing
                var hole = audioHoleDiameter;
                var del2 = 16.0;   // measured from plugs (is DEL2 above + hole)
                var boxhole = 6.0 + pcbThroughHoleSpacing; // measured for plug, add some spacing for errors
                var del1 = (w - 4*b - 2*t - 6*hole - boxhole - 4*(del2-hole))/ 4;

                var cx1 = b + t + b + del1;
                var cx2 = cx1 + del2;
                var cx3 = cx2 + del2;
                var cx4 = cx3 + del2;
                var bx  = cx4 + hole+del1;
                var cx5 = bx + boxhole + del1;
                var cx6 = cx5 + del2;

                var cy1 = d / 2-t*1.5;

                var holes = Union(
                    Circle(cx1+hole/2, cy1, hole / 2),
                    Circle(cx2+hole/2, cy1, hole / 2),
                    Circle(cx3+hole/2, cy1, hole / 2),
                    Circle(cx4+hole/2, cy1, hole / 2),
                    Rect(bx,d/2-boxhole/2,bx+boxhole,d/2+boxhole/2),
                    Circle(cx5+hole/2, cy1, hole / 2),
                    Circle(cx6+hole/2, cy1, hole / 2)
                );
                return Difference(panel, holes);

            }

            var final1 = Group(
                // top and bottom
                Top(true),
                Translate(0, h + 2 * b, Top(false)),
                // left and right
                Translate(w+2*b+t, 0, Right()),
                Translate(w+2*b+t, h+2*b, Right()),
                // front and back
                Translate(0,2*(h+2*b),Front(true)),
                Translate(0,2*(h+2*b)+d+b+2*t,Front(false))
            );

            final1.Save("AudioBox.svg");
        }
#endif
    }
}
