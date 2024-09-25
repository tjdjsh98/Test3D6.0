using Fusion;
using Fusion.Addons.KCC;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-6)]
public class PrototypeCharacterController : NetworkBehaviour, IBeforeTick
{
    // Input
    NetworkButtons _previousButtons;
    PlayerInputData _currentPlayerInputData;
    PlayerInputData _previousPlayerInputData;

    InventoryInputData _currentInventoryInputData;

    public WorkingInputData AccumulateWorkingInputData { get; set; }
    WorkingInputData _currentWorkingInputData;

    // 행동제어
    [Networked] public bool IsEnableMove { get; set; } = true;
    [Networked] public bool IsEnableRotate { get; set; } = true;


    // Component
    PrototypeCharacter _character;
    PlayerInputHandler _playerInputHandler;
    Camera _camera;


    // 퀵슬롯
    [field: SerializeField] public Inventory QuickSlotInventory { get; set; }

    [Networked] public int QuickSlotSelectIndex { get; set; } = -1;
    [Networked, OnChangedRender(nameof(OnQuickSlotChanged))] NetworkBool QuickSlotChanaged { get; set; } = false;
    public Action QuickSlotIndexChanged { get; set; }

    // 밧줄타고 올라가기
    Rope _holdRope;
    [Networked][field: SerializeField] public NetworkBool IsHoldRope { get; set; }

    // Working
    [Networked] public NetworkBool IsWorking { get; set; }
    public int CancelWorkingFrame { get; set; }             // 취소하자마자 다시 상호작용하는 것을 방지
    public float WorkingTime { get; set; }
    [Networked]public TickTimer WorkingTimer { get; set; }
    WorkingBlock _workingBlock;



    // 들고 다닐 수 있는 아이템 위치
    [SerializeField] Transform _smallItemPos;
    [SerializeField] Transform _largeItemPos;
    int _leftItemSlotIndex;
    Item _leftItem;


    [field: SerializeField] public Inventory Inventory { get; set; }

    void Awake()
    {
        _character = GetComponent<PrototypeCharacter>();
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _camera = Camera.main;
        QuickSlotInventory.ItemChanged += OnQuickSlotItemChanged;
    }

    void OnQuickSlotItemChanged()
    {
        QuickSlotChanaged = !QuickSlotChanaged;
    }
    public override void Spawned()
    {
        // 자신의 캐릭터 UI 연결
        if (HasInputAuthority)
        {
            UIManager.Instance.GetUI<UICharacter>().ConnectCharacter(_character);
            InputManager.Instance.BeforeInputDataSent += OnBeforeInputDataSent;
            InputManager.Instance.InputDataReset += OnInputDataReset;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // 자신의 캐릭터 UI 연결
        if (HasInputAuthority)
        {
            UIManager.Instance.GetUI<UICharacter>().ConnectCharacter(null);
            InputManager.Instance.BeforeInputDataSent -= OnBeforeInputDataSent;
            InputManager.Instance.InputDataReset -= OnInputDataReset;
        }
    }

    void OnBeforeInputDataSent()
    {
        InputManager.Instance.InsertWorkingInputData(AccumulateWorkingInputData);
    }

    void OnInputDataReset()
    {
        AccumulateWorkingInputData = default;
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

        if (_leftItem != null)
        {
            // 들고 있는 아이템이 퀵슬롯 자리에 없거나 다른 아이템이라면
            NetworkObject networkObj = Runner.FindObject(QuickSlotInventory.GetSlot(_leftItemSlotIndex).itemId);
            if (networkObj == null || networkObj.gameObject != _leftItem.gameObject)
            {
                _leftItem.IsUseRigidbody = true;
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
        Debug.Log(slot.itemId + " " + networkObject);
        if (networkObject != null)
        {
            networkObject.transform.SetParent(_largeItemPos, false);
            networkObject.transform.position = _largeItemPos.transform.position;
            _leftItem = networkObject.gameObject.GetComponent<Item>();
            _leftItem.IsHide = false;
            _leftItem.IsUseRigidbody = false;
            _leftItemSlotIndex = QuickSlotSelectIndex;
        }
        QuickSlotIndexChanged?.Invoke();
    }
    public override void Render()
    {
        if (HasStateAuthority)
        {
            if (_character.Stamina <= 0)
            {
                ReleaseRope();
            }
            if (IsHoldRope)
            {
                _character.Stamina -= Time.deltaTime * 3;
            }
            if (IsWorking && Input.GetKeyDown(KeyCode.E))
            {
                var data = AccumulateWorkingInputData;
                data.isCancelWorking = true;
                AccumulateWorkingInputData = data;
            }
        }
    }
    public override void FixedUpdateNetwork()
    {
        InventoryFixedUpdate(_currentInventoryInputData);

        Vector3 forward = _currentPlayerInputData.aimForwardVector;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 moveDirection = forward * _currentPlayerInputData.movementInput.y +
                right * _currentPlayerInputData.movementInput.x;

        // HoldRope
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.HoldRope))
        {
            if (!IsHoldRope)
                HoldRope(_currentPlayerInputData.holdRopeID);
            else
                ReleaseRope();
        }

        // ReleaseRope
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.ReleaseRope))
        {
            ReleaseRope();
        }
        float speed = 2;
        // Move
        if (!IsHoldRope)
        {

            // Jump
            if (_character.IsGrounded && _currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Jump))
            {
                _character.Jump(Vector3.up, 10);
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
        }
        // RopeMove
        else
        {
            if (_currentPlayerInputData.movementInput.y > 0)
            {
                moveDirection = (_holdRope.StartPos.position - _holdRope.EndPos.position).normalized;
            }
            else if (_currentPlayerInputData.movementInput.y < 0)
            {
                moveDirection = (_holdRope.EndPos.position - _holdRope.StartPos.position).normalized;
            }
            else
            {
                moveDirection = Vector3.zero;
            }
        }

