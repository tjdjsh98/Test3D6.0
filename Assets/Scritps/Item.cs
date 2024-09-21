using Fusion;
using UnityEngine;

public class Item : NetworkBehaviour, IInteractable
{
    BoxCollider _collider;
    Rigidbody _rigidbody;
    [field:SerializeField]public InteractType InteractType { get; set; }
    [field: SerializeField] public ItemType ItemType { get; set; }
    [field: SerializeField] public ItemSize ItemSize { get; set; }
    [field: SerializeField]public EquipmentType EquipmentType { get; set; }

private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _rigidbody= GetComponent<Rigidbody>();
    }

    public void Equip()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity= false;
        _collider.isTrigger = true;
        gameObject.layer = 0;
    }

    public void Unequip()
    {
        gameObject.layer = LayerMask.NameToLayer("Item");
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _collider.isTrigger = false;
    }

    public bool Interact(GameObject interactor)
    {
        PrototypeCharacterController prototypeCharacterController = interactor.GetComponent<PrototypeCharacterController>();
        if (prototypeCharacterController == null) return false;
        Inventory inventory = prototypeCharacterController.QuickSlotInventory;

        var data = inventory.AccumulateInputData;
        data.isAddItem = true;
        data.addItemID = Object.Id;
        inventory.AccumulateInputData = data;

        return true;
    }

}

public enum InteractType
{
    Item,
    Work,
}

public enum ItemSize
{
    Small,
    Middle,
    Large,
}
public interface IInteractable
{
    public InteractType InteractType { get; set; }
    public bool Interact(GameObject interactor);
}