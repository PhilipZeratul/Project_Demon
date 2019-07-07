using UnityEngine;
using System;
using System.Collections;
using Zenject;


[RequireComponent(typeof(DungeonGenerator))]
public class DungeonEnricher : MonoBehaviour
{
    public bool IsEnrichFinished { get; private set; }
    public Action EnrichFinished;

    [SerializeField]
    private TilemapSO floorTilemapSO;
    [SerializeField]
    private TilemapSO wallTilemapSO;
    [SerializeField]
    private DoorTileSO doorTileSO;

    private DungeonGenerator dungeonGenerator;
    private readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();


    [Inject]
    private void Init(DungeonGenerator _dungeonGenerator)
    {
        dungeonGenerator = _dungeonGenerator;
        IsEnrichFinished = false;
    }

    public IEnumerator Enrich()
    {
        IsEnrichFinished = false;

        SetMainRoomFunc();

        SetFloorTileSprite();
        DisableDoorCollider();
        yield return waitForFixedUpdate;
        SetWallTileSprite();
        SetDoorTile();

        RemoveFloorCollider();
        yield return waitForFixedUpdate;
        GenerateCompositeCollider();
        yield return waitForFixedUpdate;

        IsEnrichFinished = true;
        EnrichFinished?.Invoke();
    }

    private void SetMainRoomFunc()
    {
        PlaceEntry();
        PlaceRoom(Constants.DungeonRoomType.Boss);
        PlaceRoom(Constants.DungeonRoomType.Shop);
        PlaceRoom(Constants.DungeonRoomType.MiniBoss);
    }

    private void PlaceEntry()
    {
        bool isDone = false;
        while (!isDone)
        {
            int index = MathUtils.rnd.Next(0, dungeonGenerator.mainRoomList.Count);
            if (dungeonGenerator.mainRoomList[index].type == Constants.DungeonRoomType.NA &&
                dungeonGenerator.mainRoomList[index].connectedIdList.Count >= 2)
            {
                dungeonGenerator.mainRoomList[index].type = Constants.DungeonRoomType.Entry;
                isDone = true;
            }
        }
    }

    private void PlaceRoom(Constants.DungeonRoomType type)
    {
        bool isDone = false;
        while (!isDone)
        {
            int index = MathUtils.rnd.Next(0, dungeonGenerator.mainRoomList.Count);
            if (dungeonGenerator.mainRoomList[index].type == Constants.DungeonRoomType.NA && dungeonGenerator.mainRoomList[index].isClose)
            {
                switch (type)
                {
                    case Constants.DungeonRoomType.Shop:
                        PlaceShop(index);
                        break;
                    case Constants.DungeonRoomType.Boss:
                        PlaceBoss(index);
                        break;
                    case Constants.DungeonRoomType.MiniBoss:
                        PlaceMiniboss(index);
                        break;
                }
                isDone = true;
            }
        }
    }

    private void PlaceShop(int index)
    {
        dungeonGenerator.mainRoomList[index].type = Constants.DungeonRoomType.Shop;
        foreach (var floorTile in dungeonGenerator.mainRoomList[index].floorTileList)
        {
            floorTile.spriteRenderer.color = Color.yellow;
        }
    }

    private void PlaceBoss(int index)
    {
        dungeonGenerator.mainRoomList[index].type = Constants.DungeonRoomType.Boss;
        foreach (var floorTile in dungeonGenerator.mainRoomList[index].floorTileList)
        {
            floorTile.spriteRenderer.color = Color.red;
        }
    }

    private void PlaceMiniboss(int index)
    {
        dungeonGenerator.mainRoomList[index].type = Constants.DungeonRoomType.MiniBoss;
        foreach (var floorTile in dungeonGenerator.mainRoomList[index].floorTileList)
        {
            floorTile.spriteRenderer.color = Color.green;
        }
    }

