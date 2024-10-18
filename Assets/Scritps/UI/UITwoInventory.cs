using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Image = UnityEngine.UI.Image;

public class UITwoInventory : UIBase
{
    GraphicRaycaster _raycaster;
    PointerEventData _pointerEnterEvent;
    List<RaycastResult> _results = new List<RaycastResult>();

    Inventory _characterInventory;
    Inventory _chestInventory;

    [SerializeField] GameObject _characterInventoryParent;
    List<UIItemSlot> _uicharacterInventorySlotList = new List<UIItemSlot>();

    [SerializeField] GameObject _chestInventoryParent;
    List<UIItemSlot> _uiChestInventorySlotList = new List<UIItemSlot>();


    // 아이템 드래그 드롭
    Inventory _dragInventory;
    int _dragItemSlotIndex;
    Image _dragItemImage;

    public override void Init()
    {
        _dragItemImage = new GameObject().AddComponent<Image>();
        _dragItemImage.gameObject.SetActive(false);
        _dragItemImage.transform.SetParent(transform);
        _dragItemImage.GetComponent<RectTransform>().sizeDelta = Vector2.one * 100;
        _dragItemImage.transform.localScale = Vector3.one;

        _raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
        _pointerEnterEvent = new PointerEventData(null);

        for (int i = 0; i < _characterInventoryParent.transform.childCount; i++)
        {
            UIItemSlot slot = new UIItemSlot();
            FillInventorySlot(out slot, _characterInventoryParent.transform.GetChild(i));
            _uicharacterInventorySlotList.Add(slot);
        }
        for (int i = 0; i < _chestInventoryParent.transform.childCount; i++)
        {
            UIItemSlot slot = new UIItemSlot();
            FillInventorySlot(out slot, _chestInventoryParent.transform.GetChild(i));
            _uiChestInventorySlotList.Add(slot);
        }

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

    private void Update()
    {
        ControlMouse();
        if (_dragItemImage.gameObject.activeSelf)
        {
            _dragItemImage.transform.position = Input.mousePosition;
            
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

    }

    public void ConnectInventory(Inventory characterInventory, Inventory chestInventory)
    {
        _characterInventory = characterInventory;
        _chestInventory = chestInventory;
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
        if (Input.GetMouseButtonDown(0))
        {
            _pointerEnterEvent.position = Input.mousePosition;
            _results.Clear();
            _raycaster.Raycast(_pointerEnterEvent, _results);

            // 아이템이 존재한다면 드래그
            for (int i = 0; i < _results.Count; i++)
            {
                GameObject ui = _results[i].gameObject;

                // 캐릭터 인벤토리
                for (int j = 0; j < _uicharacterInventorySlotList.Count; j++)
                {
                    if (ui == _uicharacterInventorySlotList[j].itemImage.gameObject)
                    {
                        ItemSlot slot = _characterInventory.GetSlot(j);
                        if (slot.itemName == "") return;

                        _dragItemSlotIndex = j;
                        DragIItem(_uicharacterInventorySlotList[i]);
                        _dragInventory = _characterInventory;
                        return;
                    }
                }

                // 체스트 인벤토리
                for (int j = 0; j < _uiChestInventorySlotList.Count; j++)
                {
                    if (ui == _uiChestInventorySlotList[j].itemImage.gameObject)
                    {
                        ItemSlot slot = _chestInventory.GetSlot(j);
                        if (slot.itemName == "") return;

                        _dragItemSlotIndex = j;
                        DragIItem(_uiChestInventorySlotList[i]);
                        _dragInventory = _chestInventory;
                        return;
                    }
                }

           
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseItem();
        }
    }

    void DragIItem(UIItemSlot slot)
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

        bool success = false;

        // 인벤토리 레이캐스트 확인
        // 아이템이 존재한다면 드래그
        for (int i = 0; i < _results.Count; i++)
        {
            GameObject ui = _results[i].gameObject;

            // 캐릭터 인벤토리
            for (int j = 0; j < _uicharacterInventorySlotList.Count; j++)
            {
                if (ui == _uicharacterInventorySlotList[j].itemImage.gameObject)
                {
                    ItemSlot slot = _characterInventory.GetSlot(j);

                    // 아이템 슬롯을 교환한다.
                    ItemSlot dragItemSlot = _dragInventory.GetSlot(_dragItemSlotIndex);
                    if (_characterInventory.SetSlot(dragItemSlot, j))
                    {
                        _dragInventory.SetSlot(slot, _dragItemSlotIndex);
                        success = true; 
                        break;
                    }

                }
            }

            // 체스트 인벤토리
            for (int j = 0; j < _uiChestInventorySlotList.Count; j++)
            {
                if (ui == _uiChestInventorySlotList[j].itemImage.gameObject)
                {
                    ItemSlot slot = _chestInventory.GetSlot(j);

                    // 아이템 슬롯을 교환한다.
                    ItemSlot dragItemSlot = _dragInventory.GetSlot(_dragItemSlotIndex);
                    if (_chestInventory.SetSlot(dragItemSlot, j))
                    {
                        _dragInventory.SetSlot(slot, _dragItemSlotIndex);
                        success = true;
                        break;
                    }
                }
            }
            if (success) break;
        }

        _dragItemSlotIndex = -1;
        _dragItemImage.gameObject.SetActive(false);
        Refresh();
    }

    void Refresh()
    {
        RefreshInventory(_uicharacterInventorySlotList,_characterInventory);
        RefreshInventory(_uiChestInventorySlotList,_chestInventory);
    }
    void RefreshInventory(List<UIItemSlot> uiSlotList,Inventory inventory)
    {
        if (inventory != null)
        {
            int slotCount = inventory.SlotCount;

            for (int i = 0; i < uiSlotList.Count; i++)
            {
                if (i >= slotCount)
                {
                    uiSlotList[i].itemImage.color = Color.red;
                    uiSlotList[i].itemNameTextmesh.text = "";
                    continue;
                }

                ItemSlot itemSlot = inventory.GetSlot(i);
                if (itemSlot.itemName != "")
                {
                    uiSlotList[i].itemImage.color = Color.white;
                    uiSlotList[i].itemNameTextmesh.text = $"{itemSlot.itemName} x {itemSlot.count}";
                }
                else
                {
                    uiSlotList[i].itemImage.color = Color.red;
                    uiSlotList[i].itemNameTextmesh.text = "";
                }
            }
        }
        else
        {
            for (int i = 0; i < uiSlotList.Count; i++)
            {
                uiSlotList[i].itemImage.color = Color.red;
                uiSlotList[i].itemNameTextmesh.text = "";
            }
        }
    }

    
}