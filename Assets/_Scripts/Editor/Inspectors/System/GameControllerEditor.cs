using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(GameController))]
    public class GameControllerEditor : InspectorEditorBase
    {
        private SerializedProperty serverGameBehaviourTemplate = null;
        private SerializedProperty scorerTempalte = null;
        private SerializedProperty backgroundMaterial = null;
        private SerializedProperty backgroundDepth = null;

        protected override void FindProperties()
        {
            serverGameBehaviourTemplate = GetProperty(nameof(serverGameBehaviourTemplate));
            scorerTempalte = GetProperty(nameof(scorerTempalte));
            backgroundMaterial = GetProperty(nameof(backgroundMaterial));
            backgroundDepth = GetProperty(nameof(backgroundDepth));
        }

		protected override void DrawProperties()
		{
            EditorGUILayout.PropertyField(backgroundDepth);
        }

		protected override void DrawReferences()
        {
            EditorGUILayout.PropertyField(serverGameBehaviourTemplate);
            EditorGUILayout.PropertyField(scorerTempalte);
            EditorGUILayout.PropertyField(backgroundMaterial);
        }
    }
}
