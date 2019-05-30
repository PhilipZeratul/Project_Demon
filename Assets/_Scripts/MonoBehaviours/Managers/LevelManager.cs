using UnityEngine;
using Zenject;


public class LevelManager : MonoBehaviour
{
    [SerializeField] private IntRange numOfMainRooms = new IntRange(4, 10);

    private DungeonGenerator dungeonGenerator;
    private GameObject player;


    [Inject]
    private void Init(DungeonGenerator _dungeonGenerator,
                     [Inject(Id = Constants.InjectID.Player)] GameObject _player)
    {
        dungeonGenerator = _dungeonGenerator;
        player = _player;
    }

    private void Start()
    {
        dungeonGenerator.GenerationFinished += DungeonGenerated;
        StartCoroutine(dungeonGenerator.GenerateDungeon());
    }

    private void DungeonGenerated()
    {
        if (dungeonGenerator.mainRoomList.Count < numOfMainRooms.min ||
            dungeonGenerator.mainRoomList.Count > numOfMainRooms.max)
        {
            Debug.Log("ReGenerate Dungeon");
            StartCoroutine(dungeonGenerator.ReGenerateDungeon());
        }
        else
        {
            Debug.LogFormat("Dungeon Generated, num of MainRoom = {0}", dungeonGenerator.mainRoomList.Count);
            DungeonEnricher dungeonEnricher = new DungeonEnricher(dungeonGenerator);
            dungeonEnricher.EnrichFinished += DungeonEnriched;
            dungeonEnricher.Enrich();
        }
    }

    private void DungeonEnriched()
    {
        player.transform.position = dungeonGenerator.mainRoomList.Find(n => n.type == Constants.DungeonRoomType.Entry).center;
        player.SetActive(true);
    }

    private void OnDestroy()
    {
        dungeonGenerator.GenerationFinished -= DungeonGenerated;
    }
}
