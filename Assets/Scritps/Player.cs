using UnityEngine;

public class Player : MonoBehaviour
{
    CharacterController _cc;
    Animator _animator;

    [SerializeField] Transform _camera;
    [SerializeField] float _speed = 5;
    [SerializeField]Vector3 _relativeVelocity;

    float _startRotation;
    Vector3 _lastInputRotation;

    [Range(0,1)]public float DIstanceToGround;
    public LayerMask layerMask;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _cc = GetComponent<CharacterController>();
        
    }

    private void Update()
    {
        HandleMove();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if(_animator)
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,1f);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            RaycastHit hit;
            Ray ray = new Ray(_animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
            if(Physics.Raycast(ray, out hit, DIstanceToGround + 1f, layerMask))
            {
                if (hit.collider.tag == "Walkable")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DIstanceToGround;
                    _animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    _animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }

            ray = new Ray(_animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, DIstanceToGround + 1f, layerMask))
            {
                if (hit.collider.tag == "Walkable")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DIstanceToGround;
                    _animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                    _animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }
        }
    }

    void HandleMove()
    {
        var moveInput = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveInput += Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            moveInput += Vector3.back;
        if (Input.GetKey(KeyCode.A))
            moveInput += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            moveInput += Vector3.right;

        var moveInputAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;
        var moveRotation = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, moveInputAngle);


        if (moveInput != _lastInputRotation)
        {
            _startRotation = moveRotation;
            _lastInputRotation = moveInput;
        }

        _animator.SetFloat("Speed", moveInput.magnitude,0.1f,Time.deltaTime);
        _animator.SetFloat("Rotation", moveRotation, 0.35f,Time.deltaTime);
        _animator.SetFloat("Start Rotation", _startRotation, 0.1f,Time.deltaTime);


    }
}
