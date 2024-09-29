using Fusion;
using Fusion.Addons.KCC;
using NUnit.Framework.Api;
using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-6)]
public class PrototypeCharacterController : NetworkBehaviour, IBeforeTick
{
    // Component
    protected PrototypeCharacter _character;
    protected PlayerInputHandler _playerInputHandler;
    protected Camera _camera;
    protected GameObject _model;

    // Input
    bool _receiveNewData = false;
    protected NetworkButtons _previousButtons;
    protected PlayerInputData _currentPlayerInputData;
    protected PlayerInputData _previousPlayerInputData;

    protected InventoryInputData _currentInventoryInputData;

    public WorkingInputData AccumulateWorkingInputData { get; set; }
    protected WorkingInputData _currentWorkingInputData;

    public ReceiptInputData AccumulateReceiptInputData { get; set; }
    protected ReceiptInputData _currentReceiptInputData;

    // Move
    protected Vector3 _moveDirection;

    // 행동제어
    public bool IsEnableInputMove { get; set; } = true;
    public bool IsEnableInputRotate { get; set; } = true;

    // 배고픔 수치
    [Networked] public float HungryPoint { get; set; } = 100;   
    [Networked] public float MaxHungryPoint { get; set; } = 100;
    float _hungryZeroElased = 0;

    // 퀵슬롯
    [field: SerializeField] public Inventory QuickSlotInventory { get; set; }

    [Networked] public int QuickSlotSelectIndex { get; set; } = -1;
    [Networked, OnChangedRender(nameof(OnQuickSlotChanged))] protected NetworkBool QuickSlotChanaged { get; set; } = false;
    public Action QuickSlotIndexChanged { get; set; }

    // 밧줄타고 올라가기
    protected Rope _holdRope;
    [Networked][field: SerializeField] public NetworkBool IsHoldRope { get; set; }

    // Working
    [Networked] public NetworkBool IsWorking { get; set; }
    public int CancelWorkingFrame { get; set; }             // 취소하자마자 다시 상호작용하는 것을 방지
    public float WorkingTime { get; set; }
    [Networked]public TickTimer WorkingTimer { get; set; }
    WorkingBlock _workingBlock;

    // 아이템
    // 빌드
    protected NetworkObject _builing;
    protected GameObject _buildingModel;
    protected Collider _buildingModelCollider;
    public Vector3 BuildPoint { get; set; }

    // 들고 다닐 수 있는 아이템 위치
    [SerializeField] Transform _smallItemPos;
    [SerializeField] Transform _largeItemPos;
    int _leftItemSlotIndex;
    Item _leftItem;


    [field: SerializeField] public Inventory Inventory { get; set; }

    protected virtual void Awake()
    {
        _character = GetComponent<PrototypeCharacter>();
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _model = transform.Find("Model").gameObject;
        _camera = Camera.main;
    }
    private void OnEnable()
    {
        QuickSlotInventory.ItemChanged += OnQuickSlotInventoryChanged;
    }

