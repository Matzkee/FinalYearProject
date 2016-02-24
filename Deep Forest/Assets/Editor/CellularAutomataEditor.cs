using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CellularAutomata))]
public class CellularAutomataEditor : Editor {

    CellularAutomata ca;
    public override void OnInspectorGUI()
    {
        ca = target as CellularAutomata;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            ca.GenerateMap();
        }
    }
}
