using System.Collections.Generic;


namespace MinimumSpanningTree
{
    public class MST<T>
    {
        private readonly float INF = float.MaxValue - 1;


        public Dictionary<Graph<T>.Node, Graph<T>.Node> Prim(Graph<T> oriGraph)
        {
            Graph<T> graph = new Graph<T>(oriGraph);

            Dictionary<Graph<T>.Node, float> keys = new Dictionary<Graph<T>.Node, float>();
            foreach(var node in graph.NodeList)            
                keys.Add(node, INF);
            keys[graph.NodeList[0]] = 0f;

            Dictionary<Graph<T>.Node, Graph<T>.Node> tree = new Dictionary<Graph<T>.Node, Graph<T>.Node>();

            while (graph.Count() > 0)
            {
                Graph<T>.Node minNode = FindMinNodeAndDelete(graph, keys);
                foreach (var neighborNode in minNode.GetNeighbors())
                {
                    if (graph.ContainsContext(neighborNode.Context) && (minNode.GetWeight(neighborNode) <= keys[neighborNode]))
                    {
                        keys[neighborNode] = minNode.GetWeight(neighborNode);
                        tree[neighborNode] = minNode;
                    }
                }
            }

            return tree;
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
