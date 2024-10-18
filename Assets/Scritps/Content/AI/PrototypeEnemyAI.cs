using ExitGames.Client.Photon;
using Fusion;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Loading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore;

[RequireComponent(typeof(NavMeshAgent))]
public class PrototypeEnemyAI : NetworkBehaviour
{
    // Component
    NavMeshAgent _navMeshAgent;
    PrototypeCharacter _character;

    // Target / Range
    GameObject _target;
    [SerializeField] protected Range _detectRange;
    [SerializeField] protected Range _chaseRange;


    float _attackTerm = 2;
    float _attackElaspedTime = 0;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _character = GetComponent<PrototypeCharacter>();

        _navMeshAgent.isStopped = true;
    }

    private void OnDrawGizmosSelected()
    {
        Utils.DrawRange(gameObject, _detectRange, Color.green);
        Utils.DrawRange(gameObject, _chaseRange, Color.yellow);
    }

    public override void Render()
    {
        if(HasStateAuthority)
            DetectTarget();
    }

    public override void FixedUpdateNetwork()
    {
        ChaseTarget();
        AttackTarget();
    }


    void DetectTarget()
    {
        if (_target != null) return;

         Collider[] hits =Utils.RangeOverlapAll(gameObject, _detectRange, Define.CHARACTER_LAYERMASK);

        Array.Sort(hits, (item1, item2) => {
            return Vector3.Distance(transform.position, item1.gameObject.transform.position)
            > Vector3.Distance(transform.position, item2.gameObject.transform.position) ? 1 : -1;
        });

        for(int i = 0; i < hits.Length; i++)
        {
            _navMeshAgent.SetDestination(hits[i].gameObject.transform.position);

            if (_navMeshAgent.path.corners.Length <= 0)
                continue;
            PrototypeCharacter targetCharacter = hits[i].GetComponentInParent<PrototypeCharacter>();

            if(targetCharacter != null && targetCharacter != _character)
            {
                _target = hits[i].gameObject;
                break;
            }
        }
    }

    void ChaseTarget()
    {
        if (_target == null)
        {
            _character.Move(Vector3.zero);
            return;
        }

        _navMeshAgent.SetDestination(_target.transform.position);
        if (_navMeshAgent.path.corners.Length >= 2)
        {
            Vector3 direction = (_navMeshAgent.path.corners[1] - transform.position);
            float deltaAngle = Vector3.SignedAngle(transform.forward, direction,Vector3.up);

            if (Vector3.Distance(_target.transform.position , transform.position) > 2)
            {
                _character.Move(direction.normalized * 3);
            }
            if (Runner.IsFirstTick)
            {
                _character.AddLookAngle(deltaAngle);
            }
        }
        // 타겟이 너무 멀어지면 타겟을 취소합니다.
        if (Vector3.Distance(_target.transform.position, transform.position) > _chaseRange.size.x)
        {
            _target = null;
        }
    }

    void AttackTarget()
    {
        if (_target == null) return;
         if (Vector3.Distance(_target.transform.position, transform.position) > 2) return;

        Range range = new Range() { center = Vector3.forward * 0.5f, relativeTransform = true, shape = RangeShape.Box, size = new Vector3(3, 2, 3) };

        _attackElaspedTime += Time.deltaTime;
        if (_attackElaspedTime > _attackTerm)
        {
            Debug.Log("Attack");
            _attackElaspedTime = 0;
            List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

            Runner.LagCompensation.OverlapBox(transform.position + range.center, range.size / 2, transform.rotation,
                Object.StateAuthority, hits, -1, HitOptions.None);

            foreach (var hit in hits)
            {
                if (hit.Hitbox == null) continue;

                GameObject hitObject = hit.Hitbox.gameObject.transform.root.gameObject;
                if (hitObject == gameObject) continue;

                IDamageable damageable = hitObject.GetComponent<IDamageable>();

                if (damageable != null)
                {
                    damageable.Damage(new DamageInfo()
                    {
                        attacker = _character,
                        damage = 1,
                        knockbackDirection =
                        transform.forward,
                        knockbackPower = 10,
                        target = damageable
                    });
                }
            }
        }
    }
}