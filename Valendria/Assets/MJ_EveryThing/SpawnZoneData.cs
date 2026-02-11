using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Empty_SpawnZoneDataSO", menuName = "Scriptable Objects/SpawnZoneData")]
public class SpawnZoneData : ScriptableObject
{
    public List<ZoneData> zones;
}
