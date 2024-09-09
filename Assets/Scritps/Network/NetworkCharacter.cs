using Fusion;
using Fusion.Addons.SimpleKCC;
using Mono.Cecil.Cil;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using TMPro;
using Tripolygon.UModeler.UI;
using Tripolygon.UModelerX.Runtime;
using UnityEngine;
using UnityEngine.Animations;

public class NetworkCharacter : NetworkBehaviour, IDamageable, IRigidbody
{
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
    [Networked][field: SerializeField] public bool IsDamaged { get; set; }
    [Networked][field: SerializeField] public bool IsEnableMove { get; set; } = true;
    [Networked][field: SerializeField] public bool IsEnableTurn { get; set; } = true;


    // Velocity
    Vector3 _velocity;
    float _jumpImpulse = 0;
    float _breakPower = 50;

    public Vector3 Velocity => _velocity;
    float _lookAngle;

    // Components
    Animator _animator;
    SimpleKCC _kcc;
    NetworkMecanimAnimator _networkAnimator;
    NetworkManager _networkManager;
    AnimatorHelper _animatoHelper;

    // Handler
    public Action Attacked { get; set; }
    public Action AttackEnded { get; set; }
    public Action<DamageInfo> Died { get; set; }


    // Teleport
    Vector3 _teleportPosition;
    bool _isTeleport;

    Vector3 _accumlatePosition;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _networkAnimator = GetComponent<NetworkMecanimAnimator>();
        _kcc = GetComponent<SimpleKCC>();
        _animatoHelper = GetComponentInChildren<AnimatorHelper>();
        _animatoHelper.AnimatorMoved += OnAnimatorMoved;
    }

    public override void FixedUpdateNetwork()
    {
        HandleVelocity();
    }

    void OnAnimatorMoved()
    {
        _velocity = _animator.deltaPosition / Runner.DeltaTime;

        AddAngle(_animator.deltaRotation.eulerAngles.y);
    }

    public override void Spawned()
    {
        //_networkManager = FindAnyObjectByType<NetworkManager>(FindObjectsInactive.Include);

        //_networkManager.AddValueChanged<int>(this, nameof(Hp), OnHpChanged);
    }
    public void HandleVelocity()
    {
        SetAnimatorBoolean("ContactGround", _kcc == null ? true : _kcc.IsGrounded);
        _kcc?.Move(_velocity, _jumpImpulse);
        _kcc?.SetLookRotation(0, _lookAngle);
        
        _velocity = Vector3.zero;
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
        if (!IsEnableMove) return;

        ratio = Mathf.Clamp01(ratio);

        _velocity = direction * Speed * ratio;
    }
    public void Jump(float power)
    {
        if (!IsEnableMove) return;

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

    void CharacterDie(DamageInfo info)
    {
        Died?.Invoke(info);
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
        _animator.SetTrigger(name);
        _networkAnimator.SetTrigger(name);
    }
    public void SetAnimatorBoolean(string name, bool boolean)
    {
        _animator.SetBool(name, boolean);
    }
    public void SetAnimatorFloat(string name, float value,float dampTime= 0,float deltaTime =0)
    {
        _animator.SetFloat(name, value, dampTime, deltaTime);
    }
    public void SetAnimatorInt(string name, int value)
    {
        _animator.SetInteger(name, value);
    }

    public void SetAnimatorRootmotion(bool enable)
    {
        _animator.applyRootMotion = enable;
    }

    // 특정 애니메이션 상태를 기다려 줍니다.
    // endRatio은 Normalize된 값으로 ended가 실행될 시간을 정해줍니다.
    public void WaitAnimationState(string stateName, Action ended,float endRatio = 1)
    {
        StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_animator,stateName,ended,endRatio));
    }
    public void SetPosition(Vector3 position)
    {
        _isTeleport = true;
        _teleportPosition = position;
    }
    public void SetAngle(float angle)
    {
        _lookAngle = angle;
    }
    public void AddAngle(float angle)
    {
        _lookAngle += angle;
    }
}
