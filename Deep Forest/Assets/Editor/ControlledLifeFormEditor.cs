using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ControlledLifeForm))]
public class ControlledLifeFormEditor : Editor {

    ControlledLifeForm clf;
    public override void OnInspectorGUI()
    {
        clf = target as ControlledLifeForm;

        DrawDefaultInspector();

        if (GUILayout.Button("Next Generation"))
        {
            clf.DrawNextGeneration();
        }
    }

}
