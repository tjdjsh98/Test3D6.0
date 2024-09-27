using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    static DataManager _instance;
    public static DataManager Instance
    { get { InitSingleton(); return _instance; } }

    public Dictionary<string, Item> _itemDictionary = new Dictionary<string, Item>();
    public Dictionary<string, NetworkBlock> _networkBlockDictionary = new Dictionary<string, NetworkBlock>();
    public Dictionary<string, ReceiptData> _receiptDictionary = new Dictionary<string, ReceiptData>();


    static void InitSingleton()
    {
        if (_instance != null) return;

        _instance = FindAnyObjectByType<DataManager>();
        _instance.Init();
    }

    private void Awake()
    {
        InitSingleton();
    }
    void Init()
    {
        LoadData<Item>("Prefabs/Item");
        LoadData<NetworkBlock>("Prefabs/Block");
        LoadData<ReceiptData>("Datas/Receipt");
    }


    public void LoadData<T>(string path) where T : UnityEngine.Object, IData 
    {
        T[] list = Resources.LoadAll<T>(path);
        Type type = typeof(T);

        Debug.Log(type + " " + list.Length);

        foreach (T item in list)
        {
            if (type.Equals(typeof(Item)))
                _itemDictionary.Add(item.DataName.ToLower(), item as Item);
            if (type.Equals(typeof(NetworkBlock)))
                _networkBlockDictionary.Add(item.DataName.ToLower(), item as NetworkBlock);
            if (type.Equals(typeof(ReceiptData)))
                _receiptDictionary.Add(item.DataName.ToLower(), item as ReceiptData);

        }
    }

    public T GetData<T>(string dataName)  where T : UnityEngine.Object, IData
    {
        Type type = typeof(T);
        string name = dataName.ToLower();

        if (type.Equals(typeof(Item)))
        {
            _itemDictionary.TryGetValue(name, out Item value);

            return value as T;
        }
        if (type.Equals(typeof(NetworkBlock)))
        {
            _networkBlockDictionary.TryGetValue(name, out NetworkBlock value);

            return value as T;
        }
        if (type.Equals(typeof(ReceiptData)))
        {
            _receiptDictionary.TryGetValue(name, out ReceiptData value);

            return value as T;
        }

        return default(T);
    }
}