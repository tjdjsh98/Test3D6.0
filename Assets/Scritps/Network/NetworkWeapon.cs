using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class NetworkWeapon : NetworkBehaviour
{
    [SerializeField] bool _debug;
    [field: SerializeField] public Range AttackRange { get; set; }

    List<GameObject> _attackedList = new List<GameObject>();
    bool _isAttack;

    NetworkPlayerController _playerController;

    private void Awake()
    {
        _playerController = transform.root.GetComponent<NetworkPlayerController>();
        _playerController.Weapon = this;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_debug) return;


        Utils.DrawRange(gameObject, AttackRange, Color.red);
    }

    public virtual void OnAttackAnimationStarted()
    {
        _playerController.GetComponent<NetworkCharacter>().IsEnableMove = false;
    }

    public virtual void OnAttackAnimationEnded()
    {
        _playerController.GetComponent<NetworkCharacter>().IsEnableMove = true;
    }

    public virtual void StartAttack()
    {
        _isAttack = true;
    }

    public virtual void EndAttack()
    {
        _isAttack = false;
        _attackedList.Clear();
    }

    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority && Runner.IsForward)
        {
            PlayAttack();
        }
    }

    private void PlayAttack()
    {
        if (_isAttack)
        {
            List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

            Runner.LagCompensation.OverlapBox(transform.position + AttackRange.center, AttackRange.size / 2, transform.rotation,
                Object.InputAuthority, hits, -1, HitOptions.None);

            foreach(var hit in hits)
            {
                if (hit.Hitbox == null) continue;

                GameObject hitObject = hit.Hitbox.gameObject.transform.root.gameObject;

                if (_attackedList.Contains(hitObject)) continue;

                _attackedList.Add(hitObject);
                AttackCharacter(hitObject);
            }
        }
    }

    void AttackCharacter(GameObject go)
    {
        if (go == _playerController.gameObject) return;

        var character = go.GetComponentInParent<IDamageable>();
        DamageInfo damageInfo = new DamageInfo();
        Debug.Log(character);
        //damageInfo.attacker = transform.root.GetComponent<NetworkCharacter>();
        damageInfo.damage = 1;
        damageInfo.knockbackDirection = _playerController.transform.forward;
        damageInfo.knockbackPower = 10;
        character.Damaged(damageInfo);

    }
}
