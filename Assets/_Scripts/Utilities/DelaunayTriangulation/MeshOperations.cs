using UnityEngine;
using System.Collections.Generic;


namespace TriangulationMethods
{
    public static class MeshOperations
    {
        //Orient triangles so they have the correct orientation
        public static void OrientTrianglesClockwise(List<Triangle> triangles)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle tri = triangles[i];

                Vector2 v1 = tri.v1.position;
                Vector2 v2 = tri.v2.position;
                Vector2 v3 = tri.v3.position;

                if (!Geometry.IsTriangleOrientedClockwise(v1, v2, v3))
                {
                    tri.ChangeOrientation();
                }
            }
        }
    }
}
