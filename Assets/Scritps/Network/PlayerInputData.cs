using Fusion;
using System;
using Unity.Collections;
using UnityEngine;

public enum InputButton
{
    Jump,
    MouseButton0,
    Interact,
    Run,
    Throw,
    Num1,
    Num2,
    Num3,
    Num4,
    HoldRope,
    ReleaseRope,
}

public struct NetworkInputData : INetworkInput
{
    public PlayerInputData playerInputData;
    public InventoryInputData inventoryInputData;
    public InteractInputData interactInputData;
    public WorkingInputData workingInputData;
}
public struct PlayerInputData : INetworkStruct
{
    public const byte MOUSEBUTTON0 = 1;
    
    public Vector3 aimForwardVector;
    public Vector3 bodyForwardVector;
    public Vector2 movementInput;
    public NetworkButtons buttons;
    public Vector2 lookRotationDelta;
    public Vector3 moveDelta;
    public Vector3 velocity;

    public NetworkId holdRopeID;
}

public struct InventoryInputData : INetworkStruct
{
    // 아이템줍기 관련
    public NetworkBool isAddItem;
    public NetworkId addItemID;

    // 인벤토리 관련
    public NetworkBool isDropItem;
    public NetworkBool isExchangeItem;

    public NetworkId myInventoryObjectId;
    public NetworkBehaviourId myInventoryId;

    public NetworkId encounterInventoryObjectId;
    public NetworkBehaviourId encounterInventoryId;

    public int myInventoryIndex;
    public int encounterInventoryIndex;
}

public struct InteractInputData : INetworkStruct
{
    public NetworkBool isInteract;
    public NetworkId interactTargetID;
}

public struct WorkingInputData : INetworkStruct
{
    public NetworkBool isWorking;
    public NetworkBool isCancelWorking;
    public NetworkId workingTargetID;
}
