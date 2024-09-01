using Fusion;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 _moveInputVector = Vector2.zero;
    Vector2 _viewInputVector = Vector2.zero;
    bool _isJumpButtonPressed = false;
    bool _isFireButtonPressed = false;

    CharacterMovementHandler _characterMovementHandler;
    LocalCameraHandler _localCameraHandler;
    private void Awake()
    {
        _localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        _characterMovementHandler= GetComponent<CharacterMovementHandler>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!_characterMovementHandler.Object.HasInputAuthority) return;

        // View Input
        _viewInputVector.x = Input.GetAxis("Mouse X");
        _viewInputVector.y = Input.GetAxis("Mouse Y") * -1; // Invert the mouse look


        // Move Input
        _moveInputVector.x = Input.GetAxis("Horizontal");
        _moveInputVector.y = Input.GetAxis("Vertical");

        // Jump
        if (Input.GetButtonDown("Jump"))
            _isJumpButtonPressed = true;

        // Fire
        if (Input.GetButtonDown("Fire1"))
             _isFireButtonPressed = true;

        //Set View
        _localCameraHandler.SetViewInputVector(_viewInputVector);
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        // aim Data
        networkInputData.aimForwardVector = _localCameraHandler.transform.forward;

        // move Data
        networkInputData.movementInput = _moveInputVector;

        // jump Data
        networkInputData.isJumpPressed = _isJumpButtonPressed;
        _isJumpButtonPressed = false;

        // fire Data
        networkInputData.isFireButtonPressed = _isFireButtonPressed;
        _isFireButtonPressed = false;

        return networkInputData;
    }

}
