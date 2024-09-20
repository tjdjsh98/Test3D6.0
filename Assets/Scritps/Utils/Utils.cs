using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utils
{
    public static IEnumerator WaitAniationAndPlayCoroutine(Animator animator, string stateName, Action action, int layerIndex = 0, float endRatio = 1)
    {
        bool isOncePlay = false;
        float duration = 0;

        while (isOncePlay == false)
        {
            if (animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName))
            {
                duration = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
                isOncePlay = true;
            }  
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(animator.GetAnimatorTransitionInfo(0).duration);

        action?.Invoke();
    }
    // translationTime 까지 기다려준다.
    public static IEnumerator WaitAniationAndPlayCoroutine(Animator animator, string[] stateNames, Action action,int layerIndex = 0, float endRatio = 1)
    {
        bool statePlaying = false;

        float duration = 0;       
        while (!statePlaying)
        {

            if (!statePlaying)
            {
                foreach (var name in stateNames)
                {

                    statePlaying = animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(name);
                    
                    if (statePlaying)
                    {
                        duration = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
                        break;
                    }
                }
            }

            if (duration != 0) break;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(duration);
         yield return new WaitForSeconds(animator.GetAnimatorTransitionInfo(0).duration);

        action?.Invoke();
    }

    // 0 ~ 360 도 구하기
    public static float CalculateAngle(Vector3 from, Vector3 to)
    {
        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
    }

    public static void DrawRange(GameObject gameObjet, Range range, Color color)
    {
        Gizmos.color = color;

        Vector3 center = gameObjet.transform.position + range.center;
        if (gameObjet != null && range.relativeTransform)
        {
            Gizmos.matrix = gameObjet.transform.localToWorldMatrix;
            center = range.center;
        }
        if(range.shape == RangeShape.Ray)
        {
            Gizmos.DrawLine(center, center + range.distance * range.direction.normalized);
        }
        if(range.shape == RangeShape.Box)
        {
            Gizmos.DrawWireCube(center, range.size);
        }
        if(range.shape == RangeShape.Sphere)
        {
            Gizmos.DrawWireSphere(center, range.size.x/2);
        }
        Gizmos.matrix = Matrix4x4.identity;
    }

    public static RaycastHit RangeCast(GameObject gameObject, Range range, int layerMask = int.MaxValue)
    {
        RaycastHit hit = new RaycastHit();

        Matrix4x4 localToWorldMatrix = gameObject.transform.localToWorldMatrix;
        Vector3 center = (range.relativeTransform ? localToWorldMatrix.MultiplyPoint(range.center) : gameObject.transform.position + range.center);
        Vector3 direction = range.relativeTransform ? localToWorldMatrix.MultiplyVector(range.direction).normalized : range.direction.normalized;
        switch (range.shape)
        {
            case RangeShape.Ray:

                if (range.relativeTransform)
                {
                    Ray ray = new Ray(center, direction);
                    Physics.Raycast(ray,out hit,range.distance, layerMask);
                }
                else
                {
                    Ray ray = new Ray(gameObject.transform.position + range.center, range.direction.normalized);
                    Physics.Raycast(ray,out hit, range.distance, layerMask);
                }
                break;
            case RangeShape.Box:
                Physics.BoxCast(center, range.size / 2, direction,out hit, range.relativeTransform ? gameObject.transform.rotation : Quaternion.identity, range.distance, layerMask);
                break;
            case RangeShape.Sphere:
                {
                    Ray ray = new Ray(center, direction);
                    Physics.SphereCast(ray, range.size.x / 2,out hit, range.distance, layerMask);
                }
                break;
            default:
                break;
        }

        return hit;
    }

    public static RaycastHit[] RangeCastAll(GameObject gameObject, Range range, int layerMask = int.MaxValue)
    {
        RaycastHit[] hits = null;

        Matrix4x4 localToWorldMatrix = gameObject.transform.localToWorldMatrix;
        Vector3 center =  (range.relativeTransform? localToWorldMatrix.MultiplyPoint(range.center) : gameObject.transform.position + range.center);
        Vector3 direction = range.relativeTransform? localToWorldMatrix.MultiplyVector(range.direction).normalized : range.direction.normalized;
        switch (range.shape)
        {
            case RangeShape.Ray:

                if (range.relativeTransform)
                {
                    Ray ray = new Ray(center , direction);
                    hits = Physics.RaycastAll(ray, range.distance, layerMask);
                }
                else
                {
                    Ray ray = new Ray(gameObject.transform.position + range.center, range.direction.normalized);
                    hits = Physics.RaycastAll(ray, range.distance, layerMask);
                }
                break;
            case RangeShape.Box:
                hits = Physics.BoxCastAll(center, range.size / 2, direction, range.relativeTransform? gameObject.transform.rotation : Quaternion.identity, range.distance, layerMask);
                break;
            case RangeShape.Sphere:
                {
                    Ray ray = new Ray(center, direction);
                    hits = Physics.SphereCastAll(ray, range.size.x / 2, range.distance, layerMask);
                }
                break;
            default:
                break;
        }
        return hits;
    }

    public static Collider[] RangeOverlapAll(GameObject gameObject, Range range, int layerMask = int.MaxValue)
    {
        Collider[] colliders = null;

        Matrix4x4 localToWorldMatrix = gameObject.transform.localToWorldMatrix;
        Vector3 center = (range.relativeTransform ? localToWorldMatrix.MultiplyPoint(range.center) : gameObject.transform.position + range.center);
        Vector3 direction = range.relativeTransform ? localToWorldMatrix.MultiplyVector(range.direction).normalized : range.direction.normalized;
        switch (range.shape)
        {
            case RangeShape.Box:
                colliders = Physics.OverlapBox(center, range.size / 2, range.relativeTransform? gameObject.transform.rotation: Quaternion.identity, layerMask);
                break;
            case RangeShape.Sphere:
                colliders = Physics.OverlapSphere(center, range.size.x / 2, layerMask);
                break;
            default:    
                break;
        }
        return colliders;
    }
    public static Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(Random.Range(-20, 20), 4, Random.Range(-20, 20));
    }

    public static void SetRenderLayerInChildren(Transform transform, int layerNumber)
    {
        foreach(Transform trans in transform.GetComponentInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }
}
