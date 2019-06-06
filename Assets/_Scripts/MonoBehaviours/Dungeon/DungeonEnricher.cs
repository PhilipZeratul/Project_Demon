using UnityEngine;
using System;
using System.Collections;


public class DungeonEnricher : MonoBehaviour
{
    public bool IsEnrichFinished { get; private set; }
    public Action EnrichFinished;

    private DungeonGenerator dungeonGenerator;
    private readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();


    public DungeonEnricher(DungeonGenerator dungeonGenerator)
    {
        IsEnrichFinished = false;
        this.dungeonGenerator = dungeonGenerator;
    }

    public IEnumerator Enrich()
    {
        IsEnrichFinished = false;

        SetMainRoomFunc();
        RemoveFloorCollider();
        yield return waitForFixedUpdate;
        GenerateCompositeCollider();

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
            if (dungeonGenerator.mainRoomList[index].type == Constants.DungeonRoomType.NA)
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

    private void GenerateCompositeCollider()
    {
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
