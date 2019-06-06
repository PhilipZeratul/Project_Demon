using UnityEngine;
using Zenject;


public class LevelManager : MonoBehaviour
{
    [SerializeField] private IntRange numOfMainRooms = new IntRange(4, 10);

    private DungeonGenerator dungeonGenerator;
    private DungeonEnricher dungeonEnricher;
    private GameObject player;


    [Inject]
    private void Init(DungeonGenerator _dungeonGenerator,
                      DungeonEnricher _dungeonEnricher,
                     [Inject(Id = Constants.InjectID.Player)] GameObject _player)
    {
        dungeonGenerator = _dungeonGenerator;
        dungeonEnricher = _dungeonEnricher;
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
            dungeonEnricher.EnrichFinished += DungeonEnriched;
            StartCoroutine(dungeonEnricher.Enrich());
        }
    }

    private void DungeonEnriched()
    {
        // Place the player at center of level entry.
        player.transform.position = dungeonGenerator.mainRoomList.Find(n => n.type == Constants.DungeonRoomType.Entry).center;
        player.SetActive(true);
    }

    private void OnDestroy()
    {
        dungeonGenerator.GenerationFinished -= DungeonGenerated;
    }
}
