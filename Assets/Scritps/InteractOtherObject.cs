using Fusion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class InteractOtherObject : NetworkBehaviour, IBeforeTick
{
    // Components
    NetworkCharacter _networkCharacter;

    NetworkButtons _previousButtons;

    InteractInputData _accumulateInteractInputData;
    InteractInputData _currentInteractInputData;

    GameObject _target;

    [SerializeField] Range _interactRange;

    // InteractType이 Work인 오브젝트 관리
    bool _isInteracting;
    IInteractable _interactBlock;
    GameObject _interactGameObject;

    Item _leftItem;

    private void Awake()
    {
        _networkCharacter = GetComponent<NetworkCharacter>();
    }

    private void OnDrawGizmosSelected()
    {
        Utils.DrawRange(gameObject, _interactRange, Color.yellow);
    }
    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            InputManager.Instance.BeforeInputDataSent += OnBeforeInputDataSent;
            InputManager.Instance.InputDataReset += OnInputDataReset;
        }
    }

    void OnBeforeInputDataSent()
    {
        InputManager.Instance.InsertInteractInputData(_accumulateInteractInputData);
    }

    void OnInputDataReset()
    {
        _accumulateInteractInputData = default;
    }
    public void BeforeTick()
    {
        InteractInputData data = _currentInteractInputData;
        data.isInteract = default;
        _currentInteractInputData = data;

        if (GetInput(out NetworkInputData networkInputData))
        {
            _currentInteractInputData = networkInputData.interactInputData;
        }
    }

    // 상호작용은 각 클라이언트에서 판정하여
    // 메인클라이언트에게 누구와 상호작용하고 싶은지에 대한 데이터만 전송합니다.
    public override void Render()
    {
        if (!HasInputAuthority) return;

        DetectInteractableObject();

        if(Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }
    public override void FixedUpdateNetwork()
    {
        ProcessInteractData();
        //if ((_isInteracting && _interactGameObject == null))
        //{
        //    _interactBlock.Interact(gameObject);
        //    _interactBlock = null;
        //    _isInteracting = false;
        //    _interactGameObject = null;
        //    GetComponent<PlayerInputHandler>().IsEnableInputMove = true;
        //    GetComponent<PlayerInputHandler>().IsEnableInputRotation = true;
        //    _networkCharacter.SetAnimatorBoolean("Working", false);
        //}
    }
    void DetectInteractableObject()
    {
        if (!HasInputAuthority) return;

        Vector3 center = transform.position;
        
        Collider[] hits = Utils.RangeOverlapAll(gameObject, _interactRange, Define.INTERACTABLE_LAYERMASK);

        if (hits.Length == 0)
        {
            UIManager.Instance.GetUI<UIInteract>().HideAll();
            _target = null;
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
        if (isNone) return;

        if (_target != null)
        {
            string name = "";
            IData data = _target.GetComponentInParent<IData>();
            if (data != null) name = data.DataName;
            else name = _target.gameObject.name;

            UIManager.Instance.GetUI<UIInteract>().ShowText(_target, name);
        }
    }
    void Interact()
    {
        if (_target == null) return;
        if (_interactBlock != null)
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
            _accumulateInteractInputData.isInteract = true;
            _accumulateInteractInputData.interactTargetID = _target.GetComponentInParent<NetworkObject>().Id;

        }
    }


    void ProcessInteractData()
    {
        if (_currentInteractInputData.isInteract)
        {
            NetworkObject networkObject = Runner.FindObject(_currentInteractInputData.interactTargetID);

            if (networkObject == null) return;
            IInteractable interactable = networkObject.GetComponent<IInteractable>();
            if (interactable == null || !interactable.IsInteractable) return;

            interactable.Interact(gameObject);
            //if (interactBlock.Interact(gameObject))
            //{
            //    // 일을 진행하는 블록이면 저장합니다.
            //    // 한 번더 상호작용키를 누르면 취소 가능하게 합니다.
            //    // 일 진행 중 움직이지 못하게 합니다.
            //    if (interactBlock.InteractType == InteractType.Work)
            //    {
            //        _interactBlock = interactBlock;
            //        _isInteracting = true;
            //        _interactGameObject = _target;
            //        GetComponent<PlayerInputHandler>().IsEnableInputMove = false;
            //        GetComponent<PlayerInputHandler>().IsEnableInputRotation = false;
            //        _networkCharacter.SetAnimatorBoolean("Working", true);
            //        float angle = Vector3.SignedAngle(transform.forward, _target.transform.position - transform.position, Vector3.up);
            //        _networkCharacter.AddAngle(angle);
            //    }

            //    _target = null;
            //}
        }
    }

}
