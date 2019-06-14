using UnityEngine;
using System.Collections.Generic;


public class DungeonRoomData
{
    public int id;
    public Vector2 center;
    public int width;
    public int height;
    public GameObject root;
    public List<Tile> floorTileList = new List<Tile>();
    public List<Tile> wallTileList = new List<Tile>();
    public List<Tile> doorTileList = new List<Tile>();
    public Constants.DungeonRoomType type = Constants.DungeonRoomType.NA;
    public List<int> connectedIdList = new List<int>();
    public bool isClose = true;


    public class Tile
    {
        public GameObject go;
        public SpriteRenderer spriteRenderer;
        public Collider2D collider2d;

        public Tile(GameObject go)
        {
            this.go = go;
            spriteRenderer = go.GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
                Debug.LogFormat("Tile {0} does not have a sprite renderer!", go.name);
        }
    }
}