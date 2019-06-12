using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TriangulationMethods;
using MinimumSpanningTree;


public class DungeonGenerator : MonoBehaviour
{
    public List<DungeonRoomData> allRoomList = new List<DungeonRoomData>();
    public List<DungeonRoomData> mainRoomList = new List<DungeonRoomData>();
    public List<DungeonRoomData> supportRoomList = new List<DungeonRoomData>();
    public List<DungeonRoomData> corridorRoomList = new List<DungeonRoomData>();
    private List<DungeonRoomData> initialRoomList = new List<DungeonRoomData>();

    public bool IsError { get; private set; }
    public bool IsGenerationFinished { get; private set; }
    public Action GenerationFinished;

    [SerializeField] private IntRange numOfRooms = new IntRange(5, 20);
    [SerializeField] private IntRange roomWidth = new IntRange(3, 10);
    [SerializeField] private IntRange roomHeight = new IntRange(3, 10);
    [Range(0f, 1f)]
    [SerializeField] private readonly float edgeAddBackRatio = 0.1f;

    [SerializeField] private GameObject roomHolder;
    [SerializeField] private GameObject corridorHolder;
    [SerializeField] private TileSO floorTileSO;
    [SerializeField] private TileSO wallTileSO;

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

    private List<Line> corridorLineList = new List<Line>();
    private readonly WaitForSeconds waitForSpawn = new WaitForSeconds(0.2f);
    private readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();


    public IEnumerator GenerateDungeon(int numOfMainRooms)
    {
        IsGenerationFinished = false;
        IsError = false;

        GenerateRoom();
        SpawnRoomGO();
        yield return StartCoroutine(WaitForRigidbody());
        RoundRoomPositionToGrid();
        UpdateRoomInfo();
        SelectMainRoom(numOfMainRooms);
        List<Triangle> triangleList = Triangulation();
        Graph<int> graph = SpanningTree(triangleList);
        AddBackEdges(triangleList, ref graph);
        UpdateMainRoomConnection(ref graph);
        GenerateCorridorLine(graph);
        PruneRoom();
        ChangeCollider();
        yield return waitForFixedUpdate;
        BuildCorridor();
        yield return waitForFixedUpdate;


        IsGenerationFinished = true;
        GenerationFinished?.Invoke();
    }

    public IEnumerator ReGenerateDungeon(int numOfMainRooms)
    {
        yield return StartCoroutine(ClearAll());
        StartCoroutine(GenerateDungeon(numOfMainRooms));
    }

    private void GenerateRoom()
    {
        // Immediately get next rnd numOfRooms but use current(max) roomWidth & roomHeight to calculate radius.
        numOfRooms.NextOdd();
        float radius = Mathf.Sqrt(numOfRooms.Current) * (roomWidth.Current + roomHeight.Current) / 6;
        for (int i = 0; i < numOfRooms.Current; i++)
        {
            // Populate dungeon room array
            //Vector2 roomCenter = MathUtils.GetRandomPointInCircle(radius);
            //Vector2 roomCenter = MathUtils.GetRandomPointInRect(radius * 1.2f, radius);
            Vector2 roomCenter = MathUtils.GetRandomPointInEclipse(radius, 2f, 1f);

            initialRoomList.Add(new DungeonRoomData
            {
                id = i,
                center = roomCenter,
                width = roomWidth.NextOdd(),
                height = roomHeight.NextOdd()
            });
        }
    }

