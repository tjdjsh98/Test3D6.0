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
    CinemachineCamera _camera;
    CinemachineInputAxisController _cameraController;
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
        _camera = FindAnyObjectByType<GameManager>().ThirdPersonCamera;
        _cameraController = _camera.GetComponent<CinemachineInputAxisController>();

    }

    public override void Spawned()
    {
        FindAnyObjectByType<NetworkEvents>().OnInput.AddListener(OnPlayerInput);

    }

    void Awake()
    {
        InitSingleton();
    }

    public void Update()
    {
        if (IsEnableFocus)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (_cameraController.enabled == true && !IsEnableInput)
        {
            _cameraController.enabled = false;
        }
        if (_cameraController.enabled == false && IsEnableInput)
        {
            _cameraController.enabled = true;
        }
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
}
