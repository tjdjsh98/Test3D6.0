using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItemOnDied : NetworkBehaviour
{
    public List<NetworkObject> _spawnItemList = new List<NetworkObject> ();
    private void Awake()
    {
        IDamageable damageable = GetComponent<IDamageable>();
        damageable.Died += OnDied;
    }

    void OnDied(DamageInfo info)
    {
        if (HasStateAuthority)
        {
            NetworkRunner networkRunner = FindAnyObjectByType<NetworkRunner>();

            foreach (var item in _spawnItemList) 
            {
                Vector3 random = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
                networkRunner.Spawn(item, transform.position + random);
            }
        }
    }
}
