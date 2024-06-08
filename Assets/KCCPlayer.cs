using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;

public class KCCPlayer : NetworkBehaviour
{
    KCC _kcc;
    Camera _playerCamera;

    public override void Spawned()
    {
        _kcc = GetComponent<KCC>();
        if (HasInputAuthority)
        {
            _playerCamera = Camera.main;
            SetInvisableModel(transform);
        }
    }

    void SetInvisableModel(Transform tr)
    {
        for (int i = 0; i < tr.childCount; i++)
        {
            SkinnedMeshRenderer meshRenderer = tr.GetChild(i).GetComponent<SkinnedMeshRenderer>();
            if (meshRenderer)
            {
                Debug.Log(meshRenderer.name);
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            if (tr.GetChild(i).childCount > 0)
            {
                SetInvisableModel(tr.GetChild(i));
            }
            
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_kcc == null) return;
        if (GetInput(out NetworkInputData input))
        {
            _kcc.AddLookRotation(input.MouseDelta * Runner.DeltaTime);
            UpdateCamera();
            _kcc.SetInputDirection(transform.TransformDirection(input.Direction)*5);
        }

    }

    public override void Render()
    {
        UpdateCamera();
    }
    void UpdateCamera()
    {
        if (!HasInputAuthority) return;

        _playerCamera.transform.position = transform.position + Vector3.up * 1.5f;
        _playerCamera.transform.localRotation = Quaternion.Euler(_kcc.GetLookRotation().x, _kcc.GetLookRotation().y, 0);
    }
}
