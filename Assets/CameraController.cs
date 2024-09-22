using Fusion;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    Camera _camera;
    [SerializeField] GameObject _pivot;
    [SerializeField] float _distance;

    [SerializeField] float _minRoll;
    [SerializeField] float _maxRoll;
    float _pitch = 0;
    float _roll = 0;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (!HasInputAuthority) return;

        Vector3 mouseDelta = Input.mousePositionDelta;
        _pitch -= mouseDelta.x;  
        _roll -= mouseDelta.y;

        _roll = Mathf.Clamp(_roll, _minRoll, _maxRoll);
        Vector3 direction = new Vector3(Mathf.Cos(_pitch * Mathf.Deg2Rad), Mathf.Sin(_roll * Mathf.Deg2Rad), Mathf.Sin(_pitch * Mathf.Deg2Rad));
        _camera.transform.position = _pivot.transform.position +
            direction * _distance;
        _camera.transform.LookAt(_pivot.transform.position);
    }
}