        // ResultMove
        if (IsEnableMove)
            _character.Move(moveDirection * speed);

        // Rotate
        if (IsEnableRotate)
            _character.AddLookAngle(_currentPlayerInputData.lookRotationDelta.y);

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

        // Working

        StartWorking();
        Working();
        CancelWorking();

        _previousButtons = _currentPlayerInputData.buttons;

        if (_leftItem)
        {
            _leftItem.transform.localRotation = Quaternion.identity;
            _leftItem.transform.localPosition = Vector3.zero;
        }
    }

    void StartWorking()
    {
        if (_currentWorkingInputData.isWorking)
        {
            NetworkObject networkObject = Runner.FindObject(_currentWorkingInputData.workingTargetID);
            _workingBlock = networkObject.GetComponent<WorkingBlock>();
            WorkingTime = _workingBlock.RequireTime;
            WorkingTimer = TickTimer.CreateFromSeconds(Runner, WorkingTime);
            IsWorking = true;
            IsEnableRotate = false;
            IsEnableMove = false;
        }
    }

    void CancelWorking()
    {
        if (IsWorking)
        {
            if (_currentWorkingInputData.isCancelWorking)
            {
                IsWorking = false;
                IsEnableRotate = true;
                IsEnableMove = true;
                CancelWorkingFrame = Time.frameCount;
            }
        }
    }

    void Working()
    {
        if (IsWorking)
        {
            if (WorkingTimer.Expired(Runner))
            {
                if (_workingBlock)
                {
                    Debug.Log(_workingBlock.name + " " + _workingBlock.transform.position);
                    Runner.Spawn(_workingBlock.SpawnObject, _workingBlock.transform.position);
                    Runner.Despawn(_workingBlock.Object);
                }
                IsWorking = false;
                IsEnableRotate = true;
                IsEnableMove = true;
                CancelWorkingFrame = Time.frameCount;
            }
        }
    }

    void HoldRope(NetworkId ropeId)
    {
        NetworkObject networkObject = Runner.FindObject(ropeId);
        if (networkObject != null)
        {
            _holdRope = networkObject.GetComponentInParent<Rope>();
            if (_holdRope != null)
            {
                IsHoldRope = true;
                _character.IsEnableMoveYAxis = true;

                Vector3 lVec = _holdRope.EndPos.position - _holdRope.StartPos.position;
                Vector3 vec = transform.position - _holdRope.StartPos.position;
                float t = Vector3.Dot(lVec, vec) / Vector3.Dot(lVec, lVec);
                t = Mathf.Clamp01(t);
                _character.Teleport(_holdRope.StartPos.position + lVec * t);
            }
        }
    }

    void ReleaseRope()
    {
        IsHoldRope = false;
        _character.IsEnableMoveYAxis = false;
        _holdRope = null;
    }

    // 메인 클라이언트만 판정해줍니다.
    void InventoryFixedUpdate(InventoryInputData data)
    {
        if (!HasStateAuthority) return;

        if (data.isDropItem)
        {
            var inventorys = Runner.FindObject(data.myInventoryObjectId).GetComponentsInChildren<Inventory>();
            foreach (var inventory in inventorys)
            {
                if (inventory.Id == data.myInventoryId)
                {
                    inventory.DropItem(data.myInventoryIndex);
                    QuickSlotChanaged = !QuickSlotChanaged;
                }
            }
        }
        if (data.isAddItem)
        {
            NetworkObject networkObject = Runner.FindObject(data.addItemID);
            Item item = networkObject.GetComponent<Item>();

            // 상호작용이 가능한지 최종적으로 메인클라이언트가 확인한다.
            if (!item.IsInteractable) return;
            if (networkObject != null)
            {
                Inventory.InsertItem(networkObject.gameObject);
            }
        }
        if (data.isExchangeItem)
        {
            var inventorys = Runner.FindObject(data.myInventoryObjectId).GetComponentsInChildren<Inventory>();
            Inventory myInventory = null;
            foreach (var inventory in inventorys)
            {
                if (inventory.Id == data.myInventoryId)
                {
                    myInventory = inventory;
                    break;
                }
            }
            if (myInventory == null) return;
            inventorys = Runner.FindObject(data.encounterInventoryObjectId).GetComponentsInChildren<Inventory>();
            Inventory encounterInventory = null;
            foreach (var inventory in inventorys)
            {
                if (inventory.Id == data.encounterInventoryId)
                {
                    encounterInventory = inventory;
                    break;
                }
            }
            if (encounterInventory == null) return;

            ItemSlot itemSlot = myInventory.GetSlot(data.myInventoryIndex);
            if (itemSlot.itemName == "") return;


            if (myInventory.RemoveItem(data.myInventoryIndex))
            {
                encounterInventory.SetSlot(itemSlot, data.encounterInventoryIndex);
            }

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
            inventoryInputData.isExchangeItem = default;
            _currentInventoryInputData = inventoryInputData;
        }
        {
            WorkingInputData workingInputData = _currentWorkingInputData;
            workingInputData.isWorking = default;
            _currentWorkingInputData = workingInputData;
        }


        if (Object.InputAuthority != PlayerRef.None)
        {
            if (GetInput(out NetworkInputData input) == true)
            {
                // New input received, we can store it as current.
                _currentPlayerInputData = input.playerInputData;
                _currentInventoryInputData = input.inventoryInputData;
                _currentWorkingInputData = input.workingInputData;
            }
        }
    }
    void LateUpdate()
    {
        if (!HasInputAuthority) return;


    }
}
