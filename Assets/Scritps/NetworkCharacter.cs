using Fusion;
using UnityEngine;

public class NetworkCharacter : NetworkBehaviour, IDamageable
{
    NetworkCharacterControllerCustom _characterController;
    [Networked][field:SerializeField] public int MaxHp { get; set; }
    [Networked][field: SerializeField] public int Hp { get; set; }
    private void Awake()
    {
        _characterController = gameObject.GetOrAddComponent<NetworkCharacterControllerCustom>();
    }

    public int Damaged(DamageInfo damageInfo)
    {
        return 0;
    }
}
