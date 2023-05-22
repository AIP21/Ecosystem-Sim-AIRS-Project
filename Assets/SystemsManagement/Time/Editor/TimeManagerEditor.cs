using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DayTime
{
    [CustomEditor(typeof(TimeManager))]
    public class TimeManagerEditor : Editor
    {
        // Target
        private TimeManager system;

        private void OnEnable()
        {
            // Get target
            system = (TimeManager)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space(10);

            // Buttons
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New Day"))
                system.NewDay();

            GUILayout.EndHorizontal();
        }
    }
}