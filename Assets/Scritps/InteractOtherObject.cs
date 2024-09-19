using Fusion;
using UnityEngine;

public class InteractOtherObject : NetworkBehaviour
{

    // Components
    UIInteract _uiItemShower;
    NetworkCharacter _networkCharacter;
    PlayerInputHandler _playerInputHandler;


    NetworkButtons _previousButtons;

    GameObject _nameTagTarget;


    // InteractType이 Work인 오브젝트 관리
    bool _isInteracting;
    IInteractable _interactBlock;
    GameObject _interactGameObject;

    private void Awake()
    {
        _networkCharacter = GetComponent<NetworkCharacter>();
        _uiItemShower = UIManager.Instance.GetUI<UIInteract>();
    }

    private void OnDrawGizmosSelected()
    {
      
    }
    private void Update()
    {
        DetectInteractableObject();
      
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

        Vector3 center = Vector3.forward + Vector3.up * 0.5f;
        center.z = Mathf.Cos(transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
        center.x = Mathf.Sin(transform.rotation.eulerAngles.y * Mathf.Deg2Rad);
        center += transform.position;

        Collider[] hits = Physics.OverlapBox(center
            , Vector3.one, transform.rotation, Define.INTERACTABLE_LAYERMASK);

        if (hits.Length == 0)
        {
            UIManager.Instance.GetUI<UIInteract>().HideAll();
        }

        float preDistance = float.PositiveInfinity;
        GameObject result = null;
        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < preDistance)
            {
                result = hit.gameObject;
                preDistance = distance;
            }
        }

        if (result == _nameTagTarget) return;
        _nameTagTarget = result;
        UIManager.Instance.GetUI<UIInteract>().HideAll();
        if(_nameTagTarget != null )
        {
            UIManager.Instance.GetUI<UIInteract>().ShowText(_nameTagTarget, _nameTagTarget.gameObject.name);
        }
    }
    void InteractOther()
    {
        if (!Object.HasInputAuthority) return;
        if (_nameTagTarget == null) return;
        if(_interactBlock != null)
        {
            _interactBlock.Interact(gameObject);
            _interactBlock = null;
            _isInteracting = false;
            _interactGameObject = null;
            GetComponent<PlayerInputHandler>().IsEnableInputMove = true;
            GetComponent<PlayerInputHandler>().IsEnableInputRotation = true;
        }

        IInteractable interactBlock = _nameTagTarget.GetComponentInParent<IInteractable>();

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
                    _interactGameObject = _nameTagTarget;
                    GetComponent<PlayerInputHandler>().IsEnableInputMove = false;
                    GetComponent<PlayerInputHandler>().IsEnableInputRotation = false;
                    _networkCharacter.SetAnimatorBoolean("Working", true);
                    float angle = Vector3.SignedAngle(transform.forward, _nameTagTarget.transform.position - transform.position, Vector3.up);
                    _networkCharacter.AddAngle(angle);
                }
            }
        }
    }
}
