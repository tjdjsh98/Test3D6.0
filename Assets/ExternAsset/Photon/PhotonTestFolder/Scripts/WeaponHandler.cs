using Fusion;
using Fusion.Sockets;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeaponHandler : NetworkBehaviour
{
    [Networked]
    public bool isFiring { get; set; }
    float _lastTimeFired = 0;

    private ChangeDetector _changes;

    // Compoents
    [SerializeField]NetworkPlayer _networkPlayer;
    public ParticleSystem fireParticleSystem;

    private void Awake()
    {
        //_networkPlayer = GetComponentInParent<NetworkPlayer>();
        _networkPlayer.PlayerSpawned += OnPlayerSpawned;
    }

    public override void Render()
    {
        Debug.Log($"{gameObject} {gameObject.transform.parent} {Object.InputAuthority}"); 
        foreach (var change in _changes.DetectChanges(this, out var previous, out var current))
        {
            switch (change)
            {
                case nameof(isFiring):
                    var reader = GetPropertyReader<bool>(nameof(isFiring));
                    var (p, c) = reader.Read(previous, current);
                    OnFireChanged(p, c);
                    break;
            }
        }
    }
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData networkInputData))
        {
            if(networkInputData.isFireButtonPressed)
            {
                Fire(networkInputData.aimForwardVector);
            }
        }
    }

    void OnPlayerSpawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

    }

    void Fire(Vector3 aimForwardVector)
    {
        if(Time.time - _lastTimeFired < 0.15f)
            return;

        Debug.Log($"{transform.parent.name} Cor Fire");
        StartCoroutine(FireEffectCO());

        _lastTimeFired = Time.time;
    }

    IEnumerator FireEffectCO()
    {
        isFiring = true;

        //fireParticleSystem.Play();

        yield return new WaitForSeconds(0.06f);

        isFiring = false;
    }

    void OnFireChanged(bool oldValue, bool currentValue)
    {
        if (oldValue != currentValue)
            OnFireRemote();
    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
            fireParticleSystem.Play();

        if (Object.HasStateAuthority)
        {
            RpcInfo info = default;
            RPC_RelayMessage("A", info.Source);

        }
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        Debug.Log("KK");
        fireParticleSystem.Play();
    }
}
