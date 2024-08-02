using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }

    NetworkButtons _previousButton;
    public void Init()
    {
        life = TickTimer.CreateFromSeconds(Runner, 1.0f);
        Debug.Log(transform.position);
    }
    public override void Spawned()
    {
        Debug.Log("Ball Spawned");
    }

    public override void FixedUpdateNetwork()
    {
      
        if (life.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
        else
        {
            transform.position += 5 * transform.forward * Runner.DeltaTime;
        }
    }
}
