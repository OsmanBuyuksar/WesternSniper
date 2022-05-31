using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (AutoGenerator))]
public class InspectorAuto : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AutoGenerator generator = (AutoGenerator)target;

        if (GUILayout.Button("GENERATE ROAD"))
        {
            generator.Generate();
        }
    }
}
