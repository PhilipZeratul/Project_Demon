using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    public List<SceneReference> SceneList = new List<SceneReference>();


    private void Start()
    {
        SceneManager.LoadSceneAsync(SceneList[0].ScenePath, LoadSceneMode.Additive);
    }
}
