//using System.Numerics;

using System.Diagnostics;
using Lomont.Numerical;
using static System.Math;

namespace Lomont.SimpleShapes
{
    public static class PolyhedronInfo
    {
        /// <summary>
        /// Given sidelength and number of sides, returns the height of a triangle and the radius for each face triangle
        /// 
        /// </summary>
        /// <param name="numSides"></param>
        /// <param name="sideLength"></param>
        /// <returns></returns>
        public static (double height, double radius) TriangleSides(int numSides, double sideLength)
        {
            var alpha = PI * 2 / numSides; // interior angle for a triangle for a face
            var alpha2 = alpha / 2;
            var beta = PI * (0.5 - 1.0 / numSides); // other triangle angle
            var h = sideLength * Sin(beta) / (2 * Sin(alpha2)); // triangle height
            var s2 = sideLength / 2;
            var r = Sqrt(s2 * s2 + h * h); // radius
            return (h, r);
        }

        /// <summary>
        /// Given the height of an n-gon triangle, get the side length
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public static double NGonSideLengthFromHeight(int numSides, double height)
        {
            var alpha = PI * 2 / numSides; // interior angle for a triangle for a face

            // tan(angle/2) = (s/2)/height
            var sideLength = Tan(alpha / 2) * height * 2;

            // consistency check
            Debug.Assert(Abs(TriangleSides(numSides, sideLength).height - height) < 0.0001);

            return sideLength;

        }


    }

    /// <summary>
    /// dodecahedron utilities
    /// </summary>
    public static class DodecahedronInfo
    {

        /// <summary>
        /// Angle between faces
        /// </summary>
        public static double DihedralAngle => Acos(-1.0 / Sqrt(5.0));



        /// <summary>
        /// diameter of enclosing ball, area, and volume, given sidelength
        /// </summary>
        /// <param name="sideLength"></param>
        /// <returns></returns>
        public static (double circumDiameter, double area, double volume) Stats(double sideLength)
        {
            // https://rechneronline.de/pi/dodecahedron.php
            var a = sideLength;
            var area = 3 * a * a * Sqrt(25 + 10 * Sqrt(5));
            var volume = a * a * a / 4 * (15 + 7 * Sqrt(5));
            var rc = a / 4 * Sqrt(3) * (1 + Sqrt(5)); // circumsphere radius
            var circumDiameter = 2 * rc; // circumsphere diameter
            return (circumDiameter, area, volume);
        }

        /// <summary>
        /// Optimal pentagon packing in the plane, useful for cutting items
        /// See "PACKINGS OF REGULAR PENTAGONS IN THE PLANE", Hales et. al., https://arxiv.org/pdf/1602.07220.pdf
        /// Gives density (5-sqrt(5)/3 ~ 0.92
        /// Gives two lattice directions, dir1 in x direction, dir2 in slight diagonal
        /// </summary>
        public static (Vec2 dir1, Vec2 dir2) OptimalPentaPacking()
        {
            // double lattice https://en.wikipedia.org/wiki/Double_lattice
            // gif of family, allows getting usable space between pieces https://jiggerwit.wordpress.com/2016/09/16/pentagon-packings/
            // Note the one known as Durer's packing since it has the biggest usable free areas left over, but it is less dense
            // here is the one known as the pentagonal ice-ray, optimal

            // from mathematica work
            var dir1 = new Vec2(Sqrt((5 + 2 * Sqrt(5)) / 4), 0); // height + radius
            var rx = (65 + 19 * Sqrt(5)) / 160;
            var ry = 9 * (3 + Sqrt(5)) / 32;


            var dir2 = new Vec2(Sqrt(rx), Sqrt(ry));

            return (dir1, dir2);
        }

        // lots of geometry from
        // https://www.kjmaclean.com/Geometry/dodecahedron.html

        public static int VertexCount => 20;
        public static int FaceCount => 12;

        public static double GoldenMean => (1 + Sqrt(5.0)) / 2;
        /// <summary>
        /// Golden mean shorthand
        /// </summary>
        public static double ϕ = GoldenMean;
        /// <summary>
        /// 1/Golden mean shorthand
        /// </summary>
        public static double ψ = 1.0 / GoldenMean;

