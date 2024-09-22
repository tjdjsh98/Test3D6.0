using Fusion;
using UnityEngine;

public class WorkingBlock : NetworkBehaviour, IInteractable
{
    [field: SerializeField] public InteractType InteractType { get; set; } = InteractType.Work;
    [Networked] public NetworkBool IsInteractable { get; set; } = true;
    GameObject _interactor { get; set; }
    [SerializeField] float _requireTime = 3;
    float _elaspedTime =0;
    [SerializeField] NetworkObject _spawnObject;
    public override void Spawned()
    {
        base.Spawned();
    }

    public override void Render()
    {
        if(_interactor != null)
        {
            _elaspedTime += Runner.DeltaTime;

            if(_requireTime < _elaspedTime)
            {
                _interactor = null;
                _elaspedTime = 0;
                Runner.Spawn(_spawnObject, transform.position);
                Runner.Despawn(Object);
            }
        }
    }

    public bool Interact(GameObject interactor)
    {
        if (_interactor == null)
        {
            _interactor = interactor;
            return true;
        }
        else if(_interactor == interactor)
        {
            _interactor = null;
            _elaspedTime = 0;
            return true;
        }


        return false;
    }
}
