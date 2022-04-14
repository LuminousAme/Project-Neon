using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUlits
{
    public static float InverseLerp(float a, float b, float value)
    {
        return (value - a) / (b - a);
    }

    public static float Lerp(float a, float b, float t)
    {
        return (1.0f - t) * a + b * t;
    }

    public static Vector3 LerpClamped(Vector3 a, Vector3 b, float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        return Vector3.Lerp(a, b, t);
    }

    public static float ReMap(float oldMin, float oldMax, float newMin, float newMax, float value)
    {
        float t = InverseLerp(oldMin, oldMax, value);
        return Lerp(newMin, newMax, t);
    }

    public static float ReMapClamped(float oldMin, float oldMax, float newMin, float newMax, float value)
    {
        return Mathf.Clamp(ReMap(oldMin, oldMax, newMin, newMax, value), newMin, newMax);
    }

    public static float ReMapClamped(float oldMin, float oldMax, float newMin, float newMax, float value, float totalMin, float totalMax)
    {
        return Mathf.Clamp(ReMap(oldMin, oldMax, newMin, newMax, value), totalMin, totalMax);
    }

    public static float SmoothStepFromValue(float min, float max, float value)
    {
        float t = InverseLerp(min, max, value);
        return Mathf.SmoothStep(min, max, t);
    }

    public static float ReMapTwoRanges(float oldMinlower, float oldMaxLower, float newMinLower, float newMaxLower, 
        float oldMinUpper, float oldMaxUpper, float newMinUpper, float newMaxUpper, float value)
    {
        if(value <= oldMaxLower)
        {
            return ReMap(oldMinlower, oldMaxLower, newMinLower, newMaxLower, value);
        }
        else if (value >= oldMinUpper)
        {
            return ReMap(oldMinUpper, oldMaxUpper, newMinUpper, newMaxUpper, value);
        }

        return value;
    }
}
