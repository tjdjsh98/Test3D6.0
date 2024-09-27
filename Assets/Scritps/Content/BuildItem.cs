using Fusion;
using UnityEngine;

public class BuildItem : Item
{
    [Header("Build")]
    [field: SerializeField] public NetworkObject Building { get; set; }
    public override bool UseItem(PrototypeCharacterController prototypeCharacterController)
    {
        if (Building == null) return false;
        if (prototypeCharacterController == null) return false;

        Runner.Spawn(Building, prototypeCharacterController.BuildPoint);

        return true;
    }
}
