
public interface IDamageable
{
    public int MaxHp { get; set; }
    public int Hp { get; set; }
    // ���������� �޾Ƶ��� ������ ��ȯ
    public int Damaged(DamageInfo damageInfo);
}