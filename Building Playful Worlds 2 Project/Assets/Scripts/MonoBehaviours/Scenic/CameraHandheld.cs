using UnityEngine;

public class CameraHandheld : MonoBehaviour {

    public Transform target;

    public float cameraSway = 0.075f;
    public float swayForceStopTimerMultiplier = 0.2f;
    [Range(0.001f, 1f)]
    public float swaySpeed = 1f;

    [SerializeField] private float m_SwayThreshold = 0.05f;
    private float m_SwayTimer;
    private Vector3 m_DesiredLookPosition;
    private Vector3 m_CurrentLookPosition;

    private void Start()
    {
        if (!target)
        {
            Transform newTarget = new GameObject("Camera Target").transform;
            newTarget.SetParent(transform, false);
            newTarget.Translate(Vector3.forward * 5, Space.Self);
            target = newTarget;
        }
        m_SwayTimer = Time.timeSinceLevelLoad + Random.value * swayForceStopTimerMultiplier;
        m_CurrentLookPosition = target.position;
        m_DesiredLookPosition = target.position + new Vector3(Random.value * cameraSway, Random.value * cameraSway, Random.value * cameraSway);
    }

    private void LateUpdate()
    {
        if (Vector3.Distance(m_CurrentLookPosition, m_DesiredLookPosition) > m_SwayThreshold && Time.timeSinceLevelLoad <= m_SwayTimer)
        {
            m_CurrentLookPosition = Vector3.Slerp(m_CurrentLookPosition, m_DesiredLookPosition, Time.deltaTime * swaySpeed);
        }
        else
        {
            m_DesiredLookPosition = target.position + new Vector3(Random.value * cameraSway, Random.value * cameraSway, Random.value * cameraSway);
            m_SwayTimer = Time.timeSinceLevelLoad + Random.value * swayForceStopTimerMultiplier;
        }
        transform.LookAt(m_CurrentLookPosition);
    }
}
