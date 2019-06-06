using Zenject;


public class DungeonSceneInstaller : MonoInstaller<DungeonSceneInstaller>
{
    public DungeonGenerator dungeonGenerator;
    public DungeonEnricher dungeonEnricher;


    public override void InstallBindings()
    {
        //Debug.Log("Dungeon Scene Installer Binding.");

        Container.Bind<DungeonGenerator>().FromInstance(dungeonGenerator).AsSingle();
        Container.Bind<DungeonEnricher>().FromInstance(dungeonEnricher).AsSingle();
    }
}