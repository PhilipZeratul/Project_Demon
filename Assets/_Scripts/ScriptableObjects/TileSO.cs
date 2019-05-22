using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "TileSO", menuName = "Dungeon/TileSO", order = 1)]
public class TileSO : ScriptableObject
{
    public List<GameObject> tilePrefabList;
}