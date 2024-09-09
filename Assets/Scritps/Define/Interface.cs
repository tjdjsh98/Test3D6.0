using System;
using UnityEngine;

public interface IDamageable
{
    public int MaxHp { get; set; }
    public int Hp { get; set; }

    public Action<DamageInfo> Died { get; set; }

    // ���������� �޾Ƶ��� ������ ��ȯ
    public int Damaged(DamageInfo damageInfo);
}

public interface IRigidbody
{
    public void HandleVelocity();
    public void Jump(float power);
    public void Move(Vector3 diection, float ratio = 1);
    public void AddForce(Vector3 power, ForceMode forceMode = ForceMode.Impulse);
}