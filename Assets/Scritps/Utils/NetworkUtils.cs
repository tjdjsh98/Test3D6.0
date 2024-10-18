using Fusion;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Fusion.NetworkBehaviour;

public static class NetworkUtils
{
    private static FieldInfo _simulationFieldInfo = typeof(NetworkRunner).GetField("_simulation", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void GetInterpolationData(NetworkRunner runner, out int fromTick, out int toTick, out float alpha)
    {
        Simulation simulation = (Simulation)_simulationFieldInfo.GetValue(runner);

        if(runner.IsServer == true)
        {
            fromTick = simulation.TickPrevious;
            toTick = simulation.Tick;
            alpha = simulation.LocalAlpha;
        }
        else
        {
            fromTick = simulation.RemoteTickPrevious;
            toTick = simulation.RemoteTick;
            alpha = simulation.RemoteAlpha;
        }
    }
    
}