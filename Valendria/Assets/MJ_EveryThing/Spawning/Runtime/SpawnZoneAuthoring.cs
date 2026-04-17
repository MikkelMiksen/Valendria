using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PolygonData
{
    public List<Vector3> points = new List<Vector3>();
    public int waypointCount;
    public List<EntityTypeSplit> typeSplits = new List<EntityTypeSplit>();
}

public class SpawnZoneAuthoring : MonoBehaviour
{
    public List<EntityTypes> types; // Default types for the whole authoring component
    public List<PolygonData> polygons = new List<PolygonData>();
}