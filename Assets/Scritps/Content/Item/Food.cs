using UnityEngine;

public class Food : Item
{
    [Header("Food")]
    [SerializeField] protected float _fullness;


    public override bool UseItem(PrototypeCharacterController prototypeCharacterController)
    {
        if (prototypeCharacterController == null) return false;

        prototypeCharacterController.EatFood(_fullness);

        return true;
    }
}
