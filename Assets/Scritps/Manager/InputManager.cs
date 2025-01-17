using Fusion;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class InputManager : NetworkBehaviour
{
    static InputManager _instance;
    public static InputManager Instance
    { get { InitSingleton(); return _instance; } }
    NetworkInputData _accumulatedInput;

    // Components
    int _lastFrame;

    public Action BeforeInputDataSent;
    public Action InputDataReset;


    // State
    [field: SerializeField] public bool IsEnableFocus { get; set; } = true;
    [field: SerializeField] public bool IsEnableInput { get; set; } = true;

    static void InitSingleton()
    {
        if (_instance != null) return;

        _instance = FindAnyObjectByType<InputManager>();
        _instance.Init();
    }

    void Init()
    {

    }

    public override void Spawned()
    {
        FindAnyObjectByType<NetworkEvents>().OnInput.AddListener(OnPlayerInput);

    }

    void Awake()
    {
        InitSingleton();
    }

    public void OnPlayerInput(NetworkRunner runner, NetworkInput input)
    {
        if (_lastFrame == Time.frameCount) return;
        _lastFrame = Time.frameCount;

        BeforeInputDataSent?.Invoke();

        input.Set(_accumulatedInput);

        InputDataReset?.Invoke();
        _accumulatedInput = default;
    }

    public void InsertPlayerInputData(PlayerInputData data)
    {
        _accumulatedInput.playerInputData = data;
    }
    public void InsertInventoryInputData(InventoryInputData data)
    {
        _accumulatedInput.inventoryInputData = data;
    }
    public void InsertInteractInputData(InteractInputData data)
    {
        _accumulatedInput.interactInputData = data;
    }
    public void InsertWorkingInputData(WorkingInputData data)
    {
        _accumulatedInput.workingInputData = data;
    }
    public void InsertReceiptInputData(ReceiptInputData data)
    {
        _accumulatedInput.receiptInputData= data;
    }
}
