using Fusion;
using Fusion.Sockets;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner NetworkRunnerPrefab;
    NetworkRunner _networkRunnter;

    [SerializeField] string _lobbyName;

    private void Awake()
    {
        NetworkRunner sceneNetworkRunner = FindFirstObjectByType<NetworkRunner>();

        if(sceneNetworkRunner !=null)
            _networkRunnter = sceneNetworkRunner;
    }

    private void Start()
    {
        _networkRunnter = Instantiate(NetworkRunnerPrefab);
        _networkRunnter.name = "Netwrokd runner";

        if(SceneManager.GetActiveScene().name != "MainMenu")
        {
            var clientTask = InitializeNetworkRunner(_networkRunnter, GameMode.AutoHostOrClient , NetAddress.Any(),"TestRoom", SceneManager.GetActiveScene().buildIndex, null);
        }


        Debug.Log($"Server NetworkRunner started");
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address,string sessionName ,int sceneIndex, Action<NetworkRunner> initialized)
    {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = SceneRef.FromIndex(sceneIndex),
            SessionName = sessionName,
            CustomLobbyName = _lobbyName,
            OnGameStarted = initialized,
            SceneManager = sceneManager
        }) ;
    }

    public void OnJoinLobby()
    {
        var clientTask = JoinLobby();
    }
    private async Task JoinLobby()
    {
        Debug.Log("JoinLobby started");

        var result = await _networkRunnter.JoinSessionLobby(SessionLobby.Custom, _lobbyName);

        if (!result.Ok)
        {
            Debug.LogError($"Unable to join lobby{_lobbyName}");
        }
        else
        {
            Debug.Log("JoinLobby ok");
        }
    }

    public void CreateGame(string sessionName, string sceneName)
    {
        Debug.Log($"Create session {sessionName} scene {sceneName} build Index {SceneUtility.GetBuildIndexByScenePath($"Scenes/{sceneName}")}");

        var clientTask = InitializeNetworkRunner(_networkRunnter, GameMode.Host, NetAddress.Any(), sessionName, SceneUtility.GetBuildIndexByScenePath($"Scenes/{sceneName}"), null);
    }
    public void JoinGame(SessionInfo info)
    {
        Debug.Log($"Create session {info.Name}");

        var clientTask = InitializeNetworkRunner(_networkRunnter, GameMode.Client, NetAddress.Any(), info.Name, SceneManager.GetActiveScene().buildIndex, null);
    }
}
