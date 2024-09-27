using TMPro;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Camera _camera;

    [SerializeField]SpriteRenderer _background;
    [SerializeField] TextMeshPro _text;

    void Awake()
    {
        _camera = Camera.main;
        if (_background)
        {
            _background.material.renderQueue = 4000;
        }
        if (_text)
        {
            _text.material.renderQueue = 4001;

        }
    }

    void Update()
    {
        transform.LookAt( _camera.transform.position,Vector3.up);
    }
}
