using Fusion;
using System;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-6)]
public class PrototypeCharacterController : NetworkBehaviour, IBeforeTick
{
    // Input
    NetworkButtons _previousButtons;
    PlayerInputData _currentPlayerInputData;
    PlayerInputData _previousPlayerInputData;

    InventoryInputData _currentInventoryInputData;
    InventoryInputData _currentInventoryInputData2;
    InventoryInputData _currentInventoryInputData3;
    InventoryInputData _currentInventoryInputData4;
    InventoryInputData _currentInventoryInputData5;

    // Component
    PrototypeCharacter _character;
    Camera _camera;


    // 퀵슬롯
    [field:SerializeField] public Inventory QuickSlotInventory { get; set; }

    [Networked] public int QuickSlotSelectIndex { get; set; } = -1;
    [Networked, OnChangedRender(nameof(OnQuickSlotChanged))] NetworkBool QuickSlotChanaged { get; set; } = false;
    public Action QuickSlotIndexChanged { get; set; }

    // 들고 다닐 수 있는 아이템 위치
    [SerializeField] Transform _smallItemPos;
    [SerializeField] Transform _largeItemPos;
    int _leftItemSlotIndex;
    Item _leftItem;


    [field: SerializeField]public Inventory Inventory { get; set; }

    void Awake()
    {
        _character = GetComponent<PrototypeCharacter>();
        _camera = Camera.main;
    }


    public override void Spawned()
    {
        // 자신의 캐릭터 UI 연결
        if (HasInputAuthority)
        {
            UIManager.Instance.GetUI<UICharacter>().ConnectCharacter(_character);
        }
    }

