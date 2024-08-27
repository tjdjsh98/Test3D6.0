using UnityEngine;

public class Item : MonoBehaviour
{
    BoxCollider _collider;
    Rigidbody _rigidbody;

    [field: SerializeField] public ItemType ItemType { get; set; }
    [field: SerializeField]public EquipmentType EquipmentType { get; set; }

    [field:SerializeField]public int Count;

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
}
