using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody _rigidbody;
    DamageInfo _info;
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
        if(collision.gameObject.tag == "Player")
        {
            Character character = collision.gameObject.GetComponent<Character>();
            if(character != null)
            {
                character.Damaged(_info);
                Destroy(gameObject);

                //transform.SetParent(character.GetModel().transform);
                //transform.position = collision.ClosestPoint(transform.position - _rigidbody.linearVelocity* Time.fixedDeltaTime);
                //_rigidbody.isKinematic = true;
                //_trail.gameObject.SetActive(false);
                _onceAttack = true;
            }
        }
    }

    private void Update()
    {
        transform.LookAt(transform.position + _rigidbody.linearVelocity);
    }

    public void Shot(DamageInfo info, Vector3 direction, float power)
    {
        _info = info;
        _onceAttack = false;
        _rigidbody.isKinematic = false;
        _trail.gameObject.SetActive(true);
        transform.SetParent(null);
        _rigidbody.AddForce(direction.normalized * power, ForceMode.Impulse);
    }
}

