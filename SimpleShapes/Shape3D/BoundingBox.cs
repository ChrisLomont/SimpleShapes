using Lomont.Numerical;

namespace Lomont.SimpleShapes.Shape3D
{
    /// <summary>
    /// Axis aligned 3D bounding box
    /// </summary>
    public class BoundingBox
    {

        public BoundingBox(params BoundingBox[] boxes)
        {
            foreach (var b in boxes)
            {
                if (b == null) continue;
                Add(b.Min);
                Add(b.Max);
            }
        }
        public Vec3 Min { get; private set; } = Vec3.Max;
        public Vec3 Max { get; private set; } = Vec3.Min;

        public double Width => Max.X - Min.X;
        public double Height => Max.Y - Min.Y;
        public double Length => Max.Z - Min.Z;

        public Vec3 Size => Max - Min;

        /// <summary>
        /// Add a point to the box
        /// </summary>
        /// <param name="pt"></param>
        public void Add(Vec3 pt)
        {
            Min = Vec3.ComponentwiseMin(Min, pt);
            Max = Vec3.ComponentwiseMax(Max, pt);
        }

        /// <summary>
        /// Does this box contain the parameter?
        /// </summary>
        /// <param name="inner"></param>
        /// <returns></returns>
        public bool Contains(BoundingBox inner)
        {
            return
                Min.X <= inner.Min.X &&
                Max.X >= inner.Max.X &&
                Min.Y <= inner.Min.Y &&
                Max.Y >= inner.Max.Y &&
                Min.Z <= inner.Min.Z &&
                Max.Z >= inner.Max.Z
                ;
        }

        /// <summary>
        /// Compute transform to center box in this one
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public Vec3 Center(BoundingBox box)
        {
            var thisCenter = (Max + Min) / 2;
            var bCenter = (box.Max + box.Min) / 2;
            return -(bCenter - thisCenter);

        }

        public override string ToString()
        {
            return $"[{Min},{Max}]";
        }
    }
}
