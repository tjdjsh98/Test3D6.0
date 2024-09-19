using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
    MouseButton0,
    Interact,
    Run
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
}

public struct InventoryInputData : INetworkInput
{
    // 아이템줍기 관련
    public NetworkBool isAddItem;
    public NetworkId addItemID;

    // 인벤토리 관련
    public NetworkId encounter;
    public NetworkBool isDropItem;
    public int myInventoryIndex;
    public int encounterIndex;
}