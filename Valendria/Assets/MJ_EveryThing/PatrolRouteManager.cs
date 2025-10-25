using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolRouteManager : MonoBehaviour
{
        public static PatrolRouteManager Instance; void Awake() { Instance = this; }

        [SerializeField]
        private Dictionary<EntityTypes, List<Transform>> routs = new Dictionary<EntityTypes, List<Transform>>();

        private GameObject[] patrollingEntities;

        IEnumerator Start()
        {
                yield return new WaitForSeconds(10f);
                //Geyying routs with waypoint types
                GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
                foreach (var waypoint in waypoints)
                {
                        var type = waypoint.GetComponent<Waypoint>().entityType;

                        // Ensure the key exists
                        if (!routs.ContainsKey(type))
                                routs[type] = new List<Transform>();

                        // Add the waypoint to the correct list
                        routs[type].Add(waypoint.transform);
                }

                Debug.Log("Patrol routs loaded");

                patrollingEntities = GameObject.FindGameObjectsWithTag("Patrolling");
                foreach (GameObject patrollingEntity  in patrollingEntities)
                {
                        patrollingEntity.GetComponent<MJ_PatrolUnit>().isPatrolMangerReady = true;
                }
        }

        public void GetYourPatrolRoute(EntityTypes entityType)
        {
                if (routs.ContainsKey(entityType))
                {
                        MJ_PatrolUnit.Instance.rout.AddRange(routs[entityType]);
                        Debug.Log(entityType + " had rout assigned to it's list");
                }

                if (MJ_PatrolUnit.Instance.rout.Count == 0)
                {
                        Debug.Log(" - - - No route found - - - ");
                }
        }

}
