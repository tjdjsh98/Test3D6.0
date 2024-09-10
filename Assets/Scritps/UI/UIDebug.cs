using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIDebug : UIBase
{
    [SerializeField] TextMeshProUGUI _historyText;
    [SerializeField] TMP_InputField _inputField;

    RectTransform _rectTransform;

    bool _isShown;
    bool _isMoveDebugWindow = false;
    public override void Init()
    {
        _rectTransform =transform.Find("BG").GetComponent<RectTransform>();
        _inputField.resetOnDeActivation = true;
        _rectTransform.anchoredPosition = new Vector3(0, -_rectTransform.sizeDelta.y-50, 0);

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T) && !_isMoveDebugWindow)
        {
            if(!_isShown)
                ShowDebugWindow();
            else if(!_inputField.isFocused)
                HideDebugWindow();
        }

        if (_isShown)
        {
            if (Input.GetKeyDown(KeyCode.Return) && _inputField.text != string.Empty)
            {
                _historyText.text += _inputField.text + "\n";
                EnterCommand();
            }
        }
    }


    void EnterCommand()
    {
        NetworkRunner networkRunner = FindAnyObjectByType<NetworkRunner>();
        if(networkRunner.IsServer)
        {
            string[] command = _inputField.text.Split(' ');

            int index = 0;
            if (command.Length == 5) 
            {
                if (command[0].Equals("Spawn"))
                {
                    string name = command[1];
                    bool result = int.TryParse(command[2], out int x);
                    result |= int.TryParse(command[3], out int y);
                    result |= int.TryParse(command[4], out int z);

                    if (result)
                    {
                        NetworkObject networkObject = Resources.Load<NetworkObject>($"Prefabs/{name}");

                        Debug.Log(networkObject);
                        if (networkObject != null)
                        {
                            networkRunner.Spawn(networkObject, new Vector3(x, y, z), Quaternion.identity);
                        }
                    }
                }
            }
        }
        _inputField.text = string.Empty;
        _inputField.ActivateInputField();


    }
    public void ShowDebugWindow()
    {
        StartCoroutine(MoveDebugWindowCoroutine(true));
        _inputField.ActivateInputField();
        InputManager.Instance.IsEnableFocus = false;
        InputManager.Instance.IsEnableInput = false;
    }

    public void HideDebugWindow() 
    {
        StartCoroutine(MoveDebugWindowCoroutine(false));
        InputManager.Instance.IsEnableFocus = true;
        InputManager.Instance.IsEnableInput = true;
    }

    IEnumerator MoveDebugWindowCoroutine(bool isOpen)
    {
        _isMoveDebugWindow = true;
        float duration = 10;

        Vector3 currentPos = _rectTransform.anchoredPosition;
        Vector3 destination = isOpen?Vector3.zero:new Vector3(0,-_rectTransform.sizeDelta.y-50,0);

        for(int i = 1; i <= duration; i++)
        {
            _rectTransform.anchoredPosition = Vector3.Lerp(currentPos, destination, i / duration);

            yield return null;
        }

        _isMoveDebugWindow = false;
        _isShown = isOpen;
    }
}
