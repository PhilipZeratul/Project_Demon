using System.Collections.Generic;


namespace MinimumSpanningTree
{
    public class MST<T>
    {
        private readonly float INF = float.MaxValue - 1;


        public Graph<T> Prim(Graph<T> oriGraph)
        {
            Graph<T> graph = new Graph<T>(oriGraph); // Copy of the original graph for calculating MST.
            Graph<T> resultGraph = new Graph<T>(oriGraph.NodeList.ConvertAll(n => n.Context)); // Result graph, copy all the node but without edges.

            Dictionary<Graph<T>.Node, float> nodeWeightPair = new Dictionary<Graph<T>.Node, float>();
            foreach(var node in graph.NodeList)            
                nodeWeightPair.Add(node, INF);
            nodeWeightPair[graph.NodeList[0]] = 0f;

            Dictionary<Graph<T>.Node, Graph<T>.Node> tree = new Dictionary<Graph<T>.Node, Graph<T>.Node>();

            while (graph.Count() > 0)
            {
                Graph<T>.Node minNode = FindMinNodeAndDelete(graph, nodeWeightPair);
                foreach (var neighborNode in minNode.GetNeighbors())
                {
                    if (graph.ContainsContext(neighborNode.Context) && (minNode.GetWeight(neighborNode) <= nodeWeightPair[neighborNode]))
                    {
                        nodeWeightPair[neighborNode] = minNode.GetWeight(neighborNode);
                        tree[neighborNode] = minNode; // neighborNode is child, minNode is parent.
                    }
                }
            }
            foreach (var childNode in tree.Keys)
                resultGraph.AddEdge(tree[childNode].Context, childNode.Context, 1f);

            return resultGraph;
        }

        private Graph<T>.Node FindMinNodeAndDelete(Graph<T> graph, Dictionary<Graph<T>.Node, float> keys)
        {
            Graph<T>.Node minNode = null;
            float minWeight = INF;

            foreach (var node in graph.NodeList)
            {
                if (keys[node] < minWeight)
                {
                    minNode = node;
                    minWeight = keys[node];
                }
            }
            graph.RemoveNode(minNode);
            return minNode;
        }
    }
}
