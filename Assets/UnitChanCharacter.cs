using UnityEngine;

public class UnitChanCharacter : MonoBehaviour
{
    Rigidbody _rigidBody;
    Animator _animator;

    float _maxSpeed = 5f;
    private void Awake()
    {
        _rigidBody =GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _rigidBody.maxLinearVelocity = _maxSpeed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + Vector3.down*0.2f);

    }
    void Update()
    {
        ControlMovement();
        if(Physics.Raycast(transform.position, Vector3.down, 0.2f, LayerMask.GetMask("Ground")))
        {
            _animator.SetBool("ContactGround", true);
        }
        else
        {
            _animator.SetBool("ContactGround", false);
        }
    }

    void ControlMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if(Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector3.right;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            _rigidBody.AddForce(Vector3.up * 1000f,ForceMode.Impulse);
            
        }

        if (moveDirection != Vector3.zero)
        {
            float moveAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float deltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, moveAngle)*0.2f;

            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + deltaAngle, 0);

            _rigidBody.AddForce(moveDirection* 500);
        }

        _animator.SetFloat("Velocity", Mathf.Abs(_rigidBody.linearVelocity.x )+ Mathf.Abs(_rigidBody.linearVelocity.z));
        _animator.SetFloat("VelocityY", _rigidBody.linearVelocity.y);
    }
}
