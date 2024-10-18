using Fusion;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISessionItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _sessionNameText;
    [SerializeField] TextMeshProUGUI _playerCountText;
    [SerializeField] Button _joinButton;

    private SessionInfo _sessionInfo;


    // Events
    public event Action<SessionInfo> OnJoinSession;

    public void SetInformation(SessionInfo sessionInfo)
    {
        _sessionInfo = sessionInfo;

        _sessionNameText.text = _sessionInfo.Name;
        _playerCountText.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";

        bool isJoinButtonActive = true;

        if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
            isJoinButtonActive = false;

        _joinButton.gameObject.SetActive(isJoinButtonActive);
    }

    
    public void OnClick()
    {
        OnJoinSession?.Invoke(_sessionInfo);
    }

}
