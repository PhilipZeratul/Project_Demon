using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;


public class DungeonGenerator : MonoBehaviour
{
    //~TODO: Make these private but show in inspector later.
    [Range(0, 20)]
    public int minRooms = 10;
    [Range(0, 50)]
    public int maxRooms = 20;
    [Range(0, 10)]
    public int minRoomHeight = 3;
    [Range(0, 10)]
    public int maxRoomHeight = 10;
    [Range(0, 10)]
    public int minRoomWidth = 3;
    [Range(0, 10)]
    public int maxRoomWidth = 10;

    private int numOfRooms;
    private List<Vector2> roomCenterList = new List<Vector2>();


    private void Start()
    {
        if (minRooms > maxRooms)
        {
            int temp = maxRooms;
            maxRooms = minRooms;
            minRooms = temp;
        }
        numOfRooms = MathUtils.rnd.Next(minRooms, maxRooms);

        float radius = math.sqrt(numOfRooms) * (maxRoomWidth + maxRoomHeight) / 6;
        for (int i = 0; i < numOfRooms; i++)
        {
            Vector2 roomCenter = MathUtils.GetRandomPointInCircle(radius);
            roomCenterList.Add(roomCenter);
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (roomCenterList.Count > 0)
            for (int i = 0; i < roomCenterList.Count; i++)
            {
                Gizmos.DrawSphere(roomCenterList[i], 0.2f);
            }
    }
}
