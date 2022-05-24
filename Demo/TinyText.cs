using Lomont.SimpleShapes;
using static Lomont.SimpleShapes.SimpleShape2D;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "testing small fonts", "5/05/21")]
    class TinyText
    {
        /// <summary>
        /// See how small of text I can make and read
        /// </summary>
        public void Run()
        {
            var (max,min,delta) = (3.0,0.3,0.1);
            var g = Group();
            var (x, y) = (0.0, 0.0);

            void Add(string msg, double size)
            {
                var t = Text(x, y, 0, size, msg);
                var tb = t.Bounds();
                y += tb.Height + 1.0;
                g.Add(t);
            }

            Add("Tiny Text",3.0);
            var w = g.Bounds().Width;

            for (var sz = max; sz >= min; sz -= delta)
            {
                var msg = $"{sz:F1}mm";
                Add(msg,sz);
            }

            var b = g.Bounds();
            var border = 4.0;
            var rect = Rect(0,0,b.Width+2*border, b.Height+2*border);

            var final = Group(
                Translate(-border,-border,rect),
                g.Stroke(Blue,0.0).Fill(Blue)
            );
            final.Save("TinyText.svg");
        }


    }
}