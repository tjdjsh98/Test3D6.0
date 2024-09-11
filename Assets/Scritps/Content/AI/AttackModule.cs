using Fusion;
using UnityEngine;

public abstract class AttackModule : NetworkBehaviour
{
    protected NetworkCharacter _character;
    protected EnemyAI _enemyAI;
    protected virtual void Awake()
    {
        _character = GetComponent<NetworkCharacter>();
        _enemyAI = GetComponent<EnemyAI>();
    }
    public abstract void HandleAttack();
}
