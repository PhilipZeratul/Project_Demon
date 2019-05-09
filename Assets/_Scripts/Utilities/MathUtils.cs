using UnityEngine;
using Unity.Mathematics;


public static class MathUtils
{
    public static readonly System.Random rnd = new System.Random();


    public static Vector2 GetRandomPointInCircle(float radius)
    {
        float theta = 2 * math.PI * (float)rnd.NextDouble();
        float u = (float)rnd.NextDouble() + (float)rnd.NextDouble();
        float r;
        if (u > 1) r = 2 - u;
        else r = u;
        return new Vector2(RoundToGrid(radius * r * math.cos(theta), Constants.MapInfo.GridSize),
                           RoundToGrid(radius * r * math.sin(theta), Constants.MapInfo.GridSize));
    }

    private static float RandomFloatBetween(float minValue, float maxValue)
    {
        var next = rnd.NextDouble();
        return (float)(minValue + (next * (maxValue - minValue)));
    }

    public static float RoundToGrid(float n, float gridSize)
    {
        return math.floor((n + gridSize - 1) / gridSize) * gridSize;
    }
}
