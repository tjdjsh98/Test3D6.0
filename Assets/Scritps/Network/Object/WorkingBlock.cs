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

    // ����ĳ���͸� �������ش�.
    public bool Interact(GameObject interactor)
    {
        PrototypeCharacterController character = interactor.GetComponent<PrototypeCharacterController>();

        if (character == null || !character.HasInputAuthority) return false;
        if (character.IsWorking) return false;
        if(character.CancelWorkingFrame == Time.frameCount) return false;       // ��ҵ��ڸ��� �ٷ� ��ȣ�ۿ��ϴ� ���� ����

        WorkingInputData data = character.AccumulateWorkingInputData;
        data.isWorking = true;
        data.workingTargetID = Object.Id;
        character.AccumulateWorkingInputData = data;

        return false;
    }
}
