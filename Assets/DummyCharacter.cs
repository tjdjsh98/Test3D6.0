using UnityEngine;

public class DummyCharacter : MonoBehaviour
{
    Camera _camera;
    Animator _animator;
    Rigidbody _rigidBody;

    [SerializeField] float _speed;

    private void Awake()
    {
        _animator = transform.Find("Model").GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();
        _camera = Camera.main;
    }


    private void Update()
    {
        ControlMovement();
    }

    void ControlMovement()
    {
        if (_animator != null)
        {
            _animator.SetFloat("Velocity", _rigidBody.linearVelocity.magnitude);
        }

        Vector3 moveDirection = Vector3.zero;
        if(Input.GetKey(KeyCode.W)) moveDirection += _camera.transform.forward;
        if(Input.GetKey(KeyCode.S)) moveDirection -= _camera.transform.forward;
        if(Input.GetKey(KeyCode.D)) moveDirection += _camera.transform.right;
        if(Input.GetKey(KeyCode.A)) moveDirection -= _camera.transform.right;

        moveDirection.y = 0;
        float total = Mathf.Abs( moveDirection.x) +Mathf.Abs( moveDirection.z);
        if (total != 0)
        {
            moveDirection.x /= total;
            moveDirection.z /= total;
        }
        _rigidBody.linearVelocity = new Vector3(moveDirection.x * _speed, _rigidBody.linearVelocity.y, moveDirection.z * _speed);

        transform.LookAt(transform.position + moveDirection);

    }
}
