using Tripolygon.UModelerX.Runtime;
using UnityEngine;

public class TestController : MonoBehaviour
{
    Camera _camera;
    Animator _animator;
    Rigidbody _rigidBody;
    AnimatorHelper _animatorHelper;

    Vector3 playerInput;
    Vector3 moveDirection;

    public bool EnableInputMove { get; set; } = true;
    public bool EnableInputRotate { get; set; } = true;

    public bool EnableAnimationMove { get; set; } = true;
    public bool EnableAnimationRotate { get; set; } = true;

    bool _isJump;
    bool _isGrounded;

    GameObject _model;

    Vector3 _animationVelocity;
    Vector3 _animationDeltaAngle;
    
    void Awake()
    {
        _camera = Camera.main;
        _animator = GetComponentInChildren<Animator>();
        _model = transform.Find("Model").gameObject;
        _animatorHelper = GetComponentInChildren<AnimatorHelper>();
        _animatorHelper.AnimatorMoved += OnAnimatorMoved;
        _rigidBody = GetComponent<Rigidbody>();
    }

    void OnAnimatorMoved()
    { 
        _animationVelocity += _animator.deltaPosition / Time.deltaTime;
        _animationVelocity.y = 0;
        _model.transform.localPosition += new Vector3(0, _animator.deltaPosition.y, 0);
        _animationDeltaAngle += _animator.deltaRotation.eulerAngles;
    }

    private void Update()
    {
        _isGrounded = Physics.OverlapBox(transform.position, Vector3.one * 0.1f, Quaternion.identity, LayerMask.GetMask("Ground")).Length > 0;
        _animator.SetBool("IsGrounded",_isGrounded);

        playerInput = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            playerInput.y = 1;
        if (Input.GetKey(KeyCode.S))
            playerInput.y = -1;
        if (Input.GetKey(KeyCode.A))
            playerInput.x = -1;
        if (Input.GetKey(KeyCode.D))
            playerInput.x = 1;



        if (_isJump && _isGrounded && _rigidBody.linearVelocity.y < 0f)
        {
            EnableAnimationMove = true;
            EnableAnimationRotate = true;
            _isJump = false;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _animator.SetTrigger("Jump");
            _rigidBody.AddForce(Vector3.up * 200);
            EnableAnimationMove = false;
            EnableAnimationRotate = false;
            _isJump = true;
            _model.transform.localPosition = Vector3.zero;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetTrigger("Attack");
            EnableInputRotate = false;
            StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_animator, "Attack", () => { EnableInputRotate = true; }));
        }

        ControlRotate();
        ControlMove();

    }

    void ControlRotate()
    {
        Vector3 characterForward = transform.forward;
        characterForward.y = 0;
        characterForward.Normalize();

        Vector3 cameraForward = _camera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        moveDirection = cameraForward * playerInput.y + Vector3.Cross(Vector3.up, cameraForward) * playerInput.x;

        if (moveDirection == Vector3.zero) return;
        float deltaAngle = Vector3.SignedAngle(characterForward, moveDirection, Vector3.up);

      
        if (EnableInputRotate)
        {
            if (Mathf.Abs(deltaAngle) > 150)
            {
                _animator.SetTrigger("Turn");
                EnableInputRotate = false;
                _animator.SetFloat("Speed", 1);
                StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_animator,new string[] { "Walking Turn 180" , "Running Turn 180" }, () => { 
                    EnableInputRotate = true;
                    _model.transform.localPosition = Vector3.zero;
                }));
                return;
            }
            transform.Rotate(Vector3.up, deltaAngle * Time.deltaTime * 10);
        }

    }
    void ControlMove()
    {
        float speedPara = (moveDirection.x !=  0 || moveDirection.y != 0) ?1:0;


        if (Input.GetKey(KeyCode.LeftShift))
            speedPara *= 2;
        _animator.SetFloat("Speed", speedPara,0.2f,Time.deltaTime);

        if (EnableAnimationMove)
        {
            _rigidBody.linearVelocity = _animationVelocity;
        }
        if(EnableAnimationRotate)
            transform.Rotate(Vector3.up, _animationDeltaAngle.y);
        _animationVelocity = Vector3.zero;
        _animationDeltaAngle = Vector3.zero;
    }
}
