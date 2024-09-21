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

    public int inventoryCount;
    public InventoryInputData inventoryInputData;
    public InventoryInputData inventoryInputData2;
    public InventoryInputData inventoryInputData3;
    public InventoryInputData inventoryInputData4;
    public InventoryInputData inventoryInputData5;
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
    public NetworkId ObjectId;
    public NetworkBehaviourId inventoryId;
    public NetworkId encounterId;
    public NetworkBool isDropItem;
    public int myInventoryIndex;
    public int encounterIndex;
}