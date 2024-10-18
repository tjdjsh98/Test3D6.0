using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/EquipmentData")]
public class EquipmentData : ItemData
{
    [Header("Equipment Data")]
    [field:SerializeField]public EquipmentType EquipmentType{ get; set; }
    [field:SerializeField] public GameObject EquipmentPrefab { get; set; }  
}
