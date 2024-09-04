using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorHelper : MonoBehaviour
{
    public List<UnityEvent> events;

    public void EventInvoke(int index)
    {
        if(index < 0 || index >= events.Count) return;

        events[index]?.Invoke();
    }

}
