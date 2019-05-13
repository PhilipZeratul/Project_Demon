using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TriangulationMethods;


public class DungeonGenerator : MonoBehaviour
{
    public IntRange numOfRooms = new IntRange(5, 20);
    public IntRange roomWidth = new IntRange(3, 10);
    public IntRange roomHeight = new IntRange(3, 10);

    public GameObject roomHolder;
    public GameObject[] floorPrefabs;
    public GameObject[] wallPrefabs;

    private struct DungeonRoom
    {
        public int id;
        public Vector2 center;
        public int width;
        public int height;
        public GameObject root;
    }

    private DungeonRoom[] roomArray;
    private List<DungeonRoom> mainRoomList = new List<DungeonRoom>();
    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();


    private IEnumerator Start()
    {
        Generate();
        SpawnMap();
        yield return StartCoroutine(WaitForRigidbody());
        UpdateRoomInfo();
        SelectMainRoom();
        Triangulation();
    }

    private void Generate()
    {
        roomArray = new DungeonRoom[numOfRooms.Next()];

        float radius = Mathf.Sqrt(numOfRooms.Current) * (roomWidth.Current + roomHeight.Current) / 6;
        for (int i = 0; i < numOfRooms.Current; i++)
        {
            // Populate dungeon room array
            Vector2 roomCenter = MathUtils.GetRandomPointInCircle(radius);

            roomArray[i] = new DungeonRoom
            {
                id = i,
                center = roomCenter,
                width = roomWidth.Next(),
                height = roomHeight.Next()
            };
        }
    }

    private void SpawnMap()
    {
        for (int i = 0;  i < roomArray.Length; i++)
        {
            GameObject roomRoot = new GameObject("roomRoot");
            roomRoot.transform.position = roomArray[i].center;
            roomRoot.transform.SetParent(roomHolder.transform);

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
        }
    }

    private IEnumerator WaitForRigidbody()
    {
        // Speed up physics simulation.
        Time.timeScale = 10;
        while (true)
        {
            bool isAllSleeping = true;

            for (int i = 0; i < roomArray.Length; i++)
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

    private void UpdateRoomInfo()
    {
        for (int i = 0; i < roomArray.Length; i++)
        {
            roomArray[i].center = roomArray[i].root.transform.position;
        }
    }

    private void SelectMainRoom()
    {
        mainRoomList.Clear();

        // Get mean of width and height.
        float widthMean = 0f, heightMean = 0f;
        for (int i = 0; i < roomArray.Length; i++)
        {
            widthMean += roomArray[i].width;
            heightMean += roomArray[i].height;
        }
        widthMean /= (float)roomArray.Length;
        heightMean /= (float)roomArray.Length;

        for (int i = 0; i < roomArray.Length; i++)
        {
            if (roomArray[i].width > widthMean * Constants.MapInfo.MainRoomThreshold &&
                roomArray[i].height > heightMean * Constants.MapInfo.MainRoomThreshold)
            {
                mainRoomList.Add(roomArray[i]);
                Instantiate(wallPrefabs[0], roomArray[i].center, Quaternion.identity, roomArray[i].root.transform);
            }
        }
    }

    private void Triangulation()
    {
        Debug.Log("DelaunayTriangulation");



        List<Vector3> pointList = new List<Vector3>();
        for (int i = 0; i < mainRoomList.Count; i++)      
            pointList.Add(mainRoomList[i].center);

        List<Triangle> triangleList = DelaunayTriangulation.TriangulateByFlippingEdges(pointList);

        Debug.Log("Triangulation Finished");
        /////
        DrawTriangles(triangleList);
    }

    private void DrawTriangles(List<Triangle> triangleList)
    {

        Debug.LogFormat("DrawTriangles, num = {0}", triangleList.Count);
        for (int i = 0; i < triangleList.Count; i++)
        {
            Debug.DrawLine(triangleList[i].v1.position, triangleList[i].v2.position, Color.yellow, 100f);
            Debug.DrawLine(triangleList[i].v2.position, triangleList[i].v3.position, Color.yellow, 100f);
            Debug.DrawLine(triangleList[i].v3.position, triangleList[i].v1.position, Color.yellow, 100f);
        }
    }
}
