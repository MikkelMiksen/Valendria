using UnityEngine;
using UnityEngine.AI;

public class Entity : MonoBehaviour
{
    public NavMeshAgent agent;
    //public Animator animator;

    public float walking_speed = 5f;

    Transform[] waypoints = new Transform[3];
    int currentWaypoint = 0;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
}
