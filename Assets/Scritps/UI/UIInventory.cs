using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Image = UnityEngine.UI.Image;

public class UIInventory : UIBase
{
    GraphicRaycaster _raycaster;
    PointerEventData _pointerEnterEvent;
    List<RaycastResult> _results = new List<RaycastResult>();

    Inventory _connectedInventory;
    CharacterEquipment _connectedCharacterEquipment;

    [SerializeField] GameObject _itemSlotParent;
    List<UIItemSlot> _uiInventorySlotList = new List<UIItemSlot>();
   

    [Header("Equipment")]
    [SerializeField] GameObject _equipmentParent;
    UIItemSlot _heatSlot;
    UIItemSlot _bodySlot;
    UIItemSlot _weaponSlot;
    UIItemSlot _shoesSlot;

    [SerializeField] GameObject _trash;

    // 아이템 드래그 드롭
    EquipmentType _dragedEquipmentType;

    public override void Init()
    {
        
        _raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
        _pointerEnterEvent = new PointerEventData(null);

        for (int i = 0; i < _itemSlotParent.transform.childCount; i++)
        {
            UIItemSlot slot = new UIItemSlot();
            slot.parent = _itemSlotParent.transform.GetChild(i).gameObject;
            slot.itemImage = slot.parent.transform.Find("ItemImage").GetComponent<Image>();
            slot.itemNameTextmesh = slot.parent.transform.Find("ItemTextMesh").GetComponent<TextMeshProUGUI>();

            _uiInventorySlotList.Add(slot);

        }
        FillInventorySlot(out _heatSlot, _equipmentParent.transform.Find("HeadSlot"));
        FillInventorySlot(out _bodySlot, _equipmentParent.transform.Find("BodySlot"));
        FillInventorySlot(out _weaponSlot, _equipmentParent.transform.Find("WeaponSlot"));
        FillInventorySlot(out _shoesSlot, _equipmentParent.transform.Find("ShoesSlot"));

        Refresh();
        gameObject.SetActive(false);
    }
   

    void FillInventorySlot(out UIItemSlot slot, Transform parent)
    {
        slot = new UIItemSlot();
        slot.parent = parent.gameObject;
        slot.itemImage = slot.parent.transform.Find("ItemImage").GetComponent<Image>();
        slot.itemNameTextmesh = slot.parent.transform.Find("ItemTextMesh").GetComponent<TextMeshProUGUI>();
    }

    public void ConnectInventory(Inventory inventory)
    {
        DisconnectInventory();
        _connectedInventory = inventory;
        _connectedInventory.ItemChanged += Refresh;
    }
    public void DisconnectInventory()
    {
        if(_connectedInventory)
            _connectedInventory.ItemChanged -= Refresh;
        _connectedInventory = null;
    }
    public void ConnecteCharacterEquipment(CharacterEquipment characterEquipment)
    {
        _connectedCharacterEquipment = characterEquipment;
    }
    public void Open()
    {
        InputManager.Instance.IsEnableInput = false;
        InputManager.Instance.IsEnableFocus = false;

        gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        InputManager.Instance.IsEnableFocus = true;
        InputManager.Instance.IsEnableInput = true;

        DisconnectInventory();
        gameObject.SetActive(false);
    }

    public void OnItemSlotPointerDrag(GameObject itemImage)
    {
        for (int i = 0; i < _uiInventorySlotList.Count; i++)
        {
            if (_uiInventorySlotList[i].itemImage.gameObject == itemImage)
            {
                ItemSlot slot = _connectedInventory.GetSlot(i);
                if (slot.itemName == "") return;

                UIManager.Instance.DragItemSlotIndex = i;
                UIManager.Instance.StartedDragInventory = _connectedInventory;
                UIManager.Instance.DragIItem(_uiInventorySlotList[i].itemImage.sprite);
                return;
            }
        }

        // 장비창
        // TODO
        if (_heatSlot.itemImage.gameObject == itemImage) { }
        if (_weaponSlot.itemImage.gameObject == itemImage)
        {
            _dragedEquipmentType = EquipmentType.RightWeapon;
            UIManager.Instance.DragIItem(_weaponSlot.itemImage.sprite);
        }
        if (_bodySlot.itemImage.gameObject == itemImage) { }
        if (_shoesSlot.itemImage.gameObject == itemImage) { }
    }