    public void Update()
    {
        if (HasInputAuthority)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                UIInventory ui = UIManager.Instance.GetUI<UIInventory>();
                if (ui.gameObject.activeSelf)
                {
                    ui.Close();
                }
                else
                {
                    ui.ConnectInventory(Inventory);
                    ui.Open();
                }
            }
        }
  
    }

    void OnQuickSlotChanged()
    {
        ItemSlot slot = QuickSlotInventory.GetSlot(QuickSlotSelectIndex);

        if(_leftItem != null)
        {
            // 들고 있는 아이템이 퀵슬롯 자리에 없거나 다른 아이템이라면
             NetworkObject networkObj = Runner.FindObject(QuickSlotInventory.GetSlot(_leftItemSlotIndex).itemId);
            if (networkObj == null || networkObj.gameObject != _leftItem.gameObject)
            {
                _leftItem.transform.SetParent(null);
                _leftItem = null;
            }
            // 퀵슬롯의 위치만 바뀌었다면
            else
            {
                 
                _leftItem.IsHide = true;
                // 선제적으로 감춰져야지 블록이 순간이동한 것 처럼 보이지 않음.
                _leftItem.Show(!_leftItem.IsHide);
                _leftItem.transform.SetParent(null);
                _leftItem = null;
            }
        }

        NetworkObject networkObject = Runner.FindObject(slot.itemId);
        if (networkObject != null)
        {
            networkObject.transform.SetParent(_largeItemPos,false);
            networkObject.transform.position = _largeItemPos.transform.position;
            _leftItem = networkObject.gameObject.GetComponent<Item>();
            _leftItem.IsHide = false;
            _leftItemSlotIndex = QuickSlotSelectIndex;
        }
        QuickSlotIndexChanged?.Invoke();
    }


    public override void FixedUpdateNetwork()
    {
        InventoryFixedUpdate(_currentInventoryInputData);
        InventoryFixedUpdate(_currentInventoryInputData2);
        InventoryFixedUpdate(_currentInventoryInputData3);
        InventoryFixedUpdate(_currentInventoryInputData4);
        InventoryFixedUpdate(_currentInventoryInputData5);

        Vector3 forward = _currentPlayerInputData.aimForwardVector;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 moveDirection = forward * _currentPlayerInputData.movementInput.y +
                right * _currentPlayerInputData.movementInput.x;


        // Move
        _character.AddLookAngle(_currentPlayerInputData.lookRotationDelta.y);
        float speed = 2;
        if(_character.IsGrounded && _currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Jump))
        {
            _character.Jump(5,Vector3.up);
        }
        if (_currentPlayerInputData.buttons.IsSet(InputButton.Run))
        {
            _character.IsUseStamina = true;
            speed = 8;
        }
        else
        {
            _character.IsUseStamina = false;
        }
        _character.Move(moveDirection * speed);


        //QuickSlots
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Num1))
        {
            QuickSlotSelectIndex = QuickSlotSelectIndex == 0 ? -1 : 0;
            QuickSlotChanaged = !QuickSlotChanaged;
        }
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Num2))
        {
            QuickSlotSelectIndex = QuickSlotSelectIndex == 1 ? -1 : 1;
            QuickSlotChanaged = !QuickSlotChanaged;
        }
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Num3))
        { 
            QuickSlotSelectIndex = QuickSlotSelectIndex == 2 ? -1 : 2;
            QuickSlotChanaged = !QuickSlotChanaged;
        }
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Num4))
        {
            QuickSlotSelectIndex = QuickSlotSelectIndex == 3 ? -1 : 3;
            QuickSlotChanaged = !QuickSlotChanaged;
        }

        // Throw
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Throw))
        {
            if (QuickSlotSelectIndex != -1)
            {
                QuickSlotInventory.DropItem(QuickSlotSelectIndex);
                QuickSlotChanaged = !QuickSlotChanaged;
            }
        }

        _previousButtons = _currentPlayerInputData.buttons;

        if (_leftItem)
            _leftItem.transform.localPosition = Vector3.zero;
    }

    void InventoryFixedUpdate(InventoryInputData data)
    {
        if (data.isDropItem)
        {
            var inventorys = Runner.FindObject(data.ObjectId).GetComponentsInChildren<Inventory>();
            foreach ( var inventory in inventorys )
            {
                if (inventory.Id == data.inventoryId)
                {
                    inventory.DropItem(data.myInventoryIndex);
                    QuickSlotChanaged = !QuickSlotChanaged;
                }
            }
        }
        if (data.isAddItem)
        {
            QuickSlotInventory.InsertItem(Runner.FindObject(data.addItemID).gameObject);
            QuickSlotChanaged = !QuickSlotChanaged;
        }
    }

    public void BeforeTick()
    {
        _previousPlayerInputData = _currentPlayerInputData; 
        PlayerInputData inputData = _currentPlayerInputData;
        inputData.moveDelta = default;
        inputData.lookRotationDelta = default;
        _currentPlayerInputData = inputData;
        {
            InventoryInputData inventoryInputData = _currentInventoryInputData;
            inventoryInputData.isAddItem = default;
            inventoryInputData.isDropItem = default;
            _currentInventoryInputData = inventoryInputData;
        }
        {
            InventoryInputData inventoryInputData = _currentInventoryInputData2;
            inventoryInputData.isAddItem = default;
            inventoryInputData.isDropItem = default;
            _currentInventoryInputData2 = inventoryInputData;
        }
        {
            InventoryInputData inventoryInputData = _currentInventoryInputData3;
            inventoryInputData.isAddItem = default;
            inventoryInputData.isDropItem = default;
            _currentInventoryInputData3 = inventoryInputData;
        }
        {
            InventoryInputData inventoryInputData = _currentInventoryInputData4;
            inventoryInputData.isAddItem = default;
            inventoryInputData.isDropItem = default;
            _currentInventoryInputData4 = inventoryInputData;
        }
         {
            InventoryInputData inventoryInputData = _currentInventoryInputData5;
            inventoryInputData.isAddItem = default;
            inventoryInputData.isDropItem = default;
            _currentInventoryInputData5 = inventoryInputData;
        }


        if (Object.InputAuthority != PlayerRef.None)
        {
            if (GetInput(out NetworkInputData input) == true)
            {
                // New input received, we can store it as current.
                _currentPlayerInputData = input.playerInputData;
                _currentInventoryInputData = input.inventoryInputData;
                _currentInventoryInputData2 = input.inventoryInputData2;
                _currentInventoryInputData3 = input.inventoryInputData3;
                _currentInventoryInputData4 = input.inventoryInputData4;
                _currentInventoryInputData5 = input.inventoryInputData5;
            }
        }
    }

    void LateUpdate()
    {
        if (!HasInputAuthority) return;


    }
}
