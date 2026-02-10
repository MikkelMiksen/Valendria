#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnZoneBaker))]
public class SpawnZoneBakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpawnZoneBaker baker = (SpawnZoneBaker)target;

        if (GUILayout.Button("Bake Spawn Zones"))
        {
            baker.Bake();
        }
    }
}
#endif