using UnityEditor;
using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Adds draggable Scene-view handles to each <see cref="FishSpawner.FishSpawnEntry"/>'s
    /// (areaMin, areaMax) corners so the per-entry spawn rect can be edited
    /// with the mouse. Each entry gets a distinct color (cycled via
    /// FishSpawner.GetEntryColor) so it's obvious which rect belongs to which
    /// fish type. The default Inspector remains in use for typed-number
    /// editing — both paths stay in sync.
    /// </summary>
    [CustomEditor(typeof(FishSpawner))]
    public class FishSpawnerEditor : Editor
    {
        private void OnSceneGUI()
        {
            SerializedProperty entries = serializedObject.FindProperty("fishEntries");
            if (entries == null || entries.arraySize == 0) return;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                if (entry == null) continue;
                SerializedProperty areaMin = entry.FindPropertyRelative("areaMin");
                SerializedProperty areaMax = entry.FindPropertyRelative("areaMax");
                SerializedProperty prefab = entry.FindPropertyRelative("prefab");
                if (areaMin == null || areaMax == null) continue;

                Handles.color = FishSpawner.GetEntryColor(i);
                string label = prefab != null && prefab.objectReferenceValue != null
                    ? prefab.objectReferenceValue.name
                    : $"Entry {i}";

                Vector2 minNew = DragCorner(areaMin.vector2Value, $"{label} Min");
                Vector2 maxNew = DragCorner(areaMax.vector2Value, $"{label} Max");

                areaMin.vector2Value = minNew;
                areaMax.vector2Value = maxNew;
            }

            if (EditorGUI.EndChangeCheck())
            {
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
