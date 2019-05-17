using System.Collections.Generic;


namespace MinimumSpanningTree
{
    public class Graph<T>
    {
        public class Node
        {
            public List<Edge> EdgeList { get; private set; } = new List<Edge>();
            public T Context { get; private set; } // id of room in mainRoomList.


            public Node(T context)
            {
                this.Context = context;
            }
        
            public bool ContainsEdge(Node next)
            {
                return EdgeList.Find(n => n.Next.Equals(next)) != null;
            }

            public List<Node> GetNeighbors()
            {
                return EdgeList.ConvertAll(e => e.Next);
            }

            public float GetWeight(Node next)
            {
                foreach (var edge in EdgeList)
                {
                    if (edge.Next == next)
                        return edge.Weight;
                }
                return float.MaxValue;
            }
        }

        public class Edge
        {
            public Node Next { get; private set; }
            public float Weight { get; private set; }


            public Edge(Node next, float weight)
            {
                this.Next = next;
                this.Weight = weight;
            }
        }

        public List<Node> NodeList { get; private set; } = new List<Node>();


        public Graph(IEnumerable<T> initialize_list)
        {
            foreach (var i in initialize_list)
            {
                AddNode(i);
            }
        }

        public Graph(Graph<T> othr)
        {
            NodeList.Clear();
            NodeList.AddRange(othr.NodeList);
        }

        public int Count()
        {
            return NodeList.Count;
        }

        public Node FindNode(T context)
        {
            return NodeList.Find(n => n.Context.Equals(context));
        }

        public bool ContainsContext(T context)
        {
            return FindNode(context) != null;
        }

        public void AddNode(T context)
        {
            if (!ContainsContext(context))
                NodeList.Add(new Node(context));
        }

        public void RemoveNode(Node node)
        {
            NodeList.Remove(node);
        }

        public void AddEdge(T from, T to, float weight)
        {
            Node fromNode = FindNode(from);
            Node toNode = FindNode(to);

            if (fromNode != null && toNode != null && !fromNode.ContainsEdge(toNode))           
                fromNode.EdgeList.Add(new Edge(toNode, weight));
        }
    }
}
