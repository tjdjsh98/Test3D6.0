using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
}
public struct NetworkInputData : INetworkInput
{
    public Vector3 Direction;
    public Vector3 MouseDelta;
    public NetworkButtons Buttons;
}
