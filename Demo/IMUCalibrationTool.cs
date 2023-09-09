using System;
using System.Linq;
using Lomont.Graphics;
using Lomont.Numerical;
using static Lomont.SimpleShapes.SimpleShape3D;
using D = Lomont.SimpleShapes.DodecahedronInfo;
using P = Lomont.SimpleShapes.PolyhedronInfo;
using static System.Math;
using System.Collections.Generic;
using Lomont.SimpleShapes.Shape3D;

namespace Lomont.Projects
{
    [Project("Chris Lomont", "imu calibration tool", "5/23/2022")]
    class IMUCalibrator
    {


        public void Run()
        {
            int numSides = 100; // sphere and cylinder divisions

            var radius = 50; // mm radius size of outer object
            var cylRad = 2.5; // tube radius

            // polyhedra vert, face definitions
            var v = Lomont.Geometry.Polyhedra.DeltoidalIcositetrahedron.Vertices;
            var f = Lomont.Geometry.Polyhedra.DeltoidalIcositetrahedron.FaceDefinitions;

            // track edges added to prevent dups 
            var seen = new HashSet<ulong>();

            // scale points into box [-1,1]
            var pts = Enumerable.Range(0, v.Count / 3)
                .Select(i => new Vec3(v[3 * i], v[3 * i + 1], v[3 * i + 2])).ToList();
            var maxLen = pts.Max(p => p.Length);
            pts = pts.Select(p => p / maxLen).ToList();

            // create faces from tubes
            var index = 0;
            var faces = new List<Node>();
            while (index < f.Count)
            {
                var sides = f[index++];

                for (var i = 0; i < sides; ++i)
                    AddTubeA(f[index + i], f[index + ((i + 1) % sides)]);
                index += sides;
            }

            // sphere at each vertex
            var verts = new List<Node>();
            for (var i = 0; i < v.Count; i += 3)
            {
                var c = Pt(i / 3);
                verts.Add(Sphere(c, cylRad, numSides, numSides));
            }

            // axes
            var tz = cylRad;
            var ty = 12.0;
            var tx = 2 * radius - cylRad;

            faces.Add(Cube(tx, ty, tz));
            faces.Add(Cube(ty, tx, tz));

            // make solid shape
            Node ball = Union(Group(faces), Group(verts));


            // make holes
            var holes = new List<Node>();
            var holeDiam = 3.4; // mm
            var numHoles = 5; // make odd
            for (var i = 0; i < numHoles; ++i)
            {
                var c = Cylinder(2 * tz, holeDiam / 2);
                var del = (i - numHoles / 2) * (tx - cylRad * 5) / numHoles;
                holes.Add(Translate(del, 0, -tz, c));

                c = Cylinder(2 * tz, holeDiam / 2);
                holes.Add(Translate(0, del, -tz, c));
            }


            var g = Difference(ball, Group(holes));

            g.Save("IMUCalibrator.scad");

            Vec3 Pt(int index)
            {
                return radius * pts[index];
                //var i = index * 3;
                //return radius*(new Vec3(v[i], v[i + 1], v[i + 2]));
            }
            void AddTubeA(int i1, int i2)
            {
                var hash = (ulong)(Math.Min(i1, i2) * 1000 + Math.Max(i1, i2));
                if (!seen.Contains(hash))
                {
                    var (p1, p2) = (Pt(i1), Pt(i2));
                    faces.Add(Cylinder(p1, p2, cylRad, numSides));
                    seen.Add(hash);
                }
            }

        }
    }
}
