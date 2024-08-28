using UnityEngine;

public abstract class AttackModule : MonoBehaviour
{
    protected Character _character;
    protected EnemyAI _enemyAI;
    private void Awake()
    {
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();
    }
    public abstract void HandleAttack();
}
