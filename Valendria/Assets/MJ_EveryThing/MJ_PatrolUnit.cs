using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MJ_PatrolUnit : Entity
{
    public static MJ_PatrolUnit Instance;


    [SerializeField] private EntityTypes entityType = EntityTypes.TownsFolk;

    public List<Transform> rout = new List<Transform>();

    public bool isPatrolMangerReady = false;
    public bool routReady => rout.Count > 0;

    private int currentIndex = 0;
    private int direction = 1; // 1 = forward, -1 = backward

    void Awake()
    {
        Instance = this;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (isPatrolMangerReady)
        {
            PatrolRouteManager.Instance.GetYourPatrolRoute(entityType);
            isPatrolMangerReady = false;

            if (routReady)
            {
                currentIndex = 0;
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

        Debug.Log("PAtrolUnit" + routReady + " : " + agent.remainingDistance);
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
        if (rout.Count == 0) return;
        agent.SetDestination(rout[currentIndex].position);
    }
}
