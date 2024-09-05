using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Image = UnityEngine.UI.Image;

public class UIInventory : MonoBehaviour
{
    GraphicRaycaster _raycaster;
    PointerEventData _pointerEnterEvent;
    List<RaycastResult> _results = new List<RaycastResult>();

    Inventory _connectedInventory;
    CharacterEquipment _connectedCharacterEquipment;

    [SerializeField] GameObject _itemSlotParent;
    List<UIInventorySlot> _uiInventorySlotList = new List<UIInventorySlot>();

    [Header("Equipment")]

    [SerializeField] GameObject _equipmentParent;
    UIInventorySlot _heatSlot;
    UIInventorySlot _bodySlot;
    UIInventorySlot _weaponSlot;
    UIInventorySlot _shoesSlot;

    [SerializeField] GameObject _trash;

    // ������ �巡�� ���
    int _dragItemSlotIndex;
    EquipmentType _dragedEquipmentType;
    Image _dragItemImage;


    private void Awake()
    {
        _dragItemImage = new GameObject().AddComponent<Image>();
        _dragItemImage.gameObject.SetActive(false);
        _dragItemImage.transform.SetParent(transform);
        _dragItemImage.GetComponent<RectTransform>().sizeDelta = Vector2.one * 100;
        _dragItemImage.transform.localScale = Vector3.one;

        _raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
        _pointerEnterEvent = new PointerEventData(null);

        for (int i = 0; i < _itemSlotParent.transform.childCount; i++)
        {
            UIInventorySlot slot = new UIInventorySlot();
            slot.parent = _itemSlotParent.transform.GetChild(i).gameObject;
            slot.itemImage = slot.parent.transform.Find("ItemImage").GetComponent<Image>();
            slot.itemTextmesh = slot.parent.transform.Find("ItemTextMesh").GetComponent<TextMeshProUGUI>();

            _uiInventorySlotList.Add(slot);

        }
        FillInventorySlot(out _heatSlot, _equipmentParent.transform.Find("HeadSlot"));
        FillInventorySlot(out _bodySlot, _equipmentParent.transform.Find("BodySlot"));
        FillInventorySlot(out _weaponSlot, _equipmentParent.transform.Find("WeaponSlot"));
        FillInventorySlot(out _shoesSlot, _equipmentParent.transform.Find("ShoesSlot"));

        Refresh();
        gameObject.SetActive(false);
    }
    private void Update()
    {
        ControlMouse();
        if(_dragItemImage.gameObject.activeSelf)
        {
            _dragItemImage.transform.position = Input.mousePosition;
        }
    }

    void FillInventorySlot(out UIInventorySlot slot, Transform parent)
    {
        slot = new UIInventorySlot();
        slot.parent = parent.gameObject;
        slot.itemImage = slot.parent.transform.Find("ItemImage").GetComponent<Image>();
        slot.itemTextmesh = slot.parent.transform.Find("ItemTextMesh").GetComponent<TextMeshProUGUI>();
    }

