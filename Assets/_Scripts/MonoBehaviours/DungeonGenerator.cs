using UnityEngine;


public class DungeonGenerator : MonoBehaviour
{
    public IntRange numOfRooms = new IntRange(5, 20);
    public IntRange roomWidth = new IntRange(3, 10);
    public IntRange roomHeight = new IntRange(3, 10);

    public GameObject tileHolder;
    public GameObject[] floorPrefabs;
    public GameObject[] wallPrefabs;

    private DungeonRoom[] roomArray;


    private void Start()
    {
        Generate();
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
                center = roomCenter,
                width = roomWidth.Next(),
                height = roomHeight.Next()
            };

            ///
            DrawMap(floorPrefabs[0], roomCenter);
        }
    }

    private void DrawMap(GameObject prefab, Vector2 position)
    {
        Instantiate(prefab, position, Quaternion.identity, tileHolder.transform);
    }

    public struct DungeonRoom
    {
        public Vector2 center;
        public float width;
        public float height;
    }
}
