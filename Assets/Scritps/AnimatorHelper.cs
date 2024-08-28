using System;
using UnityEngine;

public class AnimatorHelper : MonoBehaviour
{
    public Action Attacked { get; set; }

    public void Attack()
    {
        Attacked?.Invoke();
    }
}