        /// <summary>
        /// 20 vertices
        /// Edge length 2/ϕ = Sqrt(5)-1
        /// </summary>
        public static Vec3[] Vertices { get; private set; }

        /// <summary>
        /// 12 faces, each is 5 indices to 
        /// </summary>
        public static int[][] FaceIndices { get; private set; } = new int[FaceCount][];

        static void GenerateGeometry()
        {
            // 8 vertices of (±1, ±1, ±1)
            Vertices = new Vec3[VertexCount];
            for (var i = 0; i < 8; ++i)
                Vertices[i] = new Vec3(Sign(i), Sign(i >> 1), Sign(i >> 2));

            // 4 vertices each of cyclic permutations of (0, ±ϕ, ±1/ϕ)
            for (var i = 0; i < 4; ++i)
            {
                var (a, b, c) = (0, Sign(i) * ϕ, Sign(i >> 1) / ϕ);
                Vertices[8 + i] = new Vec3(a, b, c);
                Vertices[12 + i] = new Vec3(c, a, b);
                Vertices[16 + i] = new Vec3(b, c, a);
            }

            // 12 faces are points satisfying (4 each eqn)
            // x ± ϕy = ±ϕ^2
            // y ± ϕz = ±ϕ^2
            // z ± ϕx = ±ϕ^2

            // can write as ax + by + cz + d=0

            FaceIndices = new int[12][];
            for (var f = 0; f < 12; ++f)
            {
                // facet coeffs (a,b,c,d) for (x,y,z,1)
                var eqn = f / 4; //0,1,2
                // 0 => (1,±ϕ,0,±ϕ^2)
                // 1 => (0,1,±ϕ,±ϕ^2)
                // 2 => (±ϕ,0,1,±ϕ^2)
                var (a, b, c, d) = (1.0, Sign(f) * ϕ, 0.0, Sign(f >> 1) * ϕ * ϕ);
                if (eqn == 1) (a, b, c, d) = (b, c, a, d);
                if (eqn == 2) (a, b, c, d) = (c, a, b, d);

                var face = FaceIndices[f] = new int[5];

                // find 5 vertices on this plane
                var j = 0;
                for (var k = 0; k < VertexCount; ++k)
                {
                    var (x, y, z) = Vertices[k];
                    var eq = a * x + b * y + c * z + d;
                    if (Abs(eq) < 0.001) // float, do not compare to 0!
                    {
                        Trace.Assert(j < 5); // must not yet have 5!
                        face[j++] = k;
                    }
                }
                Trace.Assert(j == 5); // must have exactly 5!

                OrderFace(face, 2 / ϕ);
                OrientFace(face);
            }

            // helpers
            int Sign(int v) => (v & 1) * 2 - 1; // low bit gives a sign

            // each is proper distance from neighbor
            void OrderFace(int[] face, double dist)
            {
                var pos = 1; // check dist from this to prev
                while (pos < 5)
                {
                    for (var k = pos; k < 5; ++k)
                    {
                        var d = (Vertices[face[k]] - Vertices[face[pos - 1]]).Length;
                        if (Abs(d - dist) < 0.001)
                        {
                            var temp = face[k];
                            face[k] = face[pos];
                            face[pos] = temp;
                            break;
                        }
                    }

                    pos++;
                }
            }

            void OrientFace(int[] face)
            {
                var p0 = Vertices[face[0]];
                var p1 = Vertices[face[1]];
                var p2 = Vertices[face[2]];
                var normal = Vec3.Cross(p0 - p1, p1 - p2).Normalized() / 20.0; // make short
                var pointIn = p0 - normal;
                var pointOut = p0 + normal;
                if (pointOut.Length < pointIn.Length)
                { // backwards, want cross product away from origin
                    for (var i = 0; i < face.Length / 2; ++i)
                    {
                        var j = 4 - i;
                        (face[i], face[j]) = (face[j], face[i]);
                    }


                }
                //todo
            }


        }

        /// <summary>
        /// Fill in data
        /// </summary>
        static DodecahedronInfo()
        {
            GenerateGeometry();
        }

    }


}
