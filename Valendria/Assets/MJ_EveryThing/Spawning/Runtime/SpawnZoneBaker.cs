using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class SpawnZoneBaker : MonoBehaviour
{
   public SpawnZoneData outputData;

   public void Bake()
   {
      if (outputData == null)
      {
         Debug.LogError("Output Data is not assigned to SpawnZoneBaker!");
         return;
      }

      if (outputData.zones == null)
      {
         outputData.zones = new List<ZoneData>();
      }
      
      outputData.zones.Clear();
      
      var authoringZones = FindObjectsOfType<SpawnZoneAuthoring>();

      foreach (var z in authoringZones)
      {
         foreach (var poly in z.polygons)
         {
            if (poly.points.Count < 3) continue;

            ZoneData zd = new ZoneData()
            {
               allowedTypes = new List<EntityTypes>(z.types),
               polygon = new List<Vector2>(),
               waypointCount = poly.waypointCount,
               typeSplits = new List<EntityTypeSplit>(poly.typeSplits)
            };

            foreach (var p in poly.points)
            {
               zd.polygon.Add(new Vector2(p.x, p.z));
            }
         
            outputData.zones.Add(zd);
         }
      }
#if UNITY_EDITOR
      EditorUtility.SetDirty(outputData);
      AssetDatabase.SaveAssets();
#endif
      Debug.Log("Spawn zones baked.");
   }
}