using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStats))]
public sealed class EnemyController : MonoBehaviour {

	public enum State { Idle, Detect, Chase, Attack, LostTarget, Alarmed, Yielding};
    public State state = State.Idle;            //The current AI state

    public Transform eyes;                      //Reference to the enemy eyes
    public Transform barrel;                    //Reference to the enemy barrel
    public string targetTag = "Player";         //The tag that the enemy can detect
    public float validationTime = 4;            //The time that the enemy needs to validate a target once detected
    public LayerMask detectLayers;              //Layers that are used in detection
    [Range(0, 1)]
    public float accuracy = 1f;                 //The accuracy of the enemy

    [Header("Patrol")]
    public List<Transform> patrolPoints;            //Waypoints through which the enemy will patrol

    public bool inTransition { get; private set; }
    public bool isHeadsup { get; set; }
    /// <summary>
    /// Returns true if the enemy is in the Attack, Lost Target, or Chase state.
    /// </summary>
    public bool isDangerous { get { return state == State.Attack || state == State.LostTarget || state == State.Chase && stats.health > 0; } }
    /// <summary>
    /// Returns true if the enemy is in the Alarmed, Lost Target, or Detect state.
    /// </summary>
    public bool suspicious { get { return state == State.Alarmed || state == State.LostTarget || state == State.Detect; } }
    public EnemyStats stats { get { return GetComponent<EnemyStats>(); } }
    public NavMeshAgent agent { get { return GetComponent<NavMeshAgent>(); } }
    public Color stateColor { get { return m_StateColor; } private set { m_StateColor = value; } }
    public Animator animator { get { return GetComponent<Animator>(); } }
    public Transform target { get { return m_Target; } set { m_Target = value; } }

    private Color m_StateColor = Color.gray;    //The state color to define a certain state. For debugging purposes
    private float m_StateTimer;                 //Tracker for any required state timer
    private float m_IKBodyWeight;               //The Inverse Kinematics weight
    private Transform m_Target;                 //The AI target
    private Vector3 m_TargetLastSeenAt;         //The last position the enemy has seen its target before it disappeared out of sight
    private float m_AttackTimer;                //The timer that is used to delay attacks
    private int m_NextWaypoint;                 //The next patrol waypoint during patrol
    private PlayerController player;            //Ref to player

    private void Start()
    {
        if (!barrel)
        {
            barrel = transform;
        }
        player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (!inTransition)
        {
            switch (state)
            {
                case State.Idle:
                    Idle();
                    break;
                case State.Detect:
                    Detect();
                    break;
                case State.Chase:
                    Chase();
                    break;
                case State.Attack:
                    Attack();
                    break;
                case State.LostTarget:
                    LostTarget();
                    break;
                case State.Alarmed:
                    Alarmed();
                    break;
                case State.Yielding:
                    Yielding();
                    break;
                default:
                    Idle();
                    break;
            }
            AnyState();
        }
    }

    #region States
    private void AnyState()
    {
        SetAnimatorValues();
    }

    private void Idle()
    {
        //In the idle state, the enemy wanders around and passively looks for enemies.
        stateColor = Color.green;
        m_Target = null;
        isHeadsup = false;
        agent.speed = 1;
        agent.stoppingDistance = 1;
        if (Detection())
        {
            TransitionTo(State.Detect);
        }
        if (patrolPoints.Count > 0)
        {
            Patrol();
        }
        LookAtTarget(0);
    }

    private void Detect()
    {
        //In the detect state, the enemy has detected someone else and is waiting to validate its alignment (ally or enemy).
        stateColor = Color.yellow;
        agent.speed = 1;
        if (patrolPoints.Count > 0)
        {
            MoveTowardsTarget();
        }
        agent.stoppingDistance = 1;
        LookAtTarget(1);
        if (Detection())
        {
            Debug.Log(Mathf.Min(validationTime * Vector3.Distance(eyes.position, target.position), validationTime));
            if (CheckStateCountdown(Mathf.Min(validationTime * Vector3.Distance(eyes.position, target.position), validationTime)))
                TransitionTo(State.Chase);
        }
        else
        {
            TransitionTo(State.Alarmed);
        }
        isHeadsup = true;
    }

