using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModeInfoPopup))]
public class ModeInfoPopupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ModeInfoPopup popup = (ModeInfoPopup)target;

        EditorGUILayout.Space(12);
        GUI.backgroundColor = new Color(0.96f, 0.65f, 0.12f, 1f); // Warm Golden Yellow
        if (GUILayout.Button("⚙️ Setup Mode Info UI & 2x2 Reward Boxes", GUILayout.Height(42)))
        {
            Undo.RecordObject(popup.gameObject, "Setup Mode Info UI");
            popup.SetupUI();
            EditorUtility.SetDirty(popup);
            EditorUtility.SetDirty(popup.gameObject);
        }
        GUI.backgroundColor = Color.white;
    }
}
