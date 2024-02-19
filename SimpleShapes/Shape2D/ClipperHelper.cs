using System.Diagnostics;
using ClipperLib;
using Lomont.Numerical;
using CPath = System.Collections.Generic.List<ClipperLib.IntPoint>;
using CPaths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;


namespace Lomont.SimpleShapes.Shape2D
{
    /// <summary>
    /// Helper classes for ClipperLib
    /// </summary>
    static class ClipperHelper
    {
        /// <summary>
        /// Track scaling info
        /// scales points to integer points
        /// </summary>
        class Scaling
        {
            readonly static long scaleInt = 100000;
            // todo - finish this
            public Scaling(BoundingBox bounds, long maxInt)
            {

            }

            public static IntPoint Convert(Vec2 vec2)
            {
                var x = (long)(scaleInt * vec2.X);
                var y = (long)(scaleInt * vec2.Y);
                return new IntPoint(x, y);
            }

            public static Vec2 Back(IntPoint pt)
            {
                return new Vec2(pt.X, pt.Y) / scaleInt;
            }

        }

        static (CPaths subjectPaths, CPaths clipPaths, Scaling) ToPaths(Node subjectNode, Node clipNode)
        {
            // get joint bounding box
            var sBox = subjectNode?.Bounds();
            var cBox = clipNode?.Bounds();
            var box = new BoundingBox(sBox, cBox);

            // get a scaling item
            var scaling = new Scaling(box, MaxClipper);

            // scale nodes
            var subjectPaths = Convert(subjectNode, scaling);
            var clipPaths = Convert(clipNode, scaling);
            return (subjectPaths, clipPaths, scaling);
        }

        /// <summary>
        /// Convert node to paths using given scaling
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scaling"></param>
        /// <returns></returns>
        static CPaths Convert(Node node, Scaling scaling)
        {
            // convert to paths
            var paths = new CPaths();
            node.Walk((n, t, _) =>
            {
                if (n is Path path)
                { // special case paths for now...
                    foreach (var contour in path.Contours)
                    {
                        var cpath = new CPath();
                        foreach (var pt in contour.Points)
                            cpath.Add(Scaling.Convert(t * pt));
                        paths.Add(cpath);
                    }
                }
                else if (n is IGetPoints g)
                {
                    var cpath = new CPath();
                    foreach (var pt in g.GetPoints())
                        cpath.Add(Scaling.Convert(t * pt));
                    paths.Add(cpath);
                }
            }, null);
            return paths;
        }

        static (CPaths paths, Scaling) ToPaths(Node node)
        {

            var box = node.Bounds();
            // create scale to max clipper ints
            var scaling = new Scaling(box, MaxClipper);

            // convert to paths
            var paths = Convert(node, scaling);

            return (paths, scaling);
        }

        /// <summary>
        /// Convert paths to a node with all the polygons
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="scaling"></param>
        /// <returns></returns>
        static Node ToNode(CPaths paths, Scaling scaling)
        {
            var node = new Node();

            // convert from paths
            var contours = new List<Contour>();
            foreach (var cpath in paths)
            {
                var contour = new Contour(cpath.Select(pt => Scaling.Back(pt)).ToArray()).Closed();
                contours.Add(contour);
            }
            return new Path(contours.ToArray());
        }

        // max clipper coordinates
        static readonly long MaxClipper = 0x3FFFFFFFFFFFFFFFL;

        static Node Boolean2(Node subj, Node clip, ClipType clipType)
        {
            var (subjPaths, clipPaths, scaling) = ToPaths(subj, clip);
            var clipper = new ClipperLib.ClipperLib();

            // add subject paths
            clipper.AddPaths(
                subjPaths,
                PolyType.ptSubject,
                closed: true // loops, otherwise just lines if closed =  false
            );

            // add any clip paths
            if (clipPaths != null && clipPaths.Count > 0)
            {
                clipper.AddPaths(
                    clipPaths,
                    PolyType.ptClip,
                    closed: true // loops, otherwise just lines if closed =  false
                );
            }

            var pf = PolyFillType.pftEvenOdd;

            // do the operation
            CPaths solution = new();
            if (!clipper.Execute(
                    clipType,
                    solution
                    , pf, pf
                )
            )
            {
                Trace.TraceError("Clipper boolean failed");
                return null;
            }

            // how to do offsets
            //ClipperOffset co = new ClipperOffset();
            //co.AddPaths(solution, JoinType.jtRound, EndType.etClosedPolygon);
            //co.Execute(ref solution2, (double)nudOffset.Value * scale);


            // todo - track styles through here
            // todo - track original points through here
            return ToNode(solution, scaling);

        }


