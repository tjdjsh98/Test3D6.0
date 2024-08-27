using UnityEngine;

public enum RangeShape
{
    Ray,
    Box,
    Sphere,
}

public struct Range
{
    public RangeShape shape;
    public Vector3 center;
    public Vector3 size;
    public Vector3 direction;
    public float distance;
}
