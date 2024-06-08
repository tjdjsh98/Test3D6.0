using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour
{
    [SerializeField] NetworkRunner _networkRunnerPrefab;
    NetworkRunner _networkRunner;

    float _buttonWidth = 300;
    float _buttonHeight = 100;
    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - _buttonWidth / 2, Screen.height / 2 - _buttonHeight / 2, _buttonWidth, _buttonHeight),"Host Game"))
        {
            StartGame(GameMode.AutoHostOrClient);
        }

        if (GUI.Button(new Rect(Screen.width - _buttonWidth / 2, Screen.height / 2 - _buttonHeight / 2 + _buttonHeight + 50, _buttonWidth, _buttonHeight), "Joint Game"))
        {
            StartGame(GameMode.Client);
        }
    }

    async void StartGame(GameMode gameMode)
    {
        _networkRunner = Instantiate(_networkRunnerPrefab);
        _networkRunner.ProvideInput = true;

        SceneRef scene = SceneRef.FromIndex(1);

        NetworkSceneInfo networkSceneInfo = new NetworkSceneInfo();

        if(scene.IsValid)
            networkSceneInfo.AddSceneRef(scene,LoadSceneMode.Additive);

        StartGameArgs startGameArgs = new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = "Default",
            Scene = scene,
            SceneManager = _networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        };
        await _networkRunner.StartGame(startGameArgs);
    }
}
