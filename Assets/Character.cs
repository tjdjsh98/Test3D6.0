using UnityEngine;
using UnityEngine.AI;

public class Character: MonoBehaviour
{
    [Header("NavMesh")]
    [SerializeField] protected NavMeshAgent _navAgent;
    [SerializeField] protected GameObject _navTarget;

    [Header("Status")]
    [SerializeField] protected int _maxHp;
    [SerializeField] protected int _hp;
    [SerializeField] protected float _speed;

    private void Awake()
    {
        _navAgent= GetComponent<NavMeshAgent>();

        if (_navAgent)
        {
            _navAgent.speed = _speed;
        }
    }

    public void Update()
    {
        HandleNavMesh();
    }

    void HandleNavMesh()
    {
        if (_navAgent == null) return;
        if (_navTarget == null) return;
        _navAgent.SetDestination(_navTarget.transform.position);
    }
}
