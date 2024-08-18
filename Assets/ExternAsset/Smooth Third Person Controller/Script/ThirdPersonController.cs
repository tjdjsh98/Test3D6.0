using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [SerializeField] Transform camera;
    [SerializeField] bool stabilizeFeet;
    private Animator m_Animator;
    private float startRotation;
    private float lastInputAngle;
    private bool lastRotation;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
    }
    private void Update()
    {
        var moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        var moveInputAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + camera.eulerAngles.y;

        var moveRotation = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, moveInputAngle);

        if(moveInputAngle != lastInputAngle && moveInput.magnitude > 0.1f)
        {
            startRotation = moveRotation;
            lastInputAngle = moveInputAngle;
        }
        if(moveInput.magnitude > 0.1f)
        {
            if (lastRotation == true)
                startRotation = moveRotation;
            lastRotation = false;

            m_Animator.SetFloat("Speed", moveInput.magnitude, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Rotation", moveRotation, 0.0f, Time.deltaTime);
            m_Animator.SetFloat("Smooth Rotation", moveRotation, 0.15f, Time.deltaTime);
            m_Animator.SetFloat("Start Rotation", startRotation, 0.1f, Time.deltaTime);

        }
        else
        {
            lastRotation = true;
            m_Animator.SetFloat("Smooth Rotation", 0, 0.15f, Time.deltaTime);
            m_Animator.SetFloat("Speed", moveInput.magnitude);
            m_Animator.SetFloat("Rotation", moveRotation * moveInput.magnitude);
            m_Animator.SetFloat("Start Rotation", startRotation);
        }
        if(Input.GetKeyDown(KeyCode.Space))
        m_Animator.SetTrigger("Jump");
        m_Animator.stabilizeFeet = stabilizeFeet;
    }
}
