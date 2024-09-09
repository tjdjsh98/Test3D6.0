using Fusion;
using Fusion.Addons.SimpleKCC;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

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
            UIInventory inventory = FindAnyObjectByType<UIInventory>(FindObjectsInactive.Include);

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
        DetectInteractableObject();
    }
    Vector3 _start;
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
                moveDirection *= 0.2f;
                //_character.Move(moveDirection);
            }
          

            // Rotate
            if (_character.IsEnableMove && _character.IsEnableTurn)
            {
                if (networkInputData.movementInput != Vector2.zero)
                {
                    float inputAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

                    float angle = Vector3.Angle(transform.transform.forward, moveDirection);
                    
                    if(angle >= 170)
                    {
                        _start = transform.forward;
                        _character.IsEnableMove = false;
                        _character.IsEnableTurn = false;
                        _character.SetAnimatorTrigger("Turn");
                        _character.WaitAnimationState("TurnBack", OnTurnAnimationEnded, 0.8f);
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
            // Interact
            if (networkInputData.buttons.WasPressed(_previousButtons, InputButton.Interact))
            {
                InteractOther();
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
        _character.SetAnimatorBoolean("ContactGround", _kcc.IsGrounded);
        //_character.SetAnimatorFloat("Velocity", Mathf.Abs(_character.Velocity.magnitude));
        _character.SetAnimatorFloat("VelocityY", _kcc.RealVelocity.y);
    }

    void OnAttackAnimationStarted()
    {
        Debug.Log("Start");
        _character.SetAnimatorRootmotion(true);
        _character.IsAttack = true;
        _character.Attacked = OnAttackStarted;
        _character.AttackEnded = OnAttackEnded;
        Weapon?.OnAttackAnimationStarted();

        StartCoroutine(Utils.WaitAniationAndPlayCoroutine(GetComponentInChildren<Animator>(), "Attack", OnAttackAnimationEnded));
    }

    public void OnAttackAnimationEnded()
    {
        
        Debug.Log("End");
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



    // 캐릭터 주변에 상호작용할 수 있는 물체가 있는지 확인합니다.
    // 물체가 있고 로컬캐릭터라면 UI를 띄웁니다.
    
    List<GameObject> _aroundItemList = new List<GameObject>();
    List<UIItemTag> _arountItemUIList = new List<UIItemTag>();

    void DetectInteractableObject()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2, LayerMask.GetMask("Item"));

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                GameObject gameObject = hits[i].gameObject;
                if (_aroundItemList.Contains(gameObject)) continue;

                _aroundItemList.Add(gameObject);
                if(NetworkPlayer.Local == _player)
                    _arountItemUIList.Add(UIItemShower.Instance.ShowText(gameObject, gameObject.name));
            }
        }

        for (int i = _aroundItemList.Count - 1; i >= 0; i--)
        {
            if (_aroundItemList[i] == null || Vector3.Distance(_aroundItemList[i].gameObject.transform.position, gameObject.transform.position) > 2)
            {
                _aroundItemList.RemoveAt(i);
                if (NetworkPlayer.Local == _player)
                {
                    _arountItemUIList[i].parent.gameObject.SetActive(false);
                    _arountItemUIList.RemoveAt(i);
                }
            }
        }
    }
    void InteractOther()
    {
        if (!Object.HasStateAuthority) return;

        // 가장 가까운 아이템부터 흭득한다.

        int index = -1;
        float closeDistance = float.MaxValue;
        for (int i = 0; i < _aroundItemList.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, _aroundItemList[i].transform.position);
            if (distance < closeDistance)
            {
                index = i;
                closeDistance = distance;
            }
        }
        Debug.Log(_aroundItemList.Count);

        if (index != -1)
        {
            _aroundItemList[index].GetComponentInParent<IInteractable>().Interact(gameObject);
            Debug.Log(_aroundItemList[index]);
        }
    }
    void CheckFallRespawn()
    {
        if(transform.position.y < -12)
        {
            transform.position = Utils.GetRandomSpawnPoint();
        }
    }
}
