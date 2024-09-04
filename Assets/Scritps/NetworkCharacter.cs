using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Runtime.InteropServices;
using Tripolygon.UModeler.UI;
using Tripolygon.UModelerX.Runtime;
using UnityEngine;

public class NetworkCharacter : NetworkBehaviour, IDamageable,IRigidbody
{
    [Header("Status")]
    [Networked][field:SerializeField] public int MaxHp { get; set; }
    [Networked][field: SerializeField] public int Hp { get; set; }
    [Networked][field: SerializeField] public float Speed { get; set; }
    [Networked][field: SerializeField] public int Power { get; set; }
    [Networked][field: SerializeField] public int MaxHunger{ get; set; }
    [Networked][field: SerializeField] public int Hunger { get; set; }

    [Header("ActionState")]
    [Networked][field:SerializeField] public bool IsAttack { get; set; }
    [Networked][field:SerializeField] public bool IsRun { get; set; }
    [Networked][field:SerializeField] public bool IsDamaged { get; set; }


    // Velocity
    Vector3 _velocity;
    float _jumpImpulse = 0;
    float _breakPower = 50;
    
    // Components
    Animator _animator;
    SimpleKCC _kcc;
    NetworkMecanimAnimator _networkAnimator;
    NetworkManager _networkManager;
    // Handler
    public Action Attacked { get; set; }
    public Action AttackEnded { get; set; }

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _networkAnimator = GetComponent<NetworkMecanimAnimator>();
        _kcc = GetComponent<SimpleKCC>();
    }

    public override void Render()
    {
        if (Input.GetKeyDown(KeyCode.L))
            Hp -= 1;
    }
    public override void FixedUpdateNetwork()
    {
        
        HandleVelocity();
    }


    public override void Spawned()
    {
        _networkManager = FindAnyObjectByType<NetworkManager>(FindObjectsInactive.Include);

        _networkManager.AddValueChanged<int>(this, nameof(Hp), OnHpChanged);
    }
    public void HandleVelocity()
    {
        SetAnimatorBoolean("ContactGround", _kcc.IsGrounded);
        _kcc.Move(_velocity, _jumpImpulse);

        if (_velocity != Vector3.zero)
        {
            Vector3 breakPower = _velocity.normalized * Runner.DeltaTime * _breakPower;
            if (_velocity.magnitude < breakPower.magnitude)
            {
                _velocity = Vector3.zero;
            }
            else
            {
                _velocity -= breakPower;
            }
        }


        _jumpImpulse = 0;
    }
    public void Move(Vector3 direction, float ratio = 1)
    {
        ratio = Mathf.Clamp01(ratio);

        _velocity = direction * Speed * ratio;
    }
    public void Jump(float power)
    {
        _jumpImpulse = power;
    }

    public void AddForce(Vector3 power, ForceMode forceMode = ForceMode.Impulse)
    {
        _velocity += power;
    }

    public int Damaged(DamageInfo damageInfo)
    {
        int result = 0;

        result = damageInfo.damage;
        Hp -= result;

        if(Hp <= 0)
        {
            Dead();
        }
        else
        {
            if(damageInfo.knockbackPower > 0)
            {
                AddForce(damageInfo.knockbackDirection * damageInfo.knockbackPower);
            }
        }
        return result;
    }

    void Dead()
    {

    }
    void OnHpChanged(int old, int current)
    {
        Debug.Log($"Hp Changed {old} -> {current}");
    }

    public void OnAttacked()
    {
        Attacked?.Invoke();
    }
     public void OnAttackEnded()
    {
        AttackEnded?.Invoke();
    }

    public void SetAnimatorTrigger(string name)
    {
        _animator.SetTrigger(name);
        _networkAnimator.SetTrigger(name);
    }
    public void SetAnimatorBoolean(string name, bool boolean)
    {
        _animator.SetBool(name,boolean);
    }
    public void SetAnimatorFloat(string name, float value)
    {
        _animator.SetFloat(name,value);
    }
    public void SetAnimatorInt(string name, int value)
    {
        _animator.SetInteger(name, value);
    }
}
