using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class AnimatorHelper : MonoBehaviour
{
    Animator _animator;
    public List<UnityEvent> events;
    public Action AnimatorMoved;

    [field: SerializeField][field:Range(0,1)] public float UseLeftHandRatio {get;set;}
    [field: SerializeField][field:Range(0, 1)] public float UseRightHandRatio {get;set;}

    [SerializeField] float _rot;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        _rot += _animator.deltaRotation.eulerAngles.y;
        AnimatorMoved?.Invoke();
    }
    public void EventInvoke(int index)
    {
        if(index < 0 || index >= events.Count) return;

        events[index]?.Invoke();
    }
}
