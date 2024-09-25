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
    // ������Ʈ�� �����̸� ��ġ ����ȭ�� ���ش�.
    // ����Ŭ���̾�Ʈ�� �ƴ� Ŭ���̾�Ʈ�� ���ο��� ������Ʈ�� �����̸� Collider�� ���� �ʾ� ����ȭ�� �ʿ��ϴ�.
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
