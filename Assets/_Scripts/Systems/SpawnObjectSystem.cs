﻿using Unity.Entities;
using Unity.Collections;
using System;


public class SpawnObjectSystem : ComponentSystem
{
    private EntityQuery spawnerGroup;
    private EntityQuery mapGroup;
    private Unity.Mathematics.Random rnd;


    protected override void OnCreateManager()
    {
        spawnerGroup = GetEntityQuery(typeof(Spawner), typeof(Initializer));
        mapGroup = GetEntityQuery(typeof(Dungeon));
        rnd = new Unity.Mathematics.Random((uint)Guid.NewGuid().GetHashCode());
    }

    protected override void OnUpdate()
    {
        int spawnerLength = spawnerGroup.CalculateLength();
        int mapLength = mapGroup.CalculateLength();

        if (spawnerLength <= 0 || mapLength <= 0 || spawnerLength != mapLength)
            return;

        var spawnerEntities = spawnerGroup.ToEntityArray(Allocator.TempJob);
        var mapEntities = mapGroup.ToEntityArray(Allocator.TempJob);

        PhysicsObject physicsObject;

        for (int k = 0; k < spawnerEntities.Length; k++)
        {
            Dungeon map = EntityManager.GetSharedComponentData<Dungeon>(mapEntities[k]);
            Spawner spawner = EntityManager.GetSharedComponentData<Spawner>(spawnerEntities[k]);

            for (int i = 1; i < map.rows - 1; i++)
                for (int j = 1; j < map.cols - 1; j++)
                {
                    if ((i == map.rows / 2) && (j == map.cols / 2))
                    {
                        var entity = EntityManager.Instantiate(spawner.playerPrefab);
                        physicsObject = spawner.playerPrefab.GetComponent<PhysicsObjectProxy>().Value;
                        physicsObject.cx = j;
                        physicsObject.cy = i;
                        EntityManager.SetComponentData(entity, physicsObject);
                    }
                    else if (map.mapArray[i * map.cols + j] == 0)
                    {
                        if (rnd.NextFloat(0f, 1f) > 0.3f)
                        {
                            var entity = EntityManager.Instantiate(spawner.obstaclePrefab);
                            physicsObject = spawner.playerPrefab.GetComponent<PhysicsObjectProxy>().Value;
                            physicsObject.cx = j;
                            physicsObject.cy = i;
                            EntityManager.SetComponentData(entity, physicsObject);
                        }
                    }
                }

            EntityManager.RemoveComponent<Initializer>(spawnerEntities[k]);
        }

        spawnerEntities.Dispose();
        mapEntities.Dispose();
    }
}