    private void SetFloorTileSprite()
    {
        foreach (var room in dungeonGenerator.allRoomList)
        {
            foreach (var floorTile in room.floorTileList)
            {
                floorTile.spriteRenderer.sprite = floorTilemapSO.sprites[MathUtils.rnd.Next(0, floorTilemapSO.sprites.Length)];
            }
        }
    }

    private void DisableDoorCollider()
    {
        foreach (var room in dungeonGenerator.mainRoomList)
        {
            foreach (var doorTile in room.doorTileList)
            {
                doorTile.collider2d.isTrigger = true;
            }
        }
    }

    private void SetWallTileSprite()
    {
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int index = 0;
        int hitNum = 0;

        foreach (var room in dungeonGenerator.allRoomList)
        {
            foreach (var wallTile in room.wallTileList)
            {
                Physics2D.queriesStartInColliders = false;
                int north = 0, south = 0, east = 0, west = 0;

                index = 0;
                hitNum = Physics2D.RaycastNonAlloc(wallTile.go.transform.position, Vector2.up, hits, Constants.MapInfo.GridSize);
                if (hitNum > 0)
                {
                    if (hits[0].collider.GetComponent<DungeonFloor>())
                    {
                        north = 1;
                        index += 1;
                    }
                    else
                    {
                        north = 2;
                        index += 2;
                    }
                }
                hitNum = Physics2D.RaycastNonAlloc(wallTile.go.transform.position, Vector2.left, hits, Constants.MapInfo.GridSize);
                if (hitNum > 0)
                {
                    if (hits[0].collider.GetComponent<DungeonFloor>())
                    {
                        west = 1;
                        index += 3;
                    }
                    else
                    {
                        west = 2;
                        index += 6;
                    }
                }
                hitNum = Physics2D.RaycastNonAlloc(wallTile.go.transform.position, Vector2.right, hits, Constants.MapInfo.GridSize);
                if (hitNum > 0)
                {
                    if (hits[0].collider.GetComponent<DungeonFloor>())
                    {
                        east = 1;
                        index += 9;
                    }
                    else
                    {
                        east = 2;
                        index += 18;
                    }
                }
                hitNum = Physics2D.RaycastNonAlloc(wallTile.go.transform.position, Vector2.down, hits, Constants.MapInfo.GridSize);
                if (hitNum > 0)
                {
                    if (hits[0].collider.GetComponent<DungeonFloor>())
                    {
                        south = 1;
                        index += 27;
                        // Add Isometric sorting order to tall walls
                        IsometricObjectStatic isometric = wallTile.go.AddComponent<IsometricObjectStatic>();
                        isometric.targetOffset = Constants.MapInfo.WallIsoOffset;
                        wallTile.go.GetComponent<SpriteRenderer>().sortingLayerName = Constants.SortingLayer.Player;
                    }
                    else
                    {
                        south = 2;
                        index += 54;
                    }
                }
                // Exceptions
                Physics2D.queriesStartInColliders = true;
                Vector2 checkPosition = new Vector2();
                switch (index)
                {
                    case 25:
                        checkPosition.x = wallTile.go.transform.position.x + Constants.MapInfo.GridSize;
                        checkPosition.y = wallTile.go.transform.position.y + Constants.MapInfo.GridSize;

                        hitNum = Physics2D.RaycastNonAlloc(checkPosition, Vector2.zero, hits);
                        if (hitNum > 0 && hits[0].collider.GetComponent<DungeonDoor>())
                            index = 7;
                        else
                        {
                            checkPosition.x = wallTile.go.transform.position.x - Constants.MapInfo.GridSize;
                            checkPosition.y = wallTile.go.transform.position.y + Constants.MapInfo.GridSize;

                            hitNum = Physics2D.RaycastNonAlloc(checkPosition, Vector2.zero, hits);
                            if (hitNum > 0 && hits[0].collider.GetComponent<DungeonDoor>())
                                index = 19;
                        }
                        break;
                    case 51:
                        checkPosition.x = wallTile.go.transform.position.x - Constants.MapInfo.GridSize;
                        checkPosition.y = wallTile.go.transform.position.y - Constants.MapInfo.GridSize;

                        hitNum = Physics2D.RaycastNonAlloc(checkPosition, Vector2.zero, hits);
                        if (hitNum > 0 && hits[0].collider.GetComponent<DungeonDoor>())
                            index = 45;
                        else
                        {
                            checkPosition.x = wallTile.go.transform.position.x + Constants.MapInfo.GridSize;
                            checkPosition.y = wallTile.go.transform.position.y - Constants.MapInfo.GridSize;

                            hitNum = Physics2D.RaycastNonAlloc(checkPosition, Vector2.zero, hits);
                            if (hitNum > 0 && hits[0].collider.GetComponent<DungeonDoor>())
                                index = 33;
                        }
                        break;
                    case 52:
                        checkPosition.x = wallTile.go.transform.position.x - Constants.MapInfo.GridSize;
                        checkPosition.y = wallTile.go.transform.position.y - Constants.MapInfo.GridSize;

                        hitNum = Physics2D.RaycastNonAlloc(checkPosition, Vector2.zero, hits);
                        if (hitNum > 0 && !hits[0].collider.GetComponent<DungeonFloor>())
                            index = 45;
                        else
                        {
                            checkPosition.x = wallTile.go.transform.position.x + Constants.MapInfo.GridSize;
                            checkPosition.y = wallTile.go.transform.position.y - Constants.MapInfo.GridSize;

                            hitNum = Physics2D.RaycastNonAlloc(checkPosition, Vector2.zero, hits);
                            if (hitNum > 0 && !hits[0].collider.GetComponent<DungeonFloor>())
                                index = 33;
                        }
                        break;
                    case 65:
                        checkPosition.x = wallTile.go.transform.position.x + Constants.MapInfo.GridSize;
                        checkPosition.y = wallTile.go.transform.position.y + Constants.MapInfo.GridSize;

                        hitNum = Physics2D.RaycastNonAlloc(checkPosition, Vector2.zero, hits);
                        if (hitNum > 0 && hits[0].collider.GetComponent<DungeonDoor>())
                            index = 63;
                        break;
                }

                wallTile.spriteRenderer.sprite = wallTilemapSO.sprites[index];

                if (wallTilemapSO.sprites[index] == null)
                {
                    Debug.LogFormat("No wall sprites! north {0}, west {1}, east {2}, south {3}, index {4}, position {5}",
                                    north, west, east, south, index, wallTile.go.transform.position);
                }
            }
        }
    }

