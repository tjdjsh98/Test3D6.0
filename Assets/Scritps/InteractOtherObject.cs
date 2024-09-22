using Fusion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TextCore;

public class InteractOtherObject : NetworkBehaviour
{

    // Components
    UIInteract _uiItemShower;
    NetworkCharacter _networkCharacter;
    PlayerInputHandler _playerInputHandler;


    NetworkButtons _previousButtons;

    GameObject _target;


    // InteractType이 Work인 오브젝트 관리
    bool _isInteracting;
    IInteractable _interactBlock;
    GameObject _interactGameObject;




    Item _leftItem;


    private void Awake()
    {
        _networkCharacter = GetComponent<NetworkCharacter>();
        _uiItemShower = UIManager.Instance.GetUI<UIInteract>();
    }

    private void OnDrawGizmosSelected()
    {
        Matrix4x4 rot = Matrix4x4.Rotate(transform.rotation);
        Matrix4x4 pos = Matrix4x4.Translate(transform.position + transform.forward + transform.up *0.5f);
        Gizmos.matrix =  pos * rot;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = Matrix4x4.identity;

    }
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData networkInputData))
        {
            if(networkInputData.playerInputData.buttons.WasPressed(_previousButtons, InputButton.Interact))
            {
                InteractOther();
            }
            _previousButtons = networkInputData.playerInputData.buttons;
        }
        DetectInteractableObject();
        if ((_isInteracting && _interactGameObject == null))
        {
            _interactBlock.Interact(gameObject);
            _interactBlock = null;
            _isInteracting = false;
            _interactGameObject = null;
            GetComponent<PlayerInputHandler>().IsEnableInputMove = true;
            GetComponent<PlayerInputHandler>().IsEnableInputRotation = true;
            _networkCharacter.SetAnimatorBoolean("Working", false);
        }
    }
    void DetectInteractableObject()
    {
        if (!HasInputAuthority) return;

        Vector3 center = transform.position;

        Collider[] hits = Physics.OverlapBox(center + transform.forward + transform.up * 0.5f
            , Vector3.one, transform.rotation, Define.INTERACTABLE_LAYERMASK);


        if (hits.Length == 0)
        {
            UIManager.Instance.GetUI<UIInteract>().HideAll();
        }

        Array.Sort<Collider>(hits, (num1, num2) =>
        {
            return (Vector3.Distance(transform.position, num1.transform.position) > Vector3.Distance(transform.position, num2.transform.position)) ? 1 : -1;
        });

        bool isNone = true;
        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            // Despawn이 먼저 호출되어 확인을 안해주면 interactable.IsInteractable 에서
            // 오류가 발생하여 이 줄이 필요합니다.
            if (!hit.GetComponentInParent<NetworkObject>().IsValid) continue;
            if (interactable == null) continue;
            if (!interactable.IsInteractable) continue;

            if (_target == hit.gameObject) return;
            _target = hit.gameObject;
            isNone = false;
            break;
        }

        UIManager.Instance.GetUI<UIInteract>().HideAll();
        if(isNone) return;
        
        if(_target != null )
        {
            string name = "";
            IData data = _target.GetComponentInParent<IData>();
            if (data != null) name = data.DataName;
            else name = _target.gameObject.name;

            UIManager.Instance.GetUI<UIInteract>().ShowText(_target, name);
        }
    }
    void InteractOther()
    {
        if (_target == null) return;
        if(_interactBlock != null)
        {
            _interactBlock.Interact(gameObject);
            _interactBlock = null;
            _isInteracting = false;
            _interactGameObject = null;
            GetComponent<PlayerInputHandler>().IsEnableInputMove = true;
            GetComponent<PlayerInputHandler>().IsEnableInputRotation = true;
        }

        IInteractable interactBlock = _target.GetComponentInParent<IInteractable>();

        if (interactBlock != null)
        {
            if (interactBlock.Interact(gameObject))
            {
                // 일을 진행하는 블록이면 저장합니다.
                // 한 번더 상호작용키를 누르면 취소 가능하게 합니다.
                // 일 진행 중 움직이지 못하게 합니다.
                if (interactBlock.InteractType == InteractType.Work)
                {
                    _interactBlock = interactBlock;
                    _isInteracting = true;
                    _interactGameObject = _target;
                    GetComponent<PlayerInputHandler>().IsEnableInputMove = false;
                    GetComponent<PlayerInputHandler>().IsEnableInputRotation = false;
                    _networkCharacter.SetAnimatorBoolean("Working", true);
                    float angle = Vector3.SignedAngle(transform.forward, _target.transform.position - transform.position, Vector3.up);
                    _networkCharacter.AddAngle(angle);
                }

                _target = null;
            }
        }
    }
}
