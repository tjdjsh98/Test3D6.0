using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class UnitChanCharacter : MonoBehaviour
{
    [SerializeField]Camera _camera;
    GameObject _model;
    CapsuleCollider _collider;
    Rigidbody _rigidBody;
    Animator _animator;

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

    private void Awake()
    {
        _model = transform.Find("Model").gameObject;
        _collider = GetComponent<CapsuleCollider>();
        _rigidBody =GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + Vector3.down*0.2f);

        if(_collider == null)
            _collider = GetComponent<CapsuleCollider>();
        if (_model == null)
            _model = transform.Find("Model").gameObject;
        Gizmos.DrawLine(gameObject.transform.position + _collider.center, gameObject.transform.position + _collider.center + _model.transform.forward*0.3f);

    }
    void Update()
    {
        ControlMovement();
        if(Physics.Raycast(transform.position, Vector3.down, 0.2f, LayerMask.GetMask("Ground")))
        {
            _isContactGround= true;
            _animator.SetBool("ContactGround", true);
        }
        else
        {
            _isContactGround = false;
            _animator.SetBool("ContactGround", false);
        }
        Ray ray = new Ray(transform.position + _collider.center, _collider.center + _model.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray,out hit, 0.3f, LayerMask.GetMask("Ground")))
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
            if (moveDirection != Vector3.zero)
            {
                float moveAngle = Mathf.Atan2(_rigidBody.linearVelocity.x, _rigidBody.linearVelocity.z) * Mathf.Rad2Deg;
                float deltaAngle = Mathf.DeltaAngle(_model.transform.rotation.eulerAngles.y, moveAngle) * 0.2f;


                _model.transform.rotation = Quaternion.Euler(0, _model.transform.rotation.eulerAngles.y + deltaAngle, 0);

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
                    _isClimbing = true;
                    _animator.SetBool("EndClimbing", true);
                    _climbingStartPos = transform.position;
                    _climbingDestin = transform.position + _collider.center + _model.transform.forward * 0.5f;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 velocity = -_model.transform.forward*5 + _model.transform.up*6;
                    Debug.Log(_model.transform.rotation.eulerAngles.y);
                    Debug.Log(_model.transform.rotation.eulerAngles.y + 180);
                    _model.transform.rotation = Quaternion.Euler(0, 180 + _model.transform.rotation.eulerAngles.y, 0);
                    _rigidBody.linearVelocity = velocity;
                }
            }
        }
        _animator.SetFloat("VelocityY", _rigidBody.linearVelocity.y);

    
    }
}
