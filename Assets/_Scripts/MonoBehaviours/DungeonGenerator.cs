using UnityEngine;


public class DungeonGenerator : MonoBehaviour
{
    public IntRange numOfRooms = new IntRange(5, 20);
    public IntRange roomWidth = new IntRange(3, 10);
    public IntRange roomHeight = new IntRange(3, 10);

    public GameObject roomHolder;
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
            DrawMap(roomArray[i]);
        }
    }

    private void DrawMap(DungeonRoom room)
    {
        GameObject roomRoot = new GameObject("roomRoot");
        roomRoot.transform.position = room.center;
        roomRoot.transform.SetParent(roomHolder.transform);

        for (int i = 0; i < room.width; i += Constants.MapInfo.GridSize)
        {
            for (int j = 0; j < room.height; j += Constants.MapInfo.GridSize)
            {
                Vector2 position = new Vector2(room.center.x + (room.width / 2 - i) * Constants.MapInfo.GridSize,
                                               room.center.y + (room.height / 2 - j) * Constants.MapInfo.GridSize);

                if (i == 0 || i == (room.width - Constants.MapInfo.GridSize) ||
                    j == 0 || j == (room.height - Constants.MapInfo.GridSize))
                    Instantiate(wallPrefabs[0], position, Quaternion.identity, roomRoot.transform);
                else
                    Instantiate(floorPrefabs[0], position, Quaternion.identity, roomRoot.transform);

            }
        }

        BoxCollider2D collider2d = roomRoot.AddComponent<BoxCollider2D>();
        collider2d.size = new Vector2(room.width, room.height);

        Rigidbody2D rigidbody2d = roomRoot.AddComponent<Rigidbody2D>();
        rigidbody2d.gravityScale = 0;
        rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;      
    }

    public struct DungeonRoom
    {
        public Vector2 center;
        public int width;
        public int height;
    }
}
