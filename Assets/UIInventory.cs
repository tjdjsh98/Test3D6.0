using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    public static UIInventory _instance;
    public static UIInventory Instance
    {
        get
        {
            if (_instance == null)
                Init();
            return _instance;
        }
    }

    [SerializeField] GameObject _itemSlotParent;
    List<UIInventorySlot> _uiInventorySlotList = new List<UIInventorySlot>();

    Inventory _connectedInventory;

    static void Init()
    {
        _instance = GameObject.Find("Canvas").transform.Find("UIInventory").GetComponent<UIInventory>();
        _instance.UIInit();
    }

    private void UIInit()
    {
        for(int i = 0; i < _itemSlotParent.transform.childCount; i++)
        {
            UIInventorySlot slot = new UIInventorySlot();
            slot.parent = _itemSlotParent.transform.GetChild(i).gameObject;
            slot.itemImage = slot.parent.transform.Find("ItemImage").GetComponent<Image>();
            slot.itemTextmesh = slot.parent.transform.Find("ItemTextMesh").GetComponent<TextMeshProUGUI>();

            _uiInventorySlotList.Add(slot);

        }
        Refresh();
        _instance.gameObject.SetActive(false);
    }
    public void ConnectInventory(Inventory inventory)
    {
        _connectedInventory = inventory;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void Refresh()
    {
        if(_connectedInventory)
        {
            int slotCount = _connectedInventory.SlotCount;

            for(int i = 0; i < _uiInventorySlotList.Count;i++)
            {
                if (i >= slotCount)
                {
                    _uiInventorySlotList[i].itemImage.color = Color.red;
                    _uiInventorySlotList[i].itemTextmesh.text = "";
                    continue;
                }

                ItemSlot itemSlot = _connectedInventory.GetSlot(i);

                if (itemSlot != null)
                {
                    if (itemSlot.count > 0)
                    {
                        _uiInventorySlotList[i].itemImage.color = Color.white;
                        _uiInventorySlotList[i].itemTextmesh.text = $"{itemSlot.itemName} x {itemSlot.count}";
                    }
                    else
                    {
                        _uiInventorySlotList[i].itemImage.color = Color.red;
                        _uiInventorySlotList[i].itemTextmesh.text = "";
                    }
                }

            }
        }
        else
        {
            for (int i = 0; i < _uiInventorySlotList.Count; i++)
            {
                _uiInventorySlotList[i].itemImage.color = Color.red;
                _uiInventorySlotList[i].itemTextmesh.text = "";
            }

        }
    }
}
public struct UIInventorySlot
{
    public GameObject parent;
    public Image itemImage;
    public TextMeshProUGUI itemTextmesh;
}