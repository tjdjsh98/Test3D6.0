using Fusion;
using Fusion.Addons.KCC;
using System;
using UnityEngine;

	[DefaultExecutionOrder(-5)]
public class PrototypeCharacter : NetworkBehaviour, IDamageable
{
    public GameObject GameObject => gameObject;

    public int MaxHp { get; set; }
    public int Hp { get; set; }
    public Action<DamageInfo> Damaged { get; set; }
    public Action<DamageInfo> Died { get; set; }

    // Component
    KCC _kcc;
    EnvironmentProcessor _environmentProcessor;
    // Velocity
    [SerializeField]Vector3 _velocity;
    float _lookAngle;
    float _jumpImpulse;
    public bool IsGrounded { get; set; }

    public Vector3 Velocity => _velocity;

    private void Awake()
    {
        _kcc = GetComponent<KCC>();
        
    }

    public override void Spawned()
    {
        _environmentProcessor = _kcc.GetProcessor<EnvironmentProcessor>();
    }


    public override void FixedUpdateNetwork()
    {
        IsGrounded = _kcc.Data.IsGrounded;

        _kcc.AddLookRotation(0, _lookAngle);
        _velocity.y = _kcc.Data.RealVelocity.y;
        _kcc.SetInputDirection(_velocity);
        _kcc.Jump(Vector3.up * _jumpImpulse);

        if(_kcc.Data.RealVelocity.y > 0 )
        {
            _environmentProcessor.Gravity = Vector3.down * 3f;
        }
        else
        {
            _environmentProcessor.Gravity = Vector3.down * 4f;
        }

        _lookAngle = 0;
        _jumpImpulse = 0;
    }

    public int Damage(DamageInfo damageInfo)
    {
        return 0;   
    }

    public void Move(Vector3 velocity)
    {
        _velocity = velocity;
    }

    public void Jump(float power)
    {
        _jumpImpulse = power;
    }

    public void AddForce(Vector3 force)
    {
        _velocity += force;
    }

    public void AddLookAngle(float angle)
    {
        _lookAngle += angle;
    }
}
