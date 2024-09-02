using Fusion;
using Fusion.Sockets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeaponHandler : NetworkBehaviour
{
    [Networked]
    public NetworkBool isFiring { get; set; }
    float _lastTimeFired = 0;
    private ChangeDetector _changes;

    public GameObject aimPoint;
    public LayerMask collisionLayer;


    // Compoents
    NetworkPlayer _networkPlayer;
    public ParticleSystem fireParticleSystem;

    private void Awake()
    {
        _networkPlayer = GetComponentInParent<NetworkPlayer>();
        _networkPlayer.PlayerSpawned += OnPlayerSpawned;
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this, out var previous, out var current))
        {
            switch (change)
            {
                case nameof(isFiring):
                    var reader = GetPropertyReader<NetworkBool>(nameof(isFiring));
                    var (p, c) = reader.Read(previous, current);
                    OnFireChanged(p, c);
                    break;
            }
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData networkInputData))
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

        StartCoroutine(FireEffectCO());

        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
        int count = Runner.LagCompensation.RaycastAll(aimPoint.transform.position, aimForwardVector, 100, Object.InputAuthority, hits, collisionLayer,true, (HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority));

        float hitDistance = 100;
        bool isHitOtherPlayer = false;


        for (int i = 0; i < count; i++)
        {
            if (hits[i].Distance > 0)
                hitDistance = hits[i].Distance;

            if (hits[i].Hitbox != null)
            {
                if (hits[i].Hitbox.transform.root == transform.root) continue;
                Debug.Log($"{Time.time} {transform.name} hit hitbox {hits[i].Hitbox.transform.root.name}");

                if (Object.HasStateAuthority)
                    hits[i].Hitbox.transform.root.GetComponent<HpHandler>().TakeDamage();
                isHitOtherPlayer = true;

                break;
            }
            else if (hits[i].Collider != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit physX collider {hits[i].Collider.transform.root.name}");
                break;
            }
        }

        if(isHitOtherPlayer)
        {
            Debug.DrawRay(aimPoint.transform.position, aimForwardVector * hitDistance, Color.red, 1);
        }
        else
        {
            Debug.DrawRay(aimPoint.transform.position, aimForwardVector * hitDistance, Color.green, 1);
        }

        _lastTimeFired = Time.time;
    }

    IEnumerator FireEffectCO()
    {
        isFiring = true;

        fireParticleSystem.Play();

        yield return new WaitForSeconds(0.06f);

        isFiring = false;
    }

    void OnFireChanged(bool oldValue, bool currentValue)
    {
            OnFireRemote();
    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
        {
            fireParticleSystem.Play();
        }
    }
    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    //public void RPC_SendMessage(string message, RpcInfo info = default)
    //{
    //    RPC_RelayMessage(message, info.Source);
    //}

    //[Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    //public void RPC_RelayMessage(string message, PlayerRef messageSource)
    //{
    //    Debug.Log("KK");
    //    fireParticleSystem.Play();
    //}
}
