using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UIHpBarItem
{
    public GameObject parent;
    public RectTransform front;
    public RectTransform back;
    public IDamageable character;
}

public class UIHpBar : UIBase
{
    [SerializeField] GameObject _hpbarPrefab;
    List<UIHpBarItem> _hpBarList = new List<UIHpBarItem>();

    Camera _camera;
    public override void Init()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        foreach(var item in _hpBarList)
        {
            if(item.parent.gameObject.activeSelf)
            {
                float ratio = (float)item.character.Hp / (item.character.MaxHp != 0 ? item.character.MaxHp : 1);
                item.front.offsetMax = new Vector2(-(1f-ratio)*200, 0);

                item.parent.transform.position = _camera.WorldToScreenPoint(item.character.GameObject.transform.position);
            }
        }
    }

    void InstantiateHpBar()
    {
        GameObject bar = Instantiate(_hpbarPrefab);
        bar.transform.SetParent(transform, false);
        bar.gameObject.SetActive(false);
        _hpBarList.Add(new UIHpBarItem() { parent = bar, front = bar.transform.Find("Front").GetComponent<RectTransform>(), back = bar.transform.Find("Back").GetComponent<RectTransform>() });
    }

    public void AssignHpBar(IDamageable character)
    {
        UIHpBarItem hpBar = null;
        foreach(var item in _hpBarList)
        {
            if (item.parent.gameObject.activeSelf == false)
            {
                hpBar = item;
                break;
            }
        }

        if(hpBar == null) InstantiateHpBar();

        hpBar = _hpBarList[_hpBarList.Count-1];
        hpBar.character = character;
        hpBar.parent.gameObject.SetActive(true);
    }
}
