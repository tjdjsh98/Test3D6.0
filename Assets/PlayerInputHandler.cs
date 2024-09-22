using Fusion;
using Fusion.Addons.KCC;
using System;
using Unity.Cinemachine;
using UnityEngine;
using Input = UnityEngine.Input;

public class PlayerInputHandler : NetworkBehaviour,IBeforeUpdate, IBeforeTick
{
    // InputData
    public PlayerInputData FixedInputData => _fixedInputData;

    public PlayerInputData _accumulatedInput;
    PlayerInputData _preFixedInputData;
    PlayerInputData _fixedInputData;

    // Components
    Camera _camera;
    Animator _animator;
    AnimatorHelper _animatoHelper;
    NetworkCharacter _character;
    KCC _kcc;

    public bool IsEnableInputMove { get; set; } = true;
    public bool IsEnableInputRotation { get; set; } = true;

    public Action ResetAccumulateInputData;

    Vector3 _animationMoveDelta;

    private void Awake()
    {
        _character = GetComponent<NetworkCharacter>();
        _kcc = GetComponent<KCC>(); 
        _animator = GetComponentInChildren<Animator>();
        _animatoHelper = GetComponentInChildren<AnimatorHelper>();
        if(_animatoHelper)
            _animatoHelper.AnimatorMoved += OnAnimatorMoved;

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
    void OnAnimatorMoved()
    {
        if (!_character.IsGrounded) return;
        if (HasInputAuthority)
        {
            _accumulatedInput.moveDelta += _animator.deltaPosition;
            _accumulatedInput.velocity += _animator.deltaPosition/ Runner.DeltaTime;

            Vector2 deltaAngle = (Vector2)_animator.deltaRotation.eulerAngles;
            deltaAngle.x = deltaAngle.x > 180 ? deltaAngle.x - 360 : deltaAngle.x;
            deltaAngle.y = deltaAngle.y > 180 ? deltaAngle.y - 360 : deltaAngle.y;
            _accumulatedInput.lookRotationDelta += deltaAngle;
            
        }
    }

    void OnBeforeInputDataSent()
    {
        ProcessInput();
        InputManager.Instance.InsertPlayerInputData(_accumulatedInput);
    }

    void OnReset()
    {
        _accumulatedInput = default;
        _procseeInputFrame = false;
    }
    public void BeforeUpdate()
    {
        ProcessInput();
    }

    public void BeforeTick()
    {
        if (HasInputAuthority == false)
            return;
        if (Object == null) return;

        _preFixedInputData = _fixedInputData;

        PlayerInputData currentInputData = _fixedInputData;
        currentInputData.moveDelta = default;
        currentInputData.lookRotationDelta = default;
        _fixedInputData = currentInputData;

        if(Object.InputAuthority != PlayerRef.None)
        {
            if(GetInput(out PlayerInputData input))
            {
                _fixedInputData = input;
            }
        }
    }

    int _lastTime;
    bool _procseeInputFrame;
    void ProcessInput()
    {
        if (HasInputAuthority == false) return;
        if (_lastTime == Time.frameCount) return;
        _lastTime = Time.frameCount;

        // aimForward
        _accumulatedInput.aimForwardVector = _camera.transform.forward;
        _accumulatedInput.aimForwardVector.y = 0;
        _accumulatedInput.aimForwardVector.Normalize();

        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 cameraForward = _accumulatedInput.aimForwardVector;
        Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);
        Vector3 characterMoveDirection = cameraForward * movementInput.y +
            cameraRight * movementInput.x;


        if (InputManager.Instance.IsEnableInput)
        {
            if (IsEnableInputMove)
            {
                // Move Input
                _accumulatedInput.movementInput += movementInput;
                _accumulatedInput.movementInput.Normalize();
            }

            // Body Forward
            _accumulatedInput.bodyForwardVector = transform.forward;

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

            if (IsEnableInputRotation)
            {
                if (characterMoveDirection != Vector3.zero)
                {
                    characterMoveDirection.Normalize();
                    float angle = Mathf.Atan2(characterMoveDirection.x, characterMoveDirection.z) * Mathf.Rad2Deg;

                    float deltaAngle = angle - _kcc.FixedData.GetLookRotation().y;

                    _accumulatedInput.lookRotationDelta.y = deltaAngle;
                }
            }
        _accumulatedInput.buttons = new NetworkButtons(_accumulatedInput.buttons.Bits | buttons.Bits);
        }

        InputManager.Instance.InsertPlayerInputData(_accumulatedInput);
    }

}
