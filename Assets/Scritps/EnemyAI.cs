using Fusion;
using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class EnemyAI : NetworkBehaviour
{
    NetworkCharacter _character;

    [SerializeField] bool _debug;

    
    protected NavMeshAgent _navAgent;
    public GameObject Target { private set; get; }
    float _targetDistance;

    [SerializeField]Range _detectRange;
    [SerializeField] Range _attackRange;
    Vector3 _initPosition;


    [Header("Module")]
    [SerializeField]AttackModule _attackModule;
    private void Awake()
    {
        _character = GetComponent<NetworkCharacter>();
        _navAgent = GetComponent<NavMeshAgent>();

       
    }

    private void OnDrawGizmos()
    {
        if (!_debug) return;
        Utils.DrawRange(gameObject, _detectRange, Color.green);
        Utils.DrawRange(gameObject, _attackRange, Color.red);
    }

    public override void Spawned()
    {
        if (_navAgent)
        {
            _navAgent.speed = _character.Speed;
        }
        _initPosition = transform.position;
    }

    public override void Render()
    {
        _character.SetAnimatorBoolean("Walk", (_navAgent.velocity != Vector3.zero));
        _character.SetAnimatorFloat("Velocity", _targetDistance);

    }

    public override void FixedUpdateNetwork()
    {

        HandleAttack();
        DetectCharacter();
        ChaseTarget();
    }

    void DetectCharacter()
    {
        if (Target != null) return;
        if (_character.IsAttack) return;


        Collider[] colliders = Utils.RangeOverlapAll(gameObject, _detectRange, Define.CHARACTER_LAYERMASK);

        if(colliders.Length > 0)
        {
            Target = colliders[0].gameObject;
        }
    }
    void ChaseTarget()
    {
        if (_navAgent == null) return;
        if (_character.IsAttack) return;

        Vector3 direction = Vector3.zero;
        if (_navAgent.path.corners.Length <= 2 && Target != null)
            direction = Target.transform.position - transform.position;
        else
            direction = _navAgent.nextPosition - transform.position;
        _targetDistance = direction.magnitude;
       
        // 루트 모션으로 이동한다.
        _navAgent.speed = _character.Velocity.magnitude;


        // Rotation
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float deltaAngle = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, angle) * Runner.DeltaTime * 20;
        _character.AddAngle(deltaAngle);
        


        // 타깃이 없어지면 처음 위치로 돌아간다.
        if (Target == null) 
            _navAgent.SetDestination(_initPosition);
        else
            _navAgent.SetDestination(Target.transform.position);


        // 타깃의 거리가 너무 멀어지면 처음 위치로 돌아간다.
        if (Target != null && Vector3.Distance(Target.transform.position, transform.position) > _detectRange.size.x/2)
        {
            Target = null;
            _navAgent.SetDestination(_initPosition);
        }
    }
    void HandleAttack()
    {
        if(_attackModule!= null)
        {
            _attackModule?.HandleAttack();
        }
        else
        {
            DefaultAttackModule();
        }
    }

    void DefaultAttackModule()
    {
        if (_character.IsAttack) return;

        Collider[] colliders = Utils.RangeOverlapAll(gameObject, _attackRange, Define.CHARACTER_LAYERMASK);
        
        if (colliders.Length > 0)
        {
            OnAttackAnimationStarted();
            
        }
    }

    void OnAttackAnimationStarted()
    {
        _navAgent.isStopped = true;
        _character.IsAttack = true;
        _character.Attacked = OnAttack;
        _character.SetAnimatorTrigger("Attack");
        StartCoroutine(Utils.WaitAniationAndPlayCoroutine(GetComponentInChildren<Animator>(), "Attack", OnAttackAnimationEnded));
    }
    void OnAttackAnimationEnded()
    {
        _navAgent.isStopped = false;
        _character.IsAttack = false;
    }

    public void StopNav()
    {
        _navAgent.isStopped = true;
    }
    public void ResumeNav()
    {   
        _navAgent.isStopped = false;
    }

    void OnAttack()
    {
        Collider[] colliders = Utils.RangeOverlapAll(gameObject,_attackRange, Define.CHARACTER_LAYERMASK);

        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
        Runner.LagCompensation.OverlapBox(transform.position + _attackRange.center, _attackRange.size / 2, transform.rotation,
               Object.InputAuthority, hits, -1, HitOptions.None);


        foreach (LagCompensatedHit hit in hits)
        {
            if (hit.Hitbox == null) continue;
            if (hit.Hitbox.transform.root == transform.root) continue;

            IDamageable character = hit.Hitbox.transform.root.GetComponent<IDamageable>();
            if (character == null) continue;

            DamageInfo info = new DamageInfo();
            info.attacker = _character;
            info.target = character;
            info.knockbackPower = 100;
            info.knockbackDirection =  transform.forward ;
            info.damage = 1;

            character.Damage(info);

        }
    }
}