    private void OnDisable()
    {
        QuickSlotInventory.ItemChanged -= OnQuickSlotInventoryChanged;
    }
    void OnQuickSlotInventoryChanged()
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
            UIManager.Instance.GetUI<UICharacter>().DisconnectCharacter(_character);
            InputManager.Instance.BeforeInputDataSent -= OnBeforeInputDataSent;
            InputManager.Instance.InputDataReset -= OnInputDataReset;
        }
    }

    void OnBeforeInputDataSent()
    {
        InputManager.Instance.InsertWorkingInputData(AccumulateWorkingInputData);
        InputManager.Instance.InsertReceiptInputData(AccumulateReceiptInputData);
    }
    void OnInputDataReset()
    {
        AccumulateWorkingInputData = default;
        AccumulateReceiptInputData = default;
    }
    public void BeforeTick()
    {
        ResetOnceData();

        if (Object.InputAuthority != PlayerRef.None)
        {
            if (GetInput(out NetworkInputData input) == true)
            {
                // New input received, we can store it as current.
                _currentPlayerInputData = input.playerInputData;
                _currentInventoryInputData = input.inventoryInputData;
                _currentWorkingInputData = input.workingInputData;
                _currentReceiptInputData = input.receiptInputData;
                _receiveNewData= true;
            }
        }
    }

    // 한 번만 적용하는 데이터만 제거해준다.
    void ResetOnceData()
    {
        if (_receiveNewData == false) return;
        _receiveNewData = false;
        _previousPlayerInputData = _currentPlayerInputData;
        {
            PlayerInputData inputData = _currentPlayerInputData;
            inputData.moveDelta = default;
            inputData.lookRotationDelta = default;
            inputData.animatorDeltaAngle = default;
            _currentPlayerInputData = inputData;
        }
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
        {
            ReceiptInputData receiptData = _currentReceiptInputData;
            receiptData.isReceipt = default;
            _currentReceiptInputData = receiptData;
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
            // Test
            if (Input.GetKeyDown(KeyCode.P))
            {
                ReceiptInputData data = AccumulateReceiptInputData;
                data.isReceipt = true;
                data.receptName.Set("Crate");
                AccumulateReceiptInputData = data;
            }
            PreviewBuilding();
        }

    }
    public override void Render()
    {
        if (HasStateAuthority)
        {
            if((float)HungryPoint/ MaxHungryPoint < 0.3f)
            {
                _character.StaminaRecoverMultifly = 0.5f;
            }
            else
            {
                _character.StaminaRecoverMultifly = 1;
            }

            if (HungryPoint > 0)
            {
                HungryPoint -= Runner.DeltaTime;
            }
            else
            {
                _hungryZeroElased += Runner.DeltaTime;

                if(_hungryZeroElased > 3f)
                {
                    DamageInfo info = new DamageInfo();
                    info.attacker = _character;
                    info.target = _character;
                    info.damage = 1;
                    info.knockbackPower = 0;
                    _character.Damage(info);
                    _hungryZeroElased = 0;
                }
            }
        }
        if (HasStateAuthority)
        {
            if (_character.Stamina <= 0)
            {
                ReleaseRope();
            }
            if (IsHoldRope)
            {
                _character.Stamina -= Time.deltaTime ;
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
        ProcessInputData();

        _previousButtons = _currentPlayerInputData.buttons;
    }
    void OnQuickSlotChanged()
    {
        ItemSlot slot = QuickSlotInventory.GetSlot(QuickSlotSelectIndex);


        _builing = null;
        if (HasInputAuthority && _buildingModel)
            Destroy(_buildingModel);

        _buildingModel = null;

        if (_leftItem != null && _leftItem.Object != null)
        {
            // 들고 있는 아이템이 퀵슬롯 자리에 없거나 다른 아이템이라면
            NetworkObject networkObj = Runner.FindObject(QuickSlotInventory.GetSlot(_leftItemSlotIndex).itemId);
            if (networkObj == null || networkObj.gameObject != _leftItem.gameObject)
            {
                Debug.Log(1);
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
        if (networkObject != null)
        {
            networkObject.transform.SetParent(_largeItemPos, false);
            networkObject.transform.position = _largeItemPos.transform.position;
            _leftItem = networkObject.gameObject.GetComponent<Item>();
            _leftItem.IsHide = false;
            _leftItem.IsUseRigidbody = false;
            _leftItemSlotIndex = QuickSlotSelectIndex;


            if (_leftItem.ItemType == ItemType.Build)
            {
                _builing = ((BuildItem)_leftItem).Building;
                Transform tr = _builing.transform.Find("Model");
                if (tr)
                {
                    // 로컬만 위치를 미리 보여주기 위해 프리뷰 모델 생성
                    if (HasInputAuthority)
                    {
                        _buildingModel = Instantiate(tr.gameObject);
                        _buildingModel.layer = 0;
                        _buildingModelCollider = _buildingModel.GetComponentInChildren<Collider>();
                        _buildingModelCollider.isTrigger = true;
                    }
                    else if (HasStateAuthority)
                    {
                        _buildingModel = tr.gameObject;
                        _buildingModelCollider = _buildingModel.GetComponentInChildren<Collider>();
                    }
                }
            }
        }
        QuickSlotIndexChanged?.Invoke();
    }

    protected virtual void ProcessInputData()
    {
        InventoryFixedUpdate(_currentInventoryInputData);
        Vector3 forward = _currentPlayerInputData.aimForwardVector;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        _moveDirection = forward * _currentPlayerInputData.movementInput.y +
                right * _currentPlayerInputData.movementInput.x;

        ProcessRope();
        ProcessMove();
        ProcessRotate();
        ProcessThrow();
        ProcessQuickSlot();
        // Working
        ProcessWorking();
        //Recipt
        ProcessReceipt();
        // UseItem
        ProcessItem();
    }

    // Move
    protected virtual void ProcessMove()
    {
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
                _moveDirection = (_holdRope.StartPos.position - _holdRope.EndPos.position).normalized;
            }
            else if (_currentPlayerInputData.movementInput.y < 0)
            {
                _moveDirection = (_holdRope.EndPos.position - _holdRope.StartPos.position).normalized;
            }
            else
            {
                _moveDirection = Vector3.zero;
            }
        }

        // ResultMove
        if (IsEnableInputMove)
            _character.Move(_moveDirection * speed);
    }

    protected virtual void ProcessRotate()
    {
        // Rotate
        if (IsEnableInputRotate)
            _character.AddLookAngle(_currentPlayerInputData.lookRotationDelta.y);
    }
    // Working
    protected virtual void ProcessWorking()
    {
        StartWorking();
        CancelWorking();
        Working();
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
            IsEnableInputRotate = false;
            IsEnableInputMove = false;
        }
    }
    void CancelWorking()
    {
        if (IsWorking)
        {
            if (_currentWorkingInputData.isCancelWorking)
            {
                IsWorking = false;
                IsEnableInputRotate = true;
                IsEnableInputMove = true;
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
                IsEnableInputRotate = true;
                IsEnableInputMove = true;
                CancelWorkingFrame = Time.frameCount;
            }
        }
    }

    // Throw
    protected virtual void ProcessThrow()
    {
        // Throw
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Throw))
        {
            if (QuickSlotSelectIndex != -1)
            {
                QuickSlotInventory.DropItem(QuickSlotSelectIndex);
                QuickSlotChanaged = !QuickSlotChanaged;
            }
        }

    }

    // QuickSlot
    protected virtual void ProcessQuickSlot()
    {
        //QuickSlots
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Num1))
        {
            Debug.Log(1);
            QuickSlotSelectIndex = QuickSlotSelectIndex == 0 ? -1 : 0;
            QuickSlotChanaged = !QuickSlotChanaged;
        }
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Num2))
        {
            Debug.Log(2);
            QuickSlotSelectIndex = QuickSlotSelectIndex == 1 ? -1 : 1;
            QuickSlotChanaged = !QuickSlotChanaged;
        }
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Num3))
        {
            Debug.Log(3);
            QuickSlotSelectIndex = QuickSlotSelectIndex == 2 ? -1 : 2;
            QuickSlotChanaged = !QuickSlotChanaged;
        }
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.Num4))
        {
            Debug.Log(4);
            QuickSlotSelectIndex = QuickSlotSelectIndex == 3 ? -1 : 3;
            QuickSlotChanaged = !QuickSlotChanaged;
        }

        

        if (_leftItem)
        {
            _leftItem.transform.localRotation = Quaternion.identity;
            _leftItem.transform.localPosition = Vector3.zero;
        }
    }

    // Rope
    protected virtual void ProcessRope()
    {
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
    }
    protected virtual void HoldRope(NetworkId ropeId)
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
    protected virtual void ReleaseRope()
    {
        IsHoldRope = false;
        _character.IsEnableMoveYAxis = false;
        _holdRope = null;
    }

    // Item
    // 아이템 사용은 서버에서 판정
    protected virtual void ProcessItem()
    {
        if (_currentPlayerInputData.buttons.WasPressed(_previousButtons, InputButton.MouseButton0))
        {
            UseItem();
        }

    }
    void UseItem()
    {
        if (!HasStateAuthority) return;

        if(QuickSlotSelectIndex == -1) return;
        ItemSlot slot = QuickSlotInventory.GetSlot(QuickSlotSelectIndex);
        if(slot.itemId == default) return;

        NetworkObject networkObject = Runner.FindObject(slot.itemId);
        if (networkObject == null) return;
        Item item = networkObject.GetComponent<Item>();
        if (item == null) return;

        if (_buildingModel)
        {
            Physics.Raycast(_currentPlayerInputData.cameraPosition, _currentPlayerInputData.aimForwardVector, out var hit, Mathf.Infinity, Define.GROUND_LAYERMASK);

            if (hit.collider != null)
            {
                BuildPoint = hit.point;

            }
        }

        if (item.UseItem(this))
        {
            if(item.IsDestroyWhenUse)
            {
                QuickSlotInventory.RemoveItem(_leftItemSlotIndex);
                Runner.Despawn(item.Object);
            }
        }
    }

    // Building
    void PreviewBuilding()
    {
        if(_buildingModel)
        {
            Physics.Raycast(_camera.transform.position, _camera.transform.forward, out var hit, Mathf.Infinity, Define.GROUND_LAYERMASK);

            if(hit.collider == null)
            {
                _buildingModel.gameObject.SetActive(false);
                return;
            }

            _buildingModel.gameObject.SetActive(true);
            _buildingModel.transform.position = hit.point;
        }
    }

    // Food
    public void EatFood(float fullness)
    {
        if(MaxHungryPoint < HungryPoint + fullness)
        {
            HungryPoint = MaxHungryPoint;
        }
        else
        {
            HungryPoint += fullness;    
        }
    }

    //Receipt
    public void ProcessReceipt()
    {
        if(_currentReceiptInputData.isReceipt)
        {
            ReceiptData data = DataManager.Instance.GetData<ReceiptData>(_currentReceiptInputData.receptName.Value);
            Debug.Log(data);
            if(data != null)
            {
                bool enable = true;
                for(int i = 0; i < data.ReceiptItemList.Count ; i++)
                {
                    string itemName = data.ReceiptItemList[i].itemName;
                    int requireCount = data.ReceiptItemList[i].requireItemCount;

                    requireCount -= Inventory.GetItemCount(itemName);
                    
                    if(requireCount > 0)
                        requireCount -= QuickSlotInventory.GetItemCount(itemName);

                    if (requireCount > 0)
                    {
                        Debug.Log(itemName);
                        enable = false;
                        break;
                    }
                }

                if(enable)
                {
                    for (int i = 0; i < data.ReceiptItemList.Count; i++)
                    {
                        string itemName = data.ReceiptItemList[i].itemName;
                        int requireCount = data.ReceiptItemList[i].requireItemCount;
                        
                        requireCount = Inventory.DestroyItem(itemName, requireCount);

                        if(requireCount >0)
                        {
                            Debug.Log(itemName + " " + requireCount);
                            QuickSlotInventory.DestroyItem(itemName, requireCount);
                        }
                    }
                    Debug.Log(data.resultItem);
                    if(!QuickSlotInventory.InsertItem(data.resultItem))
                    {
                        Debug.Log(data.resultItem);

                        Inventory.InsertItem(data.resultItem);
                    }
                }
            }

        }
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
            // 퀵슬롯에 넣을수 있는지 확인하고 안되면 인벤토리에 넣는다.
            if (!item.IsInteractable) return;
            if (networkObject != null)
            {
                if (!QuickSlotInventory.InsertItem(networkObject.gameObject))
                {
                    Inventory.InsertItem(networkObject.gameObject);
                }
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
    void LateUpdate()
    {
        if (!HasInputAuthority) return;


    }

 
}
