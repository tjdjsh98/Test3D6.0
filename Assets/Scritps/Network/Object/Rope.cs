using Fusion;
using UnityEngine;

public class Rope : NetworkBehaviour, IInteractable
{
    [field: SerializeField] public InteractType InteractType { get; set; }
    [field:SerializeField]public NetworkBool IsInteractable { get; set; }

    [field: SerializeField] public GameObject Model { get; set; }
    [field: SerializeField] public GameObject RopeModel { get; set; }
    
    public Transform StartPos;
    public Transform EndPos;

    public override void Spawned()
    {
        AdjustRopeSize();
    }

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

    public void AdjustRopeSize()
    {
        Vector3 topPos = transform.position + transform.forward + Vector3.up;

        if(Physics.Raycast(topPos, Vector3.down,out var hit, Mathf.Infinity))
        {
            Vector3 endPos = hit.point;
            float distance = topPos.y - endPos.y;

            Debug.Log(distance);

            RopeModel.transform.localScale = new Vector3(0.1f, distance/2, 0.1f);
            RopeModel.transform.localPosition = new Vector3(0, 1-distance/2, 0.5f);

        }
    }
}
