using Fusion;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    Camera _camera;
    [SerializeField] GameObject _pivot;
    [SerializeField] float _distance;

    Vector3 _realDistance;

    [SerializeField] float _minRoll;
    [SerializeField] float _maxRoll;
    float _pitch = 0;
    float _roll = 0;

    [SerializeField] float _sphereSize =0.1f;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (!HasInputAuthority) return;
        if(Cursor.lockState != CursorLockMode.Locked) return;

        CalcRealDistacnce();

        _camera.transform.position = _pivot.transform.position + _realDistance;
        _camera.transform.LookAt(_pivot.transform.position);
    }

    void CalcRealDistacnce()
    {
        Vector3 mouseDelta = Input.mousePositionDelta;
        _pitch -= mouseDelta.x;
        _roll -= mouseDelta.y;

        _roll = Mathf.Clamp(_roll, _minRoll, _maxRoll);
        Vector3 direction = new Vector3(Mathf.Cos(_pitch * Mathf.Deg2Rad), Mathf.Sin(_roll * Mathf.Deg2Rad), Mathf.Sin(_pitch * Mathf.Deg2Rad)).normalized;

        _realDistance = direction * _distance;

        if(_pivot != null)
        {
            if(Physics.SphereCast(_pivot.transform.position, _sphereSize,direction, out var hit, _distance, Define.GROUND_LAYERMASK))
            {
                _realDistance = direction * (hit.point - _pivot.transform.position).magnitude;
            }
        }
    }
}