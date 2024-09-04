using UnityEngine;
using Fusion;
using System;
using Unity.Cinemachine;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; private set; }

    public Transform playerModel;
    public Action PlayerSpawned { get; set; }

    public CinemachineCamera _thirdPersonCamera;

    private void Awake()
    { 
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            _thirdPersonCamera = GameObject.Find("ThirdPersonCamera").GetComponent<CinemachineCamera>();
            _thirdPersonCamera.Target = new CameraTarget() { TrackingTarget = transform };

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