    private void SpawnRoomGO()
    {
        for (int id = 0;  id < initialRoomList.Count; id++)
        {
            GameObject roomRoot = new GameObject("roomRoot");
            roomRoot.transform.position = initialRoomList[id].center;
            roomRoot.transform.SetParent(roomHolder.transform);

            // Spawn floor and wall
            for (int m = 0; m < initialRoomList[id].width; m += Constants.MapInfo.GridSize)
            {
                for (int n = 0; n < initialRoomList[id].height; n += Constants.MapInfo.GridSize)
                {
                    Vector2 position = new Vector2(initialRoomList[id].center.x + (initialRoomList[id].width / 2 - m) * Constants.MapInfo.GridSize,
                                                   initialRoomList[id].center.y + (initialRoomList[id].height / 2 - n) * Constants.MapInfo.GridSize);
                    // If on edge
                    int right = initialRoomList[id].width - Constants.MapInfo.GridSize;
                    int bottom = initialRoomList[id].height - Constants.MapInfo.GridSize;
                    if (m == 0 || m == right ||
                        n == 0 || n == bottom)
                    {
                        // If not the corners
                        if (!((m == 0 && n == 0) || (m == 0 && n == bottom) ||
                             (m == right && n == 0) || (m == right && n == bottom)))
                        {
                            GameObject wallGO = Instantiate(wallTileSO.tilePrefabList[0], position, Quaternion.identity, roomRoot.transform);
                            wallGO.GetComponent<DungeonTile>().roomId = id;
                            DungeonRoomData.Tile wallTile = new DungeonRoomData.Tile(wallGO);
                            initialRoomList[id].wallTlieList.Add(wallTile);
                        }
                    }
                    else
                    {
                        GameObject floorGO = Instantiate(floorTileSO.tilePrefabList[0], position, Quaternion.identity, roomRoot.transform);
                        floorGO.GetComponent<DungeonTile>().roomId = id;
                        DungeonRoomData.Tile floorTile = new DungeonRoomData.Tile(floorGO);
                        initialRoomList[id].floorTileList.Add(floorTile);
                    }
                }
            }

            BoxCollider2D collider2d = roomRoot.AddComponent<BoxCollider2D>();
            collider2d.size = new Vector2(initialRoomList[id].width + 4 * Constants.MapInfo.GridSize, initialRoomList[id].height + 4 * Constants.MapInfo.GridSize);

            Rigidbody2D rigidbody2d = roomRoot.AddComponent<Rigidbody2D>();
            rigidbody2d.gravityScale = 0;
            rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;

            initialRoomList[id].root = roomRoot;
            DungeonRoom dungeonRoom = roomRoot.AddComponent<DungeonRoom>();
            dungeonRoom.roomId = initialRoomList[id].id;
        }
    }

    private IEnumerator WaitForRigidbody()
    {
        // Speed up physics simulation.
        Time.timeScale = 20;
        while (true)
        {
            bool isAllSleeping = true;

            for (int i = 0; i < initialRoomList.Count; i++)
            {
                if (!initialRoomList[i].root.GetComponent<Rigidbody2D>().IsSleeping())
                {
                    isAllSleeping = false;
                    break;
                }
            }

            if (isAllSleeping)
                break;
            yield return waitForFixedUpdate;
        }

        //Debug.LogFormat("WaitForRigidbody Finished!");
        Time.timeScale = 1;
    }

    private void RoundRoomPositionToGrid()
    {
        //~TODO:
        for (int i = 0; i < initialRoomList.Count; i++)
        {
            initialRoomList[i].root.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            initialRoomList[i].root.transform.position = new Vector2(
                MathUtils.RoundToGrid(initialRoomList[i].root.transform.position.x, Constants.MapInfo.GridSize),
                MathUtils.RoundToGrid(initialRoomList[i].root.transform.position.y, Constants.MapInfo.GridSize));
        }
    }

    private void UpdateRoomInfo()
    {
        for (int i = 0; i < initialRoomList.Count; i++)
        {
            initialRoomList[i].center = initialRoomList[i].root.transform.position;
        }
    }

    private void SelectMainRoom(int numOfMainRooms)
    {
        mainRoomList.Clear();

        // Get mean of width and height.
        float widthMean = 0f, heightMean = 0f;
        for (int i = 0; i < initialRoomList.Count; i++)
        {
            widthMean += initialRoomList[i].width;
            heightMean += initialRoomList[i].height;
        }
        widthMean /= (float)initialRoomList.Count;
        heightMean /= (float)initialRoomList.Count;

        int count = 0;
        for (int i = 0; i < initialRoomList.Count; i++)
        {
            if (initialRoomList[i].width >= Mathf.FloorToInt(widthMean * Constants.MapInfo.MainRoomThreshold) &&
                initialRoomList[i].height >= Mathf.FloorToInt(heightMean * Constants.MapInfo.MainRoomThreshold))
            {
                mainRoomList.Add(initialRoomList[i]);
                count++;
                if (count >= numOfMainRooms)
                    break;
                // A marker for main room center.
                Instantiate(wallTileSO.tilePrefabList[0], initialRoomList[i].center, Quaternion.identity, initialRoomList[i].root.transform);
            }
        }

        if (count < numOfMainRooms)
            IsError = true;
    }

