using Fusion;
using Fusion.Addons.KCC;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : NetworkBehaviour
{
    [SerializeField] float _jumpPadPower = 10f;
    [SerializeField] Range _padRange;
    [SerializeField] Vector3 _jumpDirection;

    private void OnDrawGizmosSelected()
    {
        Utils.DrawRange(gameObject, _padRange, Color.green);
        Gizmos.DrawRay(transform.position, _jumpDirection * _jumpPadPower);
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && Runner.IsFirstTick)
        {
            List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
            Runner.LagCompensation.OverlapBox(transform.position + _padRange.center, _padRange.size / 2, transform.rotation,
                Object.StateAuthority, hits, -1, HitOptions.None);

            foreach (var hit in hits)
            {
                PrototypeCharacter character = hit.Hitbox.GetComponentInParent<PrototypeCharacter>();

                if (character != null)
                {
                    character.Jump(_jumpDirection,_jumpPadPower);
                }
            }
        }
    }
}
