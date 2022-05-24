using Lomont.Graphics;

namespace Lomont.SimpleShapes.Shape2D
{
    /// <summary>
    /// CSS style style
    /// </summary>
    public class Style
    {
        /// <summary>
        /// Default system style
        /// </summary>
        public static Style Default { get; }
            = new Style
            {
                StrokeThickness = 0.5,
                StrokeColor = new ColorB(0, 0, 0),
                FillColor = new ColorB(0, 0, 0, 0) // None
            };
        public double? StrokeThickness { get; set; }

#nullable enable // needed for nullable ref types for colors
        public ColorB? StrokeColor { get; set; }
        public ColorB? FillColor { get; set; }

        /// <summary>
        /// Create new style from this style using child to override
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public Style Append(Style child)
        {
            var s = new Style(this);
            if (child.StrokeThickness.HasValue)
                s.StrokeThickness = child.StrokeThickness.Value;
            if (child.StrokeColor != null)
                s.StrokeColor = child.StrokeColor;
            if (child.FillColor != null)
                s.FillColor = child.FillColor;
            return s;
        }

        public Style()
        {
        }

        public Style(Style style)
        {
            StrokeThickness = style.StrokeThickness;
            StrokeColor = style.StrokeColor;
            FillColor = style.FillColor;
        }

    }
}
