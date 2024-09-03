using UnityEngine;
using Fusion;
using System;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; private set; }

    public Transform playerModel;

    public Action PlayerSpawned { get; set; }

    private void Awake()
    { 
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
           
        }
     
        name = $"P_{Object.Id}";
        PlayerSpawned?.Invoke();
    }
    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
            Runner.Despawn(Object);
    }
}
