using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    [SerializeField] TextMeshPro _nameText;


    public void ShowText(string text)
    {
        _nameText.text = text;
    }
}
