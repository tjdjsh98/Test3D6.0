using Fusion;
using UnityEngine;

public class Rope : NetworkBehaviour, IInteractable
{
    [field: SerializeField] public InteractType InteractType { get; set; }
    [field:SerializeField]public NetworkBool IsInteractable { get; set; }

    public Transform StartPos;
    public Transform EndPos;

    public bool Interact(GameObject interactor)
    {
        PlayerInputHandler playerInputHandler = interactor.GetComponentInParent<PlayerInputHandler>();

        if (playerInputHandler == null) return false;

        PlayerInputData inputData = playerInputHandler.AccumulatedInput;
        inputData.holdRopeID = Object.Id;
        inputData.buttons.Set(InputButton.HoldRope, true);
        playerInputHandler.AccumulatedInput = inputData;
        return true;
    }
}
