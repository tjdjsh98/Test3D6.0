using Fusion;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] UITwoInventory _ui;
    Inventory _inventory;
    [field: SerializeField] public InteractType InteractType { get; set; }
    [Networked] public NetworkBool IsInteractable { get; set; } = true;

    private void Awake()
    {
        _inventory = GetComponent<Inventory>();
    }
    public bool Interact(GameObject interactor)
    {
        Inventory characterInvenotry = interactor.GetComponent<Inventory>();

        if (characterInvenotry == null) return false;


        _ui.ConnectInventory(characterInvenotry, _inventory);
        _ui.Open();

        return true;
    }
}
