using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;

public class UIItemShower : MonoBehaviour
{
    public static UIItemShower _instance;
    public static UIItemShower Instance
    {
        get
        {
            if (_instance == null)
                Init();
            return _instance;
        }
    }

    [SerializeField] GameObject _uiItemTagPrefab;

    List<UIItemTag> _uiItemTagList = new List<UIItemTag>();

    static void Init()
    {
        _instance = GameObject.Find("UIItemShower").GetComponent<UIItemShower>();
    }

    private void LateUpdate()
    {
        foreach(var itemTag in _uiItemTagList)
        {
            if(itemTag.parent.gameObject.activeSelf)
            {
                Vector3 positionSC = Camera.main.WorldToScreenPoint(itemTag.item.transform.position);
                itemTag.parent.transform.position = positionSC;
            }
        }
    }


    public UIItemTag ShowText(GameObject item, string text)
    {
        UIItemTag itemTag = null;

        foreach(var tempItemTag in _uiItemTagList)
        {
            if (tempItemTag.parent.activeSelf == false)
            {
                itemTag = tempItemTag;
                break;
            }
        }

        if(itemTag ==  null)
        {
            GameObject tag = Instantiate(_uiItemTagPrefab);

            tag.transform.SetParent(transform);
            tag.transform.localScale = Vector3.one;
            itemTag = new UIItemTag();
            itemTag.parent = tag;
            itemTag.tagTextMesh = tag.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            

            _uiItemTagList.Add(itemTag);
        }

        itemTag.item = item;
        Vector3 positionSC = Camera.main.WorldToScreenPoint(item.transform.position);
        itemTag.parent.transform.position = positionSC;

        itemTag.tagTextMesh.text = text;
        itemTag.parent.gameObject.SetActive(true);

        return itemTag;
    }

}


public class UIItemTag
{
    public GameObject parent;
    public GameObject item;
    public TextMeshProUGUI tagTextMesh;
}