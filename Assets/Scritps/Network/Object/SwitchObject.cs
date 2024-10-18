using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class SwitchObject : NetworkBehaviour,IInteractable
{
    [SerializeField][Networked, OnChangedRender(nameof(Switch))] bool _isTurnOn { get; set; }
    [SerializeField] List<GameObject> _switchObject;
    [field: SerializeField] public InteractType InteractType { get; set; }
    [Networked] public NetworkBool IsInteractable { get; set; } = true;

    public override void Spawned()
    {
        foreach (var obj in _switchObject)
        {
            obj.gameObject.SetActive(_isTurnOn);
        }
    }

    public bool Interact(GameObject interactor)
    {
        _isTurnOn = !_isTurnOn;

        return true;
    }

    void Switch(NetworkBehaviourBuffer previous)
    {
        foreach(var obj in _switchObject)
        {
            obj.gameObject.SetActive(_isTurnOn);
        }
    }

}
