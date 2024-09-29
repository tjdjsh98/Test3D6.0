using Fusion;
using Fusion.Addons.KCC;
using System;
using Unity.Cinemachine;
using UnityEngine;
using Input = UnityEngine.Input;

public class PlayerInputHandler : NetworkBehaviour,IBeforeUpdate
{
    // InputData
    public PlayerInputData FixedInputData => _fixedInputData;

    public PlayerInputData AccumulatedInput { get; set; }
    PlayerInputData _fixedInputData;

    // Components
    Camera _camera;
    KCC _kcc;

    public bool IsEnableInputMove { get; set; } = true;
    public bool IsEnableInputRotation { get; set; } = true;

    public Action ResetAccumulateInputData;

    Vector3 _animationMoveDelta;

    private void Awake()
    {
        _kcc = GetComponent<KCC>(); 
    }
    public override void Spawned()
    {
        _camera = Camera.main;

        if (HasInputAuthority)
        {
            InputManager.Instance.BeforeInputDataSent += OnBeforeInputDataSent;
            InputManager.Instance.InputDataReset += OnReset;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority)
        {
            InputManager.Instance.BeforeInputDataSent -= OnBeforeInputDataSent;
            InputManager.Instance.InputDataReset -= OnReset;
        }
    }

    void OnBeforeInputDataSent()
    {
        ProcessInput();
        InputManager.Instance.InsertPlayerInputData(AccumulatedInput);
    }

    void OnReset()
    {
        AccumulatedInput = default;
    }
    public void BeforeUpdate()
    {
        ProcessInput();
    }

   
    int _lastTime;
    void ProcessInput()
    {
        if (HasInputAuthority == false) return;
        if (_lastTime == Time.frameCount) return;
        _lastTime = Time.frameCount;

        PlayerInputData accumulatedInput = AccumulatedInput;

        // cameraPosition
        accumulatedInput.cameraPosition = _camera.transform.position;

        // aimForward
        accumulatedInput.aimForwardVector = _camera.transform.forward;

        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 cameraForward = accumulatedInput.aimForwardVector;
        Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);
        Vector3 characterMoveDirection = cameraForward * movementInput.y +
            cameraRight * movementInput.x;


        if (InputManager.Instance.IsEnableInput)
        {
            if (IsEnableInputMove)
            {
                // Move Input
                accumulatedInput.movementInput += movementInput;
                accumulatedInput.movementInput.Normalize();
            }

            // Body Forward
            accumulatedInput.bodyForwardVector = transform.forward;

            NetworkButtons buttons = default;

            if (InputManager.Instance.IsEnableInput)
            {
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

                // Throw
                if (Input.GetKeyDown(KeyCode.Q))
                    buttons.Set(InputButton.Throw, true);

                // QuickSlot
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    buttons.Set(InputButton.Num1, true);
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    buttons.Set(InputButton.Num2, true);
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    buttons.Set(InputButton.Num3, true);
                if (Input.GetKeyDown(KeyCode.Alpha4))
                    buttons.Set(InputButton.Num4, true);

            }
            if (IsEnableInputRotation)
            {
                if (characterMoveDirection != Vector3.zero)
                {
                    characterMoveDirection.Normalize();
                    float angle = Mathf.Atan2(characterMoveDirection.x, characterMoveDirection.z) * Mathf.Rad2Deg;
                    float deltaAngle = angle - _kcc.FixedData.GetLookRotation().y;
                    if (deltaAngle > 180)
                        deltaAngle = -360 + deltaAngle;
                    else if(deltaAngle < -180)
                        deltaAngle = 360 + deltaAngle;

                    accumulatedInput.lookRotationDelta.y = deltaAngle;
                }
            }
        accumulatedInput.buttons = new NetworkButtons(accumulatedInput.buttons.Bits | buttons.Bits);
        }

        AccumulatedInput = accumulatedInput;
        InputManager.Instance.InsertPlayerInputData(AccumulatedInput);
    }
}
