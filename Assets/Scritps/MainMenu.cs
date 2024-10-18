using Fusion;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    NetworkRunner _runner;

    public string SessionName;
    public int MaxPlayer;



    private void Awake()
    {
        // 네트워크 러너가 없다면 새로 만들어줍니다.
        Transform runnerTr = transform.Find("NetworkRunner");
        if (runnerTr == null)
        {
            runnerTr = new GameObject("NetworkRunner").transform;
            _runner = runnerTr.gameObject.AddComponent<NetworkRunner>();
        }
        else _runner = runnerTr.GetComponent<NetworkRunner>();

    }
    public void CreateNewSession(StartGameArgs args)
    {
        
        //_runner.JoinSessionLobby(SessionLobby.ClientServer, )
    }

    public async Task StartHost(NetworkRunner runner)
    {
        var customProps = new Dictionary<string, SessionProperty>();

        //customProps["map"] = (int)gameMap;
        //customProps["type"] = (int)gameType;

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionProperties = customProps,
        });

        if (result.Ok)
        {
            // all good
        }
        else
        {
            Debug.LogError($"Failed to Start: {result.ShutdownReason}");
        }
    }

    public async Task StartPlayer(NetworkRunner runner)
    {
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient
        });

        if(result.Ok)
        {

        }
        else
        {
            Debug.Log($"Failed to start : {result.ShutdownReason}");
        }

    }

    public async Task JoinLobby(NetworkRunner runner)
    {
        var result = await runner.JoinSessionLobby(SessionLobby.ClientServer);

        if(result.Ok)
        {

        }
        else
        {
            Debug.LogError($"Failed to Start: {result.ShutdownReason}");
        }
    }

}
