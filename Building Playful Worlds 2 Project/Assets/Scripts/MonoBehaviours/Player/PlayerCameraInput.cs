using System.Collections;
using UnityEngine;

public class PlayerCameraInput : MonoBehaviour {

    public Transform pivot;
    public Transform target;
    public float clampAngle = 72f;
    public bool inverseVerticalAxis = false;

    private PlayerController m_Controller;
    private Vector3 m_CamForward;
    private Vector3 m_Move;
    private Vector3 m_Offset;
    private float m_ZoomVelocity;
    private float m_XRotation;
    private float m_RecoilBuffer;
    private float m_OriginalFOV;
    private Camera m_Cam;
    private bool m_ZoomedIn = false;
    private bool m_Hold = false;

    private void Start()
    {
        m_Controller = target.GetComponent<PlayerController>();
        m_Offset = pivot.position - target.position;
        m_XRotation = transform.localEulerAngles.x;
        m_Cam = GetComponent<Camera>();
        m_OriginalFOV = m_Cam.fieldOfView;
    }

    public void Hold(float time)
    {
        StartCoroutine(HoldMovement(time));
    }

    private void Update()
    {
        if (m_Hold)
        {
            return;
        }
        pivot.position = target.position + m_Offset;
        if (m_Controller.dead)
        {
            m_Cam.fieldOfView = Mathf.SmoothDamp(m_Cam.fieldOfView, m_OriginalFOV, ref m_ZoomVelocity, .2f);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Controller.Jump();
        }
        m_Controller.WeaponUp();
        if (Input.GetMouseButton(1))
        {
            m_Cam.fieldOfView = Mathf.SmoothDamp(m_Cam.fieldOfView, m_OriginalFOV - 35, ref m_ZoomVelocity, .3f);
        }
        else
        {
            m_Cam.fieldOfView = Mathf.SmoothDamp(m_Cam.fieldOfView, m_OriginalFOV, ref m_ZoomVelocity, .2f);
        }
        m_ZoomedIn = Input.GetMouseButton(1);
        if ((m_Controller.equippedWeapon.semiAuto ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0)) && !m_Controller.noWeapon)
        {
            //Fire weapon and add recoil.
            if (m_Controller.WeaponFireHandling())
            {
                float recoil = m_Controller.equippedWeapon.recoil;
                m_XRotation -= recoil;
                m_RecoilBuffer = recoil / 8;
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            m_Controller.Reload();
        }
    }

    private void FixedUpdate()
    {
        if (m_Hold)
        {
            Input.ResetInputAxes();
            m_Controller.Move(Vector3.zero, false, false);
            m_Controller.GetComponent<Animator>().SetFloat("Speed", 0);
            return;
        }
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        float r = Input.GetAxis("Mouse X");

        pivot.Rotate(0, r * m_Controller.sensitivity * (m_ZoomedIn? 0.5f : 1), 0);
        m_CamForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
        m_Move = v * m_CamForward + h * transform.right;

        KeyCode walking = KeyCode.LeftAlt;
#if UNITY_EDITOR
        walking = KeyCode.X;
#endif
        m_Controller.Move(m_Move, Input.GetKey(KeyCode.LeftShift), Input.GetKey(walking));

    }

    private void LateUpdate()
    {
        if (m_Hold)
        {
            return;
        }
        //Vertical camera movement
        float mouseX = (inverseVerticalAxis ? Input.GetAxis("Mouse Y") : -Input.GetAxis("Mouse Y")) * m_Controller.sensitivity * (m_ZoomedIn ? 0.5f : 1);
        m_XRotation += mouseX;
        m_XRotation += m_RecoilBuffer;
        m_RecoilBuffer = Mathf.MoveTowards(m_RecoilBuffer, 0, Time.deltaTime * 2);
        m_XRotation = Mathf.Clamp(m_XRotation, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(m_XRotation, pivot.localEulerAngles.y, 0);
        pivot.localRotation = localRotation;
    }

    private IEnumerator HoldMovement(float time)
    {
        m_Hold = true;
        yield return new WaitForSeconds(time);
        m_Hold = false;
    }

}