    public void OnItemSlotPointerDrop(GameObject itemImage)
    {
        Inventory startedDragInventory = UIManager.Instance.StartedDragInventory;
        int dragItemSlotIndex = UIManager.Instance.DragItemSlotIndex;

        if (startedDragInventory == null || dragItemSlotIndex < 0) return;

      
        for (int i = 0; i < _uiInventorySlotList.Count; i++)
        {
            if (itemImage.gameObject != _uiInventorySlotList[i].itemImage.gameObject) continue;

            // 아이템 이동
            if(_connectedInventory != startedDragInventory || i != dragItemSlotIndex)
            {
                var data = startedDragInventory.AccumulateInputData;
                data.isExchangeItem = true;
                data.myInventoryObjectId = startedDragInventory.Object.Id;
                data.myInventoryId = startedDragInventory.Id;
                data.myInventoryIndex = dragItemSlotIndex;

                data.encounterInventoryObjectId = _connectedInventory.Object.Id;
                data.encounterInventoryId = _connectedInventory.Id;
                data.encounterInventoryIndex = i;

                _connectedInventory.AccumulateInputData = data;
            }
        }
    }
    public void OnDropItemPointerDrop(GameObject itemImage)
    {
        Inventory startedDragInventory = UIManager.Instance.StartedDragInventory;
        int dragItemSlotIndex = UIManager.Instance.DragItemSlotIndex;

        if (startedDragInventory == null || dragItemSlotIndex < 0) return;

        // 인벤토리에서 버리기
        if(_trash.gameObject == itemImage)
        {
            var data = startedDragInventory.AccumulateInputData;
            data.myInventoryObjectId = startedDragInventory.Object.Id;
            data.myInventoryId = startedDragInventory.Id;
            data.isDropItem = true;
            data.myInventoryIndex = dragItemSlotIndex;
            _connectedInventory.AccumulateInputData = data;

            Refresh();
        }
    }

  
    void Refresh()
    {
        RefreshInventory();
        RefreshEquipment();
    }
    void RefreshInventory()
    {
        if (_connectedInventory != null)
        {
            int slotCount = _connectedInventory.SlotCount;

            for (int i = 0; i < _uiInventorySlotList.Count; i++)
            {
                if (i >= slotCount)
                {
                    _uiInventorySlotList[i].itemImage.color = Color.red;
                    _uiInventorySlotList[i].itemNameTextmesh.text = "";
                    continue;
                }

                ItemSlot itemSlot = _connectedInventory.GetSlot(i);
                
                if (itemSlot.itemName != "")
                {
                    _uiInventorySlotList[i].itemImage.color = Color.white;
                    _uiInventorySlotList[i].itemNameTextmesh.text = $"{itemSlot.itemName} x {itemSlot.count}";
                }
                else
                {
                    _uiInventorySlotList[i].itemImage.color = Color.red;
                    _uiInventorySlotList[i].itemNameTextmesh.text = "";
                }
            }
        }
        else
        {
            for (int i = 0; i < _uiInventorySlotList.Count; i++)
            {
                _uiInventorySlotList[i].itemImage.color = Color.red;
                _uiInventorySlotList[i].itemNameTextmesh.text = "";
            }

        }
    }

    void RefreshEquipment()
    {
        if (_connectedCharacterEquipment)
        {
            GameObject hat = _connectedCharacterEquipment.GetEquipment(EquipmentType.Hat);
            if (hat) {
                _heatSlot.itemNameTextmesh.text = hat.name;
                _heatSlot.itemImage.color = Color.white;
            }
            else
            {
                _heatSlot.itemNameTextmesh.text = "";
                _heatSlot.itemImage.color = Color.red;
            }

            GameObject weapon = _connectedCharacterEquipment.GetEquipment(EquipmentType.RightWeapon);
            if (weapon)
            {
                _weaponSlot.itemNameTextmesh.text = weapon.name;
                _weaponSlot.itemImage.color = Color.white;
            }
            else
            {
                _weaponSlot.itemNameTextmesh.text = "";
                _weaponSlot.itemImage.color = Color.red;
            }
        }
        else
        {
            _heatSlot.itemNameTextmesh.text = "";
            _heatSlot.itemImage.color = Color.red;
            _bodySlot.itemNameTextmesh.text = "";
            _bodySlot.itemImage.color = Color.red;
            _weaponSlot.itemNameTextmesh.text = "";
            _weaponSlot.itemImage.color = Color.red;
            _shoesSlot.itemNameTextmesh.text = "";
            _shoesSlot.itemImage.color = Color.red;
        }
    }
}
public struct UIItemSlot
{
    public GameObject parent;
    public Image itemImage;
    public TextMeshProUGUI itemNameTextmesh;
    public TextMeshProUGUI itemCountTextmesh;
}