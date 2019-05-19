using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TriangulationMethods;
using MinimumSpanningTree;


public class DungeonGenerator : MonoBehaviour
{
    public IntRange numOfRooms = new IntRange(5, 20);
    public IntRange roomWidth = new IntRange(3, 10);
    public IntRange roomHeight = new IntRange(3, 10);
    [Range(0f, 1f)]
    public float edgeAddBackRatio = 0.1f;

    public GameObject roomHolder;
    public GameObject[] floorPrefabs;
    public GameObject[] wallPrefabs;

    private class DungeonRoom
    {
        public int id;
        public Vector2 center;
        public int width;
        public int height;
        public GameObject root;
    }

    private struct Line
    {
        public Vector2 start;
        public Vector2 end;
    }

    private List<DungeonRoom> roomArray = new List<DungeonRoom>();
    private List<DungeonRoom> mainRoomList = new List<DungeonRoom>();
    private List<Line> corridorLineList = new List<Line>();
    private readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();


    private IEnumerator Start()
    {
        GenerateRoom();
        SpawnMap();
        yield return StartCoroutine(WaitForRigidbody());
        RoundRoomPositionToGrid();
        UpdateRoomInfo();
        SelectMainRoom();
        List<Triangle> triangleList = Triangulation();
        Graph<int> graph = SpanningTree(triangleList);
        AddBackEdges(triangleList, ref graph);
        ConstructCorridorLine(graph);
    }

    private void GenerateRoom()
    {
        // Immediately get next rnd numOfRooms but use current(max) roomWidth & roomHeight to calculate radius.
        numOfRooms.Next();
        float radius = Mathf.Sqrt(numOfRooms.Current) * (roomWidth.Current + roomHeight.Current) / 6;
        for (int i = 0; i < numOfRooms.Current; i++)
        {
            // Populate dungeon room array
            Vector2 roomCenter = MathUtils.GetRandomPointInCircle(radius);

            roomArray.Add(new DungeonRoom
            {
                id = i,
                center = roomCenter,
                width = roomWidth.Next(),
                height = roomHeight.Next()
            });
        }
    }

    private void SpawnMap()
    {
        for (int i = 0;  i < roomArray.Count; i++)
        {
            GameObject roomRoot = new GameObject("roomRoot");
            roomRoot.transform.position = roomArray[i].center;
            roomRoot.transform.SetParent(roomHolder.transform);

            // Spawn floor and wall
            for (int m = 0; m < roomArray[i].width; m += Constants.MapInfo.GridSize)
            {
                for (int n = 0; n < roomArray[i].height; n += Constants.MapInfo.GridSize)
                {
                    Vector2 position = new Vector2(roomArray[i].center.x + (roomArray[i].width / 2 - m) * Constants.MapInfo.GridSize,
                                                   roomArray[i].center.y + (roomArray[i].height / 2 - n) * Constants.MapInfo.GridSize);

                    if (m == 0 || m == (roomArray[i].width - Constants.MapInfo.GridSize) ||
                        n == 0 || n == (roomArray[i].height - Constants.MapInfo.GridSize))
                        Instantiate(wallPrefabs[0], position, Quaternion.identity, roomRoot.transform);
                    else
                        Instantiate(floorPrefabs[0], position, Quaternion.identity, roomRoot.transform);
                }
            }

            BoxCollider2D collider2d = roomRoot.AddComponent<BoxCollider2D>();
            collider2d.size = new Vector2(roomArray[i].width, roomArray[i].height);

            Rigidbody2D rigidbody2d = roomRoot.AddComponent<Rigidbody2D>();
            rigidbody2d.gravityScale = 0;
            rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;

            roomArray[i].root = roomRoot;
            DungeonRoomId roomID = roomRoot.AddComponent<DungeonRoomId>();
            roomID.id = roomArray[i].id;
        }
    }

    private IEnumerator WaitForRigidbody()
    {
        // Speed up physics simulation.
        Time.timeScale = 10;
        while (true)
        {
            bool isAllSleeping = true;

            for (int i = 0; i < roomArray.Count; i++)
            {
                if (!roomArray[i].root.GetComponent<Rigidbody2D>().IsSleeping())
                {
                    isAllSleeping = false;
                    break;
                }
            }

            if (isAllSleeping)
                break;
            yield return waitForFixedUpdate;
        }

        Debug.LogFormat("WaitForRigidbody Finished!");
        Time.timeScale = 1;
    }

