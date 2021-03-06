﻿using UnityEngine;
using Zenject;


public class GlobalInstaller : MonoInstaller<GlobalInstaller>
{
    public GameManager GameManager;
    public GameObject player;


    public override void InstallBindings()
    {
        //Debug.Log("Global Installer Binding.");

        Container.Bind<GameManager>().FromInstance(Instantiate(GameManager)).AsSingle();

        GameObject playerGO = Instantiate(player);
        playerGO.SetActive(false);
        Container.Bind<GameObject>().WithId(Constants.InjectID.Player).FromInstance(playerGO).AsSingle();
    }
}