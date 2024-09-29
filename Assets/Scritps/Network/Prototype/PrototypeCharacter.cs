using Fusion;
using Fusion.Addons.KCC;
using System;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class PrototypeCharacter : NetworkBehaviour, IDamageable, IRigidbody
{
    public GameObject GameObject => gameObject;

    // CharacteState
    [field:SerializeField][Networked] public int MaxHp { get; set; }
    [field: SerializeField][Networked] public int Hp { get; set; }

    [field: SerializeField][Networked] public float MaxStamina{ get; set; }
    [field: SerializeField][Networked] public float Stamina { get; set; }
    [field: SerializeField][Networked] public float StaminaRecover { get; set; } = 1;
    [field: SerializeField][Networked] public float StaminaRecoverMultifly { get; set; } = 1;
    [field: SerializeField][Networked] public bool IsEnableMoveYAxis { get; set; } = false;


    // Handler
    public Action<DamageInfo> Damaged { get; set; }
    public Action<DamageInfo> Died { get; set; }

    // Component
    KCC _kcc;
    EnvironmentProcessor _environmentProcessor;
    Animator _animator;
    NetworkMecanimAnimator _networkAnimator;

    // Velocity
    Vector3 _desiredVelocity;
    float _lookAngle;
    Vector3 _jumpImpulse;
    [Networked]public bool IsGrounded { get; set; }
    [Networked]public bool IsUseStamina { get; set; }
    [Networked]public bool IsExhaust { get; set; }
    [Networked] public bool IsEnableMove { get; set; } = true;

    bool _isTeleport;
    Vector3 _teleportPos;

    private void Awake()
    {
        _kcc = GetComponent<KCC>();
        _animator = GetComponentInChildren<Animator>();
        _networkAnimator = GetComponent<NetworkMecanimAnimator>();
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
        _desiredVelocity = Vector3.Lerp(_desiredVelocity, Vector3.zero, Runner.DeltaTime * 10f);
    }

    public int Damage(DamageInfo damageInfo)
    {
        Hp -= damageInfo.damage;

        AddForce(damageInfo.knockbackPower * damageInfo.knockbackDirection);
        IsEnableMove = false;

        Invoke("ActiveEnableMove", 1);
        return 0;   
    }

    void ActiveEnableMove()
    {
        IsEnableMove = true;
    }

    public void Jump( Vector3 direction, float power)
    {
        _jumpImpulse = direction.normalized * power;
    }

    public void AddForce(Vector3 force)
    {
        _desiredVelocity += force;
    }

    public void AddLookAngle(float angle)
    {
        _lookAngle += angle;
    }
    public void HandleVelocity()
    {
        _animator?.SetFloat("FallSpeed", _kcc.Data.DynamicVelocity.y);
        if(_isTeleport)
        {
            _kcc.SetPosition(_teleportPos);
            _isTeleport = false;
        }

        IsGrounded = _kcc.Data.IsGrounded;

        float accelPower = 100;

        if (!IsGrounded && !IsEnableMoveYAxis)
        {
            accelPower = 0f;
        }
        Vector3 desiredVelocity = _desiredVelocity;
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
                Stamina -= Runner.DeltaTime;
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
                    Stamina += Runner.DeltaTime * StaminaRecover * StaminaRecoverMultifly;
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


        if (_jumpImpulse != Vector3.zero)
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

        _lookAngle = 0;
        _jumpImpulse = Vector3.zero;
    }
    public void Move(Vector3 velocity, float ratio = 1)
    {
        if (!IsEnableMove) return;
        _desiredVelocity = velocity * ratio;
    }
    public void AddForce(Vector3 power, ForceMode forceMode = ForceMode.Impulse)
    {
        if(forceMode== ForceMode.Impulse)
        {
            _desiredVelocity = power;
        }
        else if(forceMode == ForceMode.Force)
        {
            _desiredVelocity += power;
        }
    }
    public void Teleport(Vector3 position)
    {
        _isTeleport = true;
        _teleportPos = position;
    }
    public void AnimatorSetTrigget(string name)
    {
        _animator?.SetTrigger(name);
        if (HasStateAuthority)
            _networkAnimator?.SetTrigger(name);
    }
}
