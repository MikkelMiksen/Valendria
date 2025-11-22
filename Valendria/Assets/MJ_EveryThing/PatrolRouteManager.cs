using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolRouteManager : MonoBehaviour
{
        public static PatrolRouteManager Instance; void Awake() { Instance = this; }


        public Dictionary<EntityTypes, List<Transform>> routs = new Dictionary<EntityTypes, List<Transform>>();

        private GameObject[] patrollingEntities;

        IEnumerator Start()
        {
                yield return new WaitForSeconds(10f);
                //Getting routs with waypoint types
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

        public void GetYourPatrolRoute(EntityTypes entityType, MJ_PatrolUnit unit)
        {
                if (routs.ContainsKey(entityType))
                {
                        unit.rout.AddRange(routs[entityType]);
                        Debug.Log(entityType + " had rout assigned to its list");
                }
                else
                {
                        Debug.Log(" - - - No route found for: " + entityType);
                }
        }

}
