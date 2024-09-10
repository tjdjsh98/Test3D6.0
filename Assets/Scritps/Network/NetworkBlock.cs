using Fusion;
using System;
using UnityEngine;

public class NetworkBlock : NetworkBehaviour, IDamageable
{
    public GameObject GameObject => gameObject;
    [Networked] public int MaxHp { get; set; }
    [Networked] public int Hp { get; set; }

    // Handler
    public Action<DamageInfo> Died { get; set; }
    public Action<DamageInfo> Damaged { get; set; }

    public int Damage(DamageInfo damageInfo)
    {
        Debug.Log("Dam");
        Hp -= 1;

        Damaged?.Invoke(damageInfo);
        if (Hp < 0)
        {
            Die(damageInfo);
        }

        return damageInfo.damage;
    }

    void Die(DamageInfo info)
    {
        if (Object.HasStateAuthority) 
        {
            Died?.Invoke(info);
            NetworkRunner networkRunner = FindAnyObjectByType<NetworkRunner>();
            networkRunner.Despawn(Object);
        }
    }
}
