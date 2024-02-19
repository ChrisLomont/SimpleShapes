namespace Lomont.SimpleShapes
{
    // sizes of common parts
    public static class Parts
    {
        /// <summary>
        /// Radii of screw types, like "M3", in mm
        /// Rough rule of thumb: diameter of clearance hole has 10% more than diameter of screw.
        /// These are clearance hole diameters
        /// </summary>
        public static Dictionary<string, double> ScrewClearanceDiameter { get; } = new Dictionary<string, double>
        {
            // https://www.trfastenings.com/products/knowledgebase/tables-standards-terminology/Tapping-Sizes-and-Clearance-Holes
            ["M1"] = 1.2,
            ["M1.2"] = 1.4,
            ["M1.4"] = 1.6,
            ["M1.6"] = 1.8,
            ["M1.8"] = 2.0,
            ["M2"] = 2.4,
            ["M2.2"] = 2.8,
            ["M2.5"] = 2.9,
            ["M3"] = 3.4,
            ["M3.5"] = 3.9,
            ["M4"] = 4.5,
            ["M5"] = 5.5,
            ["M6"] = 6.6,
            ["M8"] = 9.0,
            ["M10"] = 11,
            ["M12"] = 13.5,
        };

    }
}