    private void RoundRoomPositionToGrid()
    {
        //~TODO:
        for (int i = 0; i < roomArray.Count; i++)
        {
            roomArray[i].root.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            roomArray[i].root.transform.position = new Vector2(
                MathUtils.RoundToGrid(roomArray[i].root.transform.position.x, Constants.MapInfo.GridSize),
                MathUtils.RoundToGrid(roomArray[i].root.transform.position.y, Constants.MapInfo.GridSize));
        }
    }

    private void UpdateRoomInfo()
    {
        for (int i = 0; i < roomArray.Count; i++)
        {
            roomArray[i].center = roomArray[i].root.transform.position;
        }
    }

    private void SelectMainRoom()
    {
        mainRoomList.Clear();

        // Get mean of width and height.
        float widthMean = 0f, heightMean = 0f;
        for (int i = 0; i < roomArray.Count; i++)
        {
            widthMean += roomArray[i].width;
            heightMean += roomArray[i].height;
        }
        widthMean /= (float)roomArray.Count;
        heightMean /= (float)roomArray.Count;

        for (int i = 0; i < roomArray.Count; i++)
        {
            if (roomArray[i].width > widthMean * Constants.MapInfo.MainRoomThreshold &&
                roomArray[i].height > heightMean * Constants.MapInfo.MainRoomThreshold)
            {
                mainRoomList.Add(roomArray[i]);
                Instantiate(wallPrefabs[0], roomArray[i].center, Quaternion.identity, roomArray[i].root.transform);
            }
        }
    }

    private List<Triangle> Triangulation()
    {
        List<Vector3> pointList = new List<Vector3>();
        for (int i = 0; i < mainRoomList.Count; i++)      
            pointList.Add(new Vector3(mainRoomList[i].center.x, mainRoomList[i].center.y, mainRoomList[i].id));

        List<Triangle> triangleList = DelaunayTriangulation.TriangulateByFlippingEdges(pointList);

        ///
        DrawTriangles(triangleList);

        return triangleList;
    }

    private void DrawTriangles(List<Triangle> triangleList)
    {
        //Debug.LogFormat("DrawTriangles, num = {0}", triangleList.Count);
        for (int i = 0; i < triangleList.Count; i++)
        {
            //Debug.LogFormat("v1: {0}, v2: {1}, Weight: {2}", triangleList[i].v1.id, triangleList[i].v2.id, Vector2.Distance(triangleList[i].v1.position, triangleList[i].v2.position));
            //Debug.LogFormat("v2: {0}, v3: {1}, Weight: {2}", triangleList[i].v2.id, triangleList[i].v3.id, Vector2.Distance(triangleList[i].v2.position, triangleList[i].v3.position));
            //Debug.LogFormat("v3: {0}, v1: {1}, Weight: {2}", triangleList[i].v3.id, triangleList[i].v1.id, Vector2.Distance(triangleList[i].v3.position, triangleList[i].v1.position));

            Debug.DrawLine(triangleList[i].v1.position, triangleList[i].v2.position, Color.yellow, 5f);
            Debug.DrawLine(triangleList[i].v2.position, triangleList[i].v3.position, Color.yellow, 5f);
            Debug.DrawLine(triangleList[i].v3.position, triangleList[i].v1.position, Color.yellow, 5f);
        }
    }

    private Graph<int> SpanningTree(List<Triangle> triangleList)
    {
        // Generate graph from triangle list.
        // The context of a node in the graph holds the value of the id in a triangle's vertex.
        // And the weight of an edge is the distance between to centers.
        Graph<int> graph = new Graph<int>(mainRoomList.ConvertAll<int>(n => n.id));

        for (int i = 0; i < triangleList.Count; i++)
        {
            graph.AddEdge(triangleList[i].v1.id, triangleList[i].v2.id, Vector2.Distance(triangleList[i].v1.position, triangleList[i].v2.position));
            graph.AddEdge(triangleList[i].v2.id, triangleList[i].v3.id, Vector2.Distance(triangleList[i].v2.position, triangleList[i].v3.position));
            graph.AddEdge(triangleList[i].v3.id, triangleList[i].v1.id, Vector2.Distance(triangleList[i].v3.position, triangleList[i].v1.position));
        }

        MST<int> mst = new MST<int>();
        Graph<int> mstGraph = mst.Prim(graph);

        ///
        DrawSpanningTree(mstGraph);

        return mstGraph;
    }

