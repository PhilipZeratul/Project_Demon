﻿using UnityEngine;


namespace TriangulationMethods
{
    public class Vertex
    {
        public Vector3 position;

        //The outgoing halfedge (a halfedge that starts at this vertex)
        //Doesnt matter which edge we connect to it
        public HalfEdge halfEdge;

        //Which triangle is this vertex a part of?
        public Triangle triangle;

        //The previous and next vertex this vertex is attached to
        public Vertex prevVertex;
        public Vertex nextVertex;

        //Properties this vertex may have
        //Reflex is concave
        public bool isReflex;
        public bool isConvex;
        public bool isEar;

        // Tune for DungeonGeneration
        public int id;


        public Vertex(Vector3 position)
        {
            this.position = position;
        }

        public Vertex(Vector2 position, int vertexId)
        {
            this.position = position;
            this.id = vertexId;
        }

        //Get 2d pos of this vertex
        public Vector2 GetPos2D()
        {
            Vector2 pos_2d = position;

            return pos_2d;
        }
    }
}
