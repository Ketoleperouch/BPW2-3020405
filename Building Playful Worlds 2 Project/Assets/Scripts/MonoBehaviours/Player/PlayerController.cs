//Copyright:
//Mythic Act 2018 Niels Weber, added optimizations and tweaks for BPW2 (this game)

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerController : MonoBehaviour {

    public Weapon[] weapons;
    public Weapon equippedWeapon;

    public float sensitivity = 5f;
    public float speed = 4f;
    public float turnBoost = 8f;
    public float sprintBoost = 0.5f;
    public float jumpForce = 3f;

    public Transform barrel;
    public Transform targetable;
    public LayerMask hitLayers;
    public ParticleSystem shotMuzzles;
    public Image crosshair;

    public bool noWeapon = true;

    [SerializeField] private float m_GroundCheckDistance = 0.1f;

    private Animator m_Animator;
    private float m_Forward;
    private float m_Turn;
    private bool m_Grounded = true;
    private Vector3 m_GroundNormal;
    private Rigidbody m_Rigidbody;
    private List<float> fireDelays = new List<float>();

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        InitializeWeapons();
        for (int i = 0; i < weapons.Length; i++)
        {
            fireDelays.Add(weapons[i].fireRate);
        }
        StartCoroutine(StartText());
    }

    private void InitializeWeapons()
    {
        equippedWeapon = weapons[0];
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].totalAmmo = weapons[i].clipSize * 3;
            weapons[i].currentAmmoInClip = weapons[i].clipSize;
        }
        crosshair.sprite = equippedWeapon.crosshair;
        if (noWeapon)
        {
            equippedWeapon = null;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Move(Vector3 move, bool sprinting, bool walking)
    {
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);

        CheckGroundStatus();

        move = Vector3.ProjectOnPlane(move, m_GroundNormal);

        m_Turn = Mathf.Atan2(move.x, move.z);
        m_Forward = move.z;

        if (walking) m_Forward /= 3;
        if (sprinting && m_Forward > 0.6f)
        {
            m_Forward += sprintBoost;
        }

        //Finalize move: set rotation and animator values

        m_Animator.applyRootMotion = m_Grounded;
        m_Animator.SetFloat("VerticalSpeed", m_Rigidbody.velocity.y);
        m_Animator.SetFloat("Speed", m_Forward, 0.1f, Time.deltaTime);
        m_Animator.SetFloat("Turn", m_Turn, 0.1f, Time.deltaTime);

        transform.Rotate(0, m_Turn * (m_Grounded ? turnBoost : 1), 0);
    }

    public void Jump()
    {
        if (m_Grounded)
        {
            m_Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void WeaponUp()
    {
        bool weaponUp = (Input.GetMouseButton(1) || Input.GetMouseButton(0)) && !noWeapon;
        m_Animator.SetFloat("WeaponUp", weaponUp ? 1 : 0, 0.1f, Time.deltaTime);
    }

    public bool WeaponFireHandling()
    {
        //Check if shot is possible at all. Bool value determines whether the player has shot or not.
        if (Time.time > fireDelays[equippedWeapon.ID])
        {
            //Check if there is enough ammo left
            if (equippedWeapon.currentAmmoInClip > 0)
            {
                equippedWeapon.outOfAmmo = false;
                Shoot();
                return true;
            }
            else
            {
                //Check if reload is possible
                if (equippedWeapon.totalAmmo > 0)
                {
                    equippedWeapon.outOfAmmo = false;
                    Reload();
                }
                //If not possible, the weapon is out of ammo.
                else
                {
                    equippedWeapon.outOfAmmo = true;
                }
            }
        }
        return false;
    }

    private void Shoot()
    {
        if (!equippedWeapon)
        {
            return;
        }
        fireDelays[equippedWeapon.ID] = Time.time + equippedWeapon.fireRate;
        m_Animator.SetTrigger("Fire");
        AudioSource.PlayClipAtPoint(equippedWeapon.shotSounds[Random.Range(0, equippedWeapon.shotSounds.Length)], barrel.position);
        shotMuzzles.Stop(true);
        shotMuzzles.Play(true);
        //Get hit point and stuff

        RaycastHit hit;
        Vector3 shotOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));

        float acc = 1 - equippedWeapon.accuracy;

        Vector3 shotInaccuracy = new Vector3(Random.Range(-acc, acc), Random.Range(-acc, acc), Random.Range(-acc, acc));

        if (!m_Grounded)
        {
            shotInaccuracy *= 1.5f;
        }

        Debug.DrawRay(barrel.position, Camera.main.transform.forward * equippedWeapon.range + shotInaccuracy, Color.yellow);
        if (Physics.Raycast(shotOrigin, Camera.main.transform.forward + shotInaccuracy, out hit, equippedWeapon.range, hitLayers, QueryTriggerInteraction.Ignore))
        {
            Instantiate(equippedWeapon.impactParticles, hit.point, Quaternion.identity);
            Debug.Log(hit.collider.tag);
            if (hit.collider.CompareTag("Enemy"))
            {
                
                EnemyStats enemy = hit.collider.GetComponent<EnemyStats>();
                enemy.TakeDamage(equippedWeapon.damage, hit.point);
            }
        }

    }

    public void Reload()
    {
        m_Animator.SetTrigger("Reload");
    }

    private void CheckGroundStatus()
    {
        RaycastHit groundHit;
#if UNITY_EDITOR
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
        if (Physics.SphereCast(transform.position + (Vector3.up * 0.3f), 0.2f, Vector3.down, out groundHit, m_GroundCheckDistance))
        {
            m_GroundNormal = groundHit.normal;
            m_Grounded = true;
        }
        else
        {
            m_GroundNormal = Vector3.up;
            m_Grounded = false;
        }
        m_Animator.SetBool("Grounded", m_Grounded);
    }

    private IEnumerator StartText()
    {
        yield return new WaitForSeconds(1);
        LogTextDirect.logText.LogText("Are you alright? That crash was rather rough.");
        yield return new WaitForSeconds(3);
        LogTextDirect.logText.LogText("Can you walk? Try moving to that spot over there.");
        WaypointSystem.system.SetWaypoint(new Vector3(15, 6.5f, 54.4f));
        yield return new WaitForSeconds(4);
        LogTextDirect.logText.LogText("Use the WASD keys to move.                   ");
    }
}