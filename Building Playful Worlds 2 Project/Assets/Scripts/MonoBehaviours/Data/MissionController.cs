using UnityEngine;

public sealed class MissionController : MonoBehaviour {

    [System.Serializable]
    public class Mission
    {
        public string missionName = "New Mission";
        public enum MissionType { ReachLocation, };
        public MissionType missionType = MissionType.ReachLocation;

        public Transform target;

        public string name { get { return missionName; } set { missionName = value; } }
        public bool isDone { get; set; }
    }

    public Mission[] missions;

    [SerializeField] private Mission m_ActiveMission;
    [SerializeField] private float m_UpdateTime = 0.5f;

    private float m_Timer;

    private void Start()
    {
        m_ActiveMission = missions[0];
    }

    private void Update()
    {
        if (Time.time > m_Timer)
        {
            m_Timer += m_UpdateTime;
            MissionUpdate();
        }
    }

    private void MissionUpdate()
    {

    }
}
