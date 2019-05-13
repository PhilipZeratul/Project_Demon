using System.Collections.Generic;


namespace TriangulationMethods
{
    public static class TriangulateHullAlgorithms
    {
        public static List<Triangle> TriangulateConvexPolygon(List<Vertex> convexHullpoints)
        {
            List<Triangle> triangles = new List<Triangle>();

            for (int i = 2; i < convexHullpoints.Count; i++)
            {
                Vertex a = convexHullpoints[0];
                Vertex b = convexHullpoints[i - 1];
                Vertex c = convexHullpoints[i];

                triangles.Add(new Triangle(a, b, c));
            }

            return triangles;
        }
    }
}
