using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor :  Editor{

    TerrainGenerator terrainGen;
	public override void OnInspectorGUI()
    {
        terrainGen = target as TerrainGenerator;
        if (DrawDefaultInspector())
        {
            if (terrainGen.autoUpdate)
            {
                terrainGen.GenerateTrees();
            }
        }
        
        if (GUILayout.Button("Generate New Map"))
        {
            terrainGen.GenerateSeed();
            terrainGen.GenerateMesh();
            terrainGen.GenerateTrees();
        }
    }
}
