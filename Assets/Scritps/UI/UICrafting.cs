using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICrafting : UIBase
{
    struct UICraftingSlot
    {
        public GameObject parent;
        public UIItemSlot result;
        public List<UIItemSlot> materials;
    }


    [SerializeField] GameObject _uiCraftingSlotPrefab;
    [SerializeField] Transform _uiCraftingSlotFolder;
    List<UICraftingSlot> _craftingSlotList = new List<UICraftingSlot>();

    int _selectCraftingUIIndex = -1;


    PrototypeCharacterController _characterController;

    public override void Init()
    {
        CreateNewUICraftingSlot();
        CreateNewUICraftingSlot();
        CreateNewUICraftingSlot();

        Close();
    }



    public void ConnectCrafting(PrototypeCharacterController characterController)
    {
        _characterController = characterController;
    }
    
    public void Open()
    {
        if (_characterController == null) return;

        InputManager.Instance.IsEnableFocus = false;
        InputManager.Instance.IsEnableInput = false;

        gameObject.SetActive(true);

        Refresh();
    }

    public void Close()
    {
        InputManager.Instance.IsEnableFocus = true;
        InputManager.Instance.IsEnableInput = true;
        gameObject.SetActive(false);

    }


    void CreateNewUICraftingSlot()
    {
        GameObject go = Instantiate(_uiCraftingSlotPrefab);

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) =>{ OnCraftingSlotClick((PointerEventData)data); });
        go.GetComponent<EventTrigger>().triggers.Add(entry);

        go.transform.SetParent(_uiCraftingSlotFolder);
        UICraftingSlot uiSlot = new UICraftingSlot();
        uiSlot.parent = go;
        uiSlot.result.parent = go.transform.Find("Result").gameObject;
        uiSlot.result.itemNameTextmesh = uiSlot.result.parent.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        uiSlot.result.itemCountTextmesh = uiSlot.result.parent.transform.Find("Count").GetComponent<TextMeshProUGUI>();

        uiSlot.materials = new List<UIItemSlot>();

        for(int i = 1; i < uiSlot.parent.transform.childCount; i++)
        {
            Transform parent = uiSlot.parent.transform.GetChild(i);
            UIItemSlot material = new UIItemSlot();
            material.parent = parent.gameObject;
            material.itemImage = parent.GetComponent<Image>();
            material.itemNameTextmesh = parent.Find("Name").GetComponent<TextMeshProUGUI>();
            material.itemCountTextmesh = parent.Find("Count").GetComponent<TextMeshProUGUI>();
            uiSlot.materials.Add(material);
        }

        _craftingSlotList.Add(uiSlot);
    }

    void Refresh()
    {
        if (_characterController == null) return;

        List<ReceiptData> receiptDataList = _characterController.CraftingDataList;

        int i = 0;
        for(; i < receiptDataList.Count; i++)
        {
            ReceiptData data = receiptDataList[i];
            if(_craftingSlotList.Count<= i)
                CreateNewUICraftingSlot();

            _craftingSlotList[i].result.itemNameTextmesh.text = data.resultItem;
            _craftingSlotList[i].result.itemCountTextmesh.text = "1";

            int j = 0;
            for(; j < data.ReceiptItemList.Count;j++)
            {
                _craftingSlotList[i].materials[j].itemNameTextmesh.text = data.ReceiptItemList[j].itemName;
                _craftingSlotList[i].materials[j].itemCountTextmesh.text = data.ReceiptItemList[j].requireItemCount.ToString(); 
                _craftingSlotList[i].materials[j].parent.gameObject.SetActive(true);
            }
            for (; j < _craftingSlotList[i].materials.Count; j++)
            {
                _craftingSlotList[i].materials[j].parent.gameObject.SetActive(false);
            }

            _craftingSlotList[i].parent.gameObject.SetActive(true);
        }
        for (; i < _craftingSlotList.Count; i++)
        {
            _craftingSlotList[i].parent.gameObject.SetActive(false);

        }
    }


    void OnCraftingSlotClick(PointerEventData data)
    {
        for(int i = 0; i < _craftingSlotList.Count;i++)
        {
            if (_craftingSlotList[i].parent == (data.pointerClick))
            {
                Image image = data.pointerClick.GetComponent<Image>();

                if (_selectCraftingUIIndex == i)
                {
                    image.color = Color.white;
                    _selectCraftingUIIndex = -1;
                }
                else if (_selectCraftingUIIndex == -1)
                {
                    image.color = Color.red;
                    _selectCraftingUIIndex = i;
                }
                else
                {
                    _craftingSlotList[_selectCraftingUIIndex].parent.GetComponent<Image>().color = Color.white;
                    image.color = Color.red;
                    _selectCraftingUIIndex = i;
                }
                break;
            }
        }
    }

   

    public void OnCraftingButtonClicked()
    {
        if (_selectCraftingUIIndex == -1) return;

        ReceiptData receiptData = _characterController.CraftingDataList[_selectCraftingUIIndex];
        ReceiptInputData data = _characterController.AccumulateReceiptInputData;
        data.isReceipt = true;
        data.receptName.Set(receiptData.DataName);
        _characterController.AccumulateReceiptInputData = data;

        _craftingSlotList[_selectCraftingUIIndex].parent.GetComponent<Image>().color = Color.white;
        _selectCraftingUIIndex = -1;
        
    }
}
