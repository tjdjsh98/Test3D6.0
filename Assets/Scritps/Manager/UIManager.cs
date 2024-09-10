using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    static UIManager _instance = null;
    public static UIManager Instance
    {
        get { InitSingleton(); return _instance; }
    }

    List<UIBase> _uiList = new List<UIBase>();


    static void InitSingleton()
    {
        if (_instance != null) return;

        _instance= FindAnyObjectByType<UIManager>();
        _instance.Init();
    }

    public void Init()
    {
        foreach (var ui in FindObjectsByType<UIBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ui.Init();
            _uiList.Add(ui);
        }
    }

    private void Awake()
    {
        InitSingleton();
    }



    public T GetUI<T>() where T : UIBase
    {
        foreach (var ui in _uiList)
        {
            T result = ui as T;
            if (result != null) return result;
        }

        return default(T);
    }
}
