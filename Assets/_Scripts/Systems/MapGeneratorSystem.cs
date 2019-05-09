using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using System;


public class MapGeneratorSystem : ComponentSystem
{
    EntityQuery mapGeneratorGroup;


    protected override void OnCreateManager()
    {
        mapGeneratorGroup = GetEntityQuery(typeof(Dungeon), typeof(Initializer));
    }

    protected override void OnUpdate()
    {
        using (var mapGeneratorEntities = mapGeneratorGroup.ToEntityArray(Allocator.TempJob))
        {
            foreach (var generator in mapGeneratorEntities)
            {

            }
        }
    }

}
