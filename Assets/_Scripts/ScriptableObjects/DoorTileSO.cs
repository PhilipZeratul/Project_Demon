using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "DoorTileSO", menuName = "Dungeon/DoorTileSO", order = 1)]
public class DoorTileSO : ScriptableObject
{
    public List<GameObject> tilePrefabList;
    public List<Sprite> tileSpriteList;
}