using System;
using System.Collections.Generic;


public class DungeonEnricher
{
    public bool IsEnrichFinished { get; private set; }
    public Action EnrichFinished;

    private List<DungeonRoom> allRoomList;
    private List<DungeonRoom> mainRoomList;
    private List<DungeonRoom> supportRoomList;
    private List<DungeonRoom> corridorRoomList;

    public DungeonEnricher(DungeonGenerator dungeonGenerator)
    {
        IsEnrichFinished = false;
        allRoomList = dungeonGenerator.allRoomList;
        mainRoomList = dungeonGenerator.mainRoomList;
        supportRoomList = dungeonGenerator.supportRoomList;
        corridorRoomList = dungeonGenerator.corridorRoomList;
    }

    public void Enrich()
    {
        IsEnrichFinished = false;
        SetMainRoomFunc();
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
            int index = MathUtils.rnd.Next(0, mainRoomList.Count);
            if (mainRoomList[index].type == Constants.DungeonRoomType.NA &&
                mainRoomList[index].connectedIdList.Count >= 2)
            {
                mainRoomList[index].type = Constants.DungeonRoomType.Entry;
                isDone = true;
            }
        }
    }

    private void PlaceRoom(Constants.DungeonRoomType type)
    {
        bool isDone = false;
        while (!isDone)
        {
            int index = MathUtils.rnd.Next(0, mainRoomList.Count);
            if (mainRoomList[index].type == Constants.DungeonRoomType.NA)
            {
                mainRoomList[index].type = type;
                isDone = true;
            }
        }
    }
}
