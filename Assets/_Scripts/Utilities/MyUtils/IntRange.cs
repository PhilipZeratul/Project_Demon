using System;


[Serializable]
public class IntRange
{
    public int min;
    public int max;

    public int Current { get; private set; }


    public IntRange(int minValue, int maxValue)
    {
        if (minValue > maxValue)
        {
            int temp = minValue;
            minValue = maxValue;
            maxValue = temp;
        }

        min = minValue;
        max = maxValue;
        Current = max;
    }

    // Get a random odd number. For simplify drawing map.
    public int NextOdd()
    {
        Current = MathUtils.rnd.Next(min, max + 1);
        if (Current % 2 == 0)
            Current++;
        return Current;
    }

    public int Next()
    {
        Current = MathUtils.rnd.Next(min, max + 1);
        return Current;
    }
}
