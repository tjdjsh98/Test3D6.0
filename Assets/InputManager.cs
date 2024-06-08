using Fusion;
using Fusion.Sockets;
using NUnit.Framework.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    NetworkInputData _accumulateInputData;
    bool _resetInput;

    public void BeforeUpdate()
    {
        if(_resetInput)
        {
            _accumulateInputData = default;
            _resetInput = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        NetworkButtons networkButtons = default;

        if (Input.GetKey(KeyCode.W))
            _accumulateInputData.Direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            _accumulateInputData.Direction += Vector3.back;
        if (Input.GetKey(KeyCode.A))
            _accumulateInputData.Direction += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            _accumulateInputData.Direction += Vector3.right;

        networkButtons.Set(InputButton.Jump, Input.GetKeyDown(KeyCode.Space));

        Vector3 mouseDelta = Input.mousePositionDelta;
        mouseDelta = new Vector3(-mouseDelta.y, mouseDelta.x);

        _accumulateInputData.MouseDelta += mouseDelta;

        _accumulateInputData.Buttons =  new NetworkButtons(_accumulateInputData.Buttons.Bits | networkButtons.Bits);
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        _accumulateInputData.Direction.Normalize();
        input.Set(_accumulateInputData);
        _resetInput = true;

        _accumulateInputData.MouseDelta = default;
    }


    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if(player == runner.LocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
