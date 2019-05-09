using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;


public class CameraMovementSystem : ComponentSystem
{
    EntityQuery cameraGroup;
    EntityQuery mapGroup;


    protected override void OnCreateManager()
    {
        cameraGroup = GetEntityQuery(typeof(Camera), typeof(Initializer));
        mapGroup = GetEntityQuery(typeof(Dungeon));
    }

    protected override void OnUpdate()
    {
        int cameraGroupLength = cameraGroup.CalculateLength();
        int mapGroupLength = mapGroup.CalculateLength();
        if ((cameraGroupLength == 0) ||
            (cameraGroupLength != mapGroupLength))
        {
            return;
        }

        var cameraEntities = cameraGroup.ToEntityArray(Allocator.TempJob);
        var mapEntities = mapGroup.ToEntityArray(Allocator.TempJob);

        Translation position;
        for (int i = 0; i < cameraEntities.Length; i++)
        {
            Dungeon map = EntityManager.GetSharedComponentData<Dungeon>(mapEntities[i]);
            position.Value = new float3(map.cols / 2, map.rows / 2, -10f);
            EntityManager.SetComponentData<Translation>(cameraEntities[i], position);
            EntityManager.RemoveComponent<Initializer>(cameraEntities[i]);
        }

        cameraEntities.Dispose();
        mapEntities.Dispose();
    }
}
