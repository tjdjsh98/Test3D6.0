using TMPro;
using TMPro.EditorUtilities;
using Tripolygon.UModelerX.Runtime;
using UnityEngine;

public class RangeAttackModule : AttackModule
{
    [SerializeField] bool _debug;

    [SerializeField] float _enableAttackDistance;
    [SerializeField] Projectile _projectilePrefab;

    [SerializeField] GameObject _animationArrow;

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
            transform.LookAt(_enemyAI.Target.transform.position, Vector3.up);
            return;
        }

        if (Vector3.Distance(_enemyAI.Target.transform.position, transform.position) < _enableAttackDistance)
        {
            _character.IsAttack = true;
            _enemyAI.StopNav();
            _character.Attacked = OnAttacked;
            _character.SetAnimatorBoolean("HasPrepareAttack",true);
            _character.SetAnimatorTrigger("Attack");

            StartCoroutine(Utils.WaitAniationAndPlayCoroutine(_character.Animator, "Attack", OnAttackEnded));
        }
    }

    void OnAttacked()
    {
        Projectile projectile = Instantiate(_projectilePrefab);
        projectile.transform.position = _animationArrow.transform.position;
        projectile.transform.rotation = _animationArrow.transform.rotation;

        DamageInfo info = new DamageInfo();
        info.attacker = _character;
        info.knockbackPower = 50;
        info.knockbackDirection = Vector3.zero;
        info.damage = 1;
        Character targetCharacter = _enemyAI.Target.GetComponent<Character>();
        projectile.Shot(info, targetCharacter.GetCenterWS() - projectile.transform.position, 50);
    }

    void OnAttackEnded()
    {
        _character.SetAnimatorBoolean("HasPrepareAttack", false);
        _character.IsAttack = false;
        _enemyAI.ResumeNav();
    }
}
