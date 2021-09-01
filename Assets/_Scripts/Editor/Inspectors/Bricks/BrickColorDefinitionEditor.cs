using Discode.Breakout.Bricks;
using Discode.Breakout.Levels;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(BrickColorDefinition))]
    public class BrickColorDefinitionEditor : InspectorEditorBase
    {
        private SerializedProperty layerColors = null;

        protected override void OnStart()
        {
            showProperties = true;
            showReferencesFoldout = false;
        }

        protected override void FindProperties()
        {
            layerColors = GetProperty(nameof(layerColors));
        }

		protected override void DrawProperties()
        {
            EditorGUILayout.PropertyField(layerColors);
        }
    }
}
