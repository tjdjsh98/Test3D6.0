using System.Globalization;
using System.Linq;
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
            _itemSlots[i].count = 0;
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
                if (slot.count == 0)
                {
                    emptySlot = slotIndex;
                    break;
                }
                if (slot.itemName == itemName)
                {
                    slot.count += count;
                    return true;
                }
                slotIndex++;
            }
            Debug.Log(emptySlot);

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

    public ItemSlot GetSlot(int index)
    {
        if (index < 0 || index >= _itemSlots.Length) return null;

        return _itemSlots[index];
    }
}

public class ItemSlot
{
    public string itemName;
    public int count;
}
