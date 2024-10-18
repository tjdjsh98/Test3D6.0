using Fusion;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkBlock : NetworkBehaviour, IDamageable,IData
{
    public GameObject GameObject => gameObject;
    [field:SerializeField] public string DataName { get; set; }
    [Networked] public int MaxHp { get; set; }
    [Networked] public int Hp { get; set; }

    // Handler
    public Action<DamageInfo> Died { get; set; }
    public Action<DamageInfo> Damaged { get; set; }

    Vector3 _prePosition;
    // 오브젝트가 움직이면 위치 동기화를 해준다.
    // 메인클라이언트가 아닌 클라이언트는 메인에서 오브젝트가 움직이면 Collider가 맞지 않아 동기화가 필요하다.
    public override void Render()
    {
        if(_prePosition != transform.position)
        {
            Physics.SyncTransforms();
            _prePosition = transform.position;
        }
    }

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
