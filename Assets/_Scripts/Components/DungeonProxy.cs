using Unity.Entities;
using UnityEngine;
using System;


// Serializable attribute is for editor support.
[Serializable]
public struct Dungeon : ISharedComponentData
{
    // int array of length rows x cols
    // 0 for road
    // 1 for wall

    [Range(0, 20)]
    public int minRooms;
    [Range(0, 50)]
    public int maxRooms;
    [Range(0, 10)]
    public int minRoomHeight;
    [Range(0, 10)]
    public int maxRoomHeight;
    [Range(0, 10)]
    public int minRoomWidth;
    [Range(0, 10)]
    public int maxRoomWidth;

    [HideInInspector]
    public DungeonRoom[] roomArray;
}

// ComponentDataProxy is for creating a MonoBehaviour representation of this component (for editor support).
[DisallowMultipleComponent]
public class DungeonProxy : SharedComponentDataProxy<Dungeon> { }
