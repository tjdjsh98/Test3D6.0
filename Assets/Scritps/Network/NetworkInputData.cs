using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
    MouseButton0,
    Interact,
}
public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;
    
    public Vector3 aimForwardVector;
    public Vector2 movementInput;
    public NetworkButtons buttons;
}
