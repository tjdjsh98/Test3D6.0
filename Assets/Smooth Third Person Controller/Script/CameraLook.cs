using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [SerializeField] private float cameraSensitivity;

    private float Xrot, Yrot;

    private void Update()
    {
        Xrot += Input.GetAxis("Mouse X") * cameraSensitivity;
        Yrot -= Input.GetAxis("Mouse Y") * cameraSensitivity;

        Yrot = Mathf.Clamp(Yrot, -90, 90);

        transform.rotation = Quaternion.Euler(Yrot, Xrot,0);
    }
}
