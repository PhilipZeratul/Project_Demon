using UnityEngine;


public static class MathUtils
{
    public static readonly System.Random rnd = new System.Random(seed); // seed
    public static readonly int seed = System.Environment.TickCount;


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

    public static Vector2 GetRandomPointInEclipse(float radius, float xScale, float yScale)
    {
        Vector2 result = GetRandomPointInCircle(radius);
        result.x = result.x * xScale;
        result.y = result.y * yScale;
        return result;
    }

    public static Vector2 GetRandomPointInRect(float width, float height)
    {
        return new Vector2((float)(rnd.NextDouble() - 0.5) * 2 * width, (float)(rnd.NextDouble() - 0.5) * 2 * height);
    }

    public static float RandomFloatBetween(float minValue, float maxValue)
    {
        var next = rnd.NextDouble();
        return (float)(minValue + (next * (maxValue - minValue)));
    }

    public static float RoundToGrid(float n, float gridSize = Constants.MapInfo.GridSize)
    {
        return Mathf.Floor((n + gridSize - 1) / gridSize) * gridSize;
    }

    public static bool NearlyEqual(float a, float b, float epsilon = 0.001f)
    {
        if (Mathf.Abs(a - b) < epsilon)
            return true;
        else
            return false;
    }
}
