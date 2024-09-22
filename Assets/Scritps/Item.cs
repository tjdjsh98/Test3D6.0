using Fusion;
using UnityEngine;

public class Item : NetworkBehaviour, IInteractable, IData
{
    BoxCollider _collider;
    Rigidbody _rigidbody;
    [field:SerializeField] public string DataName { get; set; }
    [field:SerializeField]public InteractType InteractType { get; set; }
    [Networked] public NetworkBool IsInteractable { get; set; } = true;
    [field:SerializeField] public bool IsStackable { get; set; } = false;
    [field: SerializeField] public ItemType ItemType { get; set; }
    [field: SerializeField] public ItemSize ItemSize { get; set; }
    [field: SerializeField]public EquipmentType EquipmentType { get; set; }
    [Networked, OnChangedRender(nameof(OnIsHideChanged))]
    public bool IsHide { get; set; }

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
        if (!IsInteractable) return false;

        PrototypeCharacterController prototypeCharacterController = interactor.GetComponent<PrototypeCharacterController>();
        if (prototypeCharacterController == null) return false;
        Inventory inventory = prototypeCharacterController.QuickSlotInventory;

        var data = inventory.AccumulateInputData;
        data.isAddItem = true;
        data.addItemID = Object.Id;
        inventory.AccumulateInputData = data;

        return true;
    }

    public void Show(bool isShow)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.gameObject.SetActive(isShow);
        }
    }

    void OnIsHideChanged()
    {
        Show(!IsHide);
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
    public NetworkBool IsInteractable { get; set; }
    public bool Interact(GameObject interactor);
}