
public interface IDamageable
{
    public int MaxHp { get; set; }
    public int Hp { get; set; }
    // 최종적으로 받아들인 데미지 반환
    public int Damaged(DamageInfo damageInfo);
}