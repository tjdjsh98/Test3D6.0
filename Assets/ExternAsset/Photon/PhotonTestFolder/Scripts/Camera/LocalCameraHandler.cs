using Fusion;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    public Transform cameraAnchorPoint;

    Vector2 _viewInput;

    // Rotation
    float _cameraRotationX = 0;
    float _cameraRotationY = 0;

    
    Camera _localCamera;
    NetworkCharacterControllerCustom _characterController;


    private void Awake()
    {
        _localCamera = GetComponent<Camera>();
        _characterController = GetComponentInParent<NetworkCharacterControllerCustom>();
    }

    private void Start()
    {
        if (_localCamera.enabled)
            _localCamera.transform.SetParent(null);

    }

    private void LateUpdate()
    {
        if (cameraAnchorPoint == null)
            return;

        if (!_localCamera.enabled)
            return;

        _localCamera.transform.position = cameraAnchorPoint.position;

        // Calculate rotation
        _cameraRotationX += _viewInput.y * Time.deltaTime * _characterController.viewUpDownSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -90, 90);

        _cameraRotationY += _viewInput.x * Time.deltaTime * _characterController.rotationSpeed;

        // Apply rotation
        _localCamera.transform.rotation = Quaternion.Euler(_cameraRotationX, _cameraRotationY,0);
    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        _viewInput = viewInput;
    }
}
