using Fusion;
using System;
using TMPro;
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
        Item item = gameObject.GetComponent<Item>();
        if (item == null) return false;
        string itemName = item.DataName;

        if (index == -1)
        {
            int emptySlot = -1;
            for(int i = 0; i < _slotCount; i++)  
            {
                if (_slots[i].itemId == default(NetworkId))
                {
                    emptySlot = i;
                    break;
                }
                if (item.IsStackable && _slots[i].itemName == itemName)
                {
                    ItemSlot tempSlot = _slots[i];
                    tempSlot.count += count;
                    _slots.Set(i, tempSlot);

                    Runner.Despawn(item.Object);
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
                item.IsUseRigidbody = false;
                item.IsInteractable = false;
            }
        }
        else
        {
            if(index < 0 || index >= _slotCount) return false;

            if (item.IsStackable && _slots[index].itemName == itemName )
            {
                ItemSlot tempSlot = _slots[index];
                tempSlot.count += count;
                _slots.Set(index, tempSlot);
                Runner.Despawn(item.Object);
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
                item.IsUseRigidbody = false;
                item.IsInteractable = false;
            }
        }
        ItemChanged?.Invoke();
        return true;
    }
    // 기존에 있던 아이템이어서 수량을 누적할 때는 Despawn해줍니다.
    // 새로운 아이템이라면 아이템을 만들어낸 후 비활성화해줍니다.
    public bool InsertItem(string name, int count = 1, int index = -1)
    {
        Item item = DataManager.Instance.GetData<Item>(name);
        if (item == null) return false;
        string itemName = item.DataName;

        if (index == -1)
        {
            int emptySlot = -1;
            for (int i = 0; i < _slotCount; i++)
            {
                if (emptySlot == -1 && _slots[i].itemName == "")
                {
                    emptySlot = i;
                }
                // 누적시킨다.
                if (item.IsStackable && _slots[i].itemName == itemName)
                {
                    ItemSlot tempSlot = _slots[i];
                    tempSlot.count += count;
                    _slots.Set(i, tempSlot);

                    ItemChanged?.Invoke();
                    return true;
                }
            }

            // 빈 곳이 없음
            if (emptySlot == -1) return false;
            // 빈 곳에 넣어준다.
            {
                Item networkItem = Runner.Spawn(item);

                ItemSlot tempSlot = _slots[emptySlot];
                tempSlot.itemId = networkItem.Object.Id;
                tempSlot.itemName = itemName;
                tempSlot.count = count;
                _slots.Set(emptySlot, tempSlot);

                networkItem.IsHide = true;
                networkItem.IsUseRigidbody = false;
                networkItem.IsInteractable = false;

            }
        }
        else
        {
            if(index < 0 || index >= _slotCount) return false;
            if (item.IsStackable && _slots[index].itemName == itemName)
            {
                ItemSlot tempSlot = _slots[index];
                tempSlot.count += count;
                _slots.Set(index, tempSlot);
            }
            else
            {
                Item networkItem = Runner.Spawn(item);
                ItemSlot tempSlot = _slots[index];
                tempSlot.itemId =  default;
                tempSlot.itemName = itemName;
                tempSlot.count = count;
                _slots.Set(index, tempSlot);
                networkItem.IsHide = true;
                networkItem.IsUseRigidbody = false;
                networkItem.IsInteractable = false;
            }
        }
        ItemChanged?.Invoke();
        return true;
    }
    public bool SetSlot(ItemSlot slot, int index)
    {
        if(index < 0 || index >= _slotCount) return false;
        if (_slots.Get(index).itemName != "") return false;

        _slots.Set(index, slot);
        ItemChanged?.Invoke();
        return true;
    }
    public bool DropItem(int index)
    {
        if(index < 0 || index >= _slotCount) return false;

        NetworkObject obj = Runner.FindObject(_slots[index].itemId);
        // 저장된 아이템이 있다면 활성화
        // 마지막 한 개 아이템이라면
        // 아이템 아이디는 기본값으로 변경 -> 추후에 다시 버릴 때 새로 만들어서 버리게 하기
        if (_slots[index].count == 1 && obj)
        {
            obj.transform.position = transform.position + transform.forward;
            ItemSlot slot = _slots.Get(index);
            slot.itemId = default;
            _slots.Set(index, slot);
            Item item = obj.GetComponent<Item>();
            item.IsHide = false;
            item.IsUseRigidbody = true;
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
    // 제거하고 부족한 갯수를 반환한다.
    public int DestroyItem(string itemName, int count)
    {
        int remainCount = count;
        for(int i = 0; i < _slotCount;i++)
        {
            ItemSlot slot = _slots.Get(i);
            
            int substractCount = slot.count < remainCount ? slot.count : remainCount;
            slot.count -= substractCount;
            remainCount -= substractCount;

            if (slot.count == 0)
            {
                if (slot.itemId != default)
                {
                    NetworkObject item = Runner.FindObject(slot.itemId);
                    if (item)
                        Runner.Despawn(item);
                }

                _slots.Set(i, new ItemSlot());
            }
            else
            {
                _slots.Set(i, slot);
            }

            if(remainCount == 0)
                break;
        }
        ItemChanged?.Invoke();
        return remainCount;
    }
    public bool RemoveItem(int index)
    {
        if(index < 0 || index >= _slots.Length) return false;

        if (_slots[index].count == 1)
        {
            _slots.Set(index, new ItemSlot());
        }
        else
        {
            ItemSlot slot = _slots.Get(index);
            slot.count--;
            _slots.Set(index, slot);
        }
        ItemChanged?.Invoke();
        return true;
    }
 
    public int GetItemCount(string name)
    {
        int count = 0;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].itemName.Equals(name))
            {
                
                Debug.Log(_slots[i].count);
                count += _slots[i].count;
            }
        }
        return count;
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