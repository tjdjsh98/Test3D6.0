using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacter : MonoBehaviour
{
    Character _character;

    [SerializeField] RectTransform _hpBarTr;
    [SerializeField] TextMeshProUGUI _hpTextMesh;
    Vector2 _initHpbarSize;

    public void ConnectCharacter(Character character)
    {
        _character = character;
        _initHpbarSize = _hpBarTr.sizeDelta;
    }

    public void Update()
    {
        ShowHp();
    }

    void ShowHp()
    {
        if (_character == null) return;

        float ratio = (float)_character.HP / _character.MaxHp;

        _hpBarTr.sizeDelta = new Vector2(_initHpbarSize.x * ratio, _initHpbarSize.y);
        _hpTextMesh.text = _character.HP.ToString();
    }
}
