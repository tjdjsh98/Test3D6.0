using Fusion;
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

    [field:SerializeField] public Inventory QuickSlotInventory { get; set; }
    public int QuickSlotSelectIndex { get; set; } = -1;


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
            if(Input.GetKeyDown(KeyCode.I))
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

            if(Input.GetKeyDown(KeyCode.Alpha1))
                QuickSlotSelectIndex = QuickSlotSelectIndex == 0 ? -1 : 0;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                QuickSlotSelectIndex = QuickSlotSelectIndex == 1 ? -1 : 1;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                QuickSlotSelectIndex = QuickSlotSelectIndex == 2 ? -1 : 2;
            if (Input.GetKeyDown(KeyCode.Alpha4))
                QuickSlotSelectIndex = QuickSlotSelectIndex == 3 ? -1 : 3;
        }
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

        _character.Move(moveDirection);
        _character.AddLookAngle(_currentPlayerInputData.lookRotationDelta.y);

        if(_character.IsGrounded && _currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Jump))
        {
            _character.Jump(5);
        }

        _previousButtons = _currentPlayerInputData.buttons;
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
                }
            }
        }
        if (data.isAddItem)
        {
            QuickSlotInventory.InsertItem(Runner.FindObject(data.addItemID).gameObject);
        }
    }

    public void BeforeTick()
    {
        _previousPlayerInputData = _currentPlayerInputData; 
        PlayerInputData inputData = _currentPlayerInputData;
        inputData.moveDelta = default;
        inputData.lookRotationDelta = default;

        

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
