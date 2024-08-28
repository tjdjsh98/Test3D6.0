using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class PlayerController : MonoBehaviour
{
    [SerializeField] UIInventory _uiInventory;
    [SerializeField] UICharacter _uiCharacter;

    // 컴포넌트
    GameObject _model;
    CapsuleCollider _collider;
    Rigidbody _rigidBody;
    Animator _animator;
    Inventory _inventory;
    CharacterEquipment _characterEquipment;
    Character _character;


    [Header("카메라 시점")]
    [SerializeField]Camera _camera;
    [SerializeField] CinemachineCamera _thirdPersonCamera;
    [SerializeField] CinemachineCamera _firstPersonCamera;
    [SerializeField] List<SkinnedMeshRenderer> _meshs;


    [Header("캐릭터 상태")]
    float _maxSpeed = 5f;

    // 이동관련
    float _inputAngle;

    // 올라가는 상태
    bool _isContactWall;
    Vector3 _climbingStartPos;
    Vector3 _climbingDestin;
    bool _isClimbing = false;
    float _climbElaspedTime = 0;

    // 착지 상태
    bool _isContactGround;
    float _landingElasepdTime = 0;
    float _landingTime = 1;

    [Header("바닥")]
    [SerializeField] float _slopeLimit = 45;


    [Header("벽")]
    [SerializeField] Range _wallRange;
    [SerializeField] float _climbTween = 2f;
    [SerializeField] AnimationCurve _climbYCurve;
    [SerializeField] AnimationCurve _climbForwardCurve;

    [Header("공격")]
    [SerializeField] Range _attackRange;

    // 상태 제한
    bool _isEnableMove = true;

    private void Awake()
    {
        _model = transform.Find("Model").gameObject;
        _collider = GetComponent<CapsuleCollider>();
        _rigidBody =GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _inventory = GetComponent<Inventory>();
        _characterEquipment = GetComponent<CharacterEquipment>();
        _character = GetComponentInChildren<Character>();
        _uiCharacter.ConnectCharacter(_character);
        _rigidBody.maxLinearVelocity = 100;
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + Vector3.down*0.2f);

        Utils.DrawRange(gameObject, _wallRange,Color.yellow);
        Utils.DrawRange(gameObject, _attackRange, Color.red);
    }
    void Update()
    {
       
        ControlMovement();
        AttachGround();
        AttachWall();
        CheckItem();
        ControlInventory();
        ControlAttack();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _thirdPersonCamera.gameObject.SetActive(true);
            _firstPersonCamera.gameObject.SetActive(false);
            foreach (var mesh in _meshs)
                mesh.gameObject.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _thirdPersonCamera.gameObject.SetActive(false);
            _firstPersonCamera.gameObject.SetActive(true);
            foreach (var mesh in _meshs)
                mesh.gameObject.SetActive(false);
        }
    }

    // 캐릭터 주변에 아이템이 있는지 확인합니다.
    // 아이템이 있다면 UI를 띄웁니다.

    List<GameObject> _aroundItemList = new List<GameObject>();
    List<UIItemTag> _arountItemUIList = new List<UIItemTag>();
    void CheckItem()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, 2, LayerMask.GetMask("Item"));
        //RaycastHit[] hits = Physics.BoxCastAll(transform.position, Vector3.one * 10, Vector3.zero, Quaternion.identity, 0,LayerMask.GetMask("Item"));

        if(hits.Length > 0)
        {
            for(int i = 0; i < hits.Length; i++)
            {
                GameObject gameObject = hits[i].gameObject;
                if (_aroundItemList.Contains(gameObject)) continue;

                _aroundItemList.Add(gameObject);
                _arountItemUIList.Add(UIItemShower.Instance.ShowText(gameObject,gameObject.name));
            }
        }

        for (int i = _aroundItemList.Count-1; i >= 0; i--)
        {
            bool isFar = true;
            for(int j = 0; j < hits.Length; j++)
            {
                if (hits[j].gameObject == _aroundItemList[i])
                {
                    isFar = false;
                    break;
                }
            }
            if(isFar)
            {
                _aroundItemList.RemoveAt(i);
                _arountItemUIList[i].parent.gameObject.SetActive(false);
                _arountItemUIList.RemoveAt(i);
            }
        }

        // 가장 가까운 아이템부터 흭득한다.
        if(Input.GetKeyDown(KeyCode.E))
        {
            int index = -1;
            float closeDistance = float.MaxValue;
            for(int i = 0; i <_aroundItemList.Count; i++)
            {
                float distance = Vector3.Distance(transform.position, _aroundItemList[i].transform.position);
                if(distance < closeDistance)
                {
                    closeDistance = distance;
                    index = i;
                }
            }

            if(index >= 0)
            {
                GameObject item = _aroundItemList[index];

                _aroundItemList.RemoveAt(index);
                _arountItemUIList[index].parent.gameObject.SetActive(false);
                _arountItemUIList.RemoveAt(index);

                Item itemComp = item.GetComponent<Item>();

                if (itemComp.ItemType == ItemType.Equipment)
                    GetComponent<CharacterEquipment>().EquipItem(item);
                else
                {
                    _inventory.InsertItem(item);
                }
            }
        }
    }

    void AttachWall()
    {

        RaycastHit hit = Utils.RangeCast(gameObject, _wallRange, LayerMask.GetMask("Ground"));
        if(hit.collider != null)
        {
            if (!_isContactWall)
            {
                transform.position = hit.point + hit.normal * 0.1f - _collider.center;
            }
            _rigidBody.useGravity = false;
            _isContactWall = true;
            _animator.SetBool("ContactWall", true);
            Vector3 normal = -hit.normal;
          
            transform.LookAt(transform.position + normal);

        }
        else
        { 
            _rigidBody.useGravity = true;
            _isContactWall = false;
            _animator.SetBool("ContactWall", false);
        }
    }

    void AttachGround()
    {
        RaycastHit hit;
        if (Physics.BoxCast(transform.position + Vector3.up,new Vector3(0.1f,0.1f,0.1f), Vector3.down,out hit,Quaternion.identity, 1f, LayerMask.GetMask("Ground")))
        {
            float dot = Vector3.Dot(Vector3.up, hit.normal);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

            if (_rigidBody.linearVelocity.y <= 0 && angle < _slopeLimit)
            {
                Vector3 pos = transform.position;
                pos.y = hit.point.y;
                transform.position = pos;
            }

            _isContactGround = true;
            _animator.SetBool("ContactGround", true);
        }
        else
        {
            _isContactGround = false;
            _animator.SetBool("ContactGround", false);
        }
    }
    void ControlMovement()
    {
        if (_uiInventory.gameObject.activeSelf) return;
        // A
        _animator.SetFloat("Velocity", Mathf.Abs(_rigidBody.linearVelocity.x) + Mathf.Abs(_rigidBody.linearVelocity.z));


        if (_isClimbing)
        {
            _climbElaspedTime += Time.deltaTime;
            Vector3 pos = _climbingStartPos;
            pos.y += (_climbingDestin - _climbingStartPos).y * _climbYCurve.Evaluate(_climbElaspedTime / _climbTween);
            pos.x += (_climbingDestin - _climbingStartPos).x * _climbForwardCurve.Evaluate(_climbElaspedTime / _climbTween);
            pos.z += (_climbingDestin - _climbingStartPos).z * _climbForwardCurve.Evaluate(_climbElaspedTime / _climbTween);
            transform.position = pos;

            if (_climbElaspedTime > _climbTween)
            {
                _isClimbing = false;
                _climbElaspedTime = 0;
                _animator.SetBool("EndClimbing", false);
            }
            return;
        }

        if (!_isEnableMove) return;

        Vector3 moveDirection = Vector3.zero;
        Vector3 inputDirection = Vector3.zero;
        if(Input.GetMouseButtonDown(0))
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }

        if(Input.GetKey(KeyCode.W))
        {
            inputDirection += Vector3.forward;
            Vector3 dir = _camera.transform.forward;
            dir.y = 0;
            dir.Normalize();
            moveDirection += dir;

        }
        if (Input.GetKey(KeyCode.S))
        {
            inputDirection -= Vector3.forward;
            Vector3 dir = _camera.transform.forward;
            dir.y = 0;
            dir.Normalize();
            moveDirection -= dir;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputDirection -= Vector3.right;
            Vector3 dir = _camera.transform.right;
            dir.y = 0;
            dir.Normalize();
            moveDirection -= dir;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputDirection += Vector3.right;
            Vector3 dir = _camera.transform.right;
            dir.y = 0;
            dir.Normalize();
            moveDirection += dir;
        }

        if (inputDirection != Vector3.zero)
            _animator.SetBool("InputMove", true);
        else
            _animator.SetBool("InputMove", false);


        if (_isContactGround)
        {
            if (_landingElasepdTime < _landingTime)
            { 
                _landingElasepdTime += Time.deltaTime;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 velocity = _rigidBody.linearVelocity;
                velocity.y = 5;
                _rigidBody.linearVelocity = velocity;
                _landingElasepdTime = 0;

            }
          
            float deltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, _inputAngle) * 0.5f;
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + deltaAngle, 0);


            if (inputDirection != Vector3.zero)
            {
                _inputAngle = Mathf.Atan2(_rigidBody.linearVelocity.x, _rigidBody.linearVelocity.z) * Mathf.Rad2Deg;
                moveDirection.Normalize();
                _rigidBody.linearVelocity = new Vector3(moveDirection.x * _maxSpeed, _rigidBody.linearVelocity.y, moveDirection.z * _maxSpeed);
            }
            
        }
        else
        {
            if (_isContactWall)
            {
                Vector3 wallMove = transform.up * inputDirection.z + transform.right * inputDirection.x;
                wallMove.Normalize();
                _animator.SetFloat("MoveWall", wallMove.magnitude);

                _rigidBody.linearVelocity = wallMove;
                Ray ray = new Ray(transform.position + _collider.center, _collider.center + transform.forward);
                RaycastHit hit;
                //if (!Physics.Raycast(ray, out hit, 0.3f, LayerMask.GetMask("Ground")))
                //{
                //    _isClimbing = true;
                //    _animator.SetBool("EndClimbing", true);
                //    _climbingStartPos = transform.position;
                //    _climbingDestin = transform.position + _collider.center + _model.transform.forward * 0.5f;
                //}

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 velocity = -transform.forward*5 + transform.up*6;
                    transform.rotation = Quaternion.Euler(0, 180 + transform.rotation.eulerAngles.y, 0);
                    _rigidBody.linearVelocity = velocity;
                }
            }
        }
        _animator.SetFloat("VelocityY", _rigidBody.linearVelocity.y);

    
    }
    void ControlInventory()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            if(_uiInventory.gameObject.activeSelf == false)
            {
                _uiInventory.ConnectInventory(_inventory);
                _uiInventory.ConnecteCharacterEquipment(_characterEquipment);
                _uiInventory.Open();
            }
            else
            {
                _uiInventory.Close();
            }
        }
    }

    void ControlAttack()
    {
        if (_uiInventory.gameObject.activeSelf) return;

        if (_character.IsAttack) return;

        if(Input.GetMouseButton(0))
        {
            _character.IsAttack = true;
            _character.Attacked = OnAttacked;
            _isEnableMove = false;
            _animator.applyRootMotion = true;
            _animator.SetTrigger("Attack");
            StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_animator, "Attack", EndAttack));
            
        }
    }

    void OnAttacked()
    {
        Collider[] colliders = Utils.RangeOverlapAll(gameObject, _attackRange, Define.ENEMY_LAYERMASK);

        if (colliders == null) return;

        foreach(var collider in colliders)
        {
            Character character = collider.gameObject.GetComponent<Character>();
            if(character != null)
            {
                DamageInfo info = new DamageInfo();
                info.attacker = _character;
                info.target = character;
                info.knockbackPower = 50;
                info.knockbackDirection = transform.forward;
                info.damage = 2;
                character.Damaged(info);
            }
        }
    }

    void EndAttack()
    {
        _character.IsAttack = false;
        _isEnableMove = true;
        _animator.applyRootMotion = false;
    }
}