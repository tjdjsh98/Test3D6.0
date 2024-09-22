using Fusion;
using System;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [Networked][SerializeField] int _slotCount { get; set; } = 10;
    public int SlotCount => _slotCount;
    
    [Networked, OnChangedRender(nameof(OnInventoryChanged))]
    [Capacity(16)]
    NetworkArray<ItemSlot> _slots { get; }= NetworkBehaviour.MakeInitializer(new ItemSlot[16]);
    public InventoryInputData _accumulateInputData;
    public InventoryInputData AccumulateInputData { get
        {
         return _accumulateInputData;
        } set { _accumulateInputData = value;  _isInput = true; }
    }

    public Action ItemChanged { get; set; }
    bool _isInput;

    public override void Spawned()
    {
        InputManager.Instance.BeforeInputDataSent += OnBeforeInputDataSent;
        InputManager.Instance.InputDataReset += OnInputDataReset;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        InputManager.Instance.BeforeInputDataSent -= OnBeforeInputDataSent;
        InputManager.Instance.InputDataReset -= OnInputDataReset;
    }

    void OnBeforeInputDataSent()
    {
        if (HasInputAuthority && _isInput)
        {
            InputManager.Instance.InsertInventoryInputData(AccumulateInputData);
        }
    }

    void OnInputDataReset()
    {
        if (_isInput)
        {
            AccumulateInputData = default;
            _isInput = false;
        }
    }


    // 기존에 있던 아이템이어서 수량을 누적할 때는 Despawn해줍니다.
    // 새로운 아이템이라면 아이템을 비활성화해줍니다.
    public bool InsertItem(GameObject gameObject, int count = 1, int index = -1)
    {
        var networkRunner =  GameObject.FindAnyObjectByType<NetworkRunner>();
        Item item = gameObject.GetComponent<Item>();
        if (item == null) return false;
        string itemName = item.DataName;

        if (index == -1)
        {
            int emptySlot = -1;
            for(int i = 0; i < _slots.Length; i++)  
            {
                if (_slots[i].itemName == "")
                {
                    emptySlot = i;
                    break;
                }
                if (item.IsStackable && _slots[i].itemName == itemName)
                {
                    ItemSlot tempSlot = _slots[i];
                    tempSlot.count += count;
                    _slots.Set(i, tempSlot);

                    networkRunner.Despawn(item.Object);
                    ItemChanged?.Invoke();
                    return true;
                }
            }

            if (emptySlot == -1) return false;
            {
                ItemSlot tempSlot = _slots[emptySlot];
                NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
                tempSlot.itemId = networkObject? networkObject.Id : default;
                tempSlot.itemName = itemName;
                tempSlot.count = count;
                _slots.Set(emptySlot, tempSlot);
                item.IsHide = true;
                item.IsInteractable = false;
            }
        }
        else
        {
            if (item.IsStackable && _slots[index].itemName == itemName )
            {
                ItemSlot tempSlot = _slots[index];
                tempSlot.count += count;
                _slots.Set(index, tempSlot);
                networkRunner.Despawn(item.Object);
            }
            else 
            {
                ItemSlot tempSlot = _slots[index];
                NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
                tempSlot.itemId = networkObject ? networkObject.Id : default;
                tempSlot.itemName = itemName;
                tempSlot.count = count;
                _slots.Set(index, tempSlot);
                item.IsHide = true;
                item.IsInteractable = false;
            }
        }
        ItemChanged?.Invoke();
        return true;
    }
    
    public bool SetSlot(ItemSlot slot, int index)
    {
        if(index < 0 || index >= _slots.Length) return false;


        _slots.Set(index, slot);
        return true;
    }
    public bool DropItem(int index)
    {
        if(index < 0 || index >= _slots.Length) return false;

        NetworkObject obj = Runner.FindObject(_slots[index].itemId);
        // 저장된 아이템이 있다면 활성화
        // 아이템 아이디는 기본값으로 변경 -> 추후에 다시 버릴 때 새로 만들어서 버리게 하기
        if (obj)
        {
            obj.transform.position = transform.position + transform.forward;
            ItemSlot slot = _slots.Get(index);
            slot.itemId = default;
            _slots.Set(index, slot);
            Item item = obj.GetComponent<Item>();
            item.IsHide = false;
            item.IsInteractable = true;

        }
        // 업다면 새로 만들어줍니다.
        else
        {
            Item itemPrefab = Resources.Load<Item>($"Prefabs/Item/{_slots[index].itemName}");
            if (itemPrefab == null) return false;
            Item time = Runner.Spawn(itemPrefab, transform.position + transform.forward);
        }
        RemoveItem(index);
        ItemChanged?.Invoke();
        return true;
    }
    public bool RemoveItem(int index)
    {
        if(index < 0 || index >= _slots.Length) return false;

        _slots.Set(index, new ItemSlot());
        ItemChanged?.Invoke();
        return true;
    }
    public ItemSlot GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Length) return default;

        return _slots[index];
    }

    void OnInventoryChanged(NetworkBehaviourBuffer previous)
    {
        ItemChanged?.Invoke();
    }
}

public struct ItemSlot : INetworkStruct
{
    public NetworkId itemId;
    public NetworkString<_64> itemName;
    public int count;
}