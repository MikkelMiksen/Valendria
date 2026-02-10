using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WaypointSpawner0102 : MonoBehaviour
{
    [System.Serializable]
    public struct WaypointPrefabMapping
    {
        public EntityTypes type;
        public GameObject prefab;
    }

    [System.Serializable]
    public struct EntityPrefabMapping
    {
        public EntityTypes type;
        public GameObject prefab; // Prefab containing MJ_PatrolUnit configured for this type
    }

    [Header("Data")]
    [SerializeField] private SpawnZoneData spawnZoneData;
    [FormerlySerializedAs("prefabMappings")]
        [SerializeField] private List<WaypointPrefabMapping> waypointPrefabMappings = new List<WaypointPrefabMapping>();
    [SerializeField] private List<EntityPrefabMapping> entityPrefabMappings = new List<EntityPrefabMapping>();

    [Header("Settings")]
    [SerializeField] private float raycastDistance = 50f;
    [SerializeField] private LayerMask groundLayer = ~0;

    private Dictionary<EntityTypes, GameObject> waypointPrefabMap = new Dictionary<EntityTypes, GameObject>();
    private Dictionary<EntityTypes, GameObject> entityPrefabMap = new Dictionary<EntityTypes, GameObject>();

    private void Awake()
    {
        foreach (var mapping in waypointPrefabMappings)
        {
            if (mapping.prefab != null && !waypointPrefabMap.ContainsKey(mapping.type))
            {
                waypointPrefabMap.Add(mapping.type, mapping.prefab);
            }
        }
        foreach (var mapping in entityPrefabMappings)
        {
            if (mapping.prefab != null && !entityPrefabMap.ContainsKey(mapping.type))
            {
                entityPrefabMap.Add(mapping.type, mapping.prefab);
            }
        }
    }

    private void Start()
    {
        if (spawnZoneData == null)
        {
            Debug.LogError("SpawnZoneData is not assigned to WaypointSpawner0102.");
            return;
        }

        SpawnAllForZones();
    }

    private void SpawnAllForZones()
    {
        foreach (var zone in spawnZoneData.zones)
        {
            SpawnForZone(zone);
        }
    }

    private void SpawnForZone(ZoneData zone)
    {
        if (zone.polygon == null || zone.polygon.Count < 3) return;

        int totalToSpawn = zone.waypointCount;
        if (totalToSpawn <= 0) return;

        // Calculate counts based on splits
        Dictionary<EntityTypes, int> countsPerType = new Dictionary<EntityTypes, int>();
        int remaining = totalToSpawn;

        if (zone.typeSplits != null && zone.typeSplits.Count > 0)
        {
            foreach (var split in zone.typeSplits)
            {
                int count = Mathf.FloorToInt(totalToSpawn * split.percentage);
                countsPerType[split.type] = count;
                remaining -= count;
            }

            // Distribute remainder to the first type in splits or allowed types
            if (remaining > 0)
            {
                EntityTypes fallbackType = zone.typeSplits[0].type;
                countsPerType[fallbackType] += remaining;
            }
        }
        else if (zone.allowedTypes != null && zone.allowedTypes.Count > 0)
        {
            // If no splits, but allowed types, assign all to first allowed type
            EntityTypes fallbackType = zone.allowedTypes[0];
            countsPerType[fallbackType] = totalToSpawn;
        }
        else
        {
            Debug.LogWarning("Zone has no allowed types or splits defined. Skipping.");
            return;
        }

        // Keep a per-zone route for each type in this polygon
        Dictionary<EntityTypes, List<Transform>> zoneRoutes = new Dictionary<EntityTypes, List<Transform>>();

        // Spawn waypoints for each type IN THIS ZONE ONLY
        foreach (var kvp in countsPerType)
        {
            EntityTypes type = kvp.Key;
            int count = kvp.Value;

            if (!waypointPrefabMap.ContainsKey(type))
            {
                Debug.LogWarning($"No waypoint prefab mapping for EntityType: {type}");
                continue;
            }

            GameObject prefab = waypointPrefabMap[type];
            var route = new List<Transform>(count);

            for (int i = 0; i < count; i++)
            {
                Vector3? spawnPos = GetRandomPointInPolygon(zone.polygon);
                if (spawnPos.HasValue)
                {
                    GameObject go = Instantiate(prefab, spawnPos.Value, Quaternion.identity);
                    go.tag = "Waypoint";
                    Waypoint wp = go.GetComponent<Waypoint>();
                    if (wp == null) wp = go.AddComponent<Waypoint>();
                    wp.entityType = type;
                    route.Add(go.transform);
                }
            }

            if (route.Count > 0)
            {
                zoneRoutes[type] = route;
                // Optional: register with PatrolRouteManager for gizmo drawing/central tracking
                if (PatrolRouteManager.Instance != null)
                {
                    PatrolRouteManager.Instance.RegisterRoute(type, route);
                }
            }
        }

        // Now spawn entities for this zone and assign the zone's route for their type
        if (zone.entitySpawns != null)
        {
            foreach (var spawnCfg in zone.entitySpawns)
            {
                if (!entityPrefabMap.TryGetValue(spawnCfg.type, out var entityPrefab))
                {
                    Debug.LogWarning($"No entity prefab mapping for EntityType: {spawnCfg.type}");
                    continue;
                }

                if (!zoneRoutes.TryGetValue(spawnCfg.type, out var route) || route == null || route.Count == 0)
                {
                    Debug.LogWarning($"No route generated in this zone for EntityType: {spawnCfg.type}");
                    continue;
                }

                for (int i = 0; i < spawnCfg.count; i++)
                {
                    // Spawn at the first waypoint or a random waypoint from the route
                    var spawnAt = route[Random.Range(0, route.Count)];
                    var unitGO = Instantiate(entityPrefab, spawnAt.position, Quaternion.identity);
                    unitGO.tag = "Patrolling";
                    var unit = unitGO.GetComponent<MJ_PatrolUnit>();
                    if (unit != null)
                    {
                        unit.AssignRoute(route);
                    }
                    else
                    {
                        Debug.LogWarning($"Spawned entity prefab for {spawnCfg.type} has no MJ_PatrolUnit component.");
                    }
                }
            }
        }
    }

    private Vector3? GetRandomPointInPolygon(List<Vector2> polygon)
    {
        // Get bounding box
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var p in polygon)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minZ) minZ = p.y;
            if (p.y > maxZ) maxZ = p.y;
        }

        int maxAttempts = 100;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            Vector2 randomPt = new Vector2(randomX, randomZ);

            if (IsPointInPolygon(randomPt, polygon))
            {
                // Raycast to find ground height
                Vector3 origin = new Vector3(randomX, 100f, randomZ);
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f, groundLayer))
                {
                    return hit.point;
                }
                
                // Fallback to y=0 if no ground hit
                return new Vector3(randomX, 0f, randomZ);
            }
        }

        return null;
    }

    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        bool inside = false;
        int j = polygon.Count - 1;
        for (int i = 0; i < polygon.Count; i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
            j = i;
        }
        return inside;
    }
}
