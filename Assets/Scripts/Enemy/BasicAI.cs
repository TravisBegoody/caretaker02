using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicAI : MonoBehaviour
{
    public Animator animator;
    public EnemyStats Stats;

    public float currentHealth;
    public enum AIStates { Idle, Chase, Attack, Flee, Investigate };
    public enum AISenses { See, Hear, Near, None };
    [SerializeField] protected AIStates currentState;
    [SerializeField] protected AISenses currentSense;
    private float timeInState;
    protected NavMeshAgent pawn;
    protected NavMeshHit fleePoint;

    //Used to investigate area where player was
    [SerializeField] protected Vector3 playerLastPosition;

    [SerializeField] private bool doesIdle;
    [SerializeField] protected float idleRadius;
    [SerializeField] protected float fleeRadius;
    //is it currently idle moving
    protected bool isIdle;
    protected bool isFlee;
    public bool onCooldown;
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();

        //Sets currHealth to statHealth
        currentHealth = Stats.Health;

        GameManager.Instance.AddEnemy(this);
        pawn = this.GetComponent<NavMeshAgent>();

        //Sets pawns nav agent to the stats
        pawn.speed = Stats.WalkSpeed;
        pawn.angularSpeed = Stats.TurnSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        ArtificialIntelligence();

        //Used for the default enemy
        DefaultState();
    }
    /// <summary>
    /// Used to check if the Enemy should do any type of action depending on what is happening.
    /// </summary>
    public void ArtificialIntelligence()
    {
        TargetCheck();
        if (!isFlee)
        {
            DetermineState();
        }
    }
    public virtual void Death()
    {
        GameManager.Instance.RemoveEnemy(this);

        gameObject.SetActive(false);
    }
    public void MoveTowards(Transform target)
    {
        if (target != null)
        {
            pawn.SetDestination(target.position);
        }
    }
    private void DefaultState()
    {
        switch (currentState)
        {
            case AIStates.Attack:
                pawn.isStopped = true;

                animator.SetTrigger("Attack");
                StartCoroutine(Cooldown());
                break;
            case AIStates.Chase:
                pawn.isStopped = false;
                pawn.speed = Stats.WalkSpeed * Stats.SprintMultiplier;
                pawn.SetDestination(playerLastPosition);

                animator.SetBool("isChasing", true);
                animator.SetBool("isMoving", true);

                AudioManager.Instance.CheckPlayAt("Spotted", this.transform.position);

                if (isIdle)
                {
                    StopCoroutine(IdleMovement());
                    isIdle = false;
                }
                break;
            case AIStates.Investigate: //hears enemy and moves in
                pawn.isStopped = false;
                pawn.speed = Stats.WalkSpeed;
                pawn.SetDestination(playerLastPosition);

                animator.SetBool("isChasing", false);
                animator.SetBool("isMoving", true);

                if (isIdle)
                {
                    StopCoroutine(IdleMovement());
                    isIdle = false;
                }
                break;
            case AIStates.Flee:
                pawn.speed = Stats.WalkSpeed * Stats.SprintMultiplier;

                if (!isFlee)
                {
                    StartCoroutine(FleeMovement());
                }
                break;
            case AIStates.Idle:
                pawn.speed = Stats.WalkSpeed;
                if (!isIdle)
                {
                    pawn.speed = Stats.WalkSpeed;
                    StartCoroutine(IdleMovement());
                    AudioManager.Instance.PlayAt("EnemySound",this.transform.position);
                }
                break;
        }
    }
    public void DetermineState()
    {
        switch (currentSense)
        {
            case AISenses.See: // Enemy can see the player in their line of sight
                currentState = AIStates.Chase;
                break;
            case AISenses.Hear: // Enemy can hear the player
                currentState = AIStates.Investigate;
                break;
            case AISenses.Near: // Enemy has no walls between them and player but not in line of sight
                currentState = AIStates.Attack;
                break;
            case AISenses.None: // Enemy can't see or hear player
                currentState = AIStates.Idle;
                break;
        } // Enemy is low on health
        if (onCooldown)
        {
            currentState = AIStates.Flee;
        }
    }
    //Checks if it can see or hear the player or any type of creature its against
    public void TargetCheck()
    {
        RaycastHit hit;

        Vector3 offset = new Vector3(0f, 0.5f, 0f);
        Vector3 targetPosition = GameManager.Instance.GetPlayer().transform.position + offset;
        Vector3 AIPostion = this.transform.position;
        Vector3 direction = targetPosition - AIPostion;

        float distance = Vector3.Distance(AIPostion, targetPosition);
        //Draws the line of sight for the tank
        float angle = Vector3.Angle(direction, transform.forward);

        currentSense = AISenses.None;

        //hears player
        if (distance <= Stats.HearRange)
        {
            currentSense = AISenses.Hear;
            playerLastPosition = targetPosition;
        }

        //Create a ray cast as far as sightrange, checks if it it the player, check if it is in 40 degrees of sight
        //Sees player
        if (Physics.Raycast(AIPostion, direction, out hit, Stats.SightRange) && hit.collider.transform.position + offset == targetPosition)
        {
            if (angle < Stats.SightFOV)// Has in sights
            {
                currentSense = AISenses.See;
                if (distance <= Stats.MeleeRange)
                    currentSense = AISenses.Near;
            }
        }
        //Debug.Log(hit.collider.gameObject.name);
        //Debugs line of sight of the player
        switch (currentSense)
        {
            case AISenses.See:
                Debug.DrawRay(AIPostion, direction, Color.green);
                break;
            case AISenses.Hear:
                Debug.DrawRay(AIPostion, direction, Color.yellow);
                break;
            case AISenses.Near:
                Debug.DrawRay(AIPostion, direction, Color.cyan);
                break;
            case AISenses.None:
                Debug.DrawRay(AIPostion, direction, Color.red);
                break;
        }
    }
    //Makes an enemy randomly move around
    IEnumerator IdleMovement()
    {

            isIdle = true;
            //Gets a random point inside of a unit sphere and multiplies it by the radius of the idle radius
            Vector3 randomPoint = Random.insideUnitSphere * idleRadius;

            //Makes the navmesh hit and then 
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, idleRadius, 1)) // Checks if it finds a point or not and stops if it doesn't
            {
                pawn.isStopped = false;
                pawn.SetDestination(hit.position + transform.position);
                animator.SetBool("isChasing", false);
                animator.SetBool("isMoving", true);
            }
            Vector3 distance = this.transform.position - pawn.destination;
            distance.y = 0f;
            float distanceFlat = distance.magnitude;
            while (distanceFlat > 0.5f)
            {
                distance = this.transform.position - pawn.destination;
                distance.y = 0f;
                distanceFlat = distance.magnitude;

                //Debug.Log(this.transform.position + " - " + pawn.destination + " = " +distanceFlat);
                animator.SetBool("isMoving", true);
                if(currentState != AIStates.Idle)
                {
                    isIdle = false;
                    StopCoroutine(IdleMovement());
                }
                yield return null;
            }

            animator.SetBool("isMoving", false);
            yield return new WaitForSeconds(5f); //waits three secodns before ending
            isIdle = false;
        
    }
    IEnumerator FleeMovement()
    {
        if (!isFlee)
        {
            Debug.Log("im flee");
            isFlee = true;
            //Gets a random point inside of a unit sphere and multiplies it by the radius of the idle radius
            Vector3 randomPoint = Random.insideUnitSphere * fleeRadius;

            //Makes the navmesh hit and then 
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, fleeRadius, 1)) // Checks if it finds a point or not and stops if it doesn't
            {
                pawn.isStopped = false;
                pawn.SetDestination(hit.position + transform.position);
                animator.SetBool("isFleeing", true);
            }
            Vector3 distance = this.transform.position - hit.position;
            distance.y = 0f;
            float distanceFlat = distance.magnitude;
            while (distanceFlat > 0.5f)
            {
                distance = this.transform.position - pawn.destination;
                distance.y = 0f;
                distanceFlat = distance.magnitude;

                Debug.Log(this.transform.position + " - " + pawn.destination + " = " + distanceFlat);

                animator.SetBool("isFleeing", true);
                yield return null;
                Debug.Log("fleeing");
            }
            Debug.Log("done fleeing");
            animator.SetBool("isFleeing", false);
            isFlee = false;
            yield return new WaitForSeconds(2f); //waits three secodns before ending
        }
    }
    IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(3f); //waits three secodns before ending
        onCooldown = false;
        StartCoroutine(FleeMovement());
    }
    public void DamagePlayer()
    {
        GameManager.Instance.AttackedPlayer(Stats.MeleeDamage);
    }
}
