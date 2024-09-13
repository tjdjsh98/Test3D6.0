using Fusion;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class InputManager : NetworkBehaviour
{
    static InputManager _instance;
    public static InputManager Instance
    { get { InitSingleton(); return _instance; } }
    PlayerInputData _accumulatedInput;

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

    public override void Spawned()
    {
        Runner.GetComponent<NetworkEvents>().OnInput    .AddListener(OnInput);
    }

    public override void FixedUpdateNetwork()
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

    void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (!IsEnableInput) return;

        // View Input

        // Move Input
        _accumulatedInput.movementInput += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

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
        _accumulatedInput.aimForwardVector = _camera.transform.forward;
        _accumulatedInput.aimForwardVector.y = 0;
        _accumulatedInput.aimForwardVector.Normalize();

        _accumulatedInput.buttons = new NetworkButtons(_accumulatedInput.buttons.Bits | buttons.Bits);

        input.Set(_accumulatedInput);

        _accumulatedInput = default;
    }
}
