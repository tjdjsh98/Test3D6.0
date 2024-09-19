using Fusion;
using UnityEditor.Timeline.Actions;
using UnityEngine;
public class Inventory : NetworkBehaviour
{
    [SerializeField] int _slotCount = 10;
    public int SlotCount => _slotCount;

    [Networked, OnChangedRender(nameof(OnInventoryChanged))]
    [Capacity(16)]
    NetworkArray<ItemSlot> _slots => MakeInitializer<ItemSlot>(new ItemSlot[10]);
    public InventoryInputData AccumulateInputData { get; set; }


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
        if(HasInputAuthority)
            InputManager.Instance.InsertInventoryInputData(AccumulateInputData);
    }

    void OnInputDataReset()
    {
        AccumulateInputData = default;
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput<NetworkInputData>(out var inputData))
        {
            if (HasStateAuthority)
            {
                if (inputData.inventoryInputData.isAddItem)
                {
                    InsertItem(Runner.FindObject(inputData.inventoryInputData.addItemID).gameObject);
                }

                if (inputData.inventoryInputData.isDropItem)
                {
                    DropItem(inputData.inventoryInputData.myInventoryIndex);
                }
            }
        }
    }

    public bool InsertItem(GameObject gameObject, int count = 1, int index = -1)
    {
        var networkRunner =  FindAnyObjectByType<NetworkRunner>();
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

        return true;
    }
    // 자신의 앞으로 아이템을 버린다.

    public void Awake()
    {
        name = name.Split('(')[0];
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

        Item itemPrefab = Resources.Load<Item>($"Prefabs/Item/{_slots[index].itemName}");
        if(itemPrefab == null) return false;

        var networkRunner = FindAnyObjectByType<NetworkRunner>();

        Item time = networkRunner.Spawn(itemPrefab, transform.position + transform.forward);
        RemoveItem(index);
        return true;
    }
    public bool RemoveItem(int index)
    {
        if(index < 0 || index >= _slots.Length) return false;

        _slots.Set(index, new ItemSlot());
        return true;
    }
    public ItemSlot GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Length) return default;

        return _slots[index];
    }

    void OnInventoryChanged(NetworkBehaviourBuffer previous)
    {

        string d = "";
        Debug.Log(_slots.Length);

        for (int i = 0; i < _slots.Length; i++)
        {
            d += $"{i} : {_slots[i].itemName} {_slots[i].count}\n";
        }
        Debug.Log(d);


        //var preValue = GetArrayReader<NetworkArray<ItemSlot2>>(nameof(slots)).Read(previous);
       
        //d = "";
        //for (int i = 0; i < preValue.Length; i++)
        //{
        //    d += $"{i} : {preValue[0].Get(i).count}\n";
        //}
        //Debug.Log(d);
    }
}

public struct ItemSlot : INetworkStruct
{
    public NetworkString<_64> itemName;
    public int count;
}