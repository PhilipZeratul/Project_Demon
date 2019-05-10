using UnityEngine;


public static class MathUtils
{
    public static readonly System.Random rnd = new System.Random();


    public static Vector2 GetRandomPointInCircle(float radius)
    {
        float theta = 2 * Mathf.PI * (float)rnd.NextDouble();
        float u = (float)rnd.NextDouble() + (float)rnd.NextDouble();
        float r;
        if (u > 1) r = 2 - u;
        else r = u;
        return new Vector2(RoundToGrid(radius * r * Mathf.Cos(theta), Constants.MapInfo.GridSize),
                          RoundToGrid(radius * r * Mathf.Sin(theta), Constants.MapInfo.GridSize));
    }

    private static float RandomFloatBetween(float minValue, float maxValue)
    {
        var next = rnd.NextDouble();
        return (float)(minValue + (next * (maxValue - minValue)));
    }

    public static float RoundToGrid(float n, float gridSize)
    {
        return Mathf.Floor((n + gridSize - 1) / gridSize) * gridSize;
    }
}
