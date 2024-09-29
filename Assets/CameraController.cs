using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera _camera;
    NetworkObject _networkObject;

    int _currentCameraIndex = 0;
    int _nextCameraIndex = -1;
    [SerializeField]List<CameraData> _cameraDatas;
    Coroutine _cameraChangeCoroutine;
   
    Vector3 _realDistance;

    [SerializeField] float _minRoll;
    [SerializeField] float _maxRoll;
    float _pitch = 0;
    float _roll = 0;

    [SerializeField] bool _flipX;
    [SerializeField] bool _flipY;


    Vector3 pivot;
    Vector3 offset;
    float distance;
    float sphereSize;

    Vector3 _cameraDirection;

    private void Awake()
    {
        _networkObject= GetComponent<NetworkObject>();
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_networkObject != null && !_networkObject.HasInputAuthority) return;

        if (!InputManager.Instance.IsEnableFocus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false; 
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if(Cursor.lockState != CursorLockMode.Locked) return;

        if (_cameraChangeCoroutine == null)
        {
            pivot = _cameraDatas[_currentCameraIndex].pivot.transform.position;
            offset = _cameraDatas[_currentCameraIndex].offset;
            distance = _cameraDatas[_currentCameraIndex].distance;
            sphereSize = _cameraDatas[_currentCameraIndex].sphereSize;
        }

        CalcRealDistacnce();

        _camera.transform.position = pivot + _realDistance;

        //_camera.transform.LookAt(_pivot.transform.position + _cameraDirection);
        _camera.transform.LookAt(pivot + offset.x * transform.right + offset.y * transform.up);
    }

    void CalcRealDistacnce()
    {
        Vector3 mouseDelta = Input.mousePositionDelta;
        _pitch += mouseDelta.x * (_flipX? -1 : 1);
        _roll += mouseDelta.y * (_flipY ? -1: 1);

        _roll = Mathf.Clamp(_roll, _minRoll, _maxRoll);
        Vector3 direction = new Vector3(Mathf.Cos(_pitch * Mathf.Deg2Rad), Mathf.Sin(_roll * Mathf.Deg2Rad), Mathf.Sin(_pitch * Mathf.Deg2Rad)).normalized;

        _cameraDirection = direction;
        _realDistance = direction * distance;
        if(Physics.SphereCast(pivot, sphereSize,direction, out var hit, distance, Define.GROUND_LAYERMASK))
        {
            _realDistance = direction * (hit.point - pivot).magnitude;
        }
    }

    void ChangeCamera(int indexs)
    {
        _nextCameraIndex = indexs;
        if(_cameraChangeCoroutine != null) StopCoroutine( _cameraChangeCoroutine);
        _cameraChangeCoroutine = StartCoroutine(LerpCamera());

    
    }


    IEnumerator LerpCamera()
    {
        int curretIndex = 0;
        int nextIndex = 1;
        for(int i = 1; i <= 60; i++)
        {
            pivot = Vector3.Lerp(_cameraDatas[curretIndex].pivot.transform.position, _cameraDatas[nextIndex].pivot.transform.position, i/60);
            offset= Vector3.Lerp(_cameraDatas[curretIndex].offset, _cameraDatas[nextIndex].offset, i/60);
            distance = Mathf.Lerp(_cameraDatas[curretIndex].distance, _cameraDatas[nextIndex].distance, i / 60);
            sphereSize = Mathf.Lerp(_cameraDatas[curretIndex].sphereSize, _cameraDatas[nextIndex].sphereSize, i / 60);
            yield return null;

        }
        _currentCameraIndex = _nextCameraIndex;
        _cameraChangeCoroutine = null;
    }
}

[Serializable]
public struct CameraData
{
    public GameObject pivot;
    public float distance;
    public float sphereSize;
    public Vector3 offset;

}
