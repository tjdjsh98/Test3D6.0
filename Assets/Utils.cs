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
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(animation))
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

    public static RaycastHit[] RangeCastAll(GameObject gameObject, Range range, int layerMask = int.MaxValue)
    {
        RaycastHit[] hits = null;
        switch (range.shape)
        {
            case RangeShape.Ray:
                Ray ray = new Ray(gameObject.transform.position + range.center, range.direction.normalized);
                hits = Physics.RaycastAll(ray, range.distance, layerMask);
                break;
            case RangeShape.Box:
                hits = Physics.BoxCastAll(gameObject.transform.position + range.center, range.size / 2, range.direction.normalized, Quaternion.identity, range.distance, layerMask);
                break;
            case RangeShape.Sphere:
                ray = new Ray(gameObject.transform.position + range.center, range.direction.normalized);
                hits = Physics.SphereCastAll(ray, range.size.x / 2, range.distance, layerMask);
                break;
            default:
                break;
        }
        return hits;
    }

    public static Collider[] RangeOverlapAll(GameObject gameObject, Range range, int layerMask = int.MaxValue)
    {
        Collider[] colliders = null;
        switch (range.shape)
        {
            case RangeShape.Box:
                colliders = Physics.OverlapBox(gameObject.transform.position + range.center, range.size / 2, Quaternion.identity, layerMask);
                break;
            case RangeShape.Sphere:
                colliders = Physics.OverlapSphere(gameObject.transform.position + range.center, range.size.x / 2, layerMask);
                break;
            default:
                break;
        }
        return colliders;
    }
}