    private void SetDoorTile()
    {
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int index = 0;
        int hitNum = 0;
        Physics2D.queriesStartInColliders = false;

        foreach (var room in dungeonGenerator.mainRoomList)
        {
            for (int i = 0; i < room.doorTileList.Count; i++)
            {
                DungeonRoomData.Tile doorTile = room.doorTileList[i];
                int north = 0, south = 0, east = 0, west = 0;

                index = 0;
                hitNum = Physics2D.RaycastNonAlloc(doorTile.go.transform.position, Vector2.up, hits, Constants.MapInfo.GridSize);
                if (hitNum > 0)
                {
                    if (hits[0].collider.GetComponent<DungeonWall>())
                    {
                        north = 1;
                        index += 1;
                    }
                    else if (hits[0].collider.GetComponent<DungeonDoor>())
                    {
                        north = 2;
                        index += 2;
                    }
                }
                hitNum = Physics2D.RaycastNonAlloc(doorTile.go.transform.position, Vector2.left, hits, Constants.MapInfo.GridSize);
                if (hitNum > 0)
                {
                    if (hits[0].collider.GetComponent<DungeonWall>())
                    {
                        west = 1;
                        index += 3;
                    }
                    else if (hits[0].collider.GetComponent<DungeonDoor>())
                    {
                        west = 2;
                        index += 6;
                    }
                }
                hitNum = Physics2D.RaycastNonAlloc(doorTile.go.transform.position, Vector2.right, hits, Constants.MapInfo.GridSize);
                if (hitNum > 0)
                {
                    if (hits[0].collider.GetComponent<DungeonWall>())
                    {
                        east = 1;
                        index += 9;
                    }
                    else if (hits[0].collider.GetComponent<DungeonDoor>())
                    {
                        east = 2;
                        index += 18;
                    }
                }
                hitNum = Physics2D.RaycastNonAlloc(doorTile.go.transform.position, Vector2.down, hits, Constants.MapInfo.GridSize);
                if (hitNum > 0)
                {
                    if (hits[0].collider.GetComponent<DungeonWall>())
                    {
                        south = 1;
                        index += 27;
                    }
                    else if (hits[0].collider.GetComponent<DungeonDoor>())
                    {
                        south = 2;
                        index += 54;
                    }
                }

                // Destroy the floor tile underneath this door tile.
                Physics2D.queriesHitTriggers = false;
                Physics2D.queriesStartInColliders = true;
                hitNum = Physics2D.RaycastNonAlloc(doorTile.go.transform.position, Vector2.zero, hits);
                if (hitNum > 0 && hits[0].collider.GetComponent<DungeonFloor>())
                {
                    room.floorTileList.Remove(room.floorTileList.Find((obj) => obj.go == hits[0].collider.gameObject));
                    Destroy(hits[0].collider.gameObject);
                }
                Physics2D.queriesHitTriggers = true;
                Physics2D.queriesStartInColliders = false;

                // Exceptions
                switch(index)
                {
                    case 2:
                        hitNum = Physics2D.RaycastNonAlloc(doorTile.go.transform.position, Vector2.down, hits, Constants.MapInfo.GridSize);
                        if (hitNum > 0)
                            index = 4;
                        break;
                    case 12:
                    case 56:
                        ChangeDoorTileGO(index, doorTile, room, i);
                        continue;
                }

                if (doorTileSO.tileSpriteList[index] == null)
                {
                    Debug.LogFormat("No door sprites! north {0}, west {1}, east {2}, south {3}, index {4}, position {5}",
                                    north, west, east, south, index, doorTile.go.transform.position);
                    continue;
                }
                doorTile.spriteRenderer.sprite = doorTileSO.tileSpriteList[index];
                doorTile.collider2d.isTrigger = false;
            }
        }
        Physics2D.queriesStartInColliders = true;
    }

