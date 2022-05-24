using Lomont.Graphics;

namespace Lomont.SimpleShapes.Shape3D
{
    /// <summary>
    /// CSS style style for 3D items
    /// </summary>
    public class Style
    {
        /// <summary>
        /// Default system style
        /// </summary>
        public static Style Default { get; }
            = new Style
            {
                FillColor = new ColorB(0, 0, 0, 0) // None
            };

#nullable enable // needed for nullable ref types for colors
        public ColorB? FillColor { get; set; }

        /// <summary>
        /// Create new style from this style using child to override
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public Style Append(Style child)
        {
            var s = new Style(this);
            if (child.FillColor != null)
                s.FillColor = child.FillColor;
            return s;
        }

        public Style()
        {
        }

        public Style(Style style)
        {
            FillColor = style.FillColor;
        }

    }
}