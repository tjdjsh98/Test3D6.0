using System.Collections.Generic;
using Tripolygon.UModeler.UI.Converters;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using Debug = UnityEngine.Debug;

public class UnitChanCharacter : MonoBehaviour
{
    [SerializeField]Camera _camera;
    [SerializeField] CinemachineCamera _thirdPersonCamera;
    [SerializeField] CinemachineCamera _firstPersonCamera;

    [SerializeField] List<SkinnedMeshRenderer> _meshs;

    GameObject _model;
    CapsuleCollider _collider;
    Rigidbody _rigidBody;
    Animator _animator;

    Inventory _inventory;

    float _maxSpeed = 5f;

    bool _isContactWall;
    bool _isContactGround;
    bool _isClimbing = false;

    float _landingElasepdTime = 0;
    float _landingTime = 1;

    Vector3 _climbingStartPos;
    Vector3 _climbingDestin;
    float _climbElaspedTime = 0;
    [SerializeField] float _climbTween = 2f;
    [SerializeField] AnimationCurve _climbYCurve;
    [SerializeField] AnimationCurve _climbForwardCurve;

    [SerializeField] Vector4 _climbContactRay;

    float _inputAngle;

    private void Awake()
    {
        _model = transform.Find("Model").gameObject;
        _collider = GetComponent<CapsuleCollider>();
        _rigidBody =GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _inventory = GetComponent<Inventory>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + Vector3.down*0.2f);

        if(_collider == null)
            _collider = GetComponent<CapsuleCollider>();
        if (_model == null)
            _model = transform.Find("Model").gameObject;
        Gizmos.DrawLine(gameObject.transform.position + (Vector3)_climbContactRay, gameObject.transform.position + (Vector3)_climbContactRay + _model.transform.forward * _climbContactRay.w); 

        Gizmos.DrawWireSphere(transform.position, 2);
    }
    void Update()
    {
        ControlMovement();
        CheckGroundWall();
        CheckItem();
        ControlInventory();

        if(Input.GetKeyDown(KeyCode.Alpha1))
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

                _inventory.InsertItem(item.name);
                Destroy(item);
            }
        }
    }
    void CheckGroundWall()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 0.2f, LayerMask.GetMask("Ground")))
        {
            _isContactGround = true;
            _animator.SetBool("ContactGround", true);
        }
        else
        {
            _isContactGround = false;
            _animator.SetBool("ContactGround", false);
        }
        Ray ray = new Ray(transform.position + (Vector3)_climbContactRay, _model.transform.forward * _climbContactRay.w);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 0.3f, LayerMask.GetMask("Ground")))
        {
            if (!_isContactWall)
            {
                transform.position = hit.point + hit.normal * 0.1f - _collider.center;
            }
            _rigidBody.useGravity = false;
            _isContactWall = true;
            _animator.SetBool("ContactWall", true);
            Vector3 normal = -hit.normal;
            float angle = Mathf.Atan2(normal.x, normal.z) * Mathf.Rad2Deg;
            _model.transform.rotation = Quaternion.Euler(0, angle, 0);


        }
        else
        {
            _rigidBody.useGravity = true;
            _isContactWall = false;
            _animator.SetBool("ContactWall", false);
        }
    }
    void ControlMovement()
    {
        if(_isClimbing)
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

        _animator.SetFloat("Velocity", Mathf.Abs(_rigidBody.linearVelocity.x) + Mathf.Abs(_rigidBody.linearVelocity.z));

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
          
            float deltaAngle = Mathf.DeltaAngle(_model.transform.rotation.eulerAngles.y, _inputAngle) * 0.5f;


            _model.transform.rotation = Quaternion.Euler(0, _model.transform.rotation.eulerAngles.y + deltaAngle, 0);


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
                Vector3 wallMove = _model.transform.up * inputDirection.z + _model.transform.right * inputDirection.x;
                wallMove.Normalize();
                _animator.SetFloat("MoveWall", wallMove.magnitude);

                _rigidBody.linearVelocity = wallMove;
                Ray ray = new Ray(transform.position + _collider.center, _collider.center + _model.transform.forward);
                RaycastHit hit;
                if (!Physics.Raycast(ray, out hit, 0.3f, LayerMask.GetMask("Ground")))
                {
                    Debug.Log("true");
                    _isClimbing = true;
                    _animator.SetBool("EndClimbing", true);
                    _climbingStartPos = transform.position;
                    _climbingDestin = transform.position + _collider.center + _model.transform.forward * 0.5f;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 velocity = -_model.transform.forward*5 + _model.transform.up*6;
                    _model.transform.rotation = Quaternion.Euler(0, 180 + _model.transform.rotation.eulerAngles.y, 0);
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
            if(UIInventory.Instance.gameObject.activeSelf == false)
            {
                UIInventory.Instance.ConnectInventory(_inventory);
                UIInventory.Instance.Open();
            }
            else
            {
                UIInventory.Instance.Close();
            }
        }
    }
}
