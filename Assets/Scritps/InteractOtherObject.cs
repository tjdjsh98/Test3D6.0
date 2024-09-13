using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class InteractOtherObject : NetworkBehaviour
{
    UIItemShower _uiItemShower;

    NetworkButtons _previousButtons;

    // 캐릭터 주변에 상호작용할 수 있는 물체가 있는지 확인합니다.
    // 물체가 있고 로컬캐릭터라면 UI를 띄웁니다.

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

        // 가장 가까운 아이템부터 흭득한다.

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
