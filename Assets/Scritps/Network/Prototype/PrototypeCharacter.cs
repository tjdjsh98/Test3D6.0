using Fusion;
using Fusion.Addons.KCC;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-5)]
public class PrototypeCharacter : NetworkBehaviour, IDamageable, IRigidbody
{
    public GameObject GameObject => gameObject;

    // CharacteState
    [field:SerializeField][Networked] public int MaxHp { get; set; }
    [field: SerializeField][Networked] public int Hp { get; set; }

    [field: SerializeField][Networked] public float MaxStamina{ get; set; }
    [field: SerializeField][Networked] public float Stamina { get; set; }
    [field: SerializeField][Networked] public bool IsEnableMoveYAxis { get; set; } = false;


    // Handler
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


    bool _isTeleport;
    Vector3 _teleportPos;

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
        HandleVelocity();
    }

    public override void Render()
    {
        _kcc.SetDynamicVelocity(_velocity);
    }

    public int Damage(DamageInfo damageInfo)
    {
        return 0;   
    }

    public void Jump( Vector3 direction, float power)
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

    public void HandleVelocity()
    {
        if(_isTeleport)
        {
            _kcc.SetPosition(_teleportPos);
            _isTeleport = false;
        }

        IsGrounded = _kcc.Data.IsGrounded;

        float accelPower = 50;

        if (!IsGrounded && !IsEnableMoveYAxis)
        {
            accelPower = 0f;
        }
        Vector3 desiredVelocity = _velocity;
        desiredVelocity = Vector3.Lerp(_kcc.Data.DynamicVelocity, _desiredVelocity, accelPower * Runner.DeltaTime);

        if (!IsEnableMoveYAxis)
        {
            desiredVelocity.y = _kcc.Data.DynamicVelocity.y;
        }
        else 
        {
            if(_desiredVelocity == Vector3.zero)
            {
                desiredVelocity.y = 0;
            }
        }

        _kcc.AddLookRotation(0, _lookAngle);

        if (IsUseStamina && Stamina > 0 && !IsExhaust)
        {
            if (Runner.IsFirstTick)
            {
                Stamina -= Runner.DeltaTime * 2;
                if (Stamina < 0)
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


        if (IsGrounded &&_jumpImpulse != Vector3.zero)
        {
            _kcc.Jump(_jumpImpulse);
        }

        // 공중에서 떨어지는 속도와 떨어지는 속도 다르게 조절
        if (!IsEnableMoveYAxis)
        {
            if (_kcc.Data.RealVelocity.y > 0)
            {
                _environmentProcessor.Gravity = Vector3.down * 8f;
            }
            else
            {
                _environmentProcessor.Gravity = Vector3.down * 9.8f;
            }
        }
        else
        {
            _environmentProcessor.Gravity = Vector3.up * 0.2f;
        }

        _kcc.SetDynamicVelocity(desiredVelocity);


        _velocity = desiredVelocity;
        _lookAngle = 0;
        _jumpImpulse = Vector3.zero;
    }


    public void Move(Vector3 velocity, float ratio = 1)
    {
        _desiredVelocity = velocity * ratio;
    }

    public void AddForce(Vector3 power, ForceMode forceMode = ForceMode.Impulse)
    {
        if(forceMode== ForceMode.Impulse)
        {
            _velocity = power;
        }
        else if(forceMode == ForceMode.Force)
        {
            _velocity += power;
        }
    }

    public void Teleport(Vector3 position)
    {
        _isTeleport = true;
        _teleportPos = position;
    }
}
