using Unity.Entities;
using Unity.Mathematics;


public struct DungeonRoom : IComponentData
{
    public float2 center;
    public float width;
    public float height;
}
