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
    public ReceiptInputData receiptInputData;
}

public struct PlayerInputData : INetworkStruct
{
    public const byte MOUSEBUTTON0 = 1;

    public Vector3 cameraPosition;
    public Vector3 aimForwardVector;
    public Vector3 bodyForwardVector;
    public Vector2 movementInput;
    public NetworkButtons buttons;
    public Vector2 lookRotationDelta;
    public Vector3 animatorDeltaAngle;
    public Vector3 animatorVelocity;
    public Vector3 moveDelta;
    public Vector3 velocity;

    public NetworkBool IsEnableInputMove;
    public NetworkBool IsEnableInputRotate;
    public NetworkBool IsEnableAnimationMove;
    public NetworkBool IsEnableAnimationRotate;

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

public struct ReceiptInputData : INetworkStruct
{
    public NetworkBool isReceipt;
    public NetworkString<_32> receptName;
}