        static Node Boolean(Node subj, Node clip, ClipType clipType)
        {
            var clipper = new ClipperLib.ClipperLib();
            var (subjPaths, clipPaths, scaling) = ToPaths(subj, clip);

            // add subject paths
            clipper.AddPaths(
                subjPaths,
                PolyType.ptSubject,
                closed: true // loops, otherwise just lines if closed =  false
            );

            // add any clip paths
            if (clipPaths != null && clipPaths.Count > 0)
            {
                clipper.AddPaths(
                    clipPaths,
                    PolyType.ptClip,
                    closed: true // loops, otherwise just lines if closed =  false
                );
            }


            var pf = PolyFillType.pftEvenOdd;

            // do the operation
            CPaths solution = new();
            if (!clipper.Execute(
                    clipType,
                    solution
                    , pf, pf
                    )
            )
            {
                Trace.TraceError("Clipper boolean failed");
                return null;
            }

            // how to do offsets
            //ClipperOffset co = new ClipperOffset();
            //co.AddPaths(solution, JoinType.jtRound, EndType.etClosedPolygon);
            //co.Execute(ref solution2, (double)nudOffset.Value * scale);


            // todo - track styles through here
            // todo - track original points through here
            return ToNode(solution, scaling);

        }

        /// <summary>
        /// make a union of subject and clip nodes, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Union(Node subject, Node clip)
        {
            return Boolean(subject, clip, ClipType.ctUnion);
        }

        /// <summary>
        /// make a difference of subject and clip nodes, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Difference(Node subject, Node clip)
        {
            return Boolean(subject, clip, ClipType.ctDifference);
        }
        /// <summary>
        /// make an intersection of subject and clip nodes, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Intersection(Node subject, Node clip)
        {
            return Boolean(subject, clip, ClipType.ctIntersection);
        }
        /// <summary>
        /// make an XOR of all subject and clip nodes, return new node.
        /// </summary>
        /// <returns></returns>
        public static Node Xor(Node subject, Node clip)
        {
            return Boolean(subject, clip, ClipType.ctXor);
        }

        public static Node Thicken(double amount, Node node)
        {
            var (subjPaths, scaling) = ToPaths(node);

#if false
            // add subject paths
            clipper.AddPaths(
                subjPaths,
                PolyType.ptSubject,
                closed: true // loops, otherwise just lines if closed =  false
            );

            // add any clip paths
            if (clipPaths != null && clipPaths.Count > 0)
            {
                clipper.AddPaths(
                    clipPaths,
                    PolyType.ptClip,
                    closed: true // loops, otherwise just lines if closed =  false
                );
            }


            var pf = PolyFillType.pftEvenOdd;

            // do the operation
            CPaths solution = new CPaths();
            if (!clipper.Execute(
                    clipType,
                    solution
                    , pf, pf
                )
            )
            {
                Trace.TraceError("Clipper boolean failed");
                return null;
            }
#endif
            CPaths solution = new();

            // compute length
            var ip = Scaling.Convert(new Vec2(amount, 0));
            var scaledAmount = Math.Sqrt(ip.X * ip.X + ip.Y * ip.Y);

            // how to do offsets
            ClipperOffset co = new();
            co.AddPaths(subjPaths, JoinType.jtRound, EndType.etClosedPolygon);
            co.Execute(ref solution, scaledAmount);

            // todo - track styles through here
            // todo - track original points through here
            return ToNode(solution, scaling);
        }



    }
}
