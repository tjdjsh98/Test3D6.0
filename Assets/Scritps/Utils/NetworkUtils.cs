using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Fusion.NetworkBehaviour;

public static class NetworkUtils
{
    public static void DetectChanges<T>(NetworkBehaviour networkBehaviour,ChangeDetector changeDetector, Dictionary<string, Action<T,T>> dic) where T : unmanaged
    {
        foreach (var change in changeDetector.DetectChanges(networkBehaviour, out var previous, out var current))
        {
            Debug.Log($"{change} {dic.ContainsKey(change)} {dic.Keys.Count}");
            if(dic.ContainsKey(change))
            {
                string name = change;
                Debug.Log(name);
                PropertyReader<T> reader = GetPropertyReader<T>(typeof(NetworkBehaviour), name);
                var (p, c) = reader.Read(previous, current);
                Action<T, T> action = dic[change];
                action?.Invoke(p, c);
            }      
        }
    }
}