using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    CharacterController _cc;
    Animator _animator;

    [SerializeField] float _speed = 5;
    [SerializeField]Vector3 _relativeVelocity;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        float accel = 30;
        if (Input.GetKey(KeyCode.W))
            _relativeVelocity += transform.forward * Time.deltaTime* accel;
        if (Input.GetKey(KeyCode.S))
            _relativeVelocity -= transform.forward * Time.deltaTime* accel;
        if (Input.GetKey(KeyCode.A))
            _relativeVelocity -= transform.right * Time.deltaTime*accel;
        if (Input.GetKey(KeyCode.D))
            _relativeVelocity += transform.right * Time.deltaTime*accel;

        if (_relativeVelocity.magnitude > _speed)
            _relativeVelocity = _relativeVelocity.normalized * _speed;

        _animator.SetFloat("VelocityX", transform.worldToLocalMatrix.MultiplyVector(_relativeVelocity).x);
        _animator.SetFloat("VelocityZ", transform.worldToLocalMatrix.MultiplyVector(_relativeVelocity).z);

        if (_relativeVelocity != Vector3.zero)
        {
            Vector3 look = Camera.main.transform.forward;
            look.y = 0;
            transform.rotation = Quaternion.LookRotation(Vector3.Slerp(transform.forward,look, 0.1f));
            _cc.SimpleMove(_relativeVelocity);
        }
        float breakPower = 20;
        Vector3 breakDirection = -_relativeVelocity.normalized;

        if(Mathf.Abs(_relativeVelocity.x) < Mathf.Abs(breakDirection.x * breakPower * Time.deltaTime))
            _relativeVelocity.x = 0;
        else
            _relativeVelocity.x += breakDirection.x * breakPower * Time.deltaTime;

        if(Mathf.Abs(_relativeVelocity.z) < Mathf.Abs(breakDirection.z * breakPower * Time.deltaTime))
                _relativeVelocity.z = 0;
        else
            _relativeVelocity.z += breakDirection.z * breakPower * Time.deltaTime;
    }
}
