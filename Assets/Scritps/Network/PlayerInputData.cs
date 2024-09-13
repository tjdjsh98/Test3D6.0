using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
    MouseButton0,
    Interact,
    Run
}
public struct PlayerInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;
    
    public Vector3 aimForwardVector;
    public Vector3 bodyForwardVector;
    public Vector2 movementInput;
    public NetworkButtons buttons;
    public Vector2 lookRotationDelta;
    public Vector3 moveDelta;
}
