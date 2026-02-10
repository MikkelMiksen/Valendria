using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EntityTypeSplit
{
    public EntityTypes type;
    public float percentage; // 0 to 1 (or 0 to 100, let's go with 0-1 for math ease)
}

[System.Serializable]
public struct EntitySpawnConfig
{
    public EntityTypes type;
    public int count;
}

[System.Serializable]
public class ZoneData
{
    public List<EntityTypes> allowedTypes;
    public List<Vector2> polygon; // xz points
    public int waypointCount;
    public List<EntityTypeSplit> typeSplits;
    public List<EntitySpawnConfig> entitySpawns;
}