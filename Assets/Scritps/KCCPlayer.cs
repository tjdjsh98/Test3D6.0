using Fusion;
using UnityEngine;

public class KCCPlayer : NetworkBehaviour
{
    //KCC _kcc;
    //Camera _playerCamera;
    //Animator _animator;

    //[SerializeField] Transform _cameraPostion;
    //[SerializeField] private NetworkObject _ballPrefab;

    //NetworkButtons _previousButtons;


    //public override void Spawned()
    //{
    //    _animator = GetComponentInChildren<Animator>();
    //    _kcc = GetComponent<KCC>();
    //    if (HasInputAuthority)
    //    {
    //        _playerCamera = Camera.main;
    //    }
    //}
    //public override void FixedUpdateNetwork()
    //{
    //    if (_kcc == null) return;
    //    if (GetInput(out NetworkInputData2 input))
    //    {
    //        _kcc.AddLookRotation(input.MouseDelta * Runner.DeltaTime);

    //        if (HasStateAuthority)
    //        {
    //            if (input.Buttons.WasPressed(_previousButtons,NetworkInputData2.MOUSEBUTTON0))
    //            {
    //                Debug.Log("Spanw");
    //                Runner.Spawn(_ballPrefab, transform.position + transform.forward + Vector3.up*2, Quaternion.LookRotation(transform.forward), Object.InputAuthority, (runner, o) =>
    //                {
    //                    o.GetComponent<Ball>().Init();
    //                });

    //            }
    //        }
    //        if (input.Buttons.WasPressed(_previousButtons, InputButton.Jump) )
    //        {
    //            _kcc.Jump(Vector3.up * 1);
    //        }
    //        _kcc.SetInputDirection(transform.TransformDirection(input.Direction)*5);

    //        _animator.SetFloat("VelocityX", input.Direction.x);
    //        _animator.SetFloat("VelocityZ", input.Direction.z);

    //        UpdateCamera();
    //    }

    //}

    //public override void Render()
    //{
    //    UpdateCamera();
    //}
    //void UpdateCamera()
    //{
    //    if (!HasInputAuthority) return;


    //    _playerCamera.transform.position = Vector3.Lerp(_playerCamera.transform.position, _cameraPostion.position, 0.2f);
    //    _playerCamera.transform.localRotation = Quaternion.Euler(_kcc.GetLookRotation().x, _kcc.GetLookRotation().y, 0);
    //}
}
