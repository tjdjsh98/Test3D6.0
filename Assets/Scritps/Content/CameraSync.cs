using UnityEngine;

public class CameraSync : MonoBehaviour
{
    private Camera _camera;
    public Camera cameraSync;

    void Start()
    {
        _camera = GetComponent<Camera>();   
    }

    // Update is called once per frame
    void Update()
    {
        _camera.fieldOfView = cameraSync.fieldOfView;
        _camera.transform.position = cameraSync.transform.position;
        _camera.transform.rotation = cameraSync.transform.rotation;
    }
}
