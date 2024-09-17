using Fusion;
using Fusion.Addons.KCC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NetworkCharacter : NetworkBehaviour, IDamageable, IRigidbody
{
    public GameObject GameObject => gameObject;

    [Header("Status")]
    [Networked][field: SerializeField] public int MaxHp { get; set; }
    [Networked, OnChangedRender(nameof(OnHpChanged))][field: SerializeField] public int Hp { get; set; }
    [Networked][field: SerializeField] public float Speed { get; set; }
    [Networked][field: SerializeField] public int Power { get; set; }
    [Networked][field: SerializeField] public int MaxHunger { get; set; }
    [Networked][field: SerializeField] public int Hunger { get; set; }

    [Header("ActionState")]
    [Networked][field: SerializeField] public bool IsAttack { get; set; }
    [Networked][field: SerializeField] public bool IsRun { get; set; }
    [Networked, OnChangedRender(nameof(OnGetHitChanged))][field: SerializeField] public bool IsGetHit { get; set; }
    [Networked][field: SerializeField] public bool IsGrounded { get; set; } = true;

    // Velocity
    public Vector3 Velocity { get; set; }
    float LookAnlge { get; set;}
    float _jumpImpulse = 0;
    float _breakPower = 50;


    // Components
    Animator _animator;
    KCC _kcc;
    NetworkMecanimAnimator _networkAnimator;
    NetworkManager _networkManager;
    NavMeshAgent _navMeshAgent;
    CapsuleCollider _collider;

    // Handler
    public Action Attacked { get; set; }
    public Action AttackEnded { get; set; }
    public Action<DamageInfo> Died { get; set; }
    public Action<DamageInfo> Damaged { get; set; }
    public Action GetHitEnded { get; set; }


    // WaitAnimationState
    Dictionary<string, Coroutine> _waitAnimationStateDic = new Dictionary<string, Coroutine>();
    float DeltaAngle;
  
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _networkAnimator = GetComponent<NetworkMecanimAnimator>();
        _kcc = GetComponent<KCC>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<CapsuleCollider>();

        _animator.logWarnings = false;

    }

    public override void Render()
    {
        CheckGround();
        SetAnimatorBoolean("IsGrounded", IsGrounded);
        DeltaAngle = 0;

    }
    public override void FixedUpdateNetwork()
    {
        HandleVelocity();
    }
    public void HandleVelocity()
    {
        if (_kcc)
        {
            SetAnimatorFloat("VelocityY", _kcc.FixedData.RealVelocity.y);
            _kcc.AddLookRotation(0,DeltaAngle);

            DeltaAngle = 0;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y + DeltaAngle, 0);
            //DeltaAngle = 0;
        }

        Vector3 desiredMoveVelocity = Velocity;
        _breakPower = IsGrounded ? 50 : 1;

        float acceleration = 0;

        if (desiredMoveVelocity == Vector3.zero)
        {
            acceleration = _breakPower;
        }
        else
        {
            acceleration = IsGrounded ? 50 : 20;
        }

        Velocity = Vector3.Lerp(_kcc?_kcc.Data.RealVelocity:Velocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);
        _kcc?.SetKinematicVelocity(Velocity);
        _kcc?.Jump(Vector3.up * _jumpImpulse);
        _jumpImpulse = 0;

        Velocity = Vector3.Lerp(Velocity, Vector3.zero, _breakPower * Runner.DeltaTime);
    }
    // KCC가 있다면 KCC가 확인
    // 없다면 레이캐스트로 구별한다.
    void CheckGround()
    {
        if (_kcc)
        {
            IsGrounded = _kcc.Data.IsGrounded;
        }
        else
        {
            IsGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f, Define.GROUND_LAYERMASK);
        }
    }



    public override void Spawned()
    {
      
    }
   
    public void Move(Vector3 direction, float ratio = 1)
    {
        ratio = Mathf.Clamp01(ratio);

        Velocity = direction * Speed * ratio;
    }
    public void Jump(float power)
    {
        _jumpImpulse = power;
    }

    public void AddForce(Vector3 power, ForceMode forceMode = ForceMode.Impulse)
    {
        Velocity += power;
    }

    public int Damage(DamageInfo damageInfo)
    {
        int result = 0;

        result = damageInfo.damage;
        Hp -= result;

        IsGetHit = true;

        Damaged?.Invoke(damageInfo);
        if (Hp <= 0)
        {
            CharacterDie(damageInfo);
        }
        else
        {
            if (damageInfo.knockbackPower > 0)
            {
                AddForce(damageInfo.knockbackDirection * damageInfo.knockbackPower);
            }
        }
        return result;
    }

    void OnGetHitChanged()
    {
        if(IsGetHit)
        {
            Debug.Log("Hti");
            SetAnimatorTrigger("GetHit");
            WaitAnimationState("GetHit", OnGetHit);
        }
    }
    void OnGetHit()
    {
        IsGetHit = false;
        GetHitEnded?.Invoke();
    }

    void CharacterDie(DamageInfo info)
    {
        Died?.Invoke(info);
        Runner.Despawn(Object);
    }
    void OnHpChanged(NetworkBehaviourBuffer previous)
    {
        var preValue = GetPropertyReader<int>(nameof(Hp)).Read(previous);
        Debug.Log($"Hp Changed {preValue} -> {Hp}");
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
        
        if(HasStateAuthority)
            _networkAnimator.SetTrigger(name);
        else
            _animator.SetTrigger(name);
    }
    public void SetAnimatorBoolean(string name, bool boolean)
    {
           _animator.SetBool(name, boolean);
        
    }
    public void SetAnimatorFloat(string name, float value,float dampTime= 0,float deltaTime =0.01f)
    {
            _animator.SetFloat(name, value, dampTime, deltaTime);
    }
    public void SetAnimatorInt(string name, int value)
    {
             _animator.SetInteger(name, value);
    }


    // 특정 애니메이션 상태를 기다려 줍니다.
    // endRatio은 Normalize된 값으로 ended가 실행될 시간을 정해줍니다.
    public void WaitAnimationState(string stateName, Action ended,float endRatio = 1)
    {
        if(_waitAnimationStateDic.ContainsKey(stateName))
        {
            StopCoroutine(_waitAnimationStateDic[stateName]);
            _waitAnimationStateDic.Remove(stateName);
        }
        _waitAnimationStateDic.Add(stateName,StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_animator,stateName,ended,endRatio)));
    }
    public void WaitAnimationState(string[] stateNames, Action ended, float endRatio = 1)
    {

        string stateName = "";
        foreach (var name in stateNames)
        {
            stateName += $"{name}\n";
        }
        if (_waitAnimationStateDic.ContainsKey(stateName))
        {
            StopCoroutine(_waitAnimationStateDic[stateName]);
            _waitAnimationStateDic.Remove(stateName);
        }
        _waitAnimationStateDic.Add(stateName,StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_animator, stateNames, ended, endRatio)));
    }
   
    public void AddAngle(float angle)
    {
        DeltaAngle += angle;
    }

    public Vector3 GetCenterWS()
    {
        return transform.position + _collider.center;
    }

}
