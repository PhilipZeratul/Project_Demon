using UnityEngine;
using System.Collections;


public class LevelManager : MonoBehaviour
{
    public DungeonGenerator dungeonGenerator;


    private void Start()
    {
        StartCoroutine(dungeonGenerator.GenerateDungeon());
    }
}