    private List<Triangle> Triangulation()
    {
        List<Vector3> pointList = new List<Vector3>();
        for (int i = 0; i < mainRoomList.Count; i++)      
            pointList.Add(new Vector3(mainRoomList[i].center.x, mainRoomList[i].center.y, mainRoomList[i].id));

        List<Triangle> triangleList = DelaunayTriangulation.TriangulateByFlippingEdges(pointList);

        ///
        //DrawTriangles(triangleList);

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
                Debug.DrawLine(initialRoomList[node.Context].center, initialRoomList[node.EdgeList[i].Next.Context].center, Color.red, 100f);
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
            // Get an index that is not picked.
            while (isPickedList[index])
                index = MathUtils.rnd.Next(0, v1List.Count);

            mstGraph.AddEdge(v1List[index], v2List[index], 1f);
            isPickedList[index] = true;

            ///
            Debug.DrawLine(initialRoomList[v1List[index]].center, initialRoomList[v2List[index]].center, Color.green, 100f);
        }
    }

    private void UpdateMainRoomConnection(ref Graph<int> graph)
    {
        foreach (var node in graph.NodeList)
        {
            foreach (var edge in node.EdgeList)
            {
                initialRoomList[node.Context].connectedIdList.Add(edge.Next.Context);
            }
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
                    DungeonRoomData room1 = initialRoomList[node.Context];
                    DungeonRoomData room2 = initialRoomList[nextNode.Context];
                    Vector2 start = new Vector2();
                    Vector2 end = new Vector2();

                    if (Mathf.Abs(room1.center.x - room2.center.x) < ((room1.width + room2.width) / 2 - 5 * Constants.MapInfo.GridSize)) // Construct vertical line.
                    {
                        float lineX = (room1.center.x + room2.center.x) / 2;
                        start = new Vector2(lineX, room1.center.y);
                        end = new Vector2(lineX, room2.center.y);
                        AddCorridorLine(start, end);
                    }
                    else if (Mathf.Abs(room1.center.y - room2.center.y) < ((room1.height + room2.height) / 2 - 5 * Constants.MapInfo.GridSize)) // Construct horizontal line.
                    {
                        float lineY = (room1.center.y + room2.center.y) / 2;
                        start = new Vector2(room1.center.x, lineY);
                        end = new Vector2(room2.center.x, lineY);
                        AddCorridorLine(start, end);
                    }
                    else // Construct L shape line.
                    {
                        float centerX = room1.center.x + MathUtils.rnd.Next(-1, 2);
                        float centerY = room2.center.y + MathUtils.rnd.Next(-1, 2);
                        // Vertical
                        start = new Vector2(centerX, room1.center.y);
                        end = new Vector2(centerX, centerY);
                        AddCorridorLine(start, end);
                        // Horizontal
                        start = new Vector2(room2.center.x, centerY);
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
        //Debug.DrawLine(start, end, Color.blue, 100f);
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
                int roomID = hitList[j].transform.GetComponent<DungeonRoom>().roomId;
                if (mainRoomList.Find(room => room.id == roomID) == null &&
                    !supportRoomList.Contains(initialRoomList[roomID]))
                {
                    supportRoomList.Add(initialRoomList[roomID]);
                }
            }
        }
        // Remove unused room gameObjects.
        for (int i = 0; i < initialRoomList.Count; i++)
        {
            if (!mainRoomList.Contains(initialRoomList[i]) &&
                !supportRoomList.Contains(initialRoomList[i]))
            {
                Destroy(initialRoomList[i].root);
            }
        }
        allRoomList.AddRange(mainRoomList);
        allRoomList.AddRange(supportRoomList);
    }

    private void ChangeCollider()
    {
        BoxCollider2D collider2d;
        foreach (var room in allRoomList)
        {
            Destroy(room.root.GetComponent<Collider2D>());
            Destroy(room.root.GetComponent<Rigidbody2D>());

            foreach (var floorTile in room.floorTileList)
            {
                collider2d = floorTile.go.AddComponent<BoxCollider2D>();
                floorTile.collider2d = collider2d;
            }
            foreach (var wallTile in room.wallTlieList)
            {
                collider2d = wallTile.go.AddComponent<BoxCollider2D>();
                wallTile.collider2d = collider2d;
            }
        }
    }

    private void BuildCorridor()
    {
        RaycastHit2D[] hits = new RaycastHit2D[1];
        Vector2 hitDirection = Vector2.zero;
        bool isWall;

        for (int k = 0; k < corridorLineList.Count; k++)
        {
            var line = corridorLineList[k];
            GameObject corridorRoot = new GameObject("corridorRoot");
            corridorRoot.transform.position = line.Center();
            corridorRoot.transform.SetParent(corridorHolder.transform);
            DungeonRoomData corridorRoom = new DungeonRoomData
            {
                id = 100 + k,
                center = line.Center(),
                width = (int)(line.End.x - line.Start.x),
                height = (int)(line.End.y - line.Start.y),
                root = corridorRoot
            };

            if (line.IsHorizontal())
            {
                for (float x = line.Start.x - 2f * Constants.MapInfo.GridSize;
                           x < line.End.x + 2.5f * Constants.MapInfo.GridSize;
                           x += Constants.MapInfo.GridSize)
                {
                    float[] y = new float[5];
                    y[0] = line.Start.y - 2f * Constants.MapInfo.GridSize; // Wall
                    y[1] = line.Start.y - Constants.MapInfo.GridSize;      // Floor
                    y[2] = line.Start.y;                                   // Floor
                    y[3] = line.Start.y + Constants.MapInfo.GridSize;      // Floor
                    y[4] = line.Start.y + 2f * Constants.MapInfo.GridSize; // Wall

                    for (int i = 0; i < y.Length; i++)
                    {
                        if (i == 0 || i == 4 ||
                            MathUtils.NearlyEqual(x, line.Start.x - 2f * Constants.MapInfo.GridSize) ||
                            MathUtils.NearlyEqual(x, line.End.x + 2f * Constants.MapInfo.GridSize))
                            isWall = true;
                        else
                            isWall = false;

                        Vector2 position = new Vector2(x, y[i]);
                        int hitNum = Physics2D.RaycastNonAlloc(position, hitDirection, hits);
                        if (hitNum == 0)
                        {
                            SpawnCorridorTile(position, isWall, corridorRoot, ref corridorRoom);
                        }
                        else
                        {
                            if (hits[0].collider.GetComponent<DungeonWall>())
                            {
                                Destroy(hits[0].collider.gameObject);
                                SpawnCorridorTile(position, isWall, corridorRoot, ref corridorRoom);
                            }
                        }
                    }
                }
            }
            else if (line.IsVertical())
            {
                for (float y = line.Start.y - 2f * Constants.MapInfo.GridSize;
                           y < line.End.y + 2.5f * Constants.MapInfo.GridSize;
                           y += Constants.MapInfo.GridSize)
                {
                    float[] x = new float[5];
                    x[0] = line.Start.x - 2f * Constants.MapInfo.GridSize; // Wall
                    x[1] = line.Start.x - Constants.MapInfo.GridSize;      // Floor
                    x[2] = line.Start.x;                                   // Floor
                    x[3] = line.Start.x + Constants.MapInfo.GridSize;      // Floor
                    x[4] = line.Start.x + 2f * Constants.MapInfo.GridSize; // Wall

                    for (int i = 0; i < x.Length; i++)
                    {
                        if (i == 0 || i == 4 ||
                            MathUtils.NearlyEqual(y, line.Start.y - 2f * Constants.MapInfo.GridSize) ||
                            MathUtils.NearlyEqual(y, line.End.y + 2f * Constants.MapInfo.GridSize))
                            isWall = true;
                        else
                            isWall = false;

                        Vector2 position = new Vector2(x[i], y);
                        int hitNum = Physics2D.RaycastNonAlloc(position, hitDirection, hits);
                        if (hitNum == 0)
                        {
                            SpawnCorridorTile(position, isWall, corridorRoot, ref corridorRoom);
                        }
                        else
                        {
                            if (hits[0].collider.GetComponent<DungeonWall>())
                            {
                                Destroy(hits[0].collider.gameObject);
                                SpawnCorridorTile(position, isWall, corridorRoot, ref corridorRoom);
                            }
                        }
                    }
                }
            }
            corridorRoomList.Add(corridorRoom);
        }
        allRoomList.AddRange(corridorRoomList);
    }

    private void SpawnCorridorTile(Vector2 position, bool isWall, GameObject root, ref DungeonRoomData corridorRoom)
    {
        GameObject tileGO;
        Collider2D collider2d;
        if (isWall)
        {
            tileGO = Instantiate(wallTileSO.tilePrefabList[0], position, Quaternion.identity, root.transform);
            DungeonRoomData.Tile tile = new DungeonRoomData.Tile(tileGO);
            collider2d = tileGO.AddComponent<BoxCollider2D>();
            tile.collider2d = collider2d;
            corridorRoom.wallTlieList.Add(tile);
        }
        else
        {
            tileGO = Instantiate(floorTileSO.tilePrefabList[0], position, Quaternion.identity, root.transform);
            DungeonRoomData.Tile tile = new DungeonRoomData.Tile(tileGO);
            collider2d = tileGO.AddComponent<BoxCollider2D>();
            tile.collider2d = collider2d;
            corridorRoom.floorTileList.Add(tile);
        }

    }

    private IEnumerator ClearAll()
    {
        IsGenerationFinished = false;
        IsError = false;

        allRoomList.Clear();
        mainRoomList.Clear();
        supportRoomList.Clear();
        corridorRoomList.Clear();
        corridorLineList.Clear();
        initialRoomList.Clear();

        foreach (Transform child in roomHolder.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in corridorHolder.transform)
        {
            Destroy(child.gameObject);
        }

        yield return waitForFixedUpdate;
    }
}
