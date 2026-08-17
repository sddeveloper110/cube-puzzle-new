using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameOverPanel))]
public class GameOverPanelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameOverPanel panel = (GameOverPanel)target;

        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.2f, 0.7f, 1.0f);
        if (GUILayout.Button("⚡ Auto-Assign References From Hierarchy", GUILayout.Height(35)))
        {
            panel.AutoSetupReferences();
            EditorUtility.SetDirty(panel);
        }
        GUI.backgroundColor = Color.white;
    }
}
