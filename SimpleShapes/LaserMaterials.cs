namespace Lomont.SimpleShapes
{
    /// <summary>
    /// Info for various materials and laser cutters, such as kerf
    /// </summary>
    public static class LaserMaterials
    {
        // todo - make test shape to determine pressfit values

        // so far, 1/8" clear acrylic from 2.9mm thick, 2.7 cut makes good press fit, organize

        /// <summary>
        /// For 1/8" material, this is a good pressfit hole width
        /// Essentially 2 * kerf
        /// </summary>
        public static double cutSize = 2.7;


    }
}
