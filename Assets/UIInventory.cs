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


    [SerializeField] GameObject _itemSlotParent;
    List<UIInventorySlot> _uiInventorySlotList = new List<UIInventorySlot>();


    [SerializeField] Image _hatImage;
    [SerializeField] Image _bodyImage;
    [SerializeField] Image _weaponImage;
    [SerializeField] Image _shoeImage;

    ItemSlot _dragItemData;
    Image _dragItemImage;

    Inventory _connectedInventory;

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
        Refresh();
        gameObject.SetActive(false);
    }
    private void Update()
    {
        ControlMouse();
        if(_dragItemData!= null)
        {
            _dragItemImage.transform.position = Input.mousePosition;
        }
    }

    public void ConnectInventory(Inventory inventory)
    {
        _connectedInventory = inventory;
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

            // 인벤토리 레이캐스트 확인
            // 아이템이 존재한다면 드래그
            for (int i = 0; i < _results.Count; i++)
            {
                GameObject ui = _results[i].gameObject;

                for (int j = 0; j < _uiInventorySlotList.Count; j++)
                {
                    if(ui == _uiInventorySlotList[j].itemImage.gameObject)
                    {
                        ItemSlot slot = _connectedInventory.GetSlot(j);
                        if (slot.count == 0) return;

                        DragIItem(slot, _uiInventorySlotList[i].itemImage);
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

    void DragIItem(ItemSlot itemSlot, Image image)
    {
        _dragItemImage.sprite = image.sprite;
        _dragItemData = itemSlot;
        _dragItemImage.gameObject.SetActive(true);
    }

    void ReleaseItem()
    {
        if (_dragItemData == null) return;


        _pointerEnterEvent.position = Input.mousePosition;
        _results.Clear();
        _raycaster.Raycast(_pointerEnterEvent, _results);

        // 인벤토리 레이캐스트 확인
        // 아이템이 존재한다면 드래그
        for (int i = 0; i < _results.Count; i++)
        {
            GameObject ui = _results[i].gameObject;

            for (int j = 0; j < _uiInventorySlotList.Count; j++)
            {
                if (ui == _uiInventorySlotList[j].itemImage.gameObject)
                {
                    ItemSlot slot = _connectedInventory.GetSlot(j);
                    if (slot.count == 0) return;

                    DragIItem(slot, _uiInventorySlotList[i].itemImage);
                    return;
                }
            }
            // 장착 가능한 아이템인지 확인

        }

        _dragItemImage.gameObject.SetActive(false);
        _dragItemData = null;
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