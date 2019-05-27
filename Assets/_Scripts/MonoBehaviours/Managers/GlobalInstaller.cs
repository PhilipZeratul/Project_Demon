using UnityEngine;
using Zenject;


public class GlobalInstaller : MonoInstaller<GlobalInstaller>
{
    public GameManager GameManager;
    public GameObject player;


    public override void InstallBindings()
    {
        //Debug.Log("Global Installer Binding.");

        Container.Bind<GameManager>().FromInstance(Instantiate(GameManager)).AsSingle();
        Container.Bind<GameObject>().WithId(Constants.InjectID.Player).FromInstance(Instantiate(player)).AsSingle();
    }
}