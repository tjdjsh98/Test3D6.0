using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Fusion.NetworkBehaviour;


public class NetworkManager : NetworkBehaviour
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

    //public override void Render()
    //{
    //    foreach(var key in _valueChangedDic.Keys)
    //    {
    //        foreach (var change in _valueChangedDic[key].changeDetector.DetectChanges(key, out var previous, out var current))
    //        {
    //            OnChangedValue(change,key,previous,current, _valueChangedDic[key].intChangeDictionary);
    //            OnChangedValue(change,key, previous, current, _valueChangedDic[key].floatChangeDictionary);
    //            OnChangedValue(change,key, previous, current, _valueChangedDic[key].booleanChangeDictionary);
    //            //OnChangedValue(change,key, previous, current, _valueChangedDic[key].itemSlotArrayChangeDictionary);
    //        }
    //    }
    //}

    //void OnChangedValue<T>(string name,NetworkBehaviour networkBehaviour, NetworkBehaviourBuffer previous, NetworkBehaviourBuffer current, Dictionary<string, Action<T,T> > actionDic) where T : unmanaged
    //{
    //    if (!actionDic.ContainsKey(name)) return;

    //    PropertyReader<T> reader = GetPropertyReader<T>(networkBehaviour.GetType(), name);
    //    var (p,c) = reader.Read(previous, current);
    //    Action<T, T> action = actionDic[name];
    //    action?.Invoke(p , c);
    //}
    //// Awake나 Start에는 사용하면 안됨.
    //// Spawn될 때 선언
    //public void AddValueChanged<T>(NetworkBehaviour networkBehaviour, string name , Action<T,T> action) where T : struct
    //{
    //    if(!_valueChangedDic.ContainsKey(networkBehaviour))
    //    {
    //        _valueChangedDic.Add(networkBehaviour ,new ValueChangeDectector(networkBehaviour));
    //    }

    //    ValueChangeDectector valueChangeDectector = _valueChangedDic[networkBehaviour];

    //    Type type = typeof(T);
    //    if (type == typeof(NetworkBool))
    //    {
    //        Action<NetworkBool, NetworkBool> a = action as Action<NetworkBool, NetworkBool>;
    //        valueChangeDectector.booleanChangeDictionary.Add(name, a);
    //    }
    //    else if (type == typeof(int))
    //    {
    //        Action<int, int> a = action as Action<int, int>;
    //        valueChangeDectector.intChangeDictionary.Add(name, a);
    //    }
    //    else if (type == typeof(float))
    //    {
    //        Action<float, float> a = action as Action<float, float>;
    //        valueChangeDectector.floatChangeDictionary.Add(name, a);
    //    }

    //}

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
