using System;
using System.Collections;
using UnityEngine;

public static class Utils
{
    public static IEnumerator WaitAniationAndPlayCoroutine(Animator animator, string animation, Action action)
    {
        bool isOncePlay = false;

        while (isOncePlay == false || animator.GetCurrentAnimatorStateInfo(0).IsName(animation))
        {
            if(animator.GetCurrentAnimatorStateInfo(0).IsName(animation))
                isOncePlay = true;
            yield return new WaitForSeconds(0.1f);
        }

        action?.Invoke();
    }

    // 0 ~ 360 도 구하기
    public static float CalculateAngle(Vector3 from, Vector3 to)
    {
        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
    }

}
