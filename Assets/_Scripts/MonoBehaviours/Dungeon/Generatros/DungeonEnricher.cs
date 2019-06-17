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
        SetWallTileSprite();

        yield return null;
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

    // Deprecated.
    private void DisableWallColliders()
    {
        foreach (DungeonRoomData room in dungeonGenerator.allRoomList)
        {
            foreach (DungeonRoomData.Tile wallTile in room.wallTileList)
            {
                if (wallTile.go != null)
                    wallTile.collider2d.isTrigger = true;
            }
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

    private void SetWallTileSprite()
    {
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int index = 0;
        int hitNum = 0;
        //Physics2D.queriesHitTriggers = false;

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
                    //case 43:
                    //    checkPosition.x = wallTile.go.transform.position.x - Constants.MapInfo.GridSize; 
                    //    checkPosition.y = wallTile.go.transform.position.y - Constants.MapInfo.GridSize;
                    //    hitNum = Physics2D.RaycastNonAlloc(checkPosition, Vector2.zero, hits);
                    //    if (hitNum == 0)
                    //        index = 27;
                    //    break;
                    //case 49:
                        //checkPosition.x = wallTile.go.transform.position.x + Constants.MapInfo.GridSize;
                        //checkPosition.y = wallTile.go.transform.position.y - Constants.MapInfo.GridSize;

                        //hitNum = Physics2D.RaycastNonAlloc(checkPosition, Vector2.zero, hits);
                        //if (hitNum == 0)
                        //    index = 27;
                        //break;
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

                }


                wallTile.spriteRenderer.sprite = wallTilemapSO.sprites[index];
                if (wallTilemapSO.sprites[index] == null)
                {
                    Debug.LogFormat("north {0}, west {1}, east {2}, south {3}, index {4}, position {5}", north, west, east, south, index, wallTile.go.transform.position);
                }
            }
        }
        //Physics2D.queriesHitTriggers = true;
        //Physics2D.queriesStartInColliders = true;
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
