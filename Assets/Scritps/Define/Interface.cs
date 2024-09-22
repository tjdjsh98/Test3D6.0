using System;
using UnityEngine;

public interface IDamageable
{
    public GameObject GameObject { get; }  
    public int MaxHp { get; set; }
    public int Hp { get; set; }

    public Action<DamageInfo> Damaged { get; set; }
    public Action<DamageInfo> Died { get; set; }

    // 최종적으로 받아들인 데미지 반환
    public int Damage(DamageInfo damageInfo);
}

public interface IRigidbody
{
    public void HandleVelocity();
    public void Jump(float power);
    public void Move(Vector3 diection, float ratio = 1);
    public void AddForce(Vector3 power, ForceMode forceMode = ForceMode.Impulse);
}

public interface IData
{
    public string DataName { get; set; }
}