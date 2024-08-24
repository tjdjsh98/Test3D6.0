using UnityEngine;

public class PreviewCamera : MonoBehaviour
{
    [SerializeField] Vector3 _offset;
    [SerializeField] GameObject _target;
    
    void Update()
    {
        if (_target == null) return;

        transform.position = _target.transform.position + _target.transform.forward * _offset.z + new Vector3(_offset.x,_offset.y);
        transform.LookAt(_target.transform.position);
    }
}
