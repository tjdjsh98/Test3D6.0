using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Camera _camera;
    void Awake()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        transform.LookAt( _camera.transform.position);
    }
}
