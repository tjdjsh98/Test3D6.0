using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class Character : MonoBehaviour
{
    Rigidbody _rigidBody;
    CapsuleCollider _collider;
    Animator _animator;
    NavMeshAgent _navAgent;

    public Animator Animator => _animator;
    [Header("Status")]
    [SerializeField] protected int _maxHp;
    [SerializeField] protected int _hp;
    [SerializeField] protected float _speed;
    [SerializeField] protected int _power;
    [SerializeField] protected int _maxHunger;
    [SerializeField] protected int _hunger;


    [Header("ActionState")]
    [field: ReadOnly][field: SerializeField] public bool IsAttack { get; set; }
    [field: ReadOnly][field: SerializeField] public bool IsMove { get; set; }
    [field: ReadOnly][field: SerializeField] public bool IsKnockback { get; set; }


    public int MaxHp => _maxHp;
    public int HP => _hp;
    public int MaxHunger => _maxHunger;
    public int Hunger => _hunger;
    public float Speed => _speed;

    public Action Attacked { get; set; }



    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _animator = GetComponentInChildren<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
    }

    public void Attack()
    {
        Attacked?.Invoke();
    }

    private void Update()
    {
        HandleGroundFriction();
    }
    // 캐릭터끼리 밀리지 않게 방지해준다.
    void HandleGroundFriction()
    {
        if (IsKnockback || IsMove)
        {
            _rigidBody.mass = 10;
        }
        else
        {
            _rigidBody.mass = float.MaxValue;
        }
    }

    public void Damaged(DamageInfo info)
    {
        {
            IsKnockback = true;
            Invoke("OffKnockback", 1f);
            _rigidBody.mass = 10;
            _navAgent.enabled = false;  
        }
        if (info.knockbackDirection != Vector3.zero)
        {
            _rigidBody.AddForce(info.knockbackDirection.normalized *info.knockbackPower, ForceMode.Impulse);
        }

        _hp -= info.damage;

        if(_hp <= 0)
        {
            Destroy(gameObject);
        }

       
    }

    void OffKnockback()
    {
        IsKnockback = false;
        _navAgent.enabled = true;
    }

    public Vector3 GetCenterWS()
    {
        return transform.position  + _collider.center;
    }
    public void SetAnimatorTrigger(string name)
    {
        _animator.SetTrigger(name);
    }

    public void SetAnimatorBoolean(string name, bool boolean)
    {
        _animator.SetBool(name, boolean);
    }

    public void SetAnimatorFloat(string name, float value)
    {
        _animator.SetFloat(name, value);
    }
}
