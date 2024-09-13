using UnityEngine;

public class RangeAttackModule :  AttackModule
{
    [SerializeField] bool _debug;

    [SerializeField] float _enableAttackDistance;
    [SerializeField] Projectile _projectilePrefab;

    [SerializeField] GameObject _animationArrow;


    protected override void Awake()
    {
        base.Awake();
        _character.GetHitEnded += OnGetHitEnded;
    }
    private void OnDrawGizmos()
    {
        if (!_debug) return;


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enableAttackDistance);
    }
    public override void HandleAttack()
    {
        if (_enemyAI.Target == null) return;
        if (_character.IsAttack)
        {
            Vector3 direction = _enemyAI.Target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            //_character.SetAngle(angle);
            return;
        }

        if (Vector3.Distance(_enemyAI.Target.transform.position, transform.position) < _enableAttackDistance)
        {
            _character.IsAttack = true;
            _enemyAI.StopNav();
            _character.Attacked = OnAttacked;
            _character.SetAnimatorBoolean("HasPrepareAttack",true);
            _character.SetAnimatorTrigger("Attack");
            _character.WaitAnimationState("Attack", OnAttackEnded, 1);
        }
    }

    void OnAttacked()
    {
        if (Object.HasStateAuthority) {
            Projectile projectile = Object.Runner.Spawn(_projectilePrefab, _animationArrow.transform.position, _animationArrow.transform.rotation,null);

            DamageInfo info = new DamageInfo();
            info.attacker = _character;
            info.knockbackPower = 50;
            info.knockbackDirection = Vector3.zero;
            info.damage = 1;
            NetworkCharacter targetCharacter = _enemyAI.Target.GetComponentInParent<NetworkCharacter>();
            Debug.Log(targetCharacter);
            if(targetCharacter != null)
                projectile.Shot(info, targetCharacter.GetCenterWS() - projectile.transform.position, 50);
        }


    }

    void OnAttackEnded()
    {
        _character.SetAnimatorBoolean("HasPrepareAttack", false);
        _enemyAI.ResumeNav();
        _character.IsAttack = false;
    }

    void OnGetHitEnded()
    {
        _character.SetAnimatorBoolean("HasPrepareAttack", false);
        _enemyAI.ResumeNav();
        _character.IsAttack = false;
    }
}
