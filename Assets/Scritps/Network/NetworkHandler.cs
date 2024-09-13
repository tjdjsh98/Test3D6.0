using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkObject playerPrefabs;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    UISessionListHandler _uiSessionListHandler;

    UnityEvent<NetworkRunner, PlayerInputData> InputEvent;

    InputManager _inputManager;
    
    void Awake()
    {
        _inputManager = FindAnyObjectByType<InputManager>();
    }
    void Start()
    {
        _uiSessionListHandler = FindAnyObjectByType<UISessionListHandler>(FindObjectsInactive.Include);
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        if (runner.IsServer)
        {
            Debug.Log("OnPlayerJoinedJoined we are server. Spawning player");
            NetworkObject character = runner.Spawn(playerPrefabs, Vector3.up *4, Quaternion.identity, player);

            _spawnedCharacters.Add(player, character);

        }
        else Debug.Log("OnPlayerJoined");

    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkPlayerObject))
        {
            runner.Despawn(networkPlayerObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) 
    {
     
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { Debug.Log("OnShutDown"); }
    public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("OnConnectedToServer"); }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { Debug.Log("OnDisconnectedFromServer"); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { Debug.Log("OnConnectReqest"); }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log("OnConnectFailed"); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (_uiSessionListHandler == null) return;

        if (sessionList.Count == 0)
        {
            _uiSessionListHandler.OnNoSessionFound();
        }
        else
        {
            _uiSessionListHandler.ClearList();
            foreach (SessionInfo sessionInfo in sessionList)
            {
                _uiSessionListHandler.AddToList(sessionInfo);
            }

        }
    }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
