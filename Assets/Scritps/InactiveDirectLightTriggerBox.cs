using UnityEngine;

public class InactiveDirectLightTriggerBox : MonoBehaviour
{
    [SerializeField]Light _directLight;

    [SerializeField]bool _isOnToOff;
    bool _isStartLerp;

    float _lerpValue;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            _isStartLerp = true;
        }
    }

    private void Update()
    {
        if (_directLight == null) return;

        if (_isStartLerp)
        {
            if (_isOnToOff)
            {
                _lerpValue += Time.deltaTime;
                _isStartLerp = _lerpValue >= 1 ? false : true;
            }
            else
            {
                _lerpValue -= Time.deltaTime;
                _isStartLerp = _lerpValue <= 0 ? false : true;  
            }
            _directLight.intensity = Mathf.Lerp(1, 0, _lerpValue);
        }
    }
}
