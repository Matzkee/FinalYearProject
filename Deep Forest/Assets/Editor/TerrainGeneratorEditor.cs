using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor :  Editor{

    TerrainGenerator terrainGen;
	public override void OnInspectorGUI()
    {
        terrainGen = target as TerrainGenerator;
        DrawDefaultInspector();
        
        if (GUILayout.Button("Generate New Map"))
        {
            terrainGen.Generate();
        }
    }
}
