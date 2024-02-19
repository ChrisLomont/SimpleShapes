using Lomont.Graphics;
using static Lomont.SimpleShapes.SimpleShape3D;

namespace Lomont.SimpleShapes
{

    /// <summary>
    /// Shaper Origin SVG coloring styles
    /// </summary>
    public static class ShaperOrigin
    {
        public record Style(ColorB Fill, ColorB Stroke);

        public static Style InteriorCut = new(White, Black);
        public static Style ExteriorCut = new(Black, Black);
        public static Style OnLineCut = new(White, Gray);
        public static Style PocketingCut = new(Gray, Gray);
        public static Style Guide = new(Blue, Blue);

    }
}
