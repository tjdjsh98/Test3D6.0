using Fusion;
using Fusion.Addons.KCC;
using TMPro;
using Unity.Cinemachine;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Input = UnityEngine.Input;

public class PlayerInputHandler : NetworkBehaviour,IBeforeUpdate, IBeforeTick
{

    // InputData
    public PlayerInputData FixedInputData => _fixedInputData;

    PlayerInputData _accumulatedInput;
    PlayerInputData _preFixedInputData;
    PlayerInputData _fixedInputData;

    // Components
    CinemachineCamera _camera;
    Animator _animator;
    AnimatorHelper _animatoHelper;
    NetworkCharacter _character;

    public bool IsEnableInputMove { get; set; } = true;
    public bool IsEnableInputRotation { get; set; } = true;


    [SerializeField]float _totalRot;
    private void Awake()
    {
        _character = GetComponent<NetworkCharacter>();
        _animator = GetComponentInChildren<Animator>();
        _animatoHelper = GetComponentInChildren<AnimatorHelper>();
        _animatoHelper.AnimatorMoved += OnAnimatorMoved;

    }
    public override void Spawned()
    {
        _camera = FindAnyObjectByType<GameManager>().ThirdPersonCamera;
        if (HasInputAuthority)
        {
            Runner.GetComponent<NetworkEvents>().OnInput.AddListener(OnPlayerInput);
        }
    }
    void OnAnimatorMoved()
    {
        if (!_character.IsGrounded) return;
        if (HasInputAuthority)
        {
            _accumulatedInput.moveDelta += _animator.deltaPosition;
            Vector2 deltaAngle = (Vector2)_animator.deltaRotation.eulerAngles;
            deltaAngle.x = deltaAngle.x > 180 ? deltaAngle.x - 360 : deltaAngle.x;
            deltaAngle.y = deltaAngle.y > 180 ? deltaAngle.y - 360 : deltaAngle.y;
            _accumulatedInput.lookRotationDelta += deltaAngle;
        }
    }
    public void BeforeUpdate()
    {
        if (HasInputAuthority == false)
            return;

        ProcessInput();
    }

    public void BeforeTick()
    {
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

    int _lastFrame;
    public void OnPlayerInput(NetworkRunner runner, NetworkInput input)
    {
        if (_lastFrame == Time.frameCount) return;
        _lastFrame = Time.frameCount;

        ProcessInput();

        input.Set(_accumulatedInput);

        _accumulatedInput = default;
    }

    int _procseeInputFrame;
    void ProcessInput()
    {
        if (_procseeInputFrame == Time.frameCount) return;
        _procseeInputFrame = Time.frameCount;

        // aimForward
        _accumulatedInput.aimForwardVector = _camera.transform.forward;
        _accumulatedInput.aimForwardVector.y = 0;
        _accumulatedInput.aimForwardVector.Normalize();

        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 cameraForward = _accumulatedInput.aimForwardVector;
        Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward);
        Vector3 characterMoveDirection = cameraForward * movementInput.y +
            cameraRight * movementInput.x;

        // Move Input
        _accumulatedInput.movementInput += movementInput;
        _accumulatedInput.movementInput.Normalize();

        // Body Forward
        _accumulatedInput.bodyForwardVector = transform.forward;

        NetworkButtons buttons = default;

        // Jump
        if (Input.GetButtonDown("Jump"))
            buttons.Set(InputButton.Jump, true);

        // Fire
        if (Input.GetButtonDown("Fire1"))
        {
            buttons.Set(InputButton.MouseButton0, true);
          
        }

        // Interact
        if (Input.GetKey(KeyCode.LeftShift))
            buttons.Set(InputButton.Run, true);

        // Interact
        if (Input.GetKeyDown(KeyCode.E))
            buttons.Set(InputButton.Interact, true);

        if (IsEnableInputRotation)
        {
            characterMoveDirection.Normalize();
            float deltaAngle = Vector3.SignedAngle(transform.forward, characterMoveDirection, Vector3.up);
            _accumulatedInput.lookRotationDelta.y += deltaAngle;
        }
        
        _accumulatedInput.buttons = new NetworkButtons(_accumulatedInput.buttons.Bits | buttons.Bits);
    }
}
