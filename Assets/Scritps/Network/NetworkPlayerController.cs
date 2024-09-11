using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class NetworkPlayerController : NetworkBehaviour
{
    // Input
    float _inputAngle;
    NetworkButtons _previousButtons;

    // Other Component
    SimpleKCC _kcc;
    NetworkCharacter _character;
    NetworkPlayer _player;
    Rigidbody _rigidbody;

    // Misc
    static string[] TurnStateNames = new string[] { "Walk Turn 180", "Running Turn 180" };


    public NetworkWeapon Weapon;

    void Awake()
    {
        _kcc = GetComponent<SimpleKCC>();
        _character = GetComponent<NetworkCharacter>();
        _player = GetComponent<NetworkPlayer>();
        _rigidbody = GetComponent<Rigidbody>(); 
    }


    private void Start()
    {
        
    }

    private void Update()
    {
        if(Object.HasInputAuthority && Input.GetKeyDown(KeyCode.I))
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
    }
    Vector3 _start;
    public override void FixedUpdateNetwork()
    {
        if (Runner.TryGetInputForPlayer(Object.InputAuthority,out NetworkInputData networkInputData) )
        {
            // Move
            Vector3 forward = networkInputData.aimForwardVector.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 moveDirection = forward * networkInputData.movementInput.y +
                                        right * networkInputData.movementInput.x;

            float jumpPower = 0;

            if (networkInputData.movementInput != Vector2.zero)
            {
                moveDirection.y = 0;
                moveDirection.Normalize();
                moveDirection *= 0.2f;
                //_character.Move(moveDirection);
            }
          

            // Rotate
            if (_character.IsEnableMove && _character.IsEnableTurn)
            {
                if (networkInputData.movementInput != Vector2.zero)
                {
                    float inputAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

                    float angle = Vector3.Angle(transform.forward, moveDirection);
                    
                    if(angle >= 170)
                    {
                        _start = transform.forward;
                        _character.IsEnableMove = false;
                        _character.IsEnableTurn = false;
                        _character.SetAnimatorTrigger("Turn");
                        _character.WaitAnimationState(TurnStateNames, OnTurnAnimationEnded, 0.8f);
                        _inputAngle = inputAngle;
                        return;
                    }

                    _inputAngle = inputAngle;
                }
                float deltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, _inputAngle) * 0.1f;
                _character.AddAngle(deltaAngle);                
            }



            // Jump
            if (_kcc.IsGrounded && networkInputData.buttons.WasPressed(_previousButtons, InputButton.Jump))
            {
                _character.Jump(5);
            }

            // Attack
            if (networkInputData.buttons.WasPressed(_previousButtons, InputButton.MouseButton0))
            {
                _character.SetAnimatorTrigger("Attack");
                if(!_character.IsAttack)
                    OnAttackAnimationStarted();
            }
           
            // Run
            if ((networkInputData.buttons.Bits & (1<<(int)InputButton.Run)) != 0)
            {
                moveDirection *= 5;
            }

            //Debug.Log(moveDirection);
            _previousButtons = networkInputData.buttons;
            _character.SetAnimatorFloat("Velocity", moveDirection.magnitude,0.1f,Runner.DeltaTime);
            CheckFallRespawn();
        }

        HandleAnimation();
    }

    void OnTurnAnimationEnded()
    {
        _character.IsEnableMove = true;
        _character.IsEnableTurn = true;
    }

    void HandleAnimation()
    {
    }

    void OnAttackAnimationStarted()
    {
        _character.SetAnimatorRootmotion(true);
        _character.IsAttack = true;
        _character.Attacked = OnAttackStarted;
        _character.AttackEnded = OnAttackEnded;
        Weapon?.OnAttackAnimationStarted();

        StartCoroutine(Utils.WaitAniationAndPlayCoroutine(GetComponentInChildren<Animator>(), "Attack", OnAttackAnimationEnded));
    }

    public void OnAttackAnimationEnded()
    {
        Weapon?.OnAttackAnimationEnded();
        _character.IsAttack = false;
        _character.SetAnimatorRootmotion(false);
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
        if(transform.position.y < -12)
        {
            transform.position = Utils.GetRandomSpawnPoint();
        }
    }
}