    public void ConnectInventory(Inventory inventory)
    {
        _connectedInventory = inventory;
    }
    public void ConnecteCharacterEquipment(CharacterEquipment characterEquipment)
    {
        _connectedCharacterEquipment = characterEquipment;
    }
    public void Open()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void ControlMouse()
    {
        if (Input.GetMouseButtonDown(0)) {
            _pointerEnterEvent.position = Input.mousePosition;
            _results.Clear();
            _raycaster.Raycast(_pointerEnterEvent, _results);

            // �������� �����Ѵٸ� �巡��
            for (int i = 0; i < _results.Count; i++)
            {
                GameObject ui = _results[i].gameObject;

                // �κ��丮
                for (int j = 0; j < _uiInventorySlotList.Count; j++)
                {
                    if(ui == _uiInventorySlotList[j].itemImage.gameObject)
                    {
                        ItemSlot slot = _connectedInventory.GetSlot(j);
                        if (slot.itemName == "") return;

                        _dragItemSlotIndex = j;
                        DragIItem(_uiInventorySlotList[i]);
                        return;
                    }
                }

                // ���â
                // TODO
                if (_heatSlot.itemImage.gameObject == ui) { }
                if (_weaponSlot.itemImage.gameObject == ui) 
                {
                    _dragedEquipmentType = EquipmentType.RightWeapon;
                    DragIItem(_weaponSlot);
                }
                if (_bodySlot.itemImage.gameObject == ui) { }
                if (_shoesSlot.itemImage.gameObject == ui) { }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseItem();
        }
    }

    void DragIItem(UIInventorySlot slot)
    {
        _dragItemImage.sprite = slot.itemImage.sprite;
        _dragItemImage.gameObject.SetActive(true);
    }

    void ReleaseItem()
    {
        if (!_dragItemImage.gameObject.activeSelf) return;


        _pointerEnterEvent.position = Input.mousePosition;
        _results.Clear();
        _raycaster.Raycast(_pointerEnterEvent, _results);

        // �κ��丮 ����ĳ��Ʈ Ȯ��
        // �������� �����Ѵٸ� �巡��
        for (int i = 0; i < _results.Count; i++)
        {
            GameObject ui = _results[i].gameObject;

            // �κ��丮 ȣ����


            // ���� ������ ���������� Ȯ��


            // ������ ������
            if (ui == _trash)
            {
                // �κ��丮���� ������
                if (_dragItemSlotIndex >= 0)
                {
                    _connectedInventory.DropItem(_dragItemSlotIndex);
                    Refresh();
                }

                // �������ۿ��� ������
                if(_dragedEquipmentType == EquipmentType.RightWeapon)
                {
                    _connectedCharacterEquipment.Unequip(EquipmentType.RightWeapon);
                    Refresh();
                }
            }
        }

        _dragItemSlotIndex = -1;
        _dragedEquipmentType = EquipmentType.LeftWeapon;
        _dragItemImage.gameObject.SetActive(false);
    }

    void Refresh()
    {
        RefreshInventory();
        RefreshEquipment();
    }
    void RefreshInventory()
    {
        if (_connectedInventory)
        {
            int slotCount = _connectedInventory.SlotCount;

            for (int i = 0; i < _uiInventorySlotList.Count; i++)
            {
                if (i >= slotCount)
                {
                    _uiInventorySlotList[i].itemImage.color = Color.red;
                    _uiInventorySlotList[i].itemTextmesh.text = "";
                    continue;
                }

                ItemSlot itemSlot = _connectedInventory.GetSlot(i);
                
                if (itemSlot.itemName != "")
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
        else
        {
            for (int i = 0; i < _uiInventorySlotList.Count; i++)
            {
                _uiInventorySlotList[i].itemImage.color = Color.red;
                _uiInventorySlotList[i].itemTextmesh.text = "";
            }

        }
    }

    void RefreshEquipment()
    {
        if (_connectedCharacterEquipment)
        {
            GameObject hat = _connectedCharacterEquipment.GetEquipment(EquipmentType.Hat);
            if (hat) {
                _heatSlot.itemTextmesh.text = hat.name;
                _heatSlot.itemImage.color = Color.white;
            }
            else
            {
                _heatSlot.itemTextmesh.text = "";
                _heatSlot.itemImage.color = Color.red;
            }

            GameObject weapon = _connectedCharacterEquipment.GetEquipment(EquipmentType.RightWeapon);
            if (weapon)
            {
                _weaponSlot.itemTextmesh.text = weapon.name;
                _weaponSlot.itemImage.color = Color.white;
            }
            else
            {
                _weaponSlot.itemTextmesh.text = "";
                _weaponSlot.itemImage.color = Color.red;
            }
        }
        else
        {
            _heatSlot.itemTextmesh.text = "";
            _heatSlot.itemImage.color = Color.red;
            _bodySlot.itemTextmesh.text = "";
            _bodySlot.itemImage.color = Color.red;
            _weaponSlot.itemTextmesh.text = "";
            _weaponSlot.itemImage.color = Color.red;
            _shoesSlot.itemTextmesh.text = "";
            _shoesSlot.itemImage.color = Color.red;
        }
    }
}
public struct UIInventorySlot
{
    public GameObject parent;
    public Image itemImage;
    public TextMeshProUGUI itemTextmesh;
}