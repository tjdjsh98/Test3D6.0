using Fusion;
using Unity.Collections;
using UnityEngine;

public class HpHandler : NetworkBehaviour
{
    [Networked]
    public byte HP { get; set; }

    [Networked]
    public NetworkBool IsDead { get; set; }

    const byte startHp = 5;

    NetworkPlayer _player;
    ChangeDetector _detector;

    private void Awake()
    {
        _player = GetComponent<NetworkPlayer>();
        _player.PlayerSpawned += OnPlayerSpawned;
    }

    public override void Render()
    {
        foreach (var change in _detector.DetectChanges(this, out var previous, out var current))
        {
            switch (change)
            {
                case nameof(HP):
                    {
                        var reader = GetPropertyReader<byte>(nameof(HP));
                        var (p, c) = reader.Read(previous, current);
                        OnHpChanged(p, c);
                    }
                    break;
                case nameof(IsDead):
                    {
                        var reader = GetPropertyReader<NetworkBool>(nameof(IsDead));
                        var (p, c) = reader.Read(previous, current);
                        OnStateChanged(p, c);
                    }
                    break;
            }
        }
    }

    public void TakeDamage()
    {
        if (IsDead)
            return;

        HP -= 1;

        Debug.Log($"{Time.time} took damage got {HP} left");

        if(HP <= 0)
        {
            IsDead = true;
        }
    }

    void OnPlayerSpawned()
    {
        _detector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        HP = startHp;
    }

    void OnHpChanged(byte old, byte current)
    {
        Debug.Log($"{Time.time} OnHpChanged Value {current}");
    }

    void OnStateChanged(NetworkBool old, NetworkBool current)
    {
        Debug.Log($"{Time.time} OnStateChanged isDead {current}");
    }
}
