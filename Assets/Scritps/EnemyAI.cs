using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class EnemyAI : MonoBehaviour
{
    Character _character;

    [SerializeField] bool _debug;

    
    protected NavMeshAgent _navAgent;
    public GameObject Target { private set; get; }

    [SerializeField]Range _detectRange;
    [SerializeField] Range _attackRange;
    Vector3 _initPosition;


    [Header("Module")]
    [SerializeField]AttackModule _attackModule;
    private void Awake()
    {
        _character = GetComponent<Character>();
        _navAgent = GetComponent<NavMeshAgent>();

        if (_navAgent)
        {
            _navAgent.speed = _character.Speed;
        }
        _initPosition = transform.position;
    }

    private void OnDrawGizmos()
    {
        if (!_debug) return;
        Utils.DrawRange(gameObject, _detectRange, Color.green);
        Utils.DrawRange(gameObject, _attackRange, Color.red);
    }

    private void Update()
    {
        _character.SetAnimatorBoolean("Walk", (_navAgent.velocity != Vector3.zero));

        DetectCharacter();
        ChaseTarget();
        HandleAttack();
    }


    void DetectCharacter()
    {
        if (Target != null) return;
        if (_character.IsAttack) return;

        Collider[] colliders = Utils.RangeOverlapAll(gameObject, _detectRange, Define.PLAYER_LAYERMASK);

        if(colliders.Length > 0)
        {
            Target = colliders[0].gameObject;
        }
    }

    void ChaseTarget()
    {
        if (_navAgent == null) return;
        if (Target == null) return;
        if (_character.IsAttack) return;

        if(Vector3.Distance(Target.transform.position, transform.position) > _detectRange.size.x/2)
        {
            Target = null;
            _navAgent.SetDestination(_initPosition);
            _character.IsMove = true;
            return;
        }

        _navAgent.SetDestination(Target.transform.position);
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

        Collider[] colliders = Utils.RangeOverlapAll(gameObject, _attackRange, Define.PLAYER_LAYERMASK);

        if (colliders.Length > 0)
        {
            _navAgent.isStopped = true;
            _character.IsAttack = true;
            _character.Attacked = OnAttack;
            _character.SetAnimatorTrigger("Attack");
            StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_character.Animator, "Attack", EndAttack));
        }
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
        Collider[] colliders = Utils.RangeOverlapAll(gameObject,_attackRange, Define.PLAYER_LAYERMASK);

        foreach(Collider collider in colliders)
        {
            Character character = collider.GetComponent<Character>();
            if (character == null) continue;

            DamageInfo info = new DamageInfo();
            info.attacker = _character;
            info.target = character;
            info.knockbackPower = 100;
            info.knockbackDirection =  transform.forward ;
            info.damage = 1;

            character.Damaged(info);

        }
    }
    void EndAttack()
    {
        _character.IsAttack = false;
        _character.Attacked = null;
        _navAgent.isStopped= false;
    }
}
