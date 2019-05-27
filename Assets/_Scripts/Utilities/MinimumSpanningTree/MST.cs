using System.Collections.Generic;
using UnityEngine;


namespace MinimumSpanningTree
{
    public class MST<T>
    {
        private readonly float INF = float.MaxValue - 1f;


        public Graph<T> Prim(Graph<T> oriGraph)
        {
            Graph<T> graph = new Graph<T>(oriGraph); // Copy of the original graph for calculating MST.
            Graph<T> resultGraph = new Graph<T>(oriGraph.NodeList.ConvertAll(n => n.Context)); // Result graph, copy all the node but without edges.

            Dictionary<Graph<T>.Node, float> nodeWeightPair = new Dictionary<Graph<T>.Node, float>();
            foreach(var node in graph.NodeList)            
                nodeWeightPair.Add(node, INF);
            nodeWeightPair[graph.NodeList[0]] = 0f;

            Dictionary<Graph<T>.Node, Graph<T>.Node> resultTree = new Dictionary<Graph<T>.Node, Graph<T>.Node>();

            while (graph.Count() > 0)
            {
                Graph<T>.Node minNode = FindMinNodeAndDelete(graph, nodeWeightPair);
                foreach (var neighborNode in minNode.GetNeighbors())
                {
                    if (graph.ContainsContext(neighborNode.Context) && (minNode.GetWeight(neighborNode) <= nodeWeightPair[neighborNode]))
                    {
                        nodeWeightPair[neighborNode] = minNode.GetWeight(neighborNode);
                        resultTree[neighborNode] = minNode; // neighborNode is child, minNode is parent.
                    }
                }
            }
            foreach (var childNode in resultTree.Keys)
                resultGraph.AddEdge(resultTree[childNode].Context, childNode.Context, 1f);

            return resultGraph;
        }

        private Graph<T>.Node FindMinNodeAndDelete(Graph<T> graph, Dictionary<Graph<T>.Node, float> nodeWeightPair)
        {
            Graph<T>.Node minNode = null;
            float minWeight = INF;

            foreach (var node in graph.NodeList)
            {
                if (nodeWeightPair[node] <= minWeight)
                {
                    minNode = node;
                    minWeight = nodeWeightPair[node];
                }
            }
            graph.RemoveNode(minNode);
            return minNode;
        }
    }
}
