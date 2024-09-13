using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class InteractOtherObject : NetworkBehaviour
{
    UIItemShower _uiItemShower;

    NetworkButtons _previousButtons;

    // ĳ���� �ֺ��� ��ȣ�ۿ��� �� �ִ� ��ü�� �ִ��� Ȯ���մϴ�.
    // ��ü�� �ְ� ����ĳ���Ͷ�� UI�� ���ϴ�.

    List<GameObject> _aroundItemList = new List<GameObject>();
    List<UIItemTag> _arountItemUIList = new List<UIItemTag>();

    private void Awake()
    {
        _uiItemShower = UIManager.Instance.GetUI<UIItemShower>();
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
        Collider[] hits = Physics.OverlapSphere(transform.position, 2, LayerMask.GetMask("Item"));

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                GameObject gameObject = hits[i].gameObject;
                if (_aroundItemList.Contains(gameObject)) continue;

                _aroundItemList.Add(gameObject);
                if (Object.HasInputAuthority)
                    _arountItemUIList.Add(_uiItemShower.ShowText(gameObject, gameObject.name));
            }
        }

        for (int i = _aroundItemList.Count - 1; i >= 0; i--)
        {
            if (_aroundItemList[i] == null || Vector3.Distance(_aroundItemList[i].gameObject.transform.position, gameObject.transform.position) > 2)
            {
                _aroundItemList.RemoveAt(i);
                if (Object.HasInputAuthority)
                {
                    _arountItemUIList[i].parent.gameObject.SetActive(false);
                    _arountItemUIList.RemoveAt(i);
                }
            }
        }
    }
    void InteractOther()
    {
        if (!Object.HasStateAuthority) return;

        // ���� ����� �����ۺ��� ŉ���Ѵ�.

        int index = -1;
        float closeDistance = float.MaxValue;
        for (int i = 0; i < _aroundItemList.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, _aroundItemList[i].transform.position);
            if (distance < closeDistance)
            {
                index = i;
                closeDistance = distance;
            }
        }
        Debug.Log(_aroundItemList.Count);

        if (index != -1)
        {
            _aroundItemList[index].GetComponentInParent<IInteractable>().Interact(gameObject);
            Debug.Log(_aroundItemList[index]);
        }
    }
}
