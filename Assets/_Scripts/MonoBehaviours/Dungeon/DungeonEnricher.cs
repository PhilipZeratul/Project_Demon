using UnityEngine;
using System;


public class DungeonEnricher
{
    public bool IsEnrichFinished { get; private set; }
    public Action EnrichFinished;

    private DungeonGenerator dungeonGenerator;


    public DungeonEnricher(DungeonGenerator dungeonGenerator)
    {
        IsEnrichFinished = false;
        this.dungeonGenerator = dungeonGenerator;
    }

    public void Enrich()
    {
        IsEnrichFinished = false;
        SetMainRoomFunc();
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
                dungeonGenerator.mainRoomList[index].type = type;
                isDone = true;
            }
        }
    }

    private void GenerateCompositeCollider()
    {
        Rigidbody2D rigidbody2d = dungeonGenerator.gameObject.AddComponent<Rigidbody2D>();
        rigidbody2d.bodyType = RigidbodyType2D.Kinematic;
        dungeonGenerator.gameObject.AddComponent<CompositeCollider2D>();
    }
}
