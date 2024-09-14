using Fusion;
using Fusion.Addons.KCC;
using NUnit.Framework.Interfaces;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class NetworkPlayerController : NetworkBehaviour
{
    // Input
    PlayerInputData _accumulatedInput;
    float _inputAngle;
    NetworkButtons _previousButtons;
    private Vector2Accumulator _lookRotationAccumulator = new Vector2Accumulator(0.02f, true);
    int _lastFrame;

    // Other Component
    NetworkCharacter _character;
    CinemachineCamera _camera;
    PlayerInputHandler _playerInputHandler;
    KCC _kcc;

    // Misc
    static string[] TurnStateNames = new string[] { "Walk Turn 180", "Running Turn 180" };

    public NetworkWeapon Weapon;

    bool _isFinishAniationProcess = false;

    void Awake()
    {
        _character = GetComponent<NetworkCharacter>();
        _camera = GameObject.FindAnyObjectByType<GameManager>().ThirdPersonCamera;
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _kcc = GetComponent<KCC>();


    }
    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.I))
        {
            UIInventory inventory = UIManager.Instance.GetUI<UIInventory>();

            if (inventory.gameObject.activeSelf)
            {
                inventory.Close();
            }
            else
            {
                inventory.ConnectInventory(GetComponent<Inventory>());
                inventory.Open();
            }
        }
    }
    public override void Render()
    {
        _isFinishAniationProcess = false;
    }
    int _lastFixedFrame;
    public override void FixedUpdateNetwork()
    {
        //Debug.Log(_playerInputHandler.FixedInputData.lookRotationDelta);
        // Move
        Vector3 forward = _playerInputHandler.FixedInputData.aimForwardVector.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 moveDirection = forward * _playerInputHandler.FixedInputData.movementInput.y +
                                    right * _playerInputHandler.FixedInputData.movementInput.x;
        float velocity = 0;
        float jumpPower = 0;
        bool isTurn = false;

        moveDirection.y = 0;
        moveDirection.Normalize();

        float angle = Vector3.Angle(_playerInputHandler.FixedInputData.bodyForwardVector, moveDirection);
        if (_playerInputHandler.IsEnableInputRotation&&angle >= 160)
        {
            isTurn = true;
        }

        // Jump
        if (_character.IsGrounded && _playerInputHandler.FixedInputData.buttons.WasPressed(_previousButtons, InputButton.Jump))
        {
            _character.Jump(5);
        }

      
        if (_playerInputHandler.FixedInputData.buttons.WasPressed(_previousButtons, InputButton.MouseButton0))
        {
            
            _character.SetAnimatorTrigger("Attack");
            if (!_character.IsAttack)
            {
                OnAttackAnimationStarted();
                _character.WaitAnimationState("OneHandAttack", OnAttackAnimationEnded);
            }


        }


        // 애니메이션 처리
        // 리시뮬레이션되지 않게 처리함
        if (!_isFinishAniationProcess && Runner.IsFirstTick)
        {
            _isFinishAniationProcess = true;

         
            // Attack Animation


            // 0.2  = 걷기
            // 1    = 달리기
            if (_playerInputHandler.FixedInputData.movementInput != Vector2.zero)
            {
                velocity = 0.2f;
                if (_playerInputHandler.FixedInputData.buttons.IsSet(InputButton.Run))
                {
                    velocity = 1;
                }
            }

            _character.SetAnimatorFloat("Velocity", velocity, 0.1f, Runner.DeltaTime);

            // Turn Animation 
            if (isTurn)
            {
                _playerInputHandler.IsEnableInputMove = false;
                _playerInputHandler.IsEnableInputRotation = false;
                _character.SetAnimatorTrigger("Turn");
                _character.WaitAnimationState(TurnStateNames, OnTurnAnimationEnded);
            }
        }

        // Rotation
        if(!isTurn)
            _character.AddAngle(_playerInputHandler.FixedInputData.lookRotationDelta.y);

        // Position
        _kcc.SetPosition(_kcc.Data.TargetPosition + _playerInputHandler.FixedInputData.moveDelta, false);


        _previousButtons = _playerInputHandler.FixedInputData.buttons;
        CheckFallRespawn();

        HandleAnimation();
    }
    void OnTurnAnimationEnded()
    {
        Debug.Log("TurnEnd");
        _playerInputHandler.IsEnableInputRotation = true;
        Vector3 vel = _character.Velocity;
        vel.y = 0;
        _character.Velocity = vel;
    }
    void HandleAnimation()
    {
    }
    void OnAttackAnimationStarted()
    {
        _character.IsAttack = true;
        _playerInputHandler.IsEnableInputMove = false;
        _playerInputHandler.IsEnableInputRotation = false;
        _character.Attacked = OnAttackStarted;
        _character.AttackEnded = OnAttackEnded;

        Weapon?.OnAttackAnimationStarted();
        _character.WaitAnimationState("AttackEnd", OnAttackAnimationEnded);
    }
    public void OnAttackAnimationEnded()
    {
        Weapon?.OnAttackAnimationEnded();
        _character.IsAttack = false;
        _playerInputHandler.IsEnableInputMove = true;
        _playerInputHandler.IsEnableInputRotation = true;
    }
    void OnAttackStarted()
    {
        if (Weapon)
            Weapon.StartAttack();
    }
    void OnAttackEnded()
    {
        if (Weapon)
            Weapon.EndAttack();
    }
    void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            transform.position = Utils.GetRandomSpawnPoint();
        }
    }


  
}
