using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    static UIManager _instance = null;
    public static UIManager Instance
    {
        get { InitSingleton(); return _instance; }
    }

    List<UIBase> _uiList = new List<UIBase>();
    Canvas _canvas;
    Image _dragItemImage;


    // 인벤토리 공유 변수
    public int DragItemSlotIndex { get; set; }
    public Inventory StartedDragInventory { get; set; }

    static void InitSingleton()
    {
        if (_instance != null) return;

        _instance= FindAnyObjectByType<UIManager>();
        _instance.Init();
    }

    public void Init()
    {
        _canvas = GameObject.FindAnyObjectByType<Canvas>();
        foreach (var ui in FindObjectsByType<UIBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ui.Init();
            _uiList.Add(ui);
        }
        _dragItemImage = new GameObject().AddComponent<Image>();
        _dragItemImage.gameObject.SetActive(false);
        _dragItemImage.transform.SetParent(_canvas.transform);
        _dragItemImage.raycastTarget = false;
        _dragItemImage.GetComponent<RectTransform>().sizeDelta = Vector2.one * 100;
        _dragItemImage.transform.localScale = Vector3.one;

    }

    private void Awake()
    {
        InitSingleton();
    }

    private void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            StartedDragInventory = null;
            DragItemSlotIndex = -1;
        }
    }
    private void LateUpdate()
    {
        if (_dragItemImage.gameObject.activeSelf)
        {
            _dragItemImage.transform.position = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                _dragItemImage.gameObject.SetActive(false);
            }
        }
    }

    public void DragIItem(Sprite sprite)
    {
        _dragItemImage.sprite = sprite;
        _dragItemImage.gameObject.SetActive(true);
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
