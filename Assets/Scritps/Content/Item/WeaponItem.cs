using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item
{
    [SerializeField] bool _debug;
    [field: SerializeField] public Range AttackRange { get; set; }


    List<GameObject> _attackedList = new List<GameObject>();
    bool _isAttack;

    PrototypeCharacterController _playerController;
    AnimationCharacterController _animationCharacterController;

  
    private void OnDrawGizmosSelected()
    {
        if (!_debug) return;

        Utils.DrawRange(gameObject, AttackRange, Color.red);
    }

    public virtual void OnAttackAnimationStarted()
    {

    }

    public virtual void OnAttackAnimationEnded()
    {
    }

    public override bool UseItem(PrototypeCharacterController prototypeCharacterController)
    {
        _playerController = prototypeCharacterController;
        if(_playerController)
            _playerController.PlayAttack();
        else
            return false;

        return true;
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
        //damageInfo.attacker = transform.root.GetComponent<NetworkCharacter>();
        damageInfo.damage = 1;
        damageInfo.knockbackDirection = _playerController.transform.forward;
        damageInfo.knockbackPower = 50;
        character.Damage(damageInfo);

    }
}