    private void ChangeDoorTileGO(int index, DungeonRoomData.Tile doorTile, DungeonRoomData room, int id)
    {
        if (index == 12) index = 0;
        else             index = 1;

        GameObject doorTileGO = Instantiate(doorTileSO.tilePrefabList[index], doorTile.go.transform.position, Quaternion.identity, doorTile.go.transform.parent);
        doorTileGO.GetComponent<DungeonDoor>().roomId = room.doorTileList[id].go.GetComponent<DungeonDoor>().roomId;
        Destroy(room.doorTileList[id].go);
        room.doorTileList[id].go = doorTileGO;
        room.doorTileList[id].spriteRenderer = null;
        room.doorTileList[id].collider2d = doorTileGO.GetComponent<Collider2D>();
    }

    private void GenerateCompositeCollider()
    {
        foreach (var room in dungeonGenerator.allRoomList)
        {
            foreach (var wallTile in room.wallTileList)
            {
                if (wallTile.go != null)
                {
                    wallTile.collider2d.isTrigger = false;
                    wallTile.collider2d.usedByComposite = true;
                }
            }
        }

        Rigidbody2D rigidbody2d = dungeonGenerator.gameObject.AddComponent<Rigidbody2D>();
        rigidbody2d.bodyType = RigidbodyType2D.Kinematic;
        dungeonGenerator.gameObject.AddComponent<CompositeCollider2D>();
    }

    private void RemoveFloorCollider()
    {
        foreach (var room in dungeonGenerator.allRoomList)
        {
            foreach (var floorTile in room.floorTileList)
            {
                Destroy(floorTile.collider2d);
            }
        }
    }
}
