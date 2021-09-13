using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Test myTarget = target as Test;
        if (GUILayout.Button("Build UI")) {
            myTarget.BuildUI();
        }
    }
}
