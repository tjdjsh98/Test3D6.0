using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Common Data")]
    [field:SerializeField]public ItemType ItemType { get; set; }
}
