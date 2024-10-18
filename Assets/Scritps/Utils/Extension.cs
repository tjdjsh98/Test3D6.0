using UnityEngine;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();

        if(comp == null)
            comp = go.AddComponent<T>();

        return comp;
    }
}
