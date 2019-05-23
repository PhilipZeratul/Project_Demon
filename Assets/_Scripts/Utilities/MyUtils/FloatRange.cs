using System;


[Serializable]
public class FloatRange
{
    public float min;
    public float max;

    public FloatRange(float minValue, float maxValue)
    {
        if (minValue > maxValue)
        {
            float temp = maxValue;
            maxValue = minValue;
            minValue = temp;
        }

        min = minValue;
        max = maxValue;
    }
}
