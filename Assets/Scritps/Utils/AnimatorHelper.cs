using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class AnimatorHelper : MonoBehaviour
{
    public List<UnityEvent> events;

    public Action AnimatorMoved;

    private void OnAnimatorMove()
    {
        AnimatorMoved?.Invoke();
    }
    public void EventInvoke(int index)
    {
        if(index < 0 || index >= events.Count) return;

        events[index]?.Invoke();
    }

    public void ApplyRootMotionToParent()
    {
        //Debug.Log(transform.parent.transform.localPosition + transform.localPosition);
        //var character = GetComponentInParent<NetworkCharacter>();
        //character.SetAnimatorRootmotion(false);
        //character.SetPosition(transform.position);

        //transform.localPosition = Vector3.zero;

        //transform.parent.transform.localRotation = transform.localRotation;
        //transform.localRotation = Quaternion.identity;
    }
}