    private void DrawSpanningTree(Graph<int> mstGraph)
    {
        foreach (var node in mstGraph.NodeList)
        {
            for (int i = 0; i < node.EdgeList.Count; i++)
            {
                //Debug.LogFormat("room id: {0}, child: {1}", node.Context, node.EdgeList[i].Next.Context);
                Debug.DrawLine(roomArray[node.Context].center, roomArray[node.EdgeList[i].Next.Context].center, Color.red, 100f);
            }
        }
    }

    private void AddBackEdges(List<Triangle> triangleList, ref Graph<int> mstGraph)
    {
        List<int> v1List = new List<int>();
        List<int> v2List = new List<int>();
        List<bool> isPickedList = new List<bool>();

        for (int i = 0; i < triangleList.Count; i++)
        {
            if (!mstGraph.ContainsEdge(triangleList[i].v1.id, triangleList[i].v2.id))
            {
                v1List.Add(triangleList[i].v1.id);
                v2List.Add(triangleList[i].v2.id);
                isPickedList.Add(false);
            }
            if (!mstGraph.ContainsEdge(triangleList[i].v2.id, triangleList[i].v3.id))
            {
                v1List.Add(triangleList[i].v2.id);
                v2List.Add(triangleList[i].v3.id);
                isPickedList.Add(false);
            }
            if (!mstGraph.ContainsEdge(triangleList[i].v3.id, triangleList[i].v1.id))
            {
                v1List.Add(triangleList[i].v3.id);
                v2List.Add(triangleList[i].v1.id);
                isPickedList.Add(false);
            }
        }

        int index = MathUtils.rnd.Next(0, v1List.Count);
        for (int i = 0; i < v1List.Count * edgeAddBackRatio; i++)
        {
            while (isPickedList[index])
                index = MathUtils.rnd.Next(0, v1List.Count);

            mstGraph.AddEdge(v1List[index], v2List[index], 1f);
            isPickedList[index] = true;

            ///
            Debug.DrawLine(roomArray[v1List[index]].center, roomArray[v2List[index]].center, Color.green, 100f);
        }
    }

    private void ConstructCorridorLine(Graph<int> roomGraph)
    {
        corridorLineList.Clear();
        foreach (var node in roomGraph.NodeList)
        {
            foreach (var nextNode in node.GetNeighbors())
            {
                if (MathUtils.NearlyEqual(node.GetWeight(nextNode), 1f)) // Weight 1f is the original default weight in roomGraph.
                {
                    DungeonRoom room1 = roomArray[node.Context];
                    DungeonRoom room2 = roomArray[nextNode.Context];
                    Vector2 start = new Vector2();
                    Vector2 end = new Vector2();

                    if (Mathf.Abs(room1.center.x - room2.center.x) < ((room1.width + room2.width) / 2 - Constants.MapInfo.GridSize)) // Construct vertical line.
                    {
                        float lineX = (room1.center.x + room2.center.x) / 2;
                        start = new Vector2(lineX, room1.center.y);
                        end = new Vector2(lineX, room2.center.y);
                        AddCorridorLine(start, end);
                    }
                    else if (Mathf.Abs(room1.center.y - room2.center.y) < ((room1.height + room2.height) / 2 -Constants.MapInfo.GridSize)) // Construct horizontal line.
                    {
                        float lineY = (room1.center.y + room2.center.y) / 2;
                        start = new Vector2(room1.center.x, lineY);
                        end = new Vector2(room2.center.x, lineY);
                        AddCorridorLine(start, end);
                    }
                    else // Construct L shape line.
                    {
                        start = room1.center;
                        end = new Vector2(room1.center.x, room2.center.y);
                        AddCorridorLine(start, end);

                        start = room2.center;
                        end = new Vector2(room1.center.x, room2.center.y);
                        AddCorridorLine(start, end);
                    }

                    node.SetWeight(nextNode, 2f); // Weight 2f indicates that this edge is dealt with.
                    nextNode.SetWeight(node, 2f);
                }
            }
        }
    }

    private void AddCorridorLine(Vector2 start, Vector2 end)
    {
        Line line = new Line
        {
            start = start,
            end = end
        };
        corridorLineList.Add(line);
        Debug.DrawLine(start, end, Color.blue, 100f);
    }
}
