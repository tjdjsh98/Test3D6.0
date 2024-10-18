using Fusion;
using UnityEngine;

public class BuildItem : Item
{
    [Header("Build")]
    [field: SerializeField] public NetworkObject Building { get; set; }
    public override bool UseItem(PrototypeCharacterController prototypeCharacterController)
    {
        if (!prototypeCharacterController.HasStateAuthority) return false;
        if (Building == null) return false;
        if (prototypeCharacterController == null) return false;
        if (prototypeCharacterController.BuildPoint == null) return false;

        Vector3 rotation = prototypeCharacterController.CurrentPlayerInputData.aimForwardVector;
        float angle = Mathf.Atan2(rotation.x, rotation.z) * Mathf.Rad2Deg;

        Runner.Spawn(Building, prototypeCharacterController.BuildPoint.Value,Quaternion.Euler(0,angle,0));

        return true;
    }
}
