using System.Globalization;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] int _slotCount = 10;
    private ItemSlot[] _itemSlots;

    private void Awake()
    {
        _itemSlots = new ItemSlot[_slotCount];
        for(int i = 0; i < _slotCount; i++)
        {
            _itemSlots[i]  = new ItemSlot();
        }
    }

    public bool InsertItem(string itemName, int count = 1, int index = -1)
    {
        if (index == -1)
        {
            int emptySlot = -1;
            int slotIndex = 0;
            foreach (ItemSlot slot in _itemSlots)
            {
                if (slot.itemName == "")
                    emptySlot = slotIndex;
                if (slot.itemName == itemName)
                {
                    slot.count += count;
                    return true;
                }
                slotIndex++;
            }

            if (emptySlot == -1) return false;
            _itemSlots[emptySlot].itemName = itemName;
            _itemSlots[emptySlot].count = count;

        }
        else
        {
            if (_itemSlots[index].itemName == itemName || _itemSlots[index].itemName =="")
            {
                _itemSlots[index].itemName = itemName;
                _itemSlots[index].count += count;
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}

public class ItemSlot
{
    public string itemName;
    public int count;
}
