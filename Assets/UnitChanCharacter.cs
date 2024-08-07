using UnityEngine;
using UnityEngine.UIElements;

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
    bool _isJump = false;

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
        Gizmos.DrawLine(gameObject.transform.position + _collider.center, gameObject.transform.position + _collider.center +transform.forward*0.3f);

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

        if (Physics.Raycast(transform.position + _collider.center, _collider.center + transform.forward , 0.3f, LayerMask.GetMask("Ground")))
        {
            _rigidBody.useGravity = false;
            _isContactWall = true;
            _animator.SetBool("ContactWall", true);
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
        Vector3 moveDirection = Vector3.zero;

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
            Vector3 dir = _camera.transform.forward;
            dir.y = 0;
            dir.Normalize();
            moveDirection += dir;

        }
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 dir = _camera.transform.forward;
            dir.y = 0;
            dir.Normalize();
            moveDirection -= dir;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 dir = _camera.transform.right;
            dir.y = 0;
            dir.Normalize();
            moveDirection -= dir;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 dir = _camera.transform.right;
            dir.y = 0;
            dir.Normalize();
            moveDirection += dir;
        }

       

        _animator.SetFloat("Velocity", Mathf.Abs(_rigidBody.linearVelocity.x) + Mathf.Abs(_rigidBody.linearVelocity.z));

        if (_isContactGround)
        {
            if (!_isJump)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 velocity = _rigidBody.linearVelocity;
                    velocity.y = 5;
                    _rigidBody.linearVelocity = velocity;
                    Debug.Log(_rigidBody.linearVelocity);
                    _isJump = true;

                }
                if (moveDirection != Vector3.zero)
                {
                    float moveAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                    float deltaAngle = Mathf.DeltaAngle(_model.transform.rotation.eulerAngles.y, moveAngle) * 0.2f;

                    _model.transform.rotation = Quaternion.Euler(0, _model.transform.rotation.eulerAngles.y + deltaAngle, 0);

                    moveDirection.Normalize();
                    _rigidBody.linearVelocity = new Vector3(moveDirection.x * _maxSpeed, _rigidBody.linearVelocity.y, moveDirection.z * _maxSpeed);
                }
                
            }
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                _isJump = false;
            }
        }
        else
        {
            if (_isContactWall)
            {
                _animator.SetFloat("MoveWall", moveDirection.z);
                _rigidBody.linearVelocity = Vector3.up * moveDirection.z;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 velocity = _rigidBody.linearVelocity;
                    velocity.y = 5;
                    _rigidBody.linearVelocity = velocity;
                }
            }
        }
        _animator.SetFloat("VelocityY", _rigidBody.linearVelocity.y);

    
    }
}
