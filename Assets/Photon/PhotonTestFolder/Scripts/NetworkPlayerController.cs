using Fusion;
using Fusion.Addons.SimpleKCC;
using Unity.Cinemachine;
using UnityEngine;

public class NetworkPlayerController : NetworkBehaviour
{
    // Rotation
    float _inputAngle;

    // Other Component
    SimpleKCC _kcc;
    CinemachineCamera _thirdPersionCamera;
    NetworkCharacter _character;

    //NetworkCharacterControllerCustom _networkdCharacterControllerCustom;

    public NetworkWeapon Weapon;

    void Awake()
    {
        _kcc = GetComponent<SimpleKCC>();
        _thirdPersionCamera = GameObject.Find("ThirdPersonCamera").GetComponent<CinemachineCamera>();
        _character = GetComponent<NetworkCharacter>();
    }


    private void Start()
    {
        
    }

    public override void Render()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData networkInputData) && Runner.IsForward)
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
                moveDirection *= _character.Speed;
                _character.Move(moveDirection);
                _character.SetAnimatorBoolean("InputMove", true);
            }
            else
            {
                _character.SetAnimatorBoolean("InputMove", false);
            }

            // Rotate
            if (networkInputData.movementInput != Vector2.zero)
            {
                _inputAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            }
            float deltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, _inputAngle) * 0.5f;
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + deltaAngle, 0);

            _kcc.AddLookRotation(0, deltaAngle);


            // Jump
            if (networkInputData.isJumpPressed)
            {
                _character.Jump(5);
            }

            // Attack
            if(networkInputData.isFireButtonPressed)
            {
                _character.Attacked = OnAttackStarted;
                _character.AttackEnded = OnAttackEnded;
                _character.SetAnimatorTrigger("Attack");
            }
            
            CheckFallRespawn();
        }
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
