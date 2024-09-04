using Fusion;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISessionListHandler : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public GameObject sessionItemPrefab;
    public VerticalLayoutGroup verticalLayoutGroup;

    private void Awake()
    {
        ClearList();
    }
    public void ClearList()
    {
        foreach (Transform child in verticalLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        statusText.gameObject.SetActive(false);
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        UISessionItem item = Instantiate(sessionItemPrefab,verticalLayoutGroup.transform).GetComponent<UISessionItem>();

        item.SetInformation(sessionInfo);

        item.OnJoinSession += AddedUISessionItem_OnJoinSession;
    }

    private void AddedUISessionItem_OnJoinSession(SessionInfo sessionInfo)
    {
        NetworkManager networkHandler = FindAnyObjectByType<NetworkManager>();

        networkHandler.JoinGame(sessionInfo);

        UIMainMenuHandler mainMenuUIHandler = FindAnyObjectByType<UIMainMenuHandler>();
        mainMenuUIHandler.OnJoiningServer();
    }


    public void OnNoSessionFound()
    {
        statusText.text = "No game session found";
        statusText.gameObject.SetActive(true);
    }

    public void OnLookingForGameSession()
    {
        statusText.text = "Looking for game sessions";
        statusText.gameObject.SetActive(true);
    }
}
