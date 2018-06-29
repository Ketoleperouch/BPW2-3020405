using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class MissionController : MonoBehaviour {

    public static MissionController mission;

    [System.Serializable]
    public class Mission
    {
        [System.Serializable]
        public struct DelayedUnityEvent
        {
            public UnityEvent onActivateDelayed;
            public float delay;
        }

        public string missionName = "New Mission";
        public enum MissionType { ReachLocation, InteractWithObject};
        public MissionType missionType = MissionType.ReachLocation;

        public Transform target;
        public float targetRange;

        public UnityEvent[] onStart;
        public DelayedUnityEvent[] delayedStartEvents;
        public UnityEvent[] onCompletion;

        public string name { get { return missionName; } set { missionName = value; } }
        public bool isDone { get; set; }
    }

    public Text missionText;
    public Mission[] missions;
    public Mission activeMission;

    [SerializeField] private float m_UpdateTime = 0.5f;

    private Transform m_Player;
    private float m_Timer;
    private int m_CurrentMission = 0;

    private void Start()
    {
        if (!mission)
        {
            mission = this;
        }
        m_Player = FindObjectOfType<PlayerController>().transform;
        activeMission = missions[0];
        //Activate OnStart events
        for (int i = 0; i < activeMission.onStart.Length; i++)
        {
            activeMission.onStart[i].Invoke();
        }
        //Activate Delayed Start events
        for (int i = 0; i < activeMission.delayedStartEvents.Length; i++)
        {
            StartCoroutine(InvokeDelayed(i));
        }
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
        switch (activeMission.missionType)
        {
            case Mission.MissionType.ReachLocation:
                if (activeMission.target && Vector3.Distance(m_Player.position, activeMission.target.position) <= activeMission.targetRange)
                {
                    NextMission();
                }
                break;
            case Mission.MissionType.InteractWithObject:
                //Activated externally, wait for completion call.
                break;
        }
    }

    public void NextMission()
    {
        Debug.Log(activeMission.missionName + " complete.");

        //Activate OnCompletion events
        for (int i = 0; i < activeMission.onCompletion.Length; i++)
        {
            activeMission.onCompletion[i].Invoke();
        }
        //Switch to next mission
        activeMission.isDone = true;
        if (m_CurrentMission + 1 < missions.Length)
            m_CurrentMission++;
        else
            return;
        activeMission = missions[m_CurrentMission];
        //Activate OnStart events
        for (int i = 0; i < activeMission.onStart.Length; i++)
        {
            activeMission.onStart[i].Invoke();
        }
        //Activate Delayed Start events
        for (int i = 0; i < activeMission.delayedStartEvents.Length; i++)
        {
            StartCoroutine(InvokeDelayed(i));
        }
    }

    public void SetActiveMissionTarget(Transform target)
    {
        activeMission.target = target;
    }

    private IEnumerator InvokeDelayed(int index)
    {
        yield return new WaitForSeconds(activeMission.delayedStartEvents[index].delay);
        activeMission.delayedStartEvents[index].onActivateDelayed.Invoke();
    }

    public void LoadScene(int buildIndex)
    {
        StartCoroutine(LoadFading(buildIndex));
    }

    private IEnumerator LoadFading(int buildIndex)
    {
        CanvasGroup fader = m_Player.GetComponent<PlayerHealth>().fader;
        m_Player.GetComponent<PlayerController>().enabled = false;
        while (!Mathf.Approximately(fader.alpha, 1))
        {
            fader.alpha = Mathf.MoveTowards(fader.alpha, 1, Time.deltaTime);
            yield return null;
        }
        //Save health data to Checkpoint Data
        SceneLoader sL = FindObjectOfType<SceneLoader>();
        sL.data.playerHealth = FindObjectOfType<PlayerHealth>().health;
        sL.LoadScene(buildIndex);
    }
    //All unused code for checkpoints and so
        /*private IEnumerator SetCheckpoint()
        {
            SceneLoader sL = FindObjectOfType<SceneLoader>();
            sL.loader.SetActive(true);
            yield return new WaitForSeconds(1 + Random.value);
            sL.data.LoadToData(m_Player.GetComponent<PlayerHealth>().health, m_Player.transform.position, m_Player.transform.rotation, activeMission, GetEnemies());
            sL.loader.SetActive(false);
        }

        private CheckpointData.EnemyData[] GetEnemies()
        {
            EnemyController[] enemies = FindObjectsOfType<EnemyController>();
            List<CheckpointData.EnemyData> enemyData = new List<CheckpointData.EnemyData>();
            for (int i = 0; i < enemies.Length; i++)
            {
                enemyData.Add(new CheckpointData.EnemyData(enemies[i].stats.health, enemies[i].transform.position, enemies[i].transform.rotation, enemies[i]));
            }
            return enemyData.ToArray();
        }*/
    }