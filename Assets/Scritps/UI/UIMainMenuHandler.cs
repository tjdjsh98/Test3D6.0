using TMPro;
using UnityEngine;

public class UIMainMenuHandler : UIBase
{
    [Header("Panels")]
    [SerializeField] GameObject _playerDetailspPanel;
    [SerializeField] GameObject _sessionBrowserPanel;
    [SerializeField] GameObject _statusPanel;
    [SerializeField] GameObject _createSessionPanel;


    [Header("PlayerSetting")]
    [SerializeField] TextMeshProUGUI _nicknameText;

    [Header("New Game Session")]
    [SerializeField] TextMeshProUGUI _sessionNameInputField;
    public override void Init() { }



    void HideAllPanels()
    {
        _playerDetailspPanel.gameObject.SetActive(false);
        _sessionBrowserPanel.gameObject.SetActive(false);
    }


    public void OnFindGameClicked()
    {
        PlayerPrefs.SetString("PlayerNickname", _nicknameText.text);
        PlayerPrefs.Save();

        NetworkManager networkRunnerHandler = FindFirstObjectByType<NetworkManager>();

        networkRunnerHandler.OnJoinLobby();

        HideAllPanels();

        _sessionBrowserPanel.gameObject.SetActive(true);
        FindAnyObjectByType<UISessionListHandler>(FindObjectsInactive.Include).OnLookingForGameSession();
    }

    public void OnCreateNewGameClicked()
    {
        HideAllPanels();

        _createSessionPanel.gameObject.SetActive(true);
    }

    public void OnStartNewSessionClicked()
    {
        NetworkManager networkRunnerHandler = FindFirstObjectByType<NetworkManager>();

        networkRunnerHandler.CreateGame(_sessionNameInputField.text, "InGame");

        HideAllPanels();
    }

    public void OnJoiningServer()
    {
        HideAllPanels();

    }
}
