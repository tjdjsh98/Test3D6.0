using Fusion;
using Fusion.Addons.KCC;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class NetworkPlayerController : NetworkBehaviour, IBeforeTick
{
    // Input
    NetworkButtons _previousButtons;

    // Other Component
    NetworkCharacter _character;
    CinemachineCamera _camera;
    KCC _kcc;
    PlayerInputHandler _playerInputHandler;

    // Misc
    static string[] TurnStateNames = new string[] { "Walk Turn 180", "Running Turn 180" };
    public NetworkWeapon Weapon;
    bool _isFinishAniationProcess = false;
    bool _isThrow;


    void Awake()
    {
        _character = GetComponent<NetworkCharacter>();
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _camera = GameObject.FindAnyObjectByType<GameManager>().ThirdPersonCamera;
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

    public void BeforeTick()
    {
        
    }

    int _lastFixedFrame;
    public override void FixedUpdateNetwork()
    {
        if (GetInput<NetworkInputData>(out var inputData))
        {
            PlayerInputData playerInputData = inputData.playerInputData;
            //Debug.Log(_playerInputHandler.FixedInputData.lookRotationDelta);
            // Move
            Vector3 forward = playerInputData.aimForwardVector.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 moveDirection = forward * playerInputData.movementInput.y +
                                        right * playerInputData.movementInput.x;
            float velocity = 0;
            float jumpPower = 0;
            bool isTurn = false;

            moveDirection.y = 0;
            moveDirection.Normalize();

            float angle = Vector3.Angle(playerInputData.bodyForwardVector, moveDirection);
            if (_playerInputHandler.IsEnableInputRotation && angle >= 160)
            {
                isTurn = true;
            }

            // Jump
            if (_character.IsGrounded && playerInputData.buttons.WasPressed(_previousButtons, InputButton.Jump))
            {
                _character.Jump(5);
            }

            if (playerInputData.buttons.WasPressed(_previousButtons, InputButton.MouseButton0))
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


                // Throw Animation
                if (!_isThrow&&_character.IsGrounded && playerInputData.buttons.WasPressed(_previousButtons, InputButton.Throw))
                {
                    _character.SetAnimationLayerWeight(1, 1);
                    _character.SetAnimatorBoolean("Throw", true);
                    _character.WaitAnimationState("Throw Object", OnThrowAnimationEnded, 1);
                    _isThrow = true;
                }


                // 0.2  = 걷기
                // 1    = 달리기
                if (playerInputData.movementInput != Vector2.zero)
                {
                    velocity = 0.2f;
                    if (playerInputData.buttons.IsSet(InputButton.Run))
                    {
                        velocity = 1;
                    }
                }

                _character.SetAnimatorFloat("Velocity", velocity, 0.1f, Runner.DeltaTime);

                // Turn Animation 
                if (isTurn)
                {
                    _playerInputHandler.IsEnableInputRotation = false;
                    _character.SetAnimatorTrigger("Turn");
                    _character.WaitAnimationState(TurnStateNames, OnTurnAnimationEnded);
                }
            }

            // Rotation
            if (!isTurn)
                _character.AddAngle(playerInputData.lookRotationDelta.y);

            // Position
            // 애니메이션의 이동을 욺겨준다.
            if(_character.IsGrounded)
                _character.Velocity = playerInputData.velocity;


            _previousButtons = playerInputData.buttons;
            CheckFallRespawn();

            HandleAnimation();
        }
    }

    void OnThrowAnimationEnded()
    {
        _character.SetAnimationLayerWeight(1, 0);
        _character.SetAnimatorTrigger("Throw");
        _isThrow = false;
    }
    void OnTurnAnimationEnded()
    {
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
