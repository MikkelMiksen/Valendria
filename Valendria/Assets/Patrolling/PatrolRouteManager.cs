using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolRouteManager : MonoBehaviour
{
        public static PatrolRouteManager Instance; void Awake() { Instance = this; }

        // Legacy: aggregated per-type routes (merged from all polygons)
        public Dictionary<EntityTypes, List<Transform>> routs = new Dictionary<EntityTypes, List<Transform>>();
        // New: per-type list of per-polygon routes
        public Dictionary<EntityTypes, List<List<Transform>>> routesByType = new Dictionary<EntityTypes, List<List<Transform>>>();

        private GameObject[] patrollingEntities;

        IEnumerator Start()
        {
                yield return new WaitForSeconds(3f);
                // Legacy aggregation (kept for backwards compatibility and fallback)
                GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
                foreach (var waypoint in waypoints)
                {
                        var type = waypoint.GetComponent<Waypoint>().entityType;

                        if (!routs.ContainsKey(type))
                                routs[type] = new List<Transform>();

                        routs[type].Add(waypoint.transform);
                }

                Debug.Log("Patrol routs loaded");

                patrollingEntities = GameObject.FindGameObjectsWithTag("Patrolling");
                foreach (GameObject patrollingEntity  in patrollingEntities)
                {
                        patrollingEntity.GetComponent<MJ_PatrolUnit>().isPatrolMangerReady = true;
                }
        }

        public void RegisterRoute(EntityTypes entityType, List<Transform> route)
        {
                if (route == null || route.Count == 0) return;
                if (!routesByType.TryGetValue(entityType, out var list))
                {
                        list = new List<List<Transform>>();
                        routesByType[entityType] = list;
                }
                list.Add(route);
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

        //DEBUGGING HERE

        private Dictionary<EntityTypes, Color> routeColors = new Dictionary<EntityTypes, Color>()
        {
                { EntityTypes.TownsFolk, Color.yellow },
                { EntityTypes.Hogling, Color.red },
                // Add more as needed
        };
        
        void OnDrawGizmos()
        {
                // Draw legacy aggregated routes
                if (routs != null)
                {
                        foreach (var kvp in routs)
                        {
                                EntityTypes type = kvp.Key;
                                List<Transform> points = kvp.Value;

                                if (points == null || points.Count < 2)
                                        continue;

                                if (routeColors.TryGetValue(type, out Color c)) Gizmos.color = c; else Gizmos.color = Color.white;

                                for (int i = 0; i < points.Count - 1; i++)
                                {
                                        if (points[i] != null && points[i + 1] != null)
                                        {
                                                Gizmos.DrawLine(points[i].position, points[i + 1].position);
                                        }
                                }
                        }
                }

                // Draw per-zone routes
                if (routesByType != null)
                {
                        foreach (var kvp in routesByType)
                        {
                                EntityTypes type = kvp.Key;
                                if (routeColors.TryGetValue(type, out Color c)) Gizmos.color = c; else Gizmos.color = Color.white;

                                var routes = kvp.Value;
                                if (routes == null) continue;
                                foreach (var route in routes)
                                {
                                        if (route == null || route.Count < 2) continue;
                                        for (int i = 0; i < route.Count - 1; i++)
                                        {
                                                if (route[i] != null && route[i + 1] != null)
                                                {
                                                        Gizmos.DrawLine(route[i].position, route[i + 1].position);
                                                }
                                        }
                                }
                        }
                }
        }
}
