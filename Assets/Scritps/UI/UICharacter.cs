using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacter : UIBase
{
    PrototypeCharacter _character;
    PrototypeCharacterController _characterController;

    [SerializeField] RectTransform _hpBarTr;
    [SerializeField] TextMeshProUGUI _hpTextMesh;
    Vector2 _initHpbarSize;

    [SerializeField] RectTransform _staminaBarTr;
    [SerializeField] TextMeshProUGUI _staminaTextMesh;
    Vector2 _initStaminabarSize;

    // QuickSlot
    Inventory _quickSlotInventory;
    [SerializeField] Transform _quickSlotParent;
    List<UIInventorySlot> _quickSlotList = new List<UIInventorySlot>();

    // Working
    [SerializeField]Image _workingProcessImage;

    public override void Init() 
    { 
        for(int i = 0; i <  _quickSlotParent.childCount; i++)
        {
            UIInventorySlot slot = new UIInventorySlot();
            slot.parent = _quickSlotParent.GetChild(i).gameObject;
            slot.itemTextmesh = slot.parent.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            slot.itemImage = slot.parent.GetComponent<Image>();
            _quickSlotList.Add(slot);
        }
    }

    public void ConnectCharacter(PrototypeCharacter character)
    {
        _character = character;
        _characterController = character.GetComponent<PrototypeCharacterController>();
        _initHpbarSize = _hpBarTr.sizeDelta;
        _initStaminabarSize = _staminaBarTr.sizeDelta;
        _quickSlotInventory = _characterController.QuickSlotInventory;
        _quickSlotInventory.ItemChanged += RefreshQuickSlot;
        _characterController.QuickSlotIndexChanged += RefreshQuickSlotIndex;

        RefreshQuickSlot();
    }

    public void DisconnectCharacter()
    {
        if(_quickSlotInventory != null)
        {
            _quickSlotInventory.ItemChanged -= RefreshQuickSlot;
        }
        if (_characterController)
        {
            _characterController.QuickSlotIndexChanged -= RefreshQuickSlotIndex;
        }

        _character = null;
        _characterController = null;
        _quickSlotInventory = null;
    }

    public void Update()
    {
        ShowHp();
        ShowStamina();
        ShowWorkingProcess();
    }

    void ShowWorkingProcess()
    {
        if (_characterController)
        {
            if (_characterController.IsWorking)
            {
                float? remainTime = _characterController.WorkingTimer.RemainingTime(_characterController.Runner);
                if (remainTime.HasValue)
                {
                    _workingProcessImage.transform.parent.gameObject.SetActive(true);
                    float ratio = remainTime.Value/ _characterController.WorkingTime;

                    Debug.Log(_characterController.WorkingTime + " " + remainTime.Value);
                    _workingProcessImage.fillAmount = ratio;
                    return;
                }
            }

        }
        _workingProcessImage.transform.parent.gameObject.SetActive(false);
    }

    void ShowHp()
    {
        if (_character == null) return;

        float ratio = (float)_character.Hp / _character.MaxHp;

        _hpBarTr.sizeDelta = new Vector2(_initHpbarSize.x * ratio, _initHpbarSize.y);
        _hpTextMesh.text = _character.Hp.ToString();
    }
    void ShowStamina()
    {
        if (_character == null) return;

        float ratio = (float)_character.Stamina / _character.MaxStamina;

        _staminaBarTr.sizeDelta = new Vector2(_initStaminabarSize.x * ratio, _initStaminabarSize.y);
        _staminaTextMesh.text = _character.Stamina.ToString();
    }
    void RefreshQuickSlot()
    {
        for(int i = 0; i < _quickSlotInventory.SlotCount; i++)
        {
            ItemSlot itemSlot = _quickSlotInventory.GetSlot(i);
            _quickSlotList[i].itemTextmesh.text = itemSlot.itemName.ToString();

       
        }
    }

    void RefreshQuickSlotIndex()
    {
        for (int i = 0; i < _quickSlotInventory.SlotCount; i++)
        {
            if (_characterController.QuickSlotSelectIndex == i)
            {
                _quickSlotList[i].itemImage.color = Color.green;
            }
            else
            {
                _quickSlotList[i].itemImage.color = Color.white;
            }
        }
    }
    public void OnItemSlotPointerDrag(GameObject itemImage)
    {
        for (int i = 0; i < _quickSlotList.Count; i++)
        {
            if (_quickSlotList[i].itemImage.gameObject == itemImage)
            {
                ItemSlot slot = _quickSlotInventory.GetSlot(i);
                if (slot.itemName == "") return;

                UIManager.Instance.DragItemSlotIndex = i;
                UIManager.Instance.StartedDragInventory = _quickSlotInventory;
                UIManager.Instance.DragIItem(_quickSlotList[i].itemImage.sprite);
                return;
            }
        }
    }
    public void OnItemSlotPointerDrop(GameObject itemImage)
    {
        Inventory startedDragInventory = UIManager.Instance.StartedDragInventory;
        int dragItemSlotIndex = UIManager.Instance.DragItemSlotIndex;

        if (startedDragInventory == null || dragItemSlotIndex < 0) return;

        for (int i = 0; i < _quickSlotList.Count; i++)
        {
            if (itemImage.gameObject != _quickSlotList[i].itemImage.gameObject) continue;

            // 아이템 이동
            if (_quickSlotInventory != startedDragInventory || i != dragItemSlotIndex)
            {
                var data = startedDragInventory.AccumulateInputData;
                data.isExchangeItem = true;
                data.myInventoryObjectId = startedDragInventory.Object.Id;
                data.myInventoryId = startedDragInventory.Id;
                data.myInventoryIndex = dragItemSlotIndex;

                data.encounterInventoryObjectId = _quickSlotInventory.Object.Id;
                data.encounterInventoryId = _quickSlotInventory.Id;
                data.encounterInventoryIndex = i;

                _quickSlotInventory.AccumulateInputData = data;
            }
        }
    }
}
