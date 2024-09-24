using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Item : NetworkBehaviour, IInteractable, IData
{
    Collider _collider;
    Rigidbody _rigidbody;
    NetworkRigidbody3D _networkRigidbody;


    [field:SerializeField] public string DataName { get; set; }
    [field:SerializeField]public InteractType InteractType { get; set; }
    [Networked] public NetworkBool IsInteractable { get; set; } = true;
    [field:SerializeField] public bool IsStackable { get; set; } = false;
    [field: SerializeField] public ItemType ItemType { get; set; }
    [field: SerializeField] public ItemSize ItemSize { get; set; }
    [field: SerializeField]public EquipmentType EquipmentType { get; set; }
    [Networked, OnChangedRender(nameof(OnIsHideChanged))]
    public bool IsHide { get; set; }
    [Networked, OnChangedRender(nameof(OnIsUseRigidbodyChanaged))] public bool IsUseRigidbody { get; set; } = true;

private void Awake()
    {
        _collider = GetComponentInChildren<BoxCollider>();
        _rigidbody= GetComponent<Rigidbody>();
        _networkRigidbody = GetComponent<NetworkRigidbody3D>();
    }

    public void Equip()
    {
        IsUseRigidbody = false;
        _collider.isTrigger = true;
        gameObject.layer = 0;
    }

    public void Unequip()
    {
        gameObject.layer = LayerMask.NameToLayer("Item");
        IsUseRigidbody = true;
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

    void OnIsUseRigidbodyChanaged()
    {
        if(_rigidbody)
        {
            if (!IsUseRigidbody)
            {
                _networkRigidbody.InterpolationTarget.transform.localPosition = Vector3.zero;
                _networkRigidbody.InterpolationTarget.transform.localRotation = Quaternion.identity;
            }
            _rigidbody.useGravity = IsUseRigidbody;
            _rigidbody.isKinematic = !IsUseRigidbody;
            _networkRigidbody.enabled = IsUseRigidbody;
        }
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