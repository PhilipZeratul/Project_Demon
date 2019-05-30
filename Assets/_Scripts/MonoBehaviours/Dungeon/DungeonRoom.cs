using UnityEngine;
using System.Collections.Generic;


public class DungeonRoom
{
    public int id;
    public Vector2 center;
    public int width;
    public int height;
    public GameObject root;
    public List<GameObject> floorGOList = new List<GameObject>();
    public List<GameObject> wallGOList = new List<GameObject>();
    public Constants.DungeonRoomType type = Constants.DungeonRoomType.NA;
    public List<int> connectedIdList = new List<int>();
}