using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] UITwoInventory _ui;
    Inventory _inventory;

    private void Awake()
    {
        _inventory = GetComponent<Inventory>();
    }
    public void Interact(GameObject interactor)
    {
        Inventory characterInvenotry = interactor.GetComponent<Inventory>();

        if (characterInvenotry == null) return;


        _ui.ConnectInventory(characterInvenotry, _inventory);
        _ui.Open();
    }
}
