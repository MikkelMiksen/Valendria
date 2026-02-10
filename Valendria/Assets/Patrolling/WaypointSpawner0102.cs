using System.Collections.Generic;
using UnityEngine;

public class WaypointSpawner0102 : MonoBehaviour
{
    [System.Serializable]
    public struct WaypointPrefabMapping
    {
        public EntityTypes type;
        public GameObject prefab;
    }

    [Header("Data")]
    [SerializeField] private SpawnZoneData spawnZoneData;
    [SerializeField] private List<WaypointPrefabMapping> prefabMappings = new List<WaypointPrefabMapping>();

    [Header("Settings")]
    [SerializeField] private float raycastDistance = 50f;
    [SerializeField] private LayerMask groundLayer = ~0;

    private Dictionary<EntityTypes, GameObject> prefabMap = new Dictionary<EntityTypes, GameObject>();

    private void Awake()
    {
        foreach (var mapping in prefabMappings)
        {
            if (mapping.prefab != null && !prefabMap.ContainsKey(mapping.type))
            {
                prefabMap.Add(mapping.type, mapping.prefab);
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

        SpawnAllWaypoints();
    }

    private void SpawnAllWaypoints()
    {
        foreach (var zone in spawnZoneData.zones)
        {
            SpawnWaypointsForZone(zone);
        }
    }

    private void SpawnWaypointsForZone(ZoneData zone)
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
            // If no splits, but allowed types, split equally or just use the first one
            EntityTypes fallbackType = zone.allowedTypes[0];
            countsPerType[fallbackType] = totalToSpawn;
        }
        else
        {
            Debug.LogWarning("Zone has no allowed types or splits defined. Skipping.");
            return;
        }

        // Spawn for each type
        foreach (var kvp in countsPerType)
        {
            EntityTypes type = kvp.Key;
            int count = kvp.Value;

            if (!prefabMap.ContainsKey(type))
            {
                Debug.LogWarning($"No prefab mapping for EntityType: {type}");
                continue;
            }

            GameObject prefab = prefabMap[type];

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
