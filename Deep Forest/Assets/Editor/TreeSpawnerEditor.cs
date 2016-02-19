using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestTreeSpawner))]
public class TreeSpawnerEditor : Editor {

    TestTreeSpawner spawner;

    public override void OnInspectorGUI()
    {
        spawner = target as TestTreeSpawner;
        DrawDefaultInspector();

        if (GUILayout.Button("Spawn Tree"))
        {
            spawner.SpawnTree();
        }
    }
}
