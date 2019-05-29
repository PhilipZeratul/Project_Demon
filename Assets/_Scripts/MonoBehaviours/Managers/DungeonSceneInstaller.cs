using Zenject;


public class DungeonSceneInstaller : MonoInstaller<DungeonSceneInstaller>
{
    public DungeonGenerator dungeonGenerator;


    public override void InstallBindings()
    {
        //Debug.Log("Dungeon Scene Installer Binding.");

        Container.Bind<DungeonGenerator>().FromInstance(dungeonGenerator).AsSingle();
    }
}