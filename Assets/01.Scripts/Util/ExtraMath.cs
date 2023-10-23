
using UnityEngine;

public static class ExtraMath
{
    public static Vector2 GetBezierPoint(Vector2[] points, float t)
    {
        t = Mathf.Clamp01(t);
        if (float.IsNaN(t)) t = 0;
        if (points.Length == 2) return Vector2.Lerp(points[0], points[1], t);
        Vector2[] nextPoints = new Vector2[points.Length - 1];
        for (int i = 0; i < points.Length - 1; i++)
        {
            nextPoints[i] = Vector2.Lerp(points[i], points[i + 1], t);
        }
        return GetBezierPoint(nextPoints, t);
    }

    public static float DirectionToAngle(Vector3 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    public static Vector2 AngleToDirection(float angle)
    {
        return new(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    public static float GetNormalAngle(float angle)
    {
        return (angle % 360f + 360f) % 360f;
    }

    public static float GetAngleDistance(float from, float to)
    {
        var range = GetAngleRange(from, to);
        from = range.From;
        to = range.To;
        if (from <= to) return to - from;
        else return 360 - (from - to);
    }

    public static bool IsAngleBetween(float target, float from, float to)
    {
        target = GetNormalAngle(target);
        var range = GetAngleRange(from, to);
        from = range.From;
        to = range.To;
        if (from <= to) return from <= target && target <= to;
        return from <= target || target <= to;
    }

    public static AngleRange GetAngleRange(float from, float to)
    {
        from = GetNormalAngle(from);
        to = GetNormalAngle(to);
        var rAngle = GetNormalAngle(to - from);
        if (rAngle > 180)
        {
            (from, to) = (to, from);
        }
        return new(from, to);
    }
}

public struct AngleRange
{
    public float From, To;

    public AngleRange(float from, float to)
    {
        From = from;
        To = to;
    }
}