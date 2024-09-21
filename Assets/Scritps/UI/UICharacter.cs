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

    // QuickSlot
    Inventory _quickSlotInventory;
    [SerializeField] Transform _quickSlotParent;
    List<UIInventorySlot> _quickSlotList = new List<UIInventorySlot>();

    public override void Init() 
    { 
        for(int i = 0; i <  _quickSlotParent.childCount; i++)
        {
            UIInventorySlot slot = new UIInventorySlot();
            slot.parent = _quickSlotParent.GetChild(i).gameObject;
            slot.itemTextmesh = slot.parent.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _quickSlotList.Add(slot);
        }
    }

    private void FixedUpdate()
    {
        
    }
    public void ConnectCharacter(PrototypeCharacter character)
    {
        _character = character;
        _characterController = character.GetComponent<PrototypeCharacterController>();
        _initHpbarSize = _hpBarTr.sizeDelta;
        _quickSlotInventory = _characterController.QuickSlotInventory;
        _quickSlotInventory.ItemChanged += RefreshQuickSlot;

        RefreshQuickSlot();
    }

    public void DisconnectCharacter()
    {
        if(_quickSlotInventory != null)
        {
            _quickSlotInventory.ItemChanged -= RefreshQuickSlot;
        }

        _character = null;
        _characterController = null;
        _quickSlotInventory = null;
    }

    public void Update()
    {
        ShowHp();
    }

    void ShowHp()
    {
        if (_character == null) return;

        float ratio = (float)_character.Hp / _character.MaxHp;

        _hpBarTr.sizeDelta = new Vector2(_initHpbarSize.x * ratio, _initHpbarSize.y);
        _hpTextMesh.text = _character.Hp.ToString();
    }

    void RefreshQuickSlot()
    {
        Debug.Log(_quickSlotList.Count);
        for(int i = 0; i < _quickSlotInventory.SlotCount; i++)
        {
            Debug.Log(i);
            ItemSlot itemSlot = _quickSlotInventory.GetSlot(i);
            _quickSlotList[i].itemTextmesh.text = itemSlot.itemName.ToString();
        }
    }

}