    private void Chase()
    {
        //In the chase state, the enemy has a valid target and chases it until the enemy is in attack range.
        if (!MusicController.music.hasTransitioned)
            MusicController.music.SetSnapshot(MusicController.music.dangerSnapshot);
        stateColor = Color.red;
        agent.speed = 6;
        agent.stoppingDistance = 3;
        MoveTowardsTarget();
        if (InAttackingRange() && Detection())
        {
            TransitionTo(State.Attack);
        }
        if (!Detection())
        {
            //Memorize target position.
            m_TargetLastSeenAt = m_Target.position;
            if (CheckStateCountdown(6))
            {
                TransitionTo(State.LostTarget);
            }
        }
    }

    private void Attack()
    {
        //In the attack state, the enemy attacks the target and checks if the target is still in attack range after the attack.
        stateColor = Color.black;
        agent.speed = 5;
        agent.stoppingDistance = stats.attackRange;

        if (!Detection() || !InAttackingRange())
        {
            MoveTowardsTarget();
            TransitionTo(State.Chase);
        }
        else
        {
            agent.speed = 2;
            AttackAction();
        }
    }

    private void LostTarget()
    {
        //In the lost target state, the enemy's target has left the enemy's line of sight for too long and the enemy goes to the last location where it has seen the target.
        stateColor = new Color(0.5f, 0.1f, 0);
        agent.speed = 3;
        agent.stoppingDistance = 0.5f;
        MoveTowardsTarget();
        LookAtTarget(0);
        if (Detection())
        {
            TransitionTo(State.Chase);
        }
        else if (CheckStateCountdown(5))
        {
            if (target)
                target.GetComponentInParent<PlayerController>().UpdateMusic();
            TransitionTo(State.Alarmed);
        }
    }

    private void Alarmed()
    {
        //In the alarmed state, the enemy acts the same as in the idle state but will detect enemies faster and will not patrol.
        stateColor = new Color(1, 0.5f, 0);
        m_Target = null;
        agent.speed = 1;
        agent.stoppingDistance = 1;
        LookAtTarget(0);
        if (Detection())
        {
            isHeadsup = true;
            LookAtTarget(1);
            if (CheckStateCountdown(validationTime / 2))
                TransitionTo(State.Chase);
        }
        if (CheckStateCountdown(8))
        {
            TransitionTo(State.Idle);
        }
    }

    private void Yielding()
    {
        //In the yielding state, the enemy has too low health to keep on attacking, and will surrender to the player.
        stateColor = Color.white;
        agent.speed = 0.05f;
        m_IKBodyWeight = Mathf.Lerp(m_IKBodyWeight, 1, Time.deltaTime * agent.speed);
    }
    #endregion

