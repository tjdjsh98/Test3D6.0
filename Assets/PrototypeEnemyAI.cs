using ExitGames.Client.Photon;
using Fusion;
using System;
using Unity.Cinemachine;
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


    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _character = GetComponent<PrototypeCharacter>();
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
    }


    void DetectTarget()
    {
        if (_target != null) return;

         Collider[] hits =Utils.RangeOverlapAll(gameObject, _detectRange, Define.CHARACTER_LAYERMASK);

        Array.Sort(hits, (item1, item2) => {
            return Vector3.Distance(transform.position, item1.gameObject.transform.position)
            > Vector3.Distance(transform.position, item2.gameObject.transform.position) ? 1 : -1;
        });

        Debug.Log(hits.Length);
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
        if (_target == null) return;

        _navMeshAgent.SetDestination(_target.transform.position);
        if (_navMeshAgent.path.corners.Length >= 2)
        {
            Vector3 direction = (_navMeshAgent.path.corners[1] - transform.position).normalized;
            float deltaAngle = Vector3.SignedAngle(transform.forward, direction,Vector3.up);
            _character.Move(direction * 3);
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
}