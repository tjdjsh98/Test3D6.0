using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
    MouseButton0,
    Interact,
    Run,
    Throw,
}

public struct NetworkInputData : INetworkInput
{
    public PlayerInputData playerInputData;
    public InventoryInputData inventoryInputData;
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
    public Vector3 velocity;
}

public struct InventoryInputData : INetworkInput
{
    // �������ݱ� ����
    public NetworkBool isAddItem;
    public NetworkId addItemID;

    // �κ��丮 ����
    public NetworkId encounter;
    public NetworkBool isDropItem;
    public int myInventoryIndex;
    public int encounterIndex;
}