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

    private void Start()
    {
        _networkRunnter = Instantiate(NetworkRunnerPrefab);
        _networkRunnter.name = "Netwrokd runner";

        var clientTask = InitializeNetworkRunner(_networkRunnter, GameMode.AutoHostOrClient, NetAddress.Any(), SceneManager.GetActiveScene().buildIndex, null);


        Debug.Log($"Server NetworkRunner started");
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address, int sceneIndex, Action<NetworkRunner> initialized)
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
            SessionName = "TestRoom",
            OnGameStarted = initialized,
            SceneManager = sceneManager
        }) ;

    }
}
