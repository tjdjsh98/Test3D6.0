using UnityEngine;

public class Define
{
    static int _ground_LayerMask = 0;
    static int _character_LayerMask = 0;
    static int _character_Layer = 0;
    static int _interactable_Layer = 0;
    static int _interactable_LayerMask = 0;
    public static int GROUND_LAYERMASK { get { DefineInit(); return _ground_LayerMask; } }
    public static int CHARACTER_LAYERMASK { get { DefineInit(); return _character_LayerMask; } }
    public static int CHARACTER_LAYER { get { DefineInit(); return _character_Layer; } }
    public static int INTERACTABLE_LAYER { get { DefineInit(); return _interactable_Layer; } }
    public static int INTERACTABLE_LAYERMASK { get { DefineInit(); return _interactable_LayerMask; } }

    static bool defineInit = false;
    static void DefineInit() 
    {
        if (defineInit) return;
        defineInit = true;
        _ground_LayerMask = LayerMask.GetMask("Ground");
        _character_LayerMask = LayerMask.GetMask("Character");
        _character_Layer = LayerMask.NameToLayer("Character");
        _interactable_Layer = LayerMask.NameToLayer("Interactable");
        _interactable_LayerMask = LayerMask.GetMask("Interactable");
    }

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
    public IDamageable attacker;
    public IDamageable target;
    public int damage;
    public float knockbackPower;
    public Vector3 knockbackDirection;
}