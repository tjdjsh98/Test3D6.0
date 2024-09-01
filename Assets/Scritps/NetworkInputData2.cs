using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
}
public struct NetworkInputData2 : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;

    public Vector3 Direction;
    public Vector3 MouseDelta;
    public NetworkButtons Buttons;
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public Vector3 aimForwardVector;
    public NetworkBool isJumpPressed;
    public NetworkBool isFireButtonPressed;
}