using UnityEngine;
using Zenject;


public class LevelManager : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;


    [Inject]
    private void Init(DungeonGenerator _dungeonGenerator)
    {
        dungeonGenerator = _dungeonGenerator;
    }

    private void Start()
    {
        dungeonGenerator.GenerationFinished += DungeonGenerated;
        StartCoroutine(dungeonGenerator.GenerateDungeon());
    }

    private void DungeonGenerated()
    {
        Debug.Log("DungeonGenerated");
    }

    private void OnDestroy()
    {
        dungeonGenerator.GenerationFinished -= DungeonGenerated;
    }
}
