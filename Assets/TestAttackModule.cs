using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class TestAttackModule : NetworkBehaviour
{
    [SerializeField] Range _attackRange;

    bool _attacking = false;
    List<GameObject> _attackedList = new List<GameObject>();



    private void OnDrawGizmosSelected()
    {
        Utils.DrawRange(gameObject, _attackRange, Color.red);
    }

    public override void FixedUpdateNetwork()
    {
        if (!_attacking) return;

        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

        Runner.LagCompensation.OverlapBox(transform.position + _attackRange.center, _attackRange.size / 2, transform.rotation,
            Object.InputAuthority, hits, -1, HitOptions.None);


        foreach (var hit in hits)
        {
            Debug.Log(hit.Hitbox.transform.parent.name);
            if (hit.Hitbox == null) continue;

            GameObject hitObject = hit.Hitbox.gameObject.transform.root.gameObject;

            if (_attackedList.Contains(hitObject)) continue;

            _attackedList.Add(hitObject);
            AttackCharacter(hitObject);
        }
    }
    void AttackCharacter(GameObject go)
    {
        if (go == gameObject) return;

        var character = go.GetComponentInParent<IDamageable>();
        DamageInfo damageInfo = new DamageInfo();
        //damageInfo.attacker = transform.root.GetComponent<NetworkCharacter>();
        damageInfo.damage = 1;
        damageInfo.knockbackDirection = transform.forward;
        damageInfo.knockbackPower = 10;
        Debug.Log("Hit");
        character.Damage(damageInfo);

    }
    public void ActiveAttack()
    {
        _attacking = true;
    }

    public void InactiveAttack()
    {
        _attacking = false;
        _attackedList.Clear();
    }

}