    #region Decisions
    private bool Detection()
    {
        //Optimizing Check so we don't do 5000 boxcasts every frame
        if (Vector3.Distance(transform.position, player.transform.position) < stats.viewRange)
        {
            RaycastHit hit = new RaycastHit();
            Vector3 aspect = new Vector3(0, 1 - stats.aspect, stats.aspect) * stats.viewSize;
            //Do several OverlapBox calls and check if the player is inside one of these.
            for (float f = 0; f < stats.viewRange; f += 5)
            {
                Collider[] coll = Physics.OverlapBox(eyes.position + eyes.forward * f, new Vector3(5, f * aspect.y, f * aspect.z) / 2);
                for (int i = 0; i < coll.Length; i++)
                {
                    if (coll[i].CompareTag(targetTag))
                    {
                        Debug.DrawLine(eyes.position, coll[i].ClosestPoint(eyes.position));
                        //The player is inside the frustum. Check if there is any object blocking the line of sight.
                        if (!Physics.Linecast(eyes.position, coll[i].ClosestPoint(eyes.position), out hit, detectLayers))
                        {
                            m_Target = coll[i].GetComponent<PlayerController>().targetable;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool InAttackingRange()
    {
        return Vector3.Distance(transform.position, m_Target.position) < stats.attackRange;
    }

    private bool ReachedPosition()
    {
        if (agent.remainingDistance < 0.5f)
        {
            TransitionTo(State.Alarmed);
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
    #region Actions
    private void MoveTowardsTarget()
    {
        if (m_Target)
        {
            if (state == State.LostTarget)
            {
                agent.destination = m_TargetLastSeenAt;
            }
            else
            {
                agent.destination = m_Target.position;
                if (agent.velocity.magnitude < 0.2f)
                {
                    //Turn towards target
                    Vector3 targetXZ = new Vector3(target.position.x, transform.position.y, target.position.z);
                    transform.LookAt(Vector3.Lerp(transform.position + transform.forward, targetXZ, Time.deltaTime));
                }
            }
        }
    }

    private void AttackAction()
    {
        //Take aim at the target
        if (m_IKBodyWeight < 1)
        {
            LookAtTarget(1);
        }
        //Check fire rate
        if (Time.time > m_AttackTimer)
        {
            //Fire weapon: do a raycast from the barrel forward direction and check if the player is hit.
            RaycastHit hit = new RaycastHit();
            Vector3 acc = Random.insideUnitSphere * (1 - accuracy);
            Debug.DrawRay(barrel.position, (barrel.forward + acc) * stats.attackRange, Color.yellow);
            if (Physics.SphereCast(barrel.position, 0.6f, barrel.forward + acc, out hit, stats.attackRange) && hit.collider.CompareTag(targetTag))
            {
                Debug.Log("Hit");
                hit.collider.GetComponent<PlayerHealth>().TakeDamage(stats.attackDamage, hit.point);
            }
            stats.shotImpactParticles.Stop();
            stats.shotImpactParticles.Play();
            stats.shotImpactParticles.GetComponent<AudioSource>().clip = stats.shotSounds[Random.Range(0, stats.shotSounds.Length)];
            stats.shotImpactParticles.GetComponent<AudioSource>().Stop();
            stats.shotImpactParticles.GetComponent<AudioSource>().Play();
            m_AttackTimer = Time.time + stats.attackRate;
        }
        //Make other enemies that are in bound assist this enemy.
        Collider[] enemies = Physics.OverlapSphere(barrel.position, 15);
        foreach (Collider col in enemies)
        {
            EnemyController controller = col.GetComponent<EnemyController>();
            if (controller && (!controller.isDangerous || controller.state == State.LostTarget))
            {
                Debug.Log(controller.name + " alerted.");
                controller.target = target;
                controller.TransitionTo(State.Chase);
            }
        }
        //Make sure the enemy doesn't walk into the player all the time
        agent.destination = transform.position;
    }

    private void LookAtTarget(float target)
    {
        if (m_IKBodyWeight < 0.9f)
        {
            m_IKBodyWeight = Mathf.Lerp(m_IKBodyWeight, target, Time.deltaTime * (agent.speed / 2));
        }
    }

    private void Patrol()
    {
        agent.destination = patrolPoints[m_NextWaypoint].position;
        agent.isStopped = false;
        if(agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            m_NextWaypoint = (m_NextWaypoint + 1) % patrolPoints.Count;
        }
    }

    #endregion

    public void TransitionTo(State toState)
    {
        if (!inTransition)
        {
            inTransition = true;
            m_StateTimer = 0;
            state = toState;
            inTransition = false;
        }
        return;
    }

    public bool CheckStateCountdown(float duration)
    {
        m_StateTimer += Time.deltaTime;
        return (m_StateTimer >= duration);
    }

    private void SetAnimatorValues()
    {
        float speed = agent.velocity.magnitude;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("NoMove"))
            speed = 0;
        if (isDangerous)
        {
            isHeadsup = true;
        }
        animator.SetFloat("Speed", speed);
        animator.SetBool("Headsup", isHeadsup);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!animator || !m_Target)
        {
            return;
        }
        animator.SetLookAtPosition(m_Target.position);
        animator.SetLookAtWeight(m_IKBodyWeight, 0.8f, 0.6f);
    }

    private void OnDestroy()
    {
        Destroy(agent);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(stateColor.r, stateColor.g, stateColor.b, stats.frustrumVisibility);

        Vector3 aspect = new Vector3(0, 1 - stats.aspect, stats.aspect) * stats.viewSize;
        
        for (float f = stats.frustumDistance; f < stats.viewRange; f += Mathf.Clamp(stats.frustumDistance, Mathf.Epsilon, Mathf.Infinity))
        {
            Gizmos.DrawWireCube(eyes.position + eyes.forward * f--, new Vector3(stats.frustumDistance, f * aspect.y, f * aspect.z));
        }

        Gizmos.color = Color.red;
        //Gizmos.DrawRay(eyes.position, eyes.forward * stats.attackRange);
        Gizmos.DrawRay(barrel.position, barrel.forward * stats.attackRange);
    }
#endif
}