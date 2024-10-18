using ExitGames.Client.Photon;
using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    Rigidbody _rigidbody;
    DamageInfo _damageInfo;
    GameObject _trail;

    bool _onceAttack = false;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _trail = transform.Find("Trail").gameObject;
        _trail.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (_onceAttack) return;
        if(collision.gameObject.layer == Define.CHARACTER_LAYER)
        {
            NetworkCharacter character = collision.gameObject.GetComponentInParent<NetworkCharacter>();
            if (character.gameObject == _damageInfo.attacker.GameObject) return;

            if(character != null)
            {
                character.Damage(_damageInfo);
                Runner.Despawn(Object);

                _onceAttack = true;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        transform.LookAt(transform.position + _rigidbody.linearVelocity);
    }

    public void Shot(DamageInfo info, Vector3 direction, float power)
    {
        Debug.Log("Shot");
        _damageInfo = info;
        _onceAttack = false;
        _rigidbody.isKinematic = false;
        _trail.gameObject.SetActive(true);
        transform.SetParent(null);
        _rigidbody.AddForce(direction.normalized * power, ForceMode.Impulse);
    }
}

