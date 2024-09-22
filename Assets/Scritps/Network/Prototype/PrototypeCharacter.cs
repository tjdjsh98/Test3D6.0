using Fusion;
using Fusion.Addons.KCC;
using System;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-5)]
public class PrototypeCharacter : NetworkBehaviour, IDamageable
{
    public GameObject GameObject => gameObject;

    [field:SerializeField][Networked] public int MaxHp { get; set; }
    [field: SerializeField][Networked] public int Hp { get; set; }

    [field: SerializeField][Networked] public float MaxStamina{ get; set; }
    [field: SerializeField][Networked] public float Stamina { get; set; }
    public Action<DamageInfo> Damaged { get; set; }
    public Action<DamageInfo> Died { get; set; }

    // Component
    KCC _kcc;
    EnvironmentProcessor _environmentProcessor;
    // Velocity
    [SerializeField]Vector3 _velocity;
    Vector3 _desiredVelocity;
    float _lookAngle;
    Vector3 _jumpImpulse;
    [Networked]public bool IsGrounded { get; set; }
    [Networked]public bool IsUseStamina { get; set; }
    [Networked]public bool IsExhaust { get; set; }
    public Vector3 Velocity => _velocity;

    private void Awake()
    {
        _kcc = GetComponent<KCC>();
        
    }

    public override void Spawned()
    {
        _environmentProcessor = _kcc.GetProcessor<EnvironmentProcessor>();
        Hp = MaxHp;
        Stamina = MaxStamina;
    }


    public override void FixedUpdateNetwork()
    {
        IsGrounded = _kcc.Data.IsGrounded;

        float accelPower = 50;

        if(!IsGrounded)
        {
            accelPower = 0f;
        }
        Vector3 desiredVelocity = _velocity;
        desiredVelocity = Vector3.Lerp(_kcc.Data.DynamicVelocity, _desiredVelocity, accelPower * Runner.DeltaTime);
        
        desiredVelocity.y = _kcc.Data.DynamicVelocity.y;

        _kcc.AddLookRotation(0, _lookAngle);

        if (IsUseStamina && Stamina > 0 && !IsExhaust)
        {
            if (Runner.IsFirstTick)
            {
                Stamina -= Runner.DeltaTime * 2;
                if(Stamina < 0) 
                {
                    IsExhaust = true;
                }
            }
        }
        else
        {
            if (Runner.IsFirstTick)
            {
                if (Stamina < MaxStamina)
                {
                    Stamina += Runner.DeltaTime;
                }
                if (Stamina >= MaxStamina)
                { 
                    Stamina = MaxStamina;
                    if (IsExhaust)
                    {
                        IsExhaust = false;
                    }
            }
            }
        }
        if(IsGrounded)
            _kcc.SetDynamicVelocity(desiredVelocity);

        if (_jumpImpulse != Vector3.zero)
        {
            _kcc.Jump(_jumpImpulse);
        }

        if(_kcc.Data.RealVelocity.y > 0 )
        {
            _environmentProcessor.Gravity = Vector3.down * 3f;
        }
        else
        {
            _environmentProcessor.Gravity = Vector3.down * 4f;
        }


        _lookAngle = 0;
        _jumpImpulse = Vector3.zero;
    }

    public int Damage(DamageInfo damageInfo)
    {
        return 0;   
    }

    public void Move(Vector3 velocity)
    {
        _desiredVelocity = velocity;
    }

    public void Jump(float power, Vector3 direction)
    {
        _jumpImpulse = direction.normalized * power;
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
