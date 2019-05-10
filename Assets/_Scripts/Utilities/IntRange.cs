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

    public int Next()
    {
        Current = MathUtils.rnd.Next(min, max + 1);
        return Current;
    }
}
