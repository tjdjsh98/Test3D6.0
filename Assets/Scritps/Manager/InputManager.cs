using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class InputManager : SimulationBehaviour, IBeforeUpdate
{
    static InputManager _instance;
    public static InputManager Instance
    { get { InitSingleton(); return _instance; } }
    NetworkInputData accumulatedInput;
    bool isReset;

    // Components
    CinemachineCamera _camera;
    CinemachineInputAxisController _cameraController;


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

    void Awake()
    {
        InitSingleton();
    }

    private void Update()
    {
        GetInputData();

        if (IsEnableFocus)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                Cursor.lockState= CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState= CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else if(Cursor.lockState== CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if(_cameraController.enabled == true && !IsEnableInput)
        {
            _cameraController.enabled = false;
        }
        if (_cameraController.enabled == false && IsEnableInput)
        {
            _cameraController.enabled = true;
        }

    }

    void GetInputData()
    {
        if (isReset)
            accumulatedInput = default;

        if (!IsEnableInput) return;

        // View Input

        // Move Input
        accumulatedInput.movementInput += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        NetworkButtons buttons = default;

        // Jump
        if (Input.GetButtonDown("Jump"))
            buttons.Set(InputButton.Jump, true);

        // Fire
        if (Input.GetButtonDown("Fire1"))
            buttons.Set(InputButton.MouseButton0, true);

        // Interact
        if (Input.GetKey(KeyCode.LeftShift))
            buttons.Set(InputButton.Run, true);

        // Interact
        if (Input.GetKeyDown(KeyCode.E))
            buttons.Set(InputButton.Interact, true);

        // aimForward
        accumulatedInput.aimForwardVector = _camera.transform.forward;
        accumulatedInput.aimForwardVector.y = 0;
        accumulatedInput.aimForwardVector.Normalize();

        accumulatedInput.buttons = new NetworkButtons(accumulatedInput.buttons.Bits | buttons.Bits);
    }

    public void BeforeUpdate()
    {
        GetInputData();
    }

    public NetworkInputData GetNetworkInput()
    {
        isReset = true;
        return accumulatedInput;
    }
}
