using System.Runtime.InteropServices;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);

    System.IntPtr _windowPtr;
    bool _windowPtrRec;

    public CinemachineCamera ThirdPersonCamera;
    public CinemachineCamera FirstPersonCamera;

    public Transform SpawnPosition;

    private void Update()
    {
        if (!_windowPtrRec)
        {
            _windowPtr = FindWindow(null, "Test3D6.0");
            _windowPtrRec = true;
        }
        SetWindowText(_windowPtr, (1.0f / Time.deltaTime).ToString());
    }
}
