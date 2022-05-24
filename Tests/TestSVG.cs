#if false // todo - fix
using Svg;

namespace Lomont.Lib2D
{
    public static class TestSVG
    {
        public static void Test()
        {
            var doc = new SvgDocument();
            // pixel size of view
            doc.Width = 800;
            doc.Height = 800;
            // coords of view
            doc.ViewBox = new SvgViewBox(-250, -250, 500, 500);

            //doc.Ppi = 96; // pixels per inch
            //doc.AspectRatio = 1.0;

            var rad = 100.0;
            for (var i = 0; i < 20; i++)
            {
                var line = new SvgLine();
                var ang = i*Math.PI * 2 / 20;
                line.StartX = new SvgUnit((float)(rad*Math.Cos(ang)));
                line.StartY = new SvgUnit((float)(rad * Math.Sin(ang)));
                line.EndX = new SvgUnit(-line.StartX);
                line.EndY = new SvgUnit(-line.StartY);
                // line.Fill = 
                // line.
                line.Stroke = new SvgColourServer(System.Drawing.Color.Red);
                line.StrokeWidth = new SvgUnit(1.0f);
                
                line.StrokeLineCap = SvgStrokeLineCap.Round;
                line.StrokeLineJoin = SvgStrokeLineJoin.Round;

                // line.ID // should be unique
                // line.CustomAttributes // todo


                doc.Children.Add(line);
            }


            // doc.
            //SvgDeferredPaintServer( SvgDocument.Open());

            using (var bitmap = doc.Draw())
            {
                bitmap.Save("testing.png");
            }

            using (var fs = File.Create("testing.svg"))
            {
                doc.Write(fs);
            }

        }
    }
}
#endif