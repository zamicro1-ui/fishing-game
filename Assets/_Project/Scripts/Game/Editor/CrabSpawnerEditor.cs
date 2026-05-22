using UnityEditor;
using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Adds draggable Scene-view handles to <see cref="CrabSpawner"/> so the
    /// four spawn-area corners (right-min, right-max, left-min, left-max) can
    /// be repositioned directly with the mouse instead of typing numbers in
    /// the Inspector. The default Inspector is left untouched and continues
    /// to display the same Vector2 fields — both editing paths stay in sync.
    /// </summary>
    [CustomEditor(typeof(CrabSpawner))]
    public class CrabSpawnerEditor : Editor
    {
        private static readonly Color RightColor = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color LeftColor = new Color(0.4f, 0.8f, 1f, 1f);

        private void OnSceneGUI()
        {
            SerializedProperty rMin = serializedObject.FindProperty("rightAreaMin");
            SerializedProperty rMax = serializedObject.FindProperty("rightAreaMax");
            SerializedProperty lMin = serializedObject.FindProperty("leftAreaMin");
            SerializedProperty lMax = serializedObject.FindProperty("leftAreaMax");

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            Handles.color = RightColor;
            Vector2 rMinNew = DragCorner(rMin.vector2Value, "R-Min");
            Vector2 rMaxNew = DragCorner(rMax.vector2Value, "R-Max");

            Handles.color = LeftColor;
            Vector2 lMinNew = DragCorner(lMin.vector2Value, "L-Min");
            Vector2 lMaxNew = DragCorner(lMax.vector2Value, "L-Max");

            if (EditorGUI.EndChangeCheck())
            {
                rMin.vector2Value = rMinNew;
                rMax.vector2Value = rMaxNew;
                lMin.vector2Value = lMinNew;
                lMax.vector2Value = lMaxNew;
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
