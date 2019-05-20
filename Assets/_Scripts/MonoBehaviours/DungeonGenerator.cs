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
    public GameObject corridorHolder;
    public GameObject[] floorPrefabs;
    public GameObject[] wallPrefabs;

    private class DungeonRoom
    {
        public int id;
        public Vector2 center;
        public int width;
        public int height;
        public GameObject root;
        public List<GameObject> floorGOList = new List<GameObject>();
        public List<GameObject> wallGOList = new List<GameObject>();
    }

    private class Line
    {
        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }

        // Horizontal line: from left to right
        // Vertical line: from bottom to top
        public Line(Vector2 start, Vector2 end)
        {
            if ((start.x < end.x) || (start.y < end.y))
            {
                Start = start;
                End = end;
            }
            else
            {
                Start = end;
                End = start;
            }
        }

        public Vector2 Center()
        {
            return (Start + End) / 2;
        }

        public bool IsHorizontal()
        {
            return MathUtils.NearlyEqual(Start.y, End.y);
        }

        public bool IsVertical()
        {
            return MathUtils.NearlyEqual(Start.x, End.x);
        }
    }

    private List<DungeonRoom> allRoomList = new List<DungeonRoom>();
    private List<DungeonRoom> mainRoomList = new List<DungeonRoom>();
    private List<DungeonRoom> supportRoomList = new List<DungeonRoom>();
    private List<DungeonRoom> corridorRoomList = new List<DungeonRoom>();
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
        GenerateCorridorLine(graph);
        PruneRoom();
        ChangeCollider();
        BuildCorridor();
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

            allRoomList.Add(new DungeonRoom
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
        for (int i = 0;  i < allRoomList.Count; i++)
        {
            GameObject roomRoot = new GameObject("roomRoot");
            roomRoot.transform.position = allRoomList[i].center;
            roomRoot.transform.SetParent(roomHolder.transform);

            // Spawn floor and wall
            for (int m = 0; m < allRoomList[i].width; m += Constants.MapInfo.GridSize)
            {
                for (int n = 0; n < allRoomList[i].height; n += Constants.MapInfo.GridSize)
                {
                    Vector2 position = new Vector2(allRoomList[i].center.x + (allRoomList[i].width / 2 - m) * Constants.MapInfo.GridSize,
                                                   allRoomList[i].center.y + (allRoomList[i].height / 2 - n) * Constants.MapInfo.GridSize);

                    if (m == 0 || m == (allRoomList[i].width - Constants.MapInfo.GridSize) ||
                        n == 0 || n == (allRoomList[i].height - Constants.MapInfo.GridSize))
                    {
                        GameObject wallGO = Instantiate(wallPrefabs[0], position, Quaternion.identity, roomRoot.transform);
                        allRoomList[i].wallGOList.Add(wallGO);
                    }
                    else
                    {
                        GameObject floorGO = Instantiate(floorPrefabs[0], position, Quaternion.identity, roomRoot.transform);
                        allRoomList[i].floorGOList.Add(floorGO);
                    }
                }
            }

            BoxCollider2D collider2d = roomRoot.AddComponent<BoxCollider2D>();
            collider2d.size = new Vector2(allRoomList[i].width, allRoomList[i].height);

            Rigidbody2D rigidbody2d = roomRoot.AddComponent<Rigidbody2D>();
            rigidbody2d.gravityScale = 0;
            rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;

            allRoomList[i].root = roomRoot;
            DungeonRoomId roomID = roomRoot.AddComponent<DungeonRoomId>();
            roomID.id = allRoomList[i].id;
        }
    }

    private IEnumerator WaitForRigidbody()
    {
        // Speed up physics simulation.
        Time.timeScale = 10;
        while (true)
        {
            bool isAllSleeping = true;

            for (int i = 0; i < allRoomList.Count; i++)
            {
                if (!allRoomList[i].root.GetComponent<Rigidbody2D>().IsSleeping())
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
        for (int i = 0; i < allRoomList.Count; i++)
        {
            allRoomList[i].root.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            allRoomList[i].root.transform.position = new Vector2(
                MathUtils.RoundToGrid(allRoomList[i].root.transform.position.x, Constants.MapInfo.GridSize),
                MathUtils.RoundToGrid(allRoomList[i].root.transform.position.y, Constants.MapInfo.GridSize));
        }
    }

    private void UpdateRoomInfo()
    {
        for (int i = 0; i < allRoomList.Count; i++)
        {
            allRoomList[i].center = allRoomList[i].root.transform.position;
        }
    }

    private void SelectMainRoom()
    {
        mainRoomList.Clear();

        // Get mean of width and height.
        float widthMean = 0f, heightMean = 0f;
        for (int i = 0; i < allRoomList.Count; i++)
        {
            widthMean += allRoomList[i].width;
            heightMean += allRoomList[i].height;
        }
        widthMean /= (float)allRoomList.Count;
        heightMean /= (float)allRoomList.Count;

        for (int i = 0; i < allRoomList.Count; i++)
        {
            if (allRoomList[i].width > widthMean * Constants.MapInfo.MainRoomThreshold &&
                allRoomList[i].height > heightMean * Constants.MapInfo.MainRoomThreshold)
            {
                mainRoomList.Add(allRoomList[i]);
                Instantiate(wallPrefabs[0], allRoomList[i].center, Quaternion.identity, allRoomList[i].root.transform);
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
                Debug.DrawLine(allRoomList[node.Context].center, allRoomList[node.EdgeList[i].Next.Context].center, Color.red, 100f);
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
            Debug.DrawLine(allRoomList[v1List[index]].center, allRoomList[v2List[index]].center, Color.green, 100f);
        }
    }

    //~TODO: Remove unnecessary line segments at start and end.
    private void GenerateCorridorLine(Graph<int> roomGraph)
    {
        corridorLineList.Clear();
        foreach (var node in roomGraph.NodeList)
        {
            foreach (var nextNode in node.GetNeighbors())
            {
                if (MathUtils.NearlyEqual(node.GetWeight(nextNode), 1f)) // Weight 1f is the original default weight in roomGraph.
                {
                    DungeonRoom room1 = allRoomList[node.Context];
                    DungeonRoom room2 = allRoomList[nextNode.Context];
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
        start = new Vector2(MathUtils.RoundToGrid(start.x), MathUtils.RoundToGrid(start.y));
        end = new Vector2(MathUtils.RoundToGrid(end.x), MathUtils.RoundToGrid(end.y));
        Line line = new Line(start, end);
        corridorLineList.Add(line);
        Debug.DrawLine(start, end, Color.blue, 100f);
    }

    private void PruneRoom()
    {
        // Find support rooms.
        for (int i = 0; i < corridorLineList.Count; i++)
        {
            ContactFilter2D filter2D = new ContactFilter2D();
            List<RaycastHit2D> hitList = new List<RaycastHit2D>();
            int hitNum = Physics2D.Linecast(corridorLineList[i].Start, corridorLineList[i].End, filter2D.NoFilter(), hitList);

            for (int j = 0; j < hitNum; j++)
            {
                int roomID = hitList[j].transform.GetComponent<DungeonRoomId>().id;
                if (mainRoomList.Find(room => room.id == roomID) == null)
                {
                    supportRoomList.Add(allRoomList[roomID]);
                }
            }
        }
        // Remove unused room gameObjects.
        for (int i = 0; i < allRoomList.Count; i++)
        {
            if (!mainRoomList.Contains(allRoomList[i]) &&
                !supportRoomList.Contains(allRoomList[i]))
            {
                Destroy(allRoomList[i].root);
            }
        }
        allRoomList.Clear();
        allRoomList = new List<DungeonRoom>(mainRoomList.Count + supportRoomList.Count);
        allRoomList.AddRange(mainRoomList);
        allRoomList.AddRange(supportRoomList);
    }

    private void ChangeCollider()
    {
        foreach (var room in allRoomList)
        {
            Destroy(room.root.GetComponent<Collider2D>());
            foreach (var floorGO in room.floorGOList)
            {
                floorGO.AddComponent<BoxCollider2D>();
            }
            foreach (var wallGO in room.wallGOList)
            {
                wallGO.AddComponent<BoxCollider2D>();
            }
        }
    }

    private void BuildCorridor()
    {
        float circleRadius = Constants.MapInfo.GridSize / 4f;
        ContactFilter2D filter2D = new ContactFilter2D();

        foreach (var line in corridorLineList)
        {
            GameObject corridorRoot = new GameObject("corridorRoot");
            corridorRoot.transform.position = line.Center();
            corridorRoot.transform.SetParent(corridorHolder.transform);

            if (line.IsHorizontal())
            {
                for (float x = line.Start.x - 2 * Constants.MapInfo.GridSize;
                           x < line.End.x + 3 * Constants.MapInfo.GridSize;
                           x += Constants.MapInfo.GridSize)
                {
                    float[] y = new float[5];
                    y[0] = line.Start.y - 2 * Constants.MapInfo.GridSize; // Wall
                    y[1] = line.Start.y - Constants.MapInfo.GridSize;     // Floor
                    y[2] = line.Start.y;                                  // Floor
                    y[3] = line.Start.y + Constants.MapInfo.GridSize;     // Floor
                    y[4] = line.Start.y + 2 * Constants.MapInfo.GridSize; // Wall

                    for (int i = 0; i < y.Length; i++)
                    {
                        Vector2 position = new Vector2(x, y[i]);


                        Debug.LogFormat("x = {0}, y = {1}", x, y[i]);


                        List<Collider2D> colliderList = new List<Collider2D>();
                        //int hitNum = Physics2D.BoxCast(position, boxSize, 0f, Vector2.right, filter2D.NoFilter(), hitList, 0.01f);
                        int hitNum = Physics2D.OverlapCircle(position, circleRadius, filter2D.NoFilter(), colliderList);
                        //Debug.LogFormat("hitNum = {0}", hitNum);
                        if (hitNum == 0)
                        {
                            SpawnCorridorTile(position, i, corridorRoot);
                        }
                        else
                        {
                            //foreach (var hit in colliderList)
                            //{
                            //    if (hit.GetComponent<DungeonWall>())
                            //    {
                            //        Destroy(hit.gameObject);
                            //        SpawnCorridorTile(position, i, corridorRoot);
                            //    }
                            //}
                        }

                    }
                }
            }
            else if (line.IsVertical())
            {

            }
        }
    }

    private void SpawnCorridorTile(Vector2 position, int i, GameObject root)
    {
        GameObject tileGO;
        if (i == 0 || i == 4)
            tileGO = Instantiate(wallPrefabs[0], position, Quaternion.identity, root.transform);
        else
            tileGO = Instantiate(floorPrefabs[0], position, Quaternion.identity, root.transform);
        tileGO.AddComponent<BoxCollider2D>();
    }
}
