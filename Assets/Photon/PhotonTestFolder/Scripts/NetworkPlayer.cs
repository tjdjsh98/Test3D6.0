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
    public Transform _cameraTarget;             // 이 것을 기준으로 카메라가 돌아감

    private void Awake()
    { 
    }

    private void Update()
    {
        if (Object.HasStateAuthority)
        {
            if(Input.GetKeyDown(KeyCode.N)) 
                FindAnyObjectByType<NetworkRunner>().Spawn(Resources.Load<NetworkObject>("Prefabs/NetworkMonster"), -Vector3.forward*10);
        }
    }

    // 입력권한 있는 캐릭터가 로컬 캐릭터
    // 카메라가 입력 권한이 있는 플레이어를 따라감
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            _thirdPersonCamera = GameObject.Find("ThirdPersonCamera").GetComponent<CinemachineCamera>();
            _thirdPersonCamera.Target = new CameraTarget() { TrackingTarget = _cameraTarget };
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
