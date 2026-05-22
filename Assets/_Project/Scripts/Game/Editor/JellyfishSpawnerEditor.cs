using UnityEditor;
using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Adds draggable Scene-view handles to <see cref="JellyfishSpawner"/> so
    /// the two spawn-rect corners (min, max) can be repositioned directly
    /// with the mouse. Mirrors <see cref="CrabSpawnerEditor"/>; the default
    /// Inspector stays in use for typed-number editing.
    /// </summary>
    [CustomEditor(typeof(JellyfishSpawner))]
    public class JellyfishSpawnerEditor : Editor
    {
        private static readonly Color HandleColor = new Color(0.8f, 0.4f, 1f, 1f);

        private void OnSceneGUI()
        {
            SerializedProperty min = serializedObject.FindProperty("spawnAreaMin");
            SerializedProperty max = serializedObject.FindProperty("spawnAreaMax");

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            Handles.color = HandleColor;
            Vector2 minNew = DragCorner(min.vector2Value, "Min");
            Vector2 maxNew = DragCorner(max.vector2Value, "Max");

            if (EditorGUI.EndChangeCheck())
            {
                min.vector2Value = minNew;
                max.vector2Value = maxNew;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static Vector2 DragCorner(Vector2 worldPos, string label)
        {
            Vector3 pos3 = new Vector3(worldPos.x, worldPos.y, 0f);
            float size = HandleUtility.GetHandleSize(pos3) * 0.12f;
            Vector3 moved = Handles.FreeMoveHandle(pos3, size, Vector3.zero, Handles.SphereHandleCap);
            Handles.Label(pos3 + new Vector3(size * 1.5f, size * 0.5f, 0f), label);
            return new Vector2(moved.x, moved.y);
        }
    }
}
