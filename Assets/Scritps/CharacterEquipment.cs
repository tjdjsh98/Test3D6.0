using System.Collections.Generic;
using UnityEngine;

public class CharacterEquipment : MonoBehaviour
{
    [SerializeField] GameObject _weaponPoint;

    Dictionary<EquipmentType, GameObject> _equipments = new Dictionary<EquipmentType, GameObject>();
    
    public GameObject GetEquipment(EquipmentType type)
    {
        if (!_equipments.ContainsKey(type)) return null;

        return _equipments[type];
    }

    public void EquipItem(GameObject item)
    {
        Item equipment = item.GetComponent<Item>();
        if (equipment == null || equipment.ItemType != ItemType.Equipment) return;

        EquipmentType type = equipment.EquipmentType;

        if (!_equipments.ContainsKey(type))
            _equipments.Add(type, null);

        // 기존 장비가 있다면 바닥에 떨어트린다.
        if (_equipments[type] != null)
        {
            _equipments[type].GetComponent<Item>().Unequip();
            _equipments[type].transform.SetParent(null);
        }


       if(type == EquipmentType.RightWeapon)
        {
            equipment.Equip();
            item.transform.SetParent(_weaponPoint.transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;
        }

        _equipments[type] = item;
    }


    public void Unequip(EquipmentType type)
    {
        _equipments[type].GetComponent<Item>().Unequip();
        _equipments[type].transform.SetParent(null);
        _equipments[type] = null;
    }
}
