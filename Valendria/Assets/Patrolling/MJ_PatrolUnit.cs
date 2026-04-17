using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MJ_PatrolUnit : Entity
{
    public static MJ_PatrolUnit Instance;


    [SerializeField] private EntityTypes entityType = EntityTypes.TownsFolk;

    public List<Transform> rout = new();

    public bool isPatrolMangerReady = false;
    public bool routReady => rout.Count > 0;

    private int currentIndex = 0;
    private int direction = 1; // 1 = forward, -1 = backward

    [Header("Variation Settings")]
    [SerializeField] private float speedVariation = 1.5f;

    void Awake()
    {
        Instance = this;
        agent = GetComponent<NavMeshAgent>();
        
        // Randomize speed
        if (agent != null)
        {
            float baseSpeed = agent.speed;
            agent.speed = baseSpeed + Random.Range(-speedVariation, speedVariation);
        }

        direction = Random.Range(0, 2) == 0 ? 1 : -1;
    }

    void Update()
    {
        // Only request a route from the manager if we don't already have one
        if (isPatrolMangerReady && !routReady)
        {
            PatrolRouteManager.Instance.GetYourPatrolRoute(entityType, this);
            isPatrolMangerReady = false;

            if (routReady)
            {
                currentIndex = Random.Range(0, rout.Count);
                direction = Random.Range(0, 2) == 0 ? 1 : -1;
                SetNextDestination();
            }
        }


        if (!routReady || agent == null || agent.pathPending)
            return;

        // Check if agent reached current destination
        if (agent.remainingDistance <= agent.stoppingDistance && routReady)
        {
            HandleNextWaypoint();
        }

        Debug.Log("PatrolUnit of type" + entityType + routReady + " : " + agent.remainingDistance);
    }

    public void AssignRoute(List<Transform> route)
    {
        rout = route ?? new List<Transform>();
        if (routReady)
        {
            currentIndex = Random.Range(0, rout.Count);
            direction = Random.Range(0, 2) == 0 ? 1 : -1;
            SetNextDestination();
        }
    }

    void HandleNextWaypoint()
    {
        // If reached the last waypoint (going forward)
        if (currentIndex == rout.Count - 1 && direction == 1)
        {
            // 50% chance: loop to start or reverse direction
            if (Random.value < 0.5f)
            {
                currentIndex = 0;
                direction = 1;
            }
            else
            {
                direction = -1;
                currentIndex--;
            }
        }
        // If reached the first waypoint (going backward)
        else if (currentIndex == 0 && direction == -1)
        {
            // 50% chance: loop to end or go forward again
            if (Random.value < 0.5f)
            {
                currentIndex = rout.Count - 1;
                direction = -1;
            }
            else
            {
                direction = 1;
                currentIndex++;
            }
        }
        else
        {
            currentIndex += direction;
        }

        SetNextDestination();
    }

    void SetNextDestination()
    {
        if (rout.Count == 0)
            return;
        agent.SetDestination(rout[currentIndex].position);
    }
}
