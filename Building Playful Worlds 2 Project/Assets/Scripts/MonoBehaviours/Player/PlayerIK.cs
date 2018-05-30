using UnityEngine;

public class PlayerIK : MonoBehaviour {

    public Transform IKTarget;

    private Animator m_Animator;
    private float overrideWeightFactor = 1;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (m_Animator && m_Animator.GetCurrentAnimatorStateInfo(1).IsTag("Reloading"))
        {
            overrideWeightFactor = Mathf.Lerp(overrideWeightFactor, 0, Time.deltaTime * 6);
        }
        else
        {
            overrideWeightFactor = Mathf.Lerp(overrideWeightFactor, 1, Time.deltaTime * 2);
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!m_Animator || !IKTarget)
        {
            return;
        }

        m_Animator.SetLookAtPosition(IKTarget.position);
        m_Animator.SetLookAtWeight(m_Animator.GetFloat("WeaponUp") * overrideWeightFactor, 0.4f * overrideWeightFactor);
    }
}
