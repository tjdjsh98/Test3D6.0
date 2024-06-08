using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    NetworkCharacterController _characterController;
    CharacterController _cc;
    Animator _animator;

    public Camera _playerCamera;
    public float _walkSpeed = 6f;
    public float _runSpeed = 12f;
    public float _jumpPower = 7f;
    public float _gravity = 10f;

    public float _lookSpeed = 2f;
    public float _lookXLimit = 45f;

    Vector3 _moveDirection = Vector3.zero;
    float _rotationX = 0f;

    public bool _canMove = true;


    [SerializeField] float _speed = 5;
    [SerializeField]Vector3 _relativeVelocity;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _cc = GetComponent<CharacterController>();
        _characterController = GetComponent<NetworkCharacterController>();
        
    }

    public override void FixedUpdateNetwork()
    {
        if (_playerCamera == null && Runner.ProvideInput)
        {
            _playerCamera = Camera.main;
            _playerCamera.transform.SetParent(transform);    
        }
        if (_playerCamera)
        {
            _playerCamera.transform.localPosition = Vector3.up * 1.4f;
        }


        HandleMove();
    }

    void HandleMove()
    {
        if (_characterController == null) return;

        if (GetInput(out NetworkInputData data))
        {
            _characterController.Move(data.Direction * Runner.DeltaTime * 5);
            //Vector3 forward = transform.forward;
            //Vector3 right = transform.right;

            //bool isRunning = Input.GetKey(KeyCode.LeftShift);

            //float curSpeedX = _canMove ? (isRunning ? _runSpeed : _walkSpeed) * data.direction.x : 0;
            //float curSpeedY = _canMove ? (isRunning ? _runSpeed : _walkSpeed) * data.direction.z : 0;
            //float movementDirectionY = _moveDirection.y;
            //_moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            ////if (Input.GetKey(KeyCode.Space) && _canMove && _characterController.Grounded)
            ////{
            ////    _moveDirection.y = _jumpPower;
            ////}
            ////else
            ////{
            ////    _moveDirection.y = movementDirectionY;
            ////}

            //Debug.Log(_characterController);

            //if (!_characterController.Grounded)
            //{
            //    _moveDirection.y -= _gravity * Runner.DeltaTime;
            //}

            //Debug.Log(_moveDirection);

            //_characterController.Move(_moveDirection * Runner.DeltaTime);

            //if (_canMove)
            //{
            //    _rotationX += -Input.GetAxis("Mouse Y") * _lookSpeed;
            //    _rotationX = Mathf.Clamp(_rotationX, -_lookXLimit, _lookXLimit);
            //    _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            //    transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * _lookSpeed, 0);
            //}

        }
    }
}
