﻿using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GuardBehaviour))]
public class GuardBehaviourEditor : Editor {

    GuardBehaviour gh;

    public override void OnInspectorGUI()
    {
        gh = target as GuardBehaviour;
        DrawDefaultInspector();
    }

    void OnSceneGUI()
    {
        if (gh != null)
        {
            Handles.color = new Color(1, 1, 1, 0.1f);
            Handles.DrawSolidArc(
                gh.transform.position,
                gh.transform.up,
                gh.transform.forward,
                gh.viewAngle/2,
                gh.viewRange);
            Handles.DrawSolidArc(
                gh.transform.position,
                -gh.transform.up,
                gh.transform.forward,
                gh.viewAngle / 2,
                gh.viewRange);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            Handles.Label(gh.transform.position + Vector3.up * 1.5f,
                "Pos: " + gh.transform.position.ToString() + "\nState: " + gh.state.Description(), style);
        }
    }

}