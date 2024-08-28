using UnityEngine;

public class Define
{
    public static int GROUND_LAYERMASK = LayerMask.GetMask("Ground");
    public static int PLAYER_LAYERMASK = LayerMask.GetMask("Player");
    public static int ENEMY_LAYERMASK = LayerMask.GetMask("Enemy");

}
public enum RangeShape
{
    Ray,
    Box,
    Sphere,
}

[System.Serializable]
public struct Range
{
    public RangeShape shape;
    public Vector3 center;
    public Vector3 size;
    public Vector3 direction;
    public float distance;
    public bool relativeTransform;
}

public struct DamageInfo
{
    public Character attacker;
    public Character target;
    public int damage;
    public float knockbackPower;
    public Vector3 knockbackDirection;
}
