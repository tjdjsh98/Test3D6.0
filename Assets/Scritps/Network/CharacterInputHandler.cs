using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    NetworkInputData accumulatedInput;
    bool isReset;

    NetworkPlayerController _characterMovementHandler;
    CinemachineCamera _camera;
    
    private void Awake()
    {
        _characterMovementHandler= GetComponent<NetworkPlayerController>();
        _camera = FindAnyObjectByType<GameManager>().ThirdPersonCamera;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Update()
    {
        if (!_characterMovementHandler.Object.HasInputAuthority) return;

        if (isReset)
            accumulatedInput = default;


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

        //Set View
        //_localCameraHandler.SetViewInputVector(_viewInputVector);
    }

    public NetworkInputData GetNetworkInput()
    {
        isReset = true;
        return accumulatedInput;
    }
 
}
