using System.Collections.Generic;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] int _slotCount = 10;
    public int SlotCount => _slotCount;

    private ItemSlot[] _itemSlots;

    private void Awake()
    {
        _itemSlots = new ItemSlot[_slotCount];
        for(int i = 0; i < _slotCount; i++)
        {
            _itemSlots[i]  = new ItemSlot();
            _itemSlots[i].itemName = string.Empty;
            _itemSlots[i].item = null;
        }
    }

    public bool InsertItem(GameObject gameObject, int count = 1, int index = -1)
    {
        Item item = gameObject.GetComponent<Item>();
        if (item == null) return false;
        string itemName = gameObject.name;
        
        if (index == -1)
        {
            int emptySlot = -1;
            int slotIndex = 0;
            foreach (ItemSlot slot in _itemSlots)
            {
                if (slot.item == null)
                {
                    emptySlot = slotIndex;
                    break;
                }
                if (slot.itemName == itemName)
                {
                    slot.item.Count += count;
                    Destroy(item.gameObject);
                    return true;
                }
                slotIndex++;
            }

            if (emptySlot == -1) return false;
            _itemSlots[emptySlot].itemName = itemName;
            _itemSlots[emptySlot].item = item;
            _itemSlots[emptySlot].item.Count = count;
            item.gameObject.SetActive(false);
        }
        else
        {
            if (_itemSlots[index].itemName == itemName )
            {
                _itemSlots[index].item.Count += count;
                Destroy(item.gameObject);
            }
            else if(_itemSlots[index].item = null)
            {
                _itemSlots[index].itemName = itemName;
                _itemSlots[index].item = item;
                _itemSlots[index].item.Count = count;
                item.gameObject.SetActive(false);
            }
            else
            {
                return false;
            }
        }

        return true;
    }
    // 자신의 앞으로 아이템을 버린다.
    public bool DropItem(int index)
    {
        if(index < 0 || index >= _itemSlots.Length) return false;

        Item item = _itemSlots[index].item;
        item.gameObject.SetActive(true);
        item.transform.position = transform.position + transform.forward;
        RemoveItem(index);
        return true;
    }
    public bool RemoveItem(int index)
    {
        if(index < 0 || index >= _itemSlots.Length) return false;

        _itemSlots[index].item = null;
        _itemSlots[index].itemName = "";

        return true;
    }
    public ItemSlot GetSlot(int index)
    {
        if (index < 0 || index >= _itemSlots.Length) return null;

        return _itemSlots[index];
    }
}

public class ItemSlot
{
    public string itemName;
    public Item item;
}
