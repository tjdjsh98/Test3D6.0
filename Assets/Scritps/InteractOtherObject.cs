using Fusion;
using UnityEngine;

public class InteractOtherObject : NetworkBehaviour
{
    UIInteract _uiItemShower;

    NetworkButtons _previousButtons;

    GameObject _nameTagTarget;

    NameTag _nameTagPrefab;
    NameTag _nameTag;

    private void Awake()
    {
        _uiItemShower = UIManager.Instance.GetUI<UIInteract>();
        _nameTagPrefab = Resources.Load<NameTag>("Prefabs/NameTag");
        _nameTag = Instantiate<NameTag>(_nameTagPrefab);
        _nameTag.gameObject.SetActive(false);
    }
    private void Update()
    {
        DetectInteractableObject();
    }
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out PlayerInputData networkInputData) && Runner.IsForward)
        {
            if(networkInputData.buttons.WasPressed(_previousButtons, InputButton.Interact))
            {
                InteractOther();
            }
            _previousButtons = networkInputData.buttons;
        }
    }
    void DetectInteractableObject()
    {
        Collider[] hits = Physics.OverlapBox(transform.position + Vector3.forward + Vector3.up * 0.5f
            , Vector3.one, transform.rotation, Define.INTERACTABLE_LAYERMASK);

        if (hits.Length == 0)
        {
            if(_nameTag.gameObject.activeSelf == true)
            {
                _nameTag.gameObject.SetActive(false);
                _nameTagTarget = null;
            }
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
        if(_nameTagTarget != null )
        {
            UIManager.Instance.GetUI<UIInteract>().HideAll();
           UIManager.Instance.GetUI<UIInteract>().ShowText(_nameTagTarget, _nameTagTarget.gameObject.name);
        }
    }
    void InteractOther()
    {
        if (!Object.HasStateAuthority) return;

        // 가장 가까운 아이템부터 흭득한다.
    }
}
