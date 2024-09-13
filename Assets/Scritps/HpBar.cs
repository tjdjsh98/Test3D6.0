using Fusion;
using UnityEngine;

public class HpBar : NetworkBehaviour
{
    IDamageable _damageable;
    SpriteRenderer _front;
    SpriteRenderer _back;

    private void Awake()
    {
        _damageable= GetComponentInParent<IDamageable>();
        _front = transform.Find("Front").GetComponent<SpriteRenderer>();
        _back = transform.Find("Back").GetComponent<SpriteRenderer>();
        _damageable.Damaged += OnDamaged;
    }

    public override void Spawned()
    {
        OnDamaged(new DamageInfo());
    }

    void OnDamaged(DamageInfo damageInfo)
    {
        float ratio = (float)_damageable.Hp / (_damageable.MaxHp == 0? 1: _damageable.MaxHp);

        if (ratio < 0) ratio = 0;
        float sizeX = _back.size.x * ratio;
        _front.size = new Vector2(sizeX, _back.size.y);
        _front.transform.localPosition = new Vector3( (_back.size.x - sizeX)/2, 0, 0);

    }
}
