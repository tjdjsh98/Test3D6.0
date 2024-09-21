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

    public bool InsertItem(GameObject gameObject, int count = 1, int index = -1)
    {
        var networkRunner =  GameObject.FindAnyObjectByType<NetworkRunner>();
        Item item = gameObject.GetComponent<Item>();
        if (item == null) return false;
        string itemName = gameObject.name;

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
                if (_slots[i].itemName == itemName)
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
                tempSlot.itemName = itemName;
                tempSlot.count = count;
                _slots.Set(emptySlot, tempSlot);
                networkRunner.Despawn(item.Object);
            }
        }
        else
        {
            if (_slots[index].itemName == itemName )
            {
                ItemSlot tempSlot = _slots[index];
                tempSlot.count += count;
                _slots.Set(index, tempSlot);
                networkRunner.Despawn(item.Object);
            }
            else 
            {
                ItemSlot tempSlot = _slots[index];
                tempSlot.itemName = itemName;
                tempSlot.count = count;
                _slots.Set(index, tempSlot);
                networkRunner.Despawn(item.Object);
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

        Debug.Log(index);

        Item itemPrefab = Resources.Load<Item>($"Prefabs/Item/{_slots[index].itemName}");
        if(itemPrefab == null) return false;

        var networkRunner = GameObject.FindAnyObjectByType<NetworkRunner>();

        Item time = networkRunner.Spawn(itemPrefab, transform.position + transform.forward);
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
    public NetworkString<_64> itemName;
    public int count;
}