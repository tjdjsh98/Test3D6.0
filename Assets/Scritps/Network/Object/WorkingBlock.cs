using Fusion;
using UnityEngine;

public class WorkingBlock : NetworkBehaviour, IInteractable
{
    [field: SerializeField] public InteractType InteractType { get; set; } = InteractType.Work;
    [Networked] public NetworkBool IsInteractable { get; set; } = true;
    GameObject _interactor { get; set; }
    [field: SerializeField] public float RequireTime { get; set; }
    [field: SerializeField] public NetworkObject SpawnObject { get; set; }

    public override void Spawned()
    {
        base.Spawned();
    }

    // 로컬캐릭터만 실행해준다.
    public bool Interact(GameObject interactor)
    {
        PrototypeCharacterController character = interactor.GetComponent<PrototypeCharacterController>();

        if (character == null || !character.HasInputAuthority) return false;
        if (character.IsWorking) return false;
        if(character.CancelWorkingFrame == Time.frameCount) return false;       // 취소되자마자 바로 상호작용하는 것을 방지

        WorkingInputData data = character.AccumulateWorkingInputData;
        data.isWorking = true;
        data.workingTargetID = Object.Id;
        character.AccumulateWorkingInputData = data;

        return false;
    }
}
