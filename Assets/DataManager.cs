using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    static DataManager _instance;
    public static DataManager Instance
    { get { InitSingleton(); return _instance; } }

    public Dictionary<string, Item> _itemDictionary = new Dictionary<string, Item>();

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
    }


    public void LoadData<T>(string path) where T : MonoBehaviour, IData
    {
        T[] list = Resources.LoadAll<T>(path);
        Debug.Log(list.Length);

        foreach (T item in list)
        {
            Debug.Log(item as Item);
            _itemDictionary.Add(item.DataName, item as Item);
        }
    }

    public T GetData<T>(string dataName)  where T : MonoBehaviour, IData
    {
        if(typeof(T) == typeof(Item))
        {
            Debug.Log(dataName);
            _itemDictionary.TryGetValue(dataName, out var item);
            if (item != null) return item as T;
        }

        return null;
    }